param
(
    [Parameter(Mandatory=$true)]
    [string] $envName,

    [Parameter(Mandatory=$true)]
    [string] $resourceGroupName,

    [string] $path
)

<#
.SYNOPSIS
    Creates the runtime managed-identity principal and grants access to Azure MySQL test databases.
.DESCRIPTION
    Runs inside the private bootstrap container. Azure resource discovery uses the container's
    managed identity; database administration uses the deploying Entra user's short-lived token
    supplied through MYSQL_BOOTSTRAP_ACCESS_TOKEN, never a native administrator password.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = $env:AZD_PROJECT_PATH
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
}
$resourceGroupOverride = $resourceGroupName
. (Join-Path $repoRoot 'scripts/prefix_resource_names.ps1') -envName $envName
$resourceGroupName = $resourceGroupOverride
. (Join-Path $PSScriptRoot 'mysql_entra_helpers.ps1')

if (-not (Get-Command mysql -ErrorAction SilentlyContinue)) {
    throw 'The mysql CLI is required for Azure MySQL initialization.'
}
$entraAdminLogin = $env:MYSQL_ENTRA_ADMIN_LOGIN
$bootstrapToken = $env:MYSQL_BOOTSTRAP_ACCESS_TOKEN
if ([string]::IsNullOrWhiteSpace($entraAdminLogin) -or [string]::IsNullOrWhiteSpace($bootstrapToken)) {
    throw 'MYSQL_ENTRA_ADMIN_LOGIN and MYSQL_BOOTSTRAP_ACCESS_TOKEN are required. Run the private postprovision launcher as the deploying Entra user.'
}

$identityJson = az identity show --name $identityName --resource-group $resourceGroupName
if ($LASTEXITCODE -ne 0) {
    throw "Unable to resolve runtime managed identity '$identityName'."
}
$identity = $identityJson | ConvertFrom-Json
$clientId = [guid]$identity.clientId
$principalId = [guid]$identity.principalId
if ($clientId -eq [guid]::Empty -or $principalId -eq [guid]::Empty) {
    throw 'The runtime managed identity has an invalid client or principal ID.'
}
$identityLogin = $identityName.Replace("'", "''")
$clientVersion = & mysql --version
if ($LASTEXITCODE -ne 0) {
    throw 'Unable to determine the installed mysql client version.'
}

$previousPassword = [Environment]::GetEnvironmentVariable('MYSQL_PWD')
try {
    # Keep the token out of command arguments and query/error output.
    $env:MYSQL_PWD = $bootstrapToken
    foreach ($serverName in @($mySqlServerNameA, $mySqlServerNameB)) {
        $serverJson = az mysql flexible-server show --resource-group $resourceGroupName --name $serverName
        if ($LASTEXITCODE -ne 0) {
            throw "Unable to read MySQL server '$serverName'."
        }
        $server = $serverJson | ConvertFrom-Json
        $arguments = Get-MySqlEntraClientArguments -Server $server.fullyQualifiedDomainName -User $entraAdminLogin -ClientVersion "$clientVersion"
        $mappingQuery = "SELECT plugin, authentication_string FROM mysql.user WHERE User='$identityLogin' AND Host='%';"
        $mapping = Invoke-MySqlEntraQuery -Arguments $arguments -Query $mappingQuery -Operation "Read identity mapping on $serverName"
        if ([string]::IsNullOrWhiteSpace($mapping)) {
            $null = Invoke-MySqlEntraQuery -Arguments $arguments `
                -Query "CREATE AADUSER '$identityLogin' IDENTIFIED BY '$clientId';" `
                -Operation "Create managed-identity user on $serverName"
            $mapping = Invoke-MySqlEntraQuery -Arguments $arguments -Query $mappingQuery -Operation "Verify identity mapping on $serverName"
        }
        Assert-MySqlEntraPrincipal -Mapping $mapping -ClientId $clientId -PrincipalId $principalId

        $databases = @(az mysql flexible-server db list --resource-group $resourceGroupName --server-name $serverName --query '[].name' -o tsv)
        if ($LASTEXITCODE -ne 0) {
            throw "Unable to enumerate MySQL test databases on '$serverName'."
        }
        $testDatabases = @($databases | Where-Object { $_ -match '^sbm_mysql_test\d+$' })
        if ($testDatabases.Count -eq 0) {
            throw "No sbm_mysql_test databases were found on '$serverName'."
        }
        foreach ($database in $testDatabases) {
            $null = Invoke-MySqlEntraQuery -Arguments $arguments `
                -Query "GRANT ALL PRIVILEGES ON ``$database``.* TO '$identityLogin'@'%';" `
                -Operation "Grant migration privileges on $serverName/$database"
        }
        Write-Host "Managed identity '$identityName' is mapped and authorized for $($testDatabases.Count) test databases on '$serverName'." -ForegroundColor Green
    }
}
finally {
    [Environment]::SetEnvironmentVariable('MYSQL_PWD', $previousPassword)
    $bootstrapToken = $null
    [Environment]::SetEnvironmentVariable('MYSQL_BOOTSTRAP_ACCESS_TOKEN', $null)
}
