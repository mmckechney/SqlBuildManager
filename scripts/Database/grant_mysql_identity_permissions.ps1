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
    Grants the managed identity access to all MySQL databases by creating an Entra user.

.DESCRIPTION
    Connects to each Azure MySQL Flexible Server as the configured Entra administrator and
    creates an Entra-backed database principal for the managed identity name. The principal
    is then granted privileges on each test database.

.PARAMETER envName
    The Azure Developer CLI environment name used when deploying resources.

.PARAMETER resourceGroupName
    The Azure resource group containing the MySQL servers.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = $env:AZD_PROJECT_PATH
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path (Split-Path $script:MyInvocation.MyCommand.Path -Parent) -Parent) -Parent
}

if ([string]::IsNullOrWhiteSpace($path)) {
    $path = Join-Path $repoRoot 'src' 'TestConfig'
}

$prefixScript = Join-Path $repoRoot "scripts\prefix_resource_names.ps1"
. $prefixScript -envName $envName

if (-not (Get-Command mysql -ErrorAction SilentlyContinue)) {
    Write-Error "The mysql CLI is required but was not found on PATH."
    exit 1
}

$entraAdminLogin = $env:POSTPROVISION_IDENTITY_NAME
if ([string]::IsNullOrWhiteSpace($entraAdminLogin)) {
    $entraAdminLogin = az account show --query user.name -o tsv
}
if ([string]::IsNullOrWhiteSpace($entraAdminLogin)) {
    Write-Error "Unable to determine the MySQL Entra administrator login."
    exit 1
}

Write-Host "Granting Managed Identity '$identityName' access to MySQL databases" -ForegroundColor Cyan
Write-Host "Resource Group: $resourceGroupName" -ForegroundColor DarkGreen
Write-Host "Entra Admin Login: $entraAdminLogin" -ForegroundColor DarkGreen

$identity = az identity show --name $identityName --resource-group $resourceGroupName | ConvertFrom-Json
if ($null -eq $identity) {
    Write-Host "ERROR: Could not find managed identity '$identityName' in resource group '$resourceGroupName'" -ForegroundColor Red
    exit 1
}

$identityLogin = $identityName
$escapedIdentityLogin = $identityLogin.Replace("'", "''")
$mySqlServers = @($mySqlServerNameA, $mySqlServerNameB)
$systemDatabases = @('mysql', 'information_schema', 'performance_schema', 'sys')
$failureCount = 0
$aadEndpointAccessFailure = $false

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

    $aadToken = az account get-access-token --resource-type oss-rdbms --query accessToken -o tsv
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($aadToken)) {
        $aadToken = az account get-access-token --resource https://ossrdbms-aad.database.windows.net --query accessToken -o tsv
    }
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($aadToken)) {
        Write-Host "  ✗ Unable to acquire Entra access token for MySQL on '$fqdn'" -ForegroundColor Red
        $failureCount++
        continue
    }

    $baseArgs = @(
        "--host=$fqdn",
        "--port=3306",
        "--user=$entraAdminLogin",
        "--password=$aadToken",
        "--enable-cleartext-plugin",
        "--ssl-mode=REQUIRED",
        "--skip-column-names"
    )

    $createUserSql = "CREATE AADUSER '$escapedIdentityLogin';"
    $createOutput = & mysql @baseArgs --execute=$createUserSql 2>&1
    if ($LASTEXITCODE -ne 0 -and "$createOutput" -match "ERROR 1396|Operation CREATE USER failed") {
        Write-Host "  Existing MySQL-native user detected for '$identityName'; recreating as Entra user..." -ForegroundColor Yellow
        $recreateUserSql = @(
            "DROP USER IF EXISTS '$escapedIdentityLogin'@'%'",
            "DROP USER IF EXISTS '$escapedIdentityLogin'@'localhost'",
            "CREATE AADUSER '$escapedIdentityLogin'"
        ) -join '; '

        $createOutput = & mysql @baseArgs --execute=$recreateUserSql 2>&1
    }

    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ Entra user '$identityName' created on '$fqdn'" -ForegroundColor Green
    } elseif ("$createOutput" -match "already exists") {
        Write-Host "  Entra user '$identityName' already exists on '$fqdn' — OK" -ForegroundColor DarkGreen
    } elseif ("$createOutput" -match 'ERROR 9127') {
        Write-Host "  ✗ Unable to create MySQL Entra user '$identityName' on '$fqdn': $createOutput" -ForegroundColor Red
        Write-Host "    MySQL couldn't query Microsoft Entra for principal resolution. The server identity '$postProvisionIdentityName' needs Graph permissions (User.Read.All, GroupMember.Read.All, Application.Read.All) or Directory Readers role." -ForegroundColor Yellow
        $aadEndpointAccessFailure = $true
        $failureCount++
        continue
    } else {
        Write-Host "  ✗ Unable to create MySQL Entra user '$identityName' on '$fqdn': $createOutput" -ForegroundColor Red
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
            "GRANT ALL PRIVILEGES ON ``$db``.* TO '$escapedIdentityLogin'",
            "FLUSH PRIVILEGES"
        ) -join '; '

        $grantOutput = & mysql @baseArgs --database=$db --execute=$grantSql 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Host "    ✗ Failed to grant permissions on '$db': $grantOutput" -ForegroundColor Red
            $failureCount++
        } else {
            Write-Host "    ✓ Granted permissions on '$db'" -ForegroundColor Green
        }
    }
}

if ($failureCount -gt 0) {
    if ($aadEndpointAccessFailure) {
        Write-Host "Hint: run scripts\\Database\\grant_mysql_graph_permissions.ps1 with an account that has Privileged Role Administrator or Global Administrator permissions." -ForegroundColor Yellow
    }
    Write-Error "MySQL permission initialization failed for $failureCount operation(s)."
    exit 1
}

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "MySQL Identity Permissions Complete" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
