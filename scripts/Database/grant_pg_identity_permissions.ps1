param
(
    [Parameter(Mandatory=$true)]
    [string] $prefix,

    [Parameter(Mandatory=$true)]
    [string] $resourceGroupName,

    [string] $path
)

<#
.SYNOPSIS
    Grants the managed identity access to all PostgreSQL databases using Entra ID authentication.

.DESCRIPTION
    This script connects to the Azure PostgreSQL Flexible Server and creates a role
    for the managed identity, then grants it appropriate permissions on each database.
    
    Prerequisites:
    - The current user must be an Entra ID admin on the PostgreSQL server
    - Az CLI must be installed and logged in
    - The managed identity must exist in the resource group

.PARAMETER prefix
    The resource name prefix used when deploying resources.

.PARAMETER resourceGroupName
    The Azure resource group containing the PostgreSQL server.

.PARAMETER path
    Path to TestConfig directory (for reading PG credentials).
#>

# Get the repo root
$repoRoot = $env:AZD_PROJECT_PATH
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path (Split-Path $script:MyInvocation.MyCommand.Path -Parent) -Parent) -Parent
}

if ([string]::IsNullOrWhiteSpace($path)) {
    $path = Join-Path $repoRoot "src\TestConfig"
}

# Get set resource name variables from prefix
$prefixScript = Join-Path $repoRoot "scripts\prefix_resource_names.ps1"
. $prefixScript -prefix $prefix

Write-Host "Granting Managed Identity '$identityName' access to PostgreSQL databases" -ForegroundColor Cyan
Write-Host "Resource Group: $resourceGroupName" -ForegroundColor DarkGreen

# Get the managed identity details
$identity = az identity show --name $identityName --resource-group $resourceGroupName | ConvertFrom-Json
if ($null -eq $identity) {
    Write-Host "ERROR: Could not find managed identity '$identityName' in resource group '$resourceGroupName'" -ForegroundColor Red
    exit 1
}

$identityClientId = $identity.clientId
Write-Host "Managed Identity Name: $identityName" -ForegroundColor DarkGreen
Write-Host "Managed Identity Client ID: $identityClientId" -ForegroundColor DarkGreen

# Get PG server info
$pgServer = az postgres flexible-server show --resource-group $resourceGroupName --name $pgServerName | ConvertFrom-Json
if ($null -eq $pgServer) {
    Write-Host "ERROR: Could not find PostgreSQL server '$pgServerName'" -ForegroundColor Red
    exit 1
}

$pgFqdn = $pgServer.fullyQualifiedDomainName
Write-Host "PostgreSQL Server: $pgFqdn" -ForegroundColor DarkGreen

# Get PG admin credentials
$pgAdminUser = "${prefix}pgadmin"
$pgAdminPassword = azd env get-value PG_ADMIN_PASSWORD 2>$null
if ([string]::IsNullOrWhiteSpace($pgAdminPassword) -or $pgAdminPassword -like "ERROR:*") {
    $pgPwFile = Join-Path $path "pg-pw.txt"
    if (Test-Path $pgPwFile) {
        $pgAdminPassword = (Get-Content -Path $pgPwFile).Trim()
    } else {
        Write-Host "ERROR: Cannot find PG admin password. Set PG_ADMIN_PASSWORD env var or create pg-pw.txt" -ForegroundColor Red
        exit 1
    }
}

# Ensure the rdbms-connect extension is installed (needed for az postgres flexible-server execute)
Write-Host "Ensuring rdbms-connect extension is installed..." -ForegroundColor DarkGreen
az extension add --name rdbms-connect --yes 2>$null

# Step 1: Create the managed identity role in the 'postgres' database
# pgaadauth_create_principal only exists in the postgres database and creates a server-wide role.
# IMPORTANT: This function can only be run by an Entra ID admin, not a local (password) admin.
# We acquire an Azure AD token for the current user and authenticate with that.
Write-Host "Ensuring Entra ID role '$identityName' exists..." -ForegroundColor DarkGreen

# Get Entra ID admin info from the server
$aadAdmins = az rest --method get --uri "/subscriptions/$(az account show --query id -o tsv)/resourceGroups/$resourceGroupName/providers/Microsoft.DBforPostgreSQL/flexibleServers/$pgServerName/administrators?api-version=2024-08-01" --query "value[0].properties.principalName" -o tsv 2>$null
if ([string]::IsNullOrWhiteSpace($aadAdmins)) {
    Write-Host "  ⚠ Could not determine Entra ID admin — falling back to local admin" -ForegroundColor Yellow
    $aadAdmins = $null
}

