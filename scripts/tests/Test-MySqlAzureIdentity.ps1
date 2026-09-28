<#
.SYNOPSIS
    Offline regression checks for Azure MySQL identity bootstrap and settings generation.
.DESCRIPTION
    Azure CLI, azd and mysql are mocked. Supply a locally built sbm executable to also
    verify real settings serialization; otherwise sbm is mocked. No cloud/database calls are made.
#>
[CmdletBinding()]
param([string] $SbmExe)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
. (Join-Path $repoRoot 'scripts\Database\mysql_entra_helpers.ps1')

function Assert-True([bool] $Condition, [string] $Message) {
    if (-not $Condition) { throw $Message }
}

function Assert-Throws([scriptblock] $Action, [string] $MessagePattern) {
    $failure = $null
    try { & $Action } catch { $failure = $_.Exception.Message }
    Assert-True ($null -ne $failure -and $failure -match $MessagePattern) "Expected failure matching '$MessagePattern'; received '$failure'."
}

$clientId = '11111111-1111-1111-1111-111111111111'
$principalId = '22222222-2222-2222-2222-222222222222'
$testState = @{}
$testState.azureCalls = [Collections.Generic.List[object]]::new()
$testState.mysqlCalls = [Collections.Generic.List[object]]::new()
$testState.settingsCalls = [Collections.Generic.List[object]]::new()
$testState.mappings = @{}
$testState.remainingTransientFailures = 0
$testState.permanentFailure = $false
$testState.settingsFailure = $false
$testState.sleeps = 0
$testState.childCalls = [Collections.Generic.List[string]]::new()

function az {
    $global:LASTEXITCODE = 0
    $testState.azureCalls.Add(@($args))
    $command = $args -join ' '
    switch -Regex ($command) {
        '^login --identity |^account set ' { return }
        '^identity show ' {
            $identity = $args[[Array]::IndexOf($args, '--name') + 1]
            return (@{
                clientId = $clientId; principalId = $principalId
                id = "/subscriptions/test/resourceGroups/rg-custom/providers/Microsoft.ManagedIdentity/userAssignedIdentities/$identity"
                resourceGroup = 'rg-custom'
            } | ConvertTo-Json -Compress)
        }
        '^mysql flexible-server ad-admin create ' { return }
        '^mysql flexible-server show ' {
            $server = $args[[Array]::IndexOf($args, '--name') + 1]
            return (@{ fullyQualifiedDomainName = "$server.mysql.database.azure.com" } | ConvertTo-Json -Compress)
        }
        '^mysql flexible-server db list ' { return @('mysql', 'sbm_mysql_test1', 'sbm_mysql_test2', 'unrelated_database') }
        '^account show ' { return '33333333-3333-3333-3333-333333333333' }
        '^eventhubs eventhub list ' { return 'test-eventhub' }
        '^acr show ' { return 'test.azurecr.io' }
        '^batch account show ' { return 'test.batch.azure.com' }
        '^containerapp env show ' { return 'eastus' }
        default { throw "Unexpected Azure command in offline test: $command" }
    }
}

function azd { throw 'Offline identity tests must not request azd credentials.' }
function Start-Sleep { param([int] $Seconds) $testState.sleeps++ }
function pwsh {
    $file = $args[[Array]::IndexOf($args, '-File') + 1]
    $testState.childCalls.Add($file)
    if ($file -like '*grant_mysql_identity_permissions.ps1') {
        Assert-True ($env:MYSQL_BOOTSTRAP_ACCESS_TOKEN -eq 'synthetic-token') 'MySQL bootstrap must receive the administrator token.'
    }
    else {
        Assert-True ([string]::IsNullOrEmpty($env:MYSQL_BOOTSTRAP_ACCESS_TOKEN)) 'Other database subprocesses must not inherit the MySQL administrator token.'
    }
    if ($file -like '*grant_pg_identity_permissions.ps1') {
        Assert-True ($env:PG_BOOTSTRAP_ACCESS_TOKEN -eq 'synthetic-pg-token') 'PostgreSQL bootstrap must receive its administrator token.'
    }
    else {
        Assert-True ([string]::IsNullOrEmpty($env:PG_BOOTSTRAP_ACCESS_TOKEN)) 'Other database subprocesses must not inherit the PostgreSQL administrator token.'
    }
    $global:LASTEXITCODE = 0
}

