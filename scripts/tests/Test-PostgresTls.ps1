<#
.SYNOPSIS
    Offline regression checks for PostgreSQL bootstrap verified TLS and environment cleanup.
.DESCRIPTION
    Mocks Azure CLI, psql and Linux CA bundle discovery. Makes no network/database calls.
    Run with: pwsh -File scripts\tests\Test-PostgresTls.ps1
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$grantScript = Join-Path $repoRoot 'scripts\Database\grant_pg_identity_permissions.ps1'
$state = @{
    calls = 0
    fail = $false
    throw = $false
    linuxBundle = $false
    expectedBundle = ''
    statements = [Collections.Generic.List[string]]::new()
    tokenRequests = 0
    manual = $false
}

function Assert-True([bool] $Condition, [string] $Message) {
    if (-not $Condition) { throw $Message }
}

function Assert-Throws([scriptblock] $Action, [string] $Pattern) {
    $failure = $null
    try { & $Action } catch { $failure = $_.Exception.Message }
    Assert-True ($null -ne $failure -and $failure -match $Pattern) "Expected '$Pattern', received '$failure'."
}

function Test-Path {
    param([string] $Path, [string] $LiteralPath, [string] $PathType)
    $target = if ($LiteralPath) { $LiteralPath } else { $Path }
    if ($target.StartsWith('/etc/')) {
        return $state.linuxBundle -and $target -eq '/etc/ssl/certs/ca-certificates.crt'
    }
    Microsoft.PowerShell.Management\Test-Path -LiteralPath $target -PathType $PathType
}

function az {
    $global:LASTEXITCODE = 0
    if ($args -contains '--resource-group') {
        Assert-True ($args -contains 'rg-custom') 'The explicit resource group must be preserved.'
    }
    switch -Regex ($args -join ' ') {
        '^identity show ' {
            Assert-True ($args -contains 'id-offline-worker') 'Bootstrap must grant permissions to the worker identity.'
            return '{"principalId":"11111111-1111-1111-1111-111111111111"}'
        }
        '^account show ' {
            Assert-True ($state.manual) 'Private bootstrap must not use its own login for database authentication.'
            return 'persistent-admin'
        }
        '^account get-access-token ' {
            Assert-True ($state.manual) 'Private bootstrap must not acquire its own database token.'
            $state.tokenRequests++
            return 'synthetic-token'
        }
        '^postgres flexible-server show ' { return '{"fullyQualifiedDomainName":"offline.postgres.database.azure.com"}' }
        '^postgres flexible-server db list ' { return @('postgres', 'azure_maintenance', 'azure_sys', 'sbm_pg_test1', 'sbm_pg_test0', 'sbm_pg_test1_archive', 'unrelated_database') }
        default { throw "Unexpected Azure CLI command: $($args -join ' ')" }
    }
}

function psql {
    Assert-True ($env:PGSSLMODE -eq 'verify-full') 'psql must verify the certificate chain and hostname.'
    Assert-True ($env:PGSSLROOTCERT -eq $state.expectedBundle) 'psql must use the expected trusted CA bundle.'
    Assert-True ([string]::IsNullOrEmpty($env:PG_BOOTSTRAP_ACCESS_TOKEN)) 'The handoff token must be consumed, not passed to child processes.'
    $sql = @($args | Where-Object { $_ -like '--command=*' })[0]
    $state.calls++
    Assert-True ($env:PGPASSWORD -eq 'synthetic-token') 'psql must receive the Entra token through its environment.'
    Assert-True (($args -join ' ') -notmatch 'synthetic-token') 'Tokens must not be command-line arguments.'
    Assert-True ($args -contains '--username=persistent-admin') 'Private bootstrap must use the deploying Entra administrator.'
    Assert-True ($sql -notmatch 'bootstrap-admin|DROP OWNED|DROP ROLE|ALTER ROLE|SECURITY LABEL') 'Bootstrap must not create, elevate, or attempt to clean up a bootstrap database role.'
    $state.statements.Add($sql)
    if ($sql -like '*pgaadauth_create_principal_with_oid*') {
        Assert-True ($sql -match "'id-offline-worker'.*'service', false, false") 'Worker principal must not be an administrator.'
    }
    else {
        Assert-True ($args -contains '--dbname=sbm_pg_test1') 'Grants must target generated test databases only.'
        Assert-True ($sql -match 'TO "id-offline-worker"') 'Privileges must target the worker.'
        Assert-True ($sql -notmatch 'azure_pg_admin|CREATEROLE|CREATEDB') 'Worker grants must not include server administration.'
    }
    if ($state.throw) { throw 'Simulated psql invocation failure' }
    $global:LASTEXITCODE = if ($state.fail) { 1 } else { 0 }
    if ($state.fail) { return 'Simulated certificate verification failure' }
}

$originalEnvironment = @{}
foreach ($name in @('PGPASSWORD', 'PGSSLMODE', 'PGSSLROOTCERT', 'AZD_PROJECT_PATH', 'POSTPROVISION_IDENTITY_NAME',
    'PG_BOOTSTRAP_ACCESS_TOKEN', 'PG_ENTRA_ADMIN_LOGIN')) {
    $originalEnvironment[$name] = [Environment]::GetEnvironmentVariable($name)
}

