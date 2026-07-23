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
    Grants the managed identity access to all MySQL databases.

.DESCRIPTION
    Connects to each Azure MySQL Flexible Server using the configured MySQL admin
    credentials and ensures a database principal exists for the managed identity name.
    The principal is granted privileges on each test database.

.PARAMETER envName
    The Azure Developer CLI environment name used when deploying resources.

.PARAMETER resourceGroupName
    The Azure resource group containing the MySQL servers.
#>

$repoRoot = $env:AZD_PROJECT_PATH
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path (Split-Path $script:MyInvocation.MyCommand.Path -Parent) -Parent) -Parent
}

if ([string]::IsNullOrWhiteSpace($path)) {
    $path = Join-Path $repoRoot 'src' 'TestConfig'
}

$prefixScript = Join-Path $repoRoot "scripts\prefix_resource_names.ps1"
. $prefixScript -envName $envName

$mySqlAdminPassword = $env:MYSQL_ADMIN_PASSWORD
if ([string]::IsNullOrWhiteSpace($mySqlAdminPassword)) {
    Write-Error "MYSQL_ADMIN_PASSWORD environment variable is required for MySQL initialization."
    exit 1
}

if (-not (Get-Command mysql -ErrorAction SilentlyContinue)) {
    Write-Error "The mysql CLI is required but was not found on PATH."
    exit 1
}

Write-Host "Granting Managed Identity '$identityName' access to MySQL databases" -ForegroundColor Cyan
Write-Host "Resource Group: $resourceGroupName" -ForegroundColor DarkGreen

$identity = az identity show --name $identityName --resource-group $resourceGroupName | ConvertFrom-Json
if ($null -eq $identity) {
    Write-Host "ERROR: Could not find managed identity '$identityName' in resource group '$resourceGroupName'" -ForegroundColor Red
    exit 1
}

$identityLogin = $identityName
$mySqlServers = @($mySqlServerNameA, $mySqlServerNameB)
$systemDatabases = @('mysql', 'information_schema', 'performance_schema', 'sys')
$failureCount = 0

foreach ($serverName in $mySqlServers) {
    $server = az mysql flexible-server show --resource-group $resourceGroupName --name $serverName | ConvertFrom-Json
    if ($null -eq $server) {
        Write-Host "ERROR: Could not find MySQL server '$serverName'" -ForegroundColor Red
        $failureCount++
        continue
    }

    $fqdn = $server.fullyQualifiedDomainName
    Write-Host ""
    Write-Host "Processing MySQL Server: $fqdn" -ForegroundColor Cyan

    $baseArgs = @(
        "--host=$fqdn",
        "--port=3306",
        "--user=$mySqlAdminUser",
        "--password=$mySqlAdminPassword",
        "--ssl-mode=REQUIRED"
    )

    $createUserSql = "CREATE USER IF NOT EXISTS '$identityLogin'@'%' IDENTIFIED BY '$mySqlAdminPassword';"
    & mysql @baseArgs --execute=$createUserSql | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ✗ Unable to create MySQL user '$identityName' on server '$fqdn'" -ForegroundColor Red
        $failureCount++
        continue
    }

    $dbs = @(az mysql flexible-server db list --resource-group $resourceGroupName --server-name $serverName --query "[].name" -o tsv)
    foreach ($db in $dbs) {
        if ($db -in $systemDatabases) {
            continue
        }

        Write-Host "  Granting permissions on database: $db" -ForegroundColor DarkGreen

        $grantSql = @(
            "GRANT ALL PRIVILEGES ON ``$db``.* TO '$identityLogin'@'%'",
            "FLUSH PRIVILEGES"
        ) -join '; '

        & mysql @baseArgs --database=$db --execute=$grantSql | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Host "    ✗ Failed to grant permissions on '$db'" -ForegroundColor Red
            $failureCount++
        } else {
            Write-Host "    ✓ Granted permissions on '$db'" -ForegroundColor Green
        }
    }
}

if ($failureCount -gt 0) {
    Write-Error "MySQL permission initialization failed for $failureCount operation(s)."
    exit 1
}

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "MySQL Identity Permissions Complete" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