function mysql {
    $global:LASTEXITCODE = 0
    if ($args -contains '--version') { return 'mysql Ver 15.1 Distrib 10.6.18-MariaDB' }
    $testState.mysqlCalls.Add(@($args))
    if ($testState.permanentFailure -or $testState.remainingTransientFailures -gt 0) {
        $testState.remainingTransientFailures--
        $global:LASTEXITCODE = 1
        return "ERROR 1045: simulated authentication failure $env:MYSQL_PWD"
    }
    Assert-True ($env:MYSQL_PWD -eq 'synthetic-token') 'Bootstrap must supply the token through MYSQL_PWD.'
    Assert-True (($args -join ' ') -notmatch 'synthetic-token|--password') 'Token leaked into mysql arguments.'
    $server = @($args | Where-Object { $_ -like '--host=*' })[0]
    $query = @($args | Where-Object { $_ -like '--execute=*' })[0].Substring(10)
    if ($query -like 'SELECT plugin*') {
        if ($testState.mappings.ContainsKey($server)) { return $testState.mappings[$server] }
        return ''
    }
    if ($query -like 'CREATE AADUSER*') {
        $testState.mappings[$server] = "aad_auth`tAADServicePrincipal:$clientId"
        return ''
    }
    if ($query -like 'GRANT ALL PRIVILEGES ON*') {
        Assert-True ($query -match 'ON `sbm_mysql_test[12]`\.\* TO') 'Grant escaped the intended test-database scope.'
        return ''
    }
    throw "Unexpected MySQL query: $query"
}

function Invoke-MockSbm {
    $arguments = @($args | ForEach-Object { $_ })
    $testState.settingsCalls.Add($arguments)
    if ($testState.settingsFailure) {
        $global:LASTEXITCODE = 1
    }
    elseif (-not [string]::IsNullOrWhiteSpace($testState.sbmExecutable)) {
        $logIndex = [Array]::IndexOf($arguments, '--rootloggingpath')
        if ($logIndex -ge 0) {
            $arguments[$logIndex + 1] = $temporaryDirectory
        }
        Push-Location $temporaryDirectory
        try { & $testState.sbmExecutable @arguments | Out-Host }
        finally { Pop-Location }
    }
    else { $global:LASTEXITCODE = 0 }
}

