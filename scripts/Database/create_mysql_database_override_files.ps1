param
(
    [string] $path,
    [string] $envName,
    [string] $resourceGroupName,
    [ValidateSet('ManagedIdentity', 'Password')]
    [string] $authenticationMode = 'ManagedIdentity'
)

<#
.SYNOPSIS
    Creates MySQL database override config files for integration tests.

.DESCRIPTION
    Enumerates databases on the Azure MySQL Flexible Servers and generates
    override config files targeting MySQL databases (sbm_mysql_test1..N).

.PARAMETER envName
    The Azure Developer CLI environment name used when deploying resources.

.PARAMETER path
    Output path for the generated config files.
#>

$repoRoot = $env:AZD_PROJECT_PATH
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path (Split-Path $script:MyInvocation.MyCommand.Path -Parent) -Parent) -Parent
}

if ([string]::IsNullOrWhiteSpace($path)) {
    $path = Join-Path $repoRoot "src\TestConfig"
}

$prefixScript = Join-Path $repoRoot "scripts\prefix_resource_names.ps1"
$resourceGroupOverride = $resourceGroupName
. $prefixScript -envName $envName
if (-not [string]::IsNullOrWhiteSpace($resourceGroupOverride)) {
    $resourceGroupName = $resourceGroupOverride
}
if ($authenticationMode -eq 'ManagedIdentity' -and
    ((Test-Path (Join-Path $path 'mysql-pw.txt')) -or
     (Get-ChildItem -Path $path -Filter 'settingsfile-*-mysql-password.json' -ErrorAction Stop))) {
    throw 'Stale Azure MySQL password artifacts exist in the output directory. Move them out of src/TestConfig before generating identity settings or rebuilding the Azure test image. Local/local-container credentials are not affected.'
}

Write-Host "Create MySQL database override files for servers '$mySqlServerNameA' and '$mySqlServerNameB' in resource group '$resourceGroupName'" -ForegroundColor Cyan
$path = Resolve-Path $path
Write-Host "Output path set to $path" -ForegroundColor DarkGreen

$outputDbConfigFile = Join-Path $path "mysql-databasetargets.cfg"
$clientDbConfigFile = Join-Path $path "mysql-clientdbtargets.cfg"
$doubleClientDbConfigFile = Join-Path $path "mysql-clientdbtargets-doubledb.cfg"
$mySqlServerTextFile = Join-Path $path "mysql-server.txt"

$outputDbConfig = @()
$clientDbConfig = @()
$doubleClientDbConfig = @()

$mySqlServerNames = @($mySqlServerNameA, $mySqlServerNameB)

foreach ($mySqlServerName in $mySqlServerNames) {
    $mySqlServer = az mysql flexible-server show --resource-group $resourceGroupName --name $mySqlServerName | ConvertFrom-Json
    if ($LASTEXITCODE -ne 0 -or $null -eq $mySqlServer) {
        throw "Could not find MySQL server '$mySqlServerName' in resource group '$resourceGroupName'."
    }

    $mySqlFqdn = $mySqlServer.fullyQualifiedDomainName
    Write-Host "MySQL Server FQDN: $mySqlFqdn" -ForegroundColor DarkGreen

    $dbs = az mysql flexible-server db list --resource-group $resourceGroupName --server-name $mySqlServerName --query "[].name" -o tsv
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to enumerate databases on '$mySqlServerName'."
    }
    Write-Host "Databases found on ${mySqlServerName}: $dbs" -ForegroundColor Cyan

    foreach ($db in $dbs) {
        if ($db -match '^sbm_mysql_test\d+$') {
            $outputDbConfig += "$($mySqlFqdn):sbm_mysql_test,$db"
            $clientDbConfig += "$($mySqlFqdn):client,$db"
        }
    }

    $testDbs = @($dbs | Where-Object { $_ -match '^sbm_mysql_test\d+$' } | Sort-Object)
    if ($testDbs.Count -eq 0) {
        throw "No sbm_mysql_test databases were found on '$mySqlServerName'."
    }
    for ($i = 0; $i -lt $testDbs.Count; $i += 2) {
        if ($i + 1 -lt $testDbs.Count) {
            $doubleClientDbConfig += "$($mySqlFqdn):client,$($testDbs[$i]);client2,$($testDbs[$i+1])"
        }
    }
}

Write-Host "Writing MySQL database config to $outputDbConfigFile" -ForegroundColor DarkGreen
$outputDbConfig | Set-Content -Path $outputDbConfigFile

Write-Host "Writing MySQL client database config to $clientDbConfigFile" -ForegroundColor DarkGreen
$clientDbConfig | Set-Content -Path $clientDbConfigFile

Write-Host "Writing MySQL double-client database config to $doubleClientDbConfigFile" -ForegroundColor DarkGreen
$doubleClientDbConfig | Set-Content -Path $doubleClientDbConfigFile

Write-Host "Writing MySQL server.txt to $mySqlServerTextFile" -ForegroundColor DarkGreen
$mySqlServerA = az mysql flexible-server show --resource-group $resourceGroupName --name $mySqlServerNameA | ConvertFrom-Json
if ($LASTEXITCODE -ne 0 -or $null -eq $mySqlServerA) {
    throw "Unable to read MySQL server '$mySqlServerNameA'."
}
else {
    $mySqlServerA.fullyQualifiedDomainName.Trim() | Set-Content -Path $mySqlServerTextFile
}

if ($authenticationMode -eq 'Password') {
    $mySqlPwFile = Join-Path $path "mysql-pw.txt"
    $mySqlAdminPassword = azd env get-value MYSQL_ADMIN_PASSWORD 2>$null
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($mySqlAdminPassword)) {
        throw 'MYSQL_ADMIN_PASSWORD is required for explicit password-mode settings.'
    }
    $mySqlAdminPassword | Set-Content -Path $mySqlPwFile
    $mySqlUnFile = Join-Path $path "mysql-un.txt"
    $mySqlAdminUser | Set-Content -Path $mySqlUnFile
}

Write-Host ""
Write-Host "MySQL database override files created successfully!" -ForegroundColor Green