$roleCreated = $false
if ($null -ne $aadAdmins) {
    # Authenticate as Entra ID admin using Azure AD token
    $aadToken = az account get-access-token --resource-type oss-rdbms --query accessToken -o tsv 2>$null
    if (-not [string]::IsNullOrWhiteSpace($aadToken)) {
        $createRoleSql = "SELECT * FROM pgaadauth_create_principal('${identityName}', false, true)"
        $createOutput = az postgres flexible-server execute --name $pgServerName --database-name postgres --admin-user $aadAdmins --admin-password "$aadToken" --querytext "$createRoleSql" --output none 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✓ Role '$identityName' created (Entra ID auth)" -ForegroundColor Green
            $roleCreated = $true
        } elseif ($createOutput -match "already exists") {
            Write-Host "  Role '$identityName' already exists — OK" -ForegroundColor DarkGreen
            $roleCreated = $true
        } else {
            Write-Host "  ⚠ pgaadauth_create_principal via Entra ID admin failed: $createOutput" -ForegroundColor Yellow
        }
    } else {
        Write-Host "  ⚠ Could not acquire Azure AD token — falling back to local admin" -ForegroundColor Yellow
    }
}

if (-not $roleCreated) {
    # Fallback: try with local admin (will only work if the role already exists or for non-MI roles)
    $createRoleSql = "SELECT * FROM pgaadauth_create_principal('${identityName}', false, true)"
    $createOutput = az postgres flexible-server execute --name $pgServerName --database-name postgres --admin-user $pgAdminUser --admin-password "$pgAdminPassword" --querytext "$createRoleSql" --output none 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ Role '$identityName' created" -ForegroundColor Green
    } elseif ($createOutput -match "already exists") {
        Write-Host "  Role '$identityName' already exists — OK" -ForegroundColor DarkGreen
    } else {
        Write-Host "  ⚠ pgaadauth_create_principal failed, trying direct CREATE ROLE..." -ForegroundColor Yellow
        $fallbackSql = "CREATE ROLE ""${identityName}"" WITH LOGIN"
        $fallbackOutput = az postgres flexible-server execute --name $pgServerName --database-name postgres --admin-user $pgAdminUser --admin-password "$pgAdminPassword" --querytext "$fallbackSql" --output none 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✓ Role '$identityName' created via CREATE ROLE" -ForegroundColor Green
        } elseif ($fallbackOutput -match "already exists") {
            Write-Host "  Role '$identityName' already exists — OK" -ForegroundColor DarkGreen
        } else {
            Write-Host "  ERROR: Could not create role for managed identity. Grants may fail." -ForegroundColor Red
            Write-Host "  $fallbackOutput" -ForegroundColor Red
        }
    }
}

# Step 2: Grant privileges on each test database
# List all databases
$dbs = az postgres flexible-server db list --resource-group $resourceGroupName --server-name $pgServerName --query "[].name" -o tsv

foreach ($db in $dbs) {
    if ($db -eq "postgres" -or $db -eq "azure_maintenance" -or $db -eq "azure_sys") {
        continue
    }

    Write-Host "  Processing database: $db" -ForegroundColor DarkGreen

    # Grant privileges (run each as a separate statement)
    $grantStatements = @(
        "GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO ""${identityName}""",
        "ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL PRIVILEGES ON TABLES TO ""${identityName}""",
        "GRANT USAGE, CREATE ON SCHEMA public TO ""${identityName}"""
    )

    $allSucceeded = $true
    foreach ($grantSql in $grantStatements) {
        az postgres flexible-server execute --name $pgServerName --database-name $db --admin-user $pgAdminUser --admin-password "$pgAdminPassword" --querytext "$grantSql" --output none 2>&1
        if ($LASTEXITCODE -ne 0) {
            $allSucceeded = $false
            Write-Host "    ⚠ Grant statement failed: $grantSql" -ForegroundColor Yellow
        }
    }

    if ($allSucceeded) {
        Write-Host "    ✓ Granted permissions to $identityName on $db" -ForegroundColor Green
    } else {
        Write-Host "    ⚠ Some permissions may not have been granted on $db" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PostgreSQL Identity Permissions Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "The managed identity '$identityName' has been granted access to all PostgreSQL databases." -ForegroundColor Green
Write-Host "Applications using this identity can now connect using Azure AD token authentication." -ForegroundColor Green