$originalEnvironment = @{}
foreach ($name in @('AZD_PROJECT_PATH', 'MYSQL_PWD', 'MYSQL_BOOTSTRAP_ACCESS_TOKEN', 'MYSQL_ENTRA_ADMIN_LOGIN',
    'MYSQL_AUTH_MODE', 'ENV_NAME', 'RESOURCE_GROUP_NAME', 'SUBSCRIPTION_ID', 'POSTPROVISION_CLIENT_ID',
    'DEPLOY_MYSQL', 'DEPLOY_SQLSERVER', 'DEPLOY_POSTGRESQL', 'DEPLOY_RELAY_PROXY',
    'PG_BOOTSTRAP_ACCESS_TOKEN', 'PG_ENTRA_ADMIN_LOGIN', 'POSTPROVISION_IDENTITY_NAME')) {
    $originalEnvironment[$name] = [Environment]::GetEnvironmentVariable($name)
}
$temporaryDirectory = Join-Path ([IO.Path]::GetTempPath()) "sbm-mysql-identity-$([guid]::NewGuid())"
New-Item -ItemType Directory -Path $temporaryDirectory | Out-Null
try {
    $testState.sbmExecutable = if ([string]::IsNullOrWhiteSpace($SbmExe)) { '' } else { (Resolve-Path $SbmExe).Path }
    $env:AZD_PROJECT_PATH = $repoRoot
    $env:MYSQL_ENTRA_ADMIN_LOGIN = 'admin@example.invalid'
    $env:MYSQL_PWD = 'original-local-value'

    $mariaArguments = @(Get-MySqlEntraClientArguments -Server test.mysql.database.azure.com -User admin -ClientVersion 'MariaDB')
    Assert-True ($mariaArguments -contains '--ssl-verify-server-cert') 'MariaDB must verify the server certificate.'
    Assert-True (-not ($mariaArguments -match '--ssl-mode')) 'MariaDB does not support Oracle MySQL SSL flags.'
    $oracleArguments = @(Get-MySqlEntraClientArguments -Server test.mysql.database.azure.com -User admin -ClientVersion 'MySQL 8.4')
    Assert-True ($oracleArguments -contains '--ssl-mode=VERIFY_IDENTITY' -and $oracleArguments -contains '--enable-cleartext-plugin') 'Oracle MySQL requires verified TLS and the token plugin.'

    Assert-MySqlEntraPrincipal -Mapping "aad_auth`tAADServicePrincipal:$clientId" -ClientId $clientId -PrincipalId $principalId
    Assert-MySqlEntraPrincipal -Mapping "aad_auth`tAADServicePrincipal:$principalId" -ClientId $clientId -PrincipalId $principalId
    foreach ($mapping in @("mysql_native_password`t$clientId", "aad_auth`t33333333-3333-3333-3333-333333333333", "aad_auth`t$($clientId)0")) {
        Assert-Throws { Assert-MySqlEntraPrincipal -Mapping $mapping -ClientId $clientId -PrincipalId $principalId } 'No user was dropped'
    }

    & (Join-Path $repoRoot 'scripts\Database\set_mysql_entra_admin.ps1') -envName offline -resourceGroupName rg-custom `
        -userObjectId $principalId -userLogin admin@example.invalid
    Assert-True ($testState.azureCalls.Count -eq 2) 'Both MySQL servers require an Entra administrator.'
    foreach ($call in $testState.azureCalls) {
        Assert-True ($call -contains 'admin@example.invalid' -and $call -contains 'id-offline-mysql-directory') 'Administrator and server directory identity must be distinct from the bootstrap identity.'
    }

    $grantScript = Join-Path $repoRoot 'scripts\Database\grant_mysql_identity_permissions.ps1'
    for ($run = 0; $run -lt 2; $run++) {
        $env:MYSQL_BOOTSTRAP_ACCESS_TOKEN = 'synthetic-token'
        & $grantScript -envName offline -resourceGroupName rg-custom
        Assert-True ($env:MYSQL_PWD -eq 'original-local-value') 'MYSQL_PWD must be restored.'
        Assert-True ([string]::IsNullOrEmpty($env:MYSQL_BOOTSTRAP_ACCESS_TOKEN)) 'Bootstrap token must be cleared.'
    }
    $queries = @($testState.mysqlCalls | ForEach-Object { $_ | Where-Object { $_ -like '--execute=*' } })
    Assert-True (@($queries | Where-Object { $_ -like '--execute=CREATE AADUSER*' }).Count -eq 2) 'Reruns must not recreate mapped users.'
    Assert-True (@($queries | Where-Object { $_ -like '--execute=GRANT*' }).Count -eq 8) 'Both databases on both servers must receive grants on each run.'
    Assert-True (-not ($queries -match 'DROP USER|aad_auth_validate_oids_in_tenant')) 'Bootstrap must not drop users or bypass directory validation.'
    foreach ($call in $testState.azureCalls) {
        Assert-True ($call -contains 'rg-custom') 'Explicit resource-group overrides must be preserved.'
    }
    $mappingKey = @($testState.mappings.Keys)[0]
    $previousMapping = $testState.mappings[$mappingKey]
    $testState.mappings[$mappingKey] = "mysql_native_password`t$clientId"
    $env:MYSQL_BOOTSTRAP_ACCESS_TOKEN = 'synthetic-token'
    Assert-Throws { & $grantScript -envName offline -resourceGroupName rg-custom } 'No user was dropped'
    Assert-True ($env:MYSQL_PWD -eq 'original-local-value') 'Password environment must also be restored on failure.'
    $testState.mappings[$mappingKey] = $previousMapping

    $env:MYSQL_PWD = 'synthetic-token'
    $testState.remainingTransientFailures = 2
    $null = Invoke-MySqlEntraQuery -Arguments $mariaArguments -Query "SELECT plugin" -Operation 'Retry test'
    Assert-True ($testState.sleeps -eq 2) 'Transient propagation failures must retry.'
    $testState.permanentFailure = $true
    $before = $testState.mysqlCalls.Count
    $failureMessage = ''
    try {
        Invoke-MySqlEntraQuery -Arguments $mariaArguments -Query "SELECT plugin" -Operation 'Failure test'
    }
    catch { $failureMessage = $_.Exception.Message }
    Assert-True ($testState.mysqlCalls.Count - $before -eq 6) 'Authentication retries must be bounded.'
    Assert-True ($failureMessage -match '<redacted>' -and $failureMessage -notmatch 'synthetic-token') 'Failure details must redact the token.'
    $testState.permanentFailure = $false

    & (Join-Path $repoRoot 'scripts\create_all_settingsfiles_mi_only.ps1') -envName offline -resourceGroupName rg-custom `
        -path $temporaryDirectory -sbmExe Invoke-MockSbm -databasePlatform MySQL -settingsFileSuffix mysql-mi-only
    Assert-True ($testState.settingsCalls.Count -eq 5) 'Expected ACI, AKS, Container App, and two Batch settings.'
    foreach ($call in $testState.settingsCalls) {
        foreach ($pair in @(@('--authtype', 'ManagedIdentity'), @('--platform', 'MySQL'), @('--identityname', 'id-offline-worker'), @('--clientid', $clientId))) {
            $index = [Array]::IndexOf($call, $pair[0])
            Assert-True ($index -ge 0 -and $call[$index + 1] -eq $pair[1]) "Missing expected settings option $($pair[0])."
        }
        Assert-True (-not ($call -contains '--password') -and -not ($call -contains '--username')) 'Azure identity settings must not carry native credentials.'
        Assert-True (($call -join ' ') -match 'mysql-mi-only\.json') 'Azure MySQL settings need dedicated filenames.'
        if (-not [string]::IsNullOrWhiteSpace($testState.sbmExecutable)) {
            $file = $call[[Array]::IndexOf($call, '--settingsfile') + 1]
            $settings = Get-Content $file -Raw | ConvertFrom-Json -AsHashtable
            Assert-True ($settings['AuthenticationArgs']['AuthenticationType'] -eq 'ManagedIdentity') 'Serialized settings must use managed identity.'
            Assert-True ($settings['AuthenticationArgs']['DatabasePlatform'] -eq 'MySQL') 'Serialized settings lost the MySQL platform.'
            Assert-True ([string]::IsNullOrEmpty($settings['AuthenticationArgs']['Password'])) 'Serialized settings contain a native password.'
            Assert-True ($settings['IdentityArgs']['IdentityName'] -eq 'id-offline-worker') 'Serialized settings lost the worker database identity name.'
            if ($call[0] -eq 'k8s') {
                Assert-True ($settings['IdentityArgs']['ServiceAccountName'] -eq 'sa-offline') 'AKS settings must select the workload identity service account.'
                Assert-True ([string]::IsNullOrEmpty($settings['IdentityArgs']['ClientId'])) 'AKS must retain its existing workload-environment credential selection.'
            }
            else {
                Assert-True ($settings['IdentityArgs']['ClientId'] -eq $clientId) 'Serialized settings lost the managed identity client ID.'
            }
        }
    }
    $testState.settingsFailure = $true
    Assert-Throws {
        & (Join-Path $repoRoot 'scripts\create_all_settingsfiles_mi_only.ps1') -envName offline -resourceGroupName rg-custom `
            -path $temporaryDirectory -sbmExe Invoke-MockSbm -databasePlatform MySQL -settingsFileSuffix mysql-mi-only
    } 'Failed to generate Batch settings'

    $overrideScript = Join-Path $repoRoot 'scripts\Database\create_mysql_database_override_files.ps1'
    & $overrideScript -envName offline -resourceGroupName rg-custom -path $temporaryDirectory
    Assert-True (-not (Test-Path (Join-Path $temporaryDirectory 'mysql-pw.txt'))) 'Identity target generation must not export a native password.'
    $targets = @(Get-Content (Join-Path $temporaryDirectory 'mysql-databasetargets.cfg'))
    Assert-True ($targets.Count -eq 4 -and -not ($targets -match 'unrelated_database')) 'Targets must match the granted test databases.'
    Set-Content -Path (Join-Path $temporaryDirectory 'mysql-pw.txt') -Value 'synthetic-stale-value'
    Assert-Throws { & $overrideScript -envName offline -resourceGroupName rg-custom -path $temporaryDirectory } 'Stale Azure MySQL password artifacts'

    $env:ENV_NAME = 'offline'
    $env:RESOURCE_GROUP_NAME = 'rg-custom'
    $env:SUBSCRIPTION_ID = $principalId
    $env:POSTPROVISION_CLIENT_ID = $clientId
    $env:POSTPROVISION_IDENTITY_NAME = 'id-offline-postprovision'
    $env:DEPLOY_MYSQL = $env:DEPLOY_SQLSERVER = $env:DEPLOY_POSTGRESQL = 'true'
    $env:DEPLOY_RELAY_PROXY = 'false'
    $env:MYSQL_AUTH_MODE = 'ManagedIdentity'
    $env:MYSQL_BOOTSTRAP_ACCESS_TOKEN = 'synthetic-token'
    $env:PG_BOOTSTRAP_ACCESS_TOKEN = 'synthetic-pg-token'
    $env:PG_ENTRA_ADMIN_LOGIN = 'admin@example.invalid'
    & (Join-Path $repoRoot 'infra\postprovision\run-private-postprovision.ps1')
    Assert-True ($testState.childCalls.Count -eq 3 -and $testState.childCalls[0] -like '*grant_mysql_identity_permissions.ps1') 'MySQL must initialize first while its administrator token is fresh.'
    Assert-True ([string]::IsNullOrEmpty($env:MYSQL_BOOTSTRAP_ACCESS_TOKEN)) 'Parent bootstrap must clear its token.'
    Assert-True ([string]::IsNullOrEmpty($env:PG_BOOTSTRAP_ACCESS_TOKEN)) 'Parent bootstrap must clear its PostgreSQL token.'

    Write-Host 'PASS: Azure MySQL identity helpers, repeatable grants, token handling, settings and target generation (offline).' -ForegroundColor Green
}
finally {
    foreach ($entry in $originalEnvironment.GetEnumerator()) {
        [Environment]::SetEnvironmentVariable($entry.Key, $entry.Value)
    }
    Remove-Item -LiteralPath $temporaryDirectory -Recurse -Force
}
