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
    - The active Azure identity must be an Entra ID admin on the PostgreSQL server
    - Az CLI must be installed and logged in
    - psql must be installed
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
    $path = Join-Path $repoRoot 'src' 'TestConfig'
}

# Get set resource name variables from prefix
$prefixScript = Join-Path $repoRoot 'scripts' 'prefix_resource_names.ps1'
. $prefixScript -prefix $prefix

Write-Host "Granting Managed Identity '$identityName' access to PostgreSQL databases" -ForegroundColor Cyan
Write-Host "Resource Group: $resourceGroupName" -ForegroundColor DarkGreen

# Get the managed identity details
$identity = az identity show --name $identityName --resource-group $resourceGroupName | ConvertFrom-Json
if ($null -eq $identity) {
    Write-Host "ERROR: Could not find managed identity '$identityName' in resource group '$resourceGroupName'" -ForegroundColor Red
    exit 1
}

$identityPrincipalId = $identity.principalId
Write-Host "Managed Identity Name: $identityName" -ForegroundColor DarkGreen
Write-Host "Managed Identity Object ID: $identityPrincipalId" -ForegroundColor DarkGreen

$entraAdminName = $env:POSTPROVISION_IDENTITY_NAME
if ([string]::IsNullOrWhiteSpace($entraAdminName)) {
    $entraAdminName = az account show --query user.name -o tsv
}
if ([string]::IsNullOrWhiteSpace($entraAdminName)) {
    Write-Error "Unable to determine the PostgreSQL Entra administrator name."
    exit 1
}

$aadToken = az account get-access-token --resource-type oss-rdbms --query accessToken -o tsv
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($aadToken)) {
    Write-Error "Unable to acquire a PostgreSQL access token."
    exit 1
}

$env:PGPASSWORD = $aadToken
$escapedIdentityName = $identityName.Replace("'", "''")
$quotedIdentityName = $identityName.Replace('"', '""')
$failureCount = 0

# Process both PostgreSQL servers
$pgServerNames = @($pgServerNameA, $pgServerNameB)

foreach ($pgServerName in $pgServerNames) {

# Get PG server info
$pgServer = az postgres flexible-server show --resource-group $resourceGroupName --name $pgServerName | ConvertFrom-Json
if ($null -eq $pgServer) {
    Write-Host "ERROR: Could not find PostgreSQL server '$pgServerName'" -ForegroundColor Red
    $failureCount++
    continue
}

$pgFqdn = $pgServer.fullyQualifiedDomainName
Write-Host ""
Write-Host "Processing PostgreSQL Server: $pgFqdn" -ForegroundColor Cyan

Write-Host "Ensuring Entra ID role '$identityName' exists..." -ForegroundColor DarkGreen

$createRoleSql = "SELECT * FROM pgaadauth_create_principal_with_oid('$escapedIdentityName', '$identityPrincipalId', 'service', false, false);"
$createOutput = & psql --host=$pgFqdn --port=5432 --dbname=postgres --username=$entraAdminName --set=ON_ERROR_STOP=1 --command=$createRoleSql 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "  ✓ Role '$identityName' created" -ForegroundColor Green
} elseif ("$createOutput" -match "already exists") {
    Write-Host "  Role '$identityName' already exists — OK" -ForegroundColor DarkGreen
} else {
    Write-Host "  ✗ Unable to create role '$identityName': $createOutput" -ForegroundColor Red
    $failureCount++
    continue
}

$dbs = @(az postgres flexible-server db list --resource-group $resourceGroupName --server-name $pgServerName --query "[].name" -o tsv)

foreach ($db in $dbs) {
    if ($db -eq "postgres" -or $db -eq "azure_maintenance" -or $db -eq "azure_sys") {
        continue
    }

    Write-Host "  Processing database: $db" -ForegroundColor DarkGreen

    # Grant privileges (run each as a separate statement)
    $grantStatements = @(
        "GRANT CONNECT ON DATABASE ""$db"" TO ""$quotedIdentityName""",
        "GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO ""$quotedIdentityName""",
        "GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO ""$quotedIdentityName""",
        "ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL PRIVILEGES ON TABLES TO ""$quotedIdentityName""",
        "ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL PRIVILEGES ON SEQUENCES TO ""$quotedIdentityName""",
        "GRANT USAGE, CREATE ON SCHEMA public TO ""$quotedIdentityName"""
    )

    $allSucceeded = $true
    foreach ($grantSql in $grantStatements) {
        $grantOutput = & psql --host=$pgFqdn --port=5432 --dbname=$db --username=$entraAdminName --set=ON_ERROR_STOP=1 --command=$grantSql 2>&1
        if ($LASTEXITCODE -ne 0) {
            $allSucceeded = $false
            Write-Host "    ✗ Grant statement failed: $grantOutput" -ForegroundColor Red
        }
    }

    if ($allSucceeded) {
        Write-Host "    ✓ Granted permissions to $identityName on $db" -ForegroundColor Green
    } else {
        $failureCount++
    }
}

} # end foreach pgServerName

if ($failureCount -gt 0) {
    Write-Error "PostgreSQL permission initialization failed for $failureCount operation(s)."
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PostgreSQL Identity Permissions Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "The managed identity '$identityName' has been granted access to all PostgreSQL databases on both servers." -ForegroundColor Green
Write-Host "Applications using this identity can now connect using Azure AD token authentication." -ForegroundColor Green
