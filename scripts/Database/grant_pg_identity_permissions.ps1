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
    Grants the worker managed identity access to generated PostgreSQL test databases using Entra ID authentication.

.DESCRIPTION
    This script connects to the Azure PostgreSQL Flexible Server and creates a role
    for the worker managed identity, then grants migration permissions on sbm_pg_testN databases.
    Private postprovision uses the deploying Entra administrator's short-lived token;
    the bootstrap managed identity is never made a PostgreSQL administrator.
    Supply PG_ENTRA_ADMIN_LOGIN and PG_BOOTSTRAP_ACCESS_TOKEN (a secure environment
    value consumed on entry) when POSTPROVISION_IDENTITY_NAME is set.
    
    Prerequisites:
    - The active Azure identity must be an Entra ID admin on the PostgreSQL server
    - Az CLI must be installed and logged in
    - psql must be installed
    - The managed identity must exist in the resource group
    - TLS requires a trusted CA bundle: set PGSSLROOTCERT, or install the Linux
      ca-certificates package. Connections always verify the server certificate and hostname.

.PARAMETER envName
    The Azure Developer CLI environment name used when deploying resources.

.PARAMETER resourceGroupName
    The Azure resource group containing the PostgreSQL server.

.PARAMETER path
    Path to TestConfig directory (for reading PG credentials).
#>

# Get the repo root
$aadToken = $env:PG_BOOTSTRAP_ACCESS_TOKEN
$env:PG_BOOTSTRAP_ACCESS_TOKEN = $null
$entraAdminName = $env:PG_ENTRA_ADMIN_LOGIN
$bootstrapLogin = $env:POSTPROVISION_IDENTITY_NAME
$isPrivateBootstrap = -not [string]::IsNullOrWhiteSpace($bootstrapLogin)
if ($isPrivateBootstrap -and
    ([string]::IsNullOrWhiteSpace($entraAdminName) -or [string]::IsNullOrWhiteSpace($aadToken) -or
        $entraAdminName -eq $bootstrapLogin)) {
    throw 'Private PostgreSQL bootstrap requires the deploying Entra administrator: set PG_ENTRA_ADMIN_LOGIN and secure PG_BOOTSTRAP_ACCESS_TOKEN. The bootstrap managed identity must not be a PostgreSQL administrator.'
}

$repoRoot = $env:AZD_PROJECT_PATH
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path (Split-Path $script:MyInvocation.MyCommand.Path -Parent) -Parent) -Parent
}

if ([string]::IsNullOrWhiteSpace($path)) {
    $path = Join-Path $repoRoot 'src' 'TestConfig'
}

# Get resource name variables from the environment name
$prefixScript = Join-Path $repoRoot "scripts\prefix_resource_names.ps1"
$requestedResourceGroupName = $resourceGroupName
. $prefixScript -envName $envName
$resourceGroupName = $requestedResourceGroupName

Write-Host "Granting Managed Identity '$identityName' access to generated PostgreSQL test databases" -ForegroundColor Cyan
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

if (-not $isPrivateBootstrap) {
    $entraAdminName = az account show --query user.name -o tsv
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($entraAdminName)) {
        throw 'Unable to determine the PostgreSQL Entra administrator name.'
    }
    $aadToken = az account get-access-token --resource-type oss-rdbms --query accessToken -o tsv
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($aadToken)) {
        Write-Error "Unable to acquire a PostgreSQL access token."
        exit 1
    }
}

$rootCertificate = $env:PGSSLROOTCERT
if ([string]::IsNullOrWhiteSpace($rootCertificate)) {
    $rootCertificate = @(
        '/etc/ssl/certs/ca-certificates.crt',
        '/etc/pki/tls/certs/ca-bundle.crt',
        '/etc/ssl/ca-bundle.pem',
        '/etc/pki/ca-trust/extracted/pem/tls-ca-bundle.pem'
    ) | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | Select-Object -First 1
}
if ([string]::IsNullOrWhiteSpace($rootCertificate) -or
    -not (Test-Path -LiteralPath $rootCertificate -PathType Leaf)) {
    throw 'PostgreSQL verified TLS requires a trusted CA bundle. Set PGSSLROOTCERT to an existing trusted PEM CA bundle, or install the Linux ca-certificates package. An explicit PGSSLROOTCERT must reference an existing file; no TLS downgrade is allowed.'
}

$originalEnvironment = @{}
foreach ($name in @('PGPASSWORD', 'PGSSLMODE', 'PGSSLROOTCERT')) {
    $originalEnvironment[$name] = [Environment]::GetEnvironmentVariable($name)
}

$pgServerNames = @($pgServerNameA, $pgServerNameB)
try {
$env:PGPASSWORD = $aadToken
$env:PGSSLMODE = 'verify-full'
$env:PGSSLROOTCERT = $rootCertificate
$escapedIdentityName = $identityName.Replace("'", "''")
$quotedIdentityName = $identityName.Replace('"', '""')
$failureCount = 0

# Process both PostgreSQL servers
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
    if ($db -cnotmatch '^sbm_pg_test[1-9][0-9]*$') {
        continue
    }

    Write-Host "  Processing database: $db" -ForegroundColor DarkGreen

    # Default ACLs belong to the persistent deploying user, never the bootstrap managed identity.
    # Workers own objects they create; these grants do not transfer existing-object ownership.
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
}
finally {
    $aadToken = $null
    $env:PG_BOOTSTRAP_ACCESS_TOKEN = $null
    foreach ($name in $originalEnvironment.Keys) {
        [Environment]::SetEnvironmentVariable($name, $originalEnvironment[$name])
    }
}

if ($failureCount -gt 0) {
    Write-Error "PostgreSQL permission initialization failed for $failureCount operation(s)."
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PostgreSQL Identity Permissions Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "The managed identity '$identityName' has been granted access to generated PostgreSQL test databases on both servers." -ForegroundColor Green
Write-Host "Applications using this identity can now connect using Azure AD token authentication." -ForegroundColor Green