function Assert-Restored([string] $Bundle) {
    Assert-True ($env:PGPASSWORD -eq 'original-password') 'PGPASSWORD must be restored.'
    Assert-True ($env:PGSSLMODE -eq 'original-mode') 'PGSSLMODE must be restored.'
    Assert-True ([string] $env:PGSSLROOTCERT -eq $Bundle) 'PGSSLROOTCERT must be restored.'
    Assert-True ([string]::IsNullOrEmpty($env:PG_BOOTSTRAP_ACCESS_TOKEN)) 'The handoff token must be cleared after success or failure.'
}

function Invoke-TestGrant {
    $env:PG_BOOTSTRAP_ACCESS_TOKEN = 'synthetic-token'
    & $grantScript -envName offline -resourceGroupName rg-custom
}

try {
    $env:AZD_PROJECT_PATH = $repoRoot
    $env:POSTPROVISION_IDENTITY_NAME = 'bootstrap-admin'
    $env:PG_ENTRA_ADMIN_LOGIN = 'persistent-admin'
    $env:PGPASSWORD = 'original-password'
    $env:PGSSLMODE = 'original-mode'
    # psql is mocked, so any existing file can stand in for a user-supplied PEM bundle.
    $env:PGSSLROOTCERT = $grantScript
    $state.expectedBundle = $grantScript
    Invoke-TestGrant
    Assert-True ($state.calls -eq 14) 'Both servers must create a role and run six baseline grants.'
    Assert-True (@($state.statements | Where-Object { $_ -like '*ALTER DEFAULT PRIVILEGES*' }).Count -eq 4) 'Preserve table and sequence defaults owned by the persistent deploying user.'
    foreach ($grant in @(
        'GRANT CONNECT ON DATABASE "sbm_pg_test1"',
        'GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public',
        'GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public',
        'ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL PRIVILEGES ON TABLES',
        'ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL PRIVILEGES ON SEQUENCES',
        'GRANT USAGE, CREATE ON SCHEMA public'
    )) {
        Assert-True (@($state.statements | Where-Object { $_ -eq "--command=$grant TO `"id-offline-worker`"" }).Count -eq 2) "Both servers must preserve runtime grant '$grant'."
    }
    Assert-Restored $grantScript
    Assert-True ($state.tokenRequests -eq 0) 'Private bootstrap must use only its supplied administrator token.'

    $state.calls = 0
    $state.fail = $true
    Assert-Throws { Invoke-TestGrant } 'permission initialization failed'
    Assert-True ($state.calls -eq 2) 'TLS failures must not retry with weaker trust.'
    Assert-Restored $grantScript
    $state.fail = $false

    $state.throw = $true
    Assert-Throws { Invoke-TestGrant } 'Simulated psql invocation failure'
    Assert-Restored $grantScript
    $state.throw = $false

    $env:PGSSLROOTCERT = $null
    $state.linuxBundle = $true
    $state.expectedBundle = '/etc/ssl/certs/ca-certificates.crt'
    Invoke-TestGrant
    Assert-Restored ''

    $state.calls = 0
    $env:PGSSLROOTCERT = Join-Path $repoRoot 'missing-offline-ca-bundle.pem'
    Assert-Throws { Invoke-TestGrant } 'Set PGSSLROOTCERT'
    Assert-True ($state.calls -eq 0) 'Invalid explicit bundles must not fall back to a system bundle.'
    Assert-Restored $env:PGSSLROOTCERT

    $env:PGSSLROOTCERT = $null
    $state.linuxBundle = $false
    Assert-Throws { Invoke-TestGrant } 'ca-certificates'
    Assert-True ($state.calls -eq 0) 'Missing CA trust must fail before calling psql.'
    Assert-Restored ''
    $env:PG_ENTRA_ADMIN_LOGIN = 'bootstrap-admin'
    Assert-Throws { Invoke-TestGrant } 'deploying Entra administrator'
    Assert-True ([string]::IsNullOrEmpty($env:PG_BOOTSTRAP_ACCESS_TOKEN)) 'Failed preflight must consume the handoff token.'
    $env:PG_ENTRA_ADMIN_LOGIN = $null
    Assert-Throws { Invoke-TestGrant } 'PG_ENTRA_ADMIN_LOGIN'
    $env:PG_ENTRA_ADMIN_LOGIN = 'persistent-admin'
    Assert-Throws { & $grantScript -envName offline -resourceGroupName rg-custom } 'PG_BOOTSTRAP_ACCESS_TOKEN'
    Assert-True ($state.tokenRequests -eq 0) 'Missing private bootstrap credentials must never fall back to managed identity.'

    $env:POSTPROVISION_IDENTITY_NAME = $null
    $env:PG_ENTRA_ADMIN_LOGIN = 'stale-private-login'
    $env:PG_BOOTSTRAP_ACCESS_TOKEN = 'stale-private-token'
    $env:PGSSLROOTCERT = $grantScript
    $state.expectedBundle = $grantScript
    $state.manual = $true
    & $grantScript -envName offline -resourceGroupName rg-custom
    Assert-True ($state.tokenRequests -eq 1) 'Manual invocation must retain active Azure login token acquisition.'
    Assert-Restored $grantScript
    Write-Host 'PostgreSQL verified TLS offline tests passed.'
}
finally {
    foreach ($name in $originalEnvironment.Keys) {
        [Environment]::SetEnvironmentVariable($name, $originalEnvironment[$name])
    }
}
