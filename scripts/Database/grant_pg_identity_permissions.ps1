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

# Get an Entra ID access token for PostgreSQL
Write-Host "Obtaining Entra ID access token for PostgreSQL..." -ForegroundColor DarkGreen
$accessToken = az account get-access-token --resource-type oss-rdbms --query accessToken -o tsv
if ([string]::IsNullOrWhiteSpace($accessToken)) {
    Write-Host "WARNING: Could not obtain Entra ID token for PostgreSQL. Falling back to password auth for granting permissions." -ForegroundColor Yellow
}

# List all databases
$dbs = az postgres flexible-server db list --resource-group $resourceGroupName --server-name $pgServerName --query "[].name" -o tsv

foreach ($db in $dbs) {
    if ($db -eq "postgres" -or $db -eq "azure_maintenance" -or $db -eq "azure_sys") {
        continue
    }

    Write-Host "  Processing database: $db" -ForegroundColor DarkGreen

    # Use az postgres flexible-server execute to grant permissions
    # The managed identity needs to be registered as a PG role with azure_ad_user attribute
    $sql = @"
DO \$\$
BEGIN
    -- Create role for the managed identity if it doesn't exist
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = '$identityName') THEN
        EXECUTE format('CREATE ROLE %I WITH LOGIN IN ROLE azure_ad_user', '$identityName');
        RAISE NOTICE 'Created role %', '$identityName';
    END IF;
    
    -- Grant all privileges on all tables 
    EXECUTE format('GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO %I', '$identityName');
    EXECUTE format('ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL PRIVILEGES ON TABLES TO %I', '$identityName');
    
    -- Grant usage and create on schema
    EXECUTE format('GRANT USAGE, CREATE ON SCHEMA public TO %I', '$identityName');
    
    RAISE NOTICE 'Permissions granted for % on database %', '$identityName', current_database();
END
\$\$;
"@

    try {
        az postgres flexible-server execute --name $pgServerName --resource-group $resourceGroupName --database-name $db --admin-user $pgAdminUser --admin-password $pgAdminPassword --querytext $sql --output none 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "    ✓ Granted permissions to $identityName on $db" -ForegroundColor Green
        } else {
            Write-Host "    ✗ Failed to grant permissions on $db (exit code: $LASTEXITCODE)" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "    ✗ Failed to grant permissions on $db : $_" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PostgreSQL Identity Permissions Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "The managed identity '$identityName' has been granted access to all PostgreSQL databases." -ForegroundColor Green
Write-Host "Applications using this identity can now connect using Azure AD token authentication." -ForegroundColor Green
