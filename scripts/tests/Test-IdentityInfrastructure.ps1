param(
    [Parameter(Mandatory=$true)]
    [string] $CompiledTemplatePath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$template = Get-Content $CompiledTemplatePath -Raw | ConvertFrom-Json -AsHashtable

function Assert-IdentityContract([bool] $Condition, [string] $Message) {
    if (-not $Condition) { throw $Message }
}
function Get-ModuleTemplate([string] $Name) {
    $module = @($template.resources | Where-Object { $_.name -eq $Name })
    Assert-IdentityContract ($module.Count -eq 1) "Expected exactly one '$Name' module."
    return $module[0].properties.template
}
function Get-Assignment($Module, [string] $NamePart) {
    $assignments = @($Module.resources | Where-Object { $_.type -eq 'Microsoft.Authorization/roleAssignments' -and $_.name.Contains($NamePart) })
    Assert-IdentityContract ($assignments.Count -eq 1) "Expected one role assignment matching '$NamePart'."
    return $assignments[0]
}
function Read-Source([string] $Path) {
    return Get-Content (Join-Path $repoRoot $Path) -Raw
}

Assert-IdentityContract ($template.outputs.Count -le 64) 'ARM permits only 64 outputs.'
foreach ($prefix in @('MANAGED', 'ORCHESTRATOR', 'MYSQL_DIRECTORY', 'POSTPROVISION')) {
    foreach ($suffix in @('NAME', 'ID', 'CLIENT_ID', 'PRINCIPAL_ID')) {
        Assert-IdentityContract ($template.outputs.ContainsKey("${prefix}_IDENTITY_$suffix")) "Missing ${prefix}_IDENTITY_$suffix."
    }
}
Assert-IdentityContract ($template.outputs.MANAGED_IDENTITY_ID.value.Contains("'identityResource'")) 'Legacy output must mean worker.'
Assert-IdentityContract ($template.outputs.ORCHESTRATOR_IDENTITY_ID.value.Contains("'orchestratorIdentity'")) 'Orchestrator output is not separate.'
foreach ($entry in @{
    batchAccount = 'deployBatchAccount'
    aks = 'deployAks'
    mysql = 'deployMySQL'
    mysqlDirectoryIdentity = 'deployMySQL'
    relayProxy = 'deployRelayProxy'
    containerAppEnv = 'deployContainerAppEnv'
    postgresql = 'deployPostgreSQL'
}.GetEnumerator()) {
    $module = @($template.resources | Where-Object { $_.name -eq $entry.Key })[0]
    Assert-IdentityContract ($module.condition.Contains($entry.Value)) "Feature toggle lost for '$($entry.Key)'."
}
foreach ($name in @('identityResource', 'orchestratorIdentity', 'mysqlDirectoryIdentity')) {
    $identity = Get-ModuleTemplate $name
    Assert-IdentityContract (@($identity.resources | Where-Object { $_.type -eq 'Microsoft.Authorization/roleAssignments' }).Count -eq 0) "$name must not carry ambient RG grants."
}

$data = Get-ModuleTemplate 'runtimeDataAccess'
$workerQueue = Get-Assignment $data 'ServiceBusDataReceiver'
Assert-IdentityContract ($workerQueue.properties.roleDefinitionId.Contains('4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0')) 'Worker must receive, not own, Service Bus.'
Assert-IdentityContract ($workerQueue.properties.principalId -eq "[parameters('workerPrincipalId')]") 'Queue receiver must target worker.'
Assert-IdentityContract ($workerQueue.scope.Contains('Microsoft.ServiceBus/namespaces/topics')) 'Queue grant must be topic scoped.'
foreach ($assignment in @($data.resources | Where-Object { $_.type -eq 'Microsoft.Authorization/roleAssignments' })) {
    Assert-IdentityContract ($assignment.ContainsKey('scope')) 'Runtime data grants must not be RG-wide.'
}
$dataText = $data | ConvertTo-Json -Depth 100
Assert-IdentityContract (-not $dataText.Contains('b24988ac-6180-42a0-ab88-20f7382dd24c')) 'Generic Contributor is forbidden.'
Assert-IdentityContract ($dataText.Contains('consumergroups/write') -and $dataText.Contains('consumergroups/delete')) 'Orchestrator needs consumer-group lifecycle.'

$compute = Get-ModuleTemplate 'orchestratorAccess'
$computeRole = @($compute.resources | Where-Object { $_.type -eq 'Microsoft.Authorization/roleDefinitions' -and $_.name.Contains('SbmComputeOrchestrator') })[0]
Assert-IdentityContract (-not ($computeRole.properties.permissions[0].actions | ConvertTo-Json -Compress).Contains('logstream/action')) 'Container Apps logstream is a DataAction, not an ARM Action.'
Assert-IdentityContract (($computeRole.properties.permissions[0].dataActions | ConvertTo-Json -Compress).Contains('Microsoft.App/containerApps/logstream/action')) 'Container Apps logstream DataAction must be granted.'
$assignment = Get-Assignment $compute 'ManagedIdentityOperator'
Assert-IdentityContract ($assignment.scope.Contains("'workerIdentityName'")) 'Orchestrator may attach only the worker identity.'
foreach ($role in @($compute.resources | Where-Object { $_.type -eq 'Microsoft.Authorization/roleDefinitions' })) {
    $roleText = $role | ConvertTo-Json -Depth 30
    Assert-IdentityContract (-not $roleText.Contains('*')) 'Compute actions must be enumerated, never wildcarded.'
    Assert-IdentityContract (-not $roleText.Contains('roleAssignments/write')) 'Compute role must not assign RBAC.'
}
$aks = Get-ModuleTemplate 'aks'
$cluster = @($aks.resources | Where-Object { $_.type -eq 'Microsoft.ContainerService/managedClusters' })[0]
Assert-IdentityContract (($cluster.identity.userAssignedIdentities.Keys -join '').Contains("'controlPlaneIdentityName'")) 'AKS control plane must not be worker.'
Assert-IdentityContract ($cluster.properties.identityProfile.kubeletidentity.resourceId.Contains("'kubeletIdentityName'")) 'AKS must have dedicated kubelet.'
$federation = @($aks.resources | Where-Object { $_.type -like '*federatedIdentityCredentials' })[0]
Assert-IdentityContract ($federation.name.Contains("'identityName'") -and $federation.properties.subject.Contains('system:serviceaccount:sqlbuildmanager:')) 'Federation must bind worker to SBM service account only.'
$writer = Get-Assignment $aks 'RbacWriter'
Assert-IdentityContract ($writer.scope.Contains('sqlbuildmanager') -and $writer.scope.Contains('/namespaces')) 'AKS writer must be namespace scoped.'
Assert-IdentityContract ($writer.properties.roleDefinitionId.Contains('a7ffa36f-339b-4b5c-8bdf-e2c188b2c0eb')) 'AKS workload role must be Writer.'
Assert-IdentityContract ($cluster.dependsOn.Count -ge 3) 'AKS network/kubelet grants must precede cluster creation.'

$batch = Get-ModuleTemplate 'batchAccount'
$batchAccount = @($batch.resources | Where-Object { $_.type -eq 'Microsoft.Batch/batchAccounts' })[0]
Assert-IdentityContract (($batchAccount.identity.userAssignedIdentities.Keys -join '').Contains("'storageIdentityName'")) 'Batch account identity must be separate.'
Assert-IdentityContract ($batchAccount.properties.autoStorage.nodeIdentityReference.resourceId.Contains('userAssignedIdentityId')) 'Batch nodes must still reference the worker pool identity.'
Assert-IdentityContract ((Get-Assignment $batch 'AzureBatchDataContributor').properties.roleDefinitionId.Contains('6aaa78f1-f7de-44ca-8722-c64a23943cae')) 'Batch requires management and data-plane contributor, not generic Contributor.'
$mysql = Get-ModuleTemplate 'mysql'
foreach ($server in @($mysql.resources | Where-Object { $_.type -eq 'Microsoft.DBforMySQL/flexibleServers' })) {
    Assert-IdentityContract (($server.identity.userAssignedIdentities.Keys -join '').Contains('directoryIdentityResourceId')) 'MySQL server must use directory identity.'
}
$pg = Get-ModuleTemplate 'postgresql'
Assert-IdentityContract (-not (($pg | ConvertTo-Json -Depth 100).Contains('postProvisionAdmin'))) 'Bootstrap PostgreSQL admin must not be persistent IaC.'
$bootstrap = Get-ModuleTemplate 'postProvisionIdentity'
$bootstrapText = $bootstrap | ConvertTo-Json -Depth 100
Assert-IdentityContract (-not $bootstrapText.Contains('acdd72a7-3385-48ef-bd42-f606fba81ae7')) 'Bootstrap must use narrow discovery, not ambient Reader.'
Assert-IdentityContract ((Get-Assignment $bootstrap 'AcrPull').scope.Contains('Microsoft.ContainerRegistry/registries')) 'Bootstrap ACR grant must be registry scoped.'

$graphSource = Read-Source 'scripts\Database\grant_mysql_graph_permissions.ps1'
Assert-IdentityContract ($graphSource.Contains('$mysqlDirectoryIdentityName') -and -not $graphSource.Contains('$postProvisionIdentity')) 'Graph roles belong only to directory identity.'
$relaySource = Read-Source 'scripts\ContainerRegistry\build_and_deploy_relay_proxy.ps1'
Assert-IdentityContract ($relaySource.Contains("'--assign-identity', `$identity.id,") -and -not $relaySource.Contains('$runtimeIdentity')) 'Relay must attach only its own identity.'
Assert-IdentityContract ($relaySource.Contains('SQL_MANAGED_IDENTITY_CLIENT_ID=$($identity.clientId)')) 'Relay SQL must use Relay identity.'
$hook = Read-Source 'infra\scripts\postprovision.ps1'
Assert-IdentityContract (-not $hook.Contains('--admin')) 'AKS bootstrap must authenticate as deployer, not local cluster administrator.'
Assert-IdentityContract ($hook.Contains('kind: Namespace') -and $hook.Contains('kind: ServiceAccount')) 'Deploy hook must create namespace and service account.'
$privateHook = Read-Source 'scripts\ContainerRegistry\run_private_postprovision_container.ps1'
Assert-IdentityContract ($privateHook.Contains('PG_BOOTSTRAP_ACCESS_TOKEN') -and -not $privateHook.Contains('pgServersWithBootstrapAdmin')) 'Bootstrap must use a short-lived PG user token, never a reusable PG administrator identity.'
Assert-IdentityContract ($privateHook.Contains('$sqlOriginalAdministrators') -and $privateHook.Contains('Bootstrap cleanup failed:')) 'Bootstrap must restore captured SQL administrators and report failures.'
Assert-IdentityContract ($privateHook.Contains('--secure-environment-variables') -and $privateHook.Contains('$mySqlBootstrapToken = $null')) 'MySQL secure token handoff and clearing must remain.'

$savedProjectPath = $env:AZD_PROJECT_PATH
try {
    $env:AZD_PROJECT_PATH = $repoRoot
    . (Join-Path $repoRoot 'scripts\prefix_resource_names.ps1') -envName 'r3test'
    Assert-IdentityContract ($identityName -eq 'id-r3test-worker' -and $userAssignedIdentity -eq $identityName -and $userAssignedIdentityName -eq $identityName) 'Worker naming aliases must agree.'
    Assert-IdentityContract ($orchestratorIdentityName -eq 'id-r3test-orchestrator' -and $mysqlDirectoryIdentityName -eq 'id-r3test-mysql-directory') 'Separated identity names must agree.'
    $global:IdentityTestSqlCalls = [System.Collections.Generic.List[object]]::new()
    function Get-Module { return @{ Name = 'SqlServer'; Version = [version]'22.0' } }
    function Import-Module {}
    function Install-Module { throw 'Offline test must never install modules.' }
    function Start-Sleep {}
    function az {
        $global:LASTEXITCODE = 0
        $command = $args -join ' '
        if ($command -like 'identity show*') {
            Assert-IdentityContract ($command.Contains('--resource-group rg-explicit')) 'Grant script lost the explicit RG override.'
            return '{"clientId":"11111111-2222-3333-4444-555555555555"}'
        }
        if ($command -like 'account get-access-token*') { return 'offline-placeholder' }
        if ($command -like 'sql server list*') {
            return '[{"name":"sql-r3test-a","fullyQualifiedDomainName":"sql-r3test-a.database.windows.net"},{"name":"sql-r3test-b","fullyQualifiedDomainName":"sql-r3test-b.database.windows.net"},{"name":"unrelated","fullyQualifiedDomainName":"unrelated.database.windows.net"}]'
        }
        if ($command -like 'sql db list*') { return @('master','customer','SqlBuildTest1','SqlBuildTest2','SqlBuildTest0','SqlBuildTest1_backup') }
        throw "Unexpected Azure command in offline test: $command"
    }
    function Invoke-Sqlcmd {
        param($ServerInstance, $Database, $AccessToken, $Query, $Encrypt, [switch] $TrustServerCertificate, $ErrorAction)
        Assert-IdentityContract ($Encrypt -eq 'Mandatory' -and $PSBoundParameters.ContainsKey('TrustServerCertificate') -and -not $TrustServerCertificate) 'SQL bootstrap must explicitly encrypt and verify the server certificate.'
        $global:IdentityTestSqlCalls.Add(@{ Server = $ServerInstance; Database = $Database; Query = $Query })
    }
    foreach ($purpose in @('Worker', 'Relay')) {
        $global:IdentityTestSqlCalls.Clear()
        & (Join-Path $repoRoot 'scripts\Database/grant_identity_permissions.ps1') -envName 'r3test' -resourceGroupName 'rg-explicit' -identityPurpose $purpose
        Assert-IdentityContract ($global:IdentityTestSqlCalls.Count -eq 4) 'SQL grants must cover only two generated DBs on each generated server.'
        foreach ($call in $global:IdentityTestSqlCalls) {
            Assert-IdentityContract ($call.Database -in @('SqlBuildTest1','SqlBuildTest2')) 'Unrelated database received a grant.'
            Assert-IdentityContract ($call.Query.Contains('Existing database principal does not match')) 'SID conflict must fail closed.'
            if ($purpose -eq 'Worker') {
                Assert-IdentityContract ($call.Query.Contains('ALTER ROLE [db_owner] ADD MEMBER [id-r3test-worker]')) 'Worker migration permissions must remain.'
            }
            else {
                Assert-IdentityContract (-not $call.Query.Contains('db_owner') -and $call.Query.Contains('GRANT CREATE TABLE') -and $call.Query.Contains('GRANT VIEW DEFINITION')) 'Relay needs scoped DDL and metadata, not db_owner.'
            }
        }
    }

    $global:IdentityBootstrapCalls = [System.Collections.Generic.List[string]]::new()
    $global:IdentityBootstrapValues = @{
        CONTAINER_REGISTRY_NAME = 'acrr3test'
        CONTAINER_REGISTRY_LOGIN_SERVER = 'acrr3test.azurecr.io'
        ACI_SUBNET_ID = '/subscriptions/offline/resourceGroups/rg-explicit/providers/Microsoft.Network/virtualNetworks/vnet/subnets/aci'
        POSTPROVISION_IDENTITY_NAME = 'id-r3test-postprovision'
        POSTPROVISION_IDENTITY_ID = '/subscriptions/offline/resourceGroups/rg-explicit/providers/Microsoft.ManagedIdentity/userAssignedIdentities/id-r3test-postprovision'
        POSTPROVISION_IDENTITY_CLIENT_ID = '11111111-2222-3333-4444-555555555555'
        POSTPROVISION_IDENTITY_PRINCIPAL_ID = 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee'
        MANAGED_IDENTITY_NAME = 'id-r3test-worker'
        AZURE_PRINCIPAL_ID = '22222222-3333-4444-5555-666666666666'
        AZURE_PRINCIPAL_NAME = 'deployer@example.invalid'
    }
    function azd {
        $global:LASTEXITCODE = 0
        if ($args[0] -ne 'env' -or $args[1] -ne 'get-value' -or -not $global:IdentityBootstrapValues.ContainsKey($args[2])) {
            throw 'Unexpected azd command in offline bootstrap test.'
        }
        return $global:IdentityBootstrapValues[$args[2]]
    }
    function az {
        $global:LASTEXITCODE = 0
        $command = $args -join ' '
        $global:IdentityBootstrapCalls.Add($command)
        if ($command -eq 'account show --query id -o tsv') { return 'offline' }
        if ($command -eq 'ad signed-in-user show --query id -o tsv') { return $global:IdentityBootstrapValues.AZURE_PRINCIPAL_ID }
        if ($command -like 'account get-access-token --resource-type oss-rdbms*') { return 'offline-short-lived-user-token' }
        if ($command -like 'acr build*') { return }
        if ($command -like 'sql server list*') { return @('sql-r3test-a','sql-r3test-b','unrelated') }
        if ($command -like 'sql server ad-admin list*') {
            if ($command.Contains('sql-r3test-a')) {
                return '[{"login":"captured-admin-a","sid":"33333333-4444-5555-6666-777777777777"}]'
            }
            return '[{"login":"captured-admin-b","sid":"44444444-5555-6666-7777-888888888888"}]'
        }
        if ($command -like 'sql server ad-admin update*') {
            if ($global:IdentityBootstrapOutcome -eq 'cleanup-failed' -and $command.Contains('--display-name captured-admin-a')) {
                $global:LASTEXITCODE = 1
            }
            return
        }
        if ($command -like 'container show*--output none') { $global:LASTEXITCODE = 1; return }
        if ($command -like 'container create*' -or $command -like 'container delete*' -or $command -like 'container logs*') { return }
        if ($command -like 'container show*--query provisioningState*') { return 'Succeeded' }
        if ($command -like 'container show*currentState.state*') { return 'Terminated' }
        if ($command -like 'container show*currentState.exitCode*') {
            if ($global:IdentityBootstrapOutcome -eq 'container-failed') { return '1' }
            return '0'
        }
        throw "Unexpected Azure command in offline bootstrap test: $command"
    }
    foreach ($outcome in @('success', 'container-failed', 'cleanup-failed', 'disabled', 'both-token-platforms')) {
        $global:IdentityBootstrapOutcome = $outcome
        $global:IdentityBootstrapCalls.Clear()
        $failure = $null
        try {
            & (Join-Path $repoRoot 'scripts\ContainerRegistry\run_private_postprovision_container.ps1') `
                -envName 'r3test' -resourceGroupName 'rg-explicit' -repoRoot $repoRoot `
                -deploySqlServer ($outcome -ne 'disabled') -deployPostgreSQL ($outcome -ne 'disabled') -deployMySQL ($outcome -eq 'both-token-platforms') -deployRelayProxy ($outcome -ne 'disabled')
        }
        catch { $failure = $_.Exception.Message }
        Assert-IdentityContract (($outcome -in @('success', 'disabled', 'both-token-platforms')) -eq ($null -eq $failure)) "Unexpected bootstrap result for '$outcome': $failure"
        if ($outcome -eq 'cleanup-failed') {
            Assert-IdentityContract ($failure.Contains('Bootstrap cleanup failed:')) 'Revocation failure must not be silently accepted.'
        }
        $calls = $global:IdentityBootstrapCalls -join "`n"
        if ($outcome -eq 'disabled') {
            Assert-IdentityContract (-not $calls.Contains('ad-admin') -and $calls.Contains('container delete')) 'Disabled databases must not be administered, and bootstrap must still be deleted.'
            continue
        }
        Assert-IdentityContract (-not $calls.Contains('--server-name unrelated')) 'Bootstrap must not change unrelated SQL server administrators.'
        Assert-IdentityContract (@($global:IdentityBootstrapCalls | Where-Object { $_ -like 'sql server ad-admin update*--display-name captured-admin-*' }).Count -ge 2) 'Both captured SQL administrators must be restored even after failure.'
        Assert-IdentityContract ($calls.Contains('--display-name captured-admin-a --object-id 33333333-4444-5555-6666-777777777777') -and $calls.Contains('--display-name captured-admin-b --object-id 44444444-5555-6666-7777-888888888888')) 'SQL restoration must preserve each original principal, not choose the configured deployer.'
        Assert-IdentityContract (-not $calls.Contains('postgres flexible-server ad-admin') -and $calls.Contains('container delete')) 'Bootstrap must never receive PostgreSQL administration, and its container must be cleaned up.'
        $create = @($global:IdentityBootstrapCalls | Where-Object { $_ -like 'container create*' })[0]
        Assert-IdentityContract ($create.Contains("--assign-identity $($global:IdentityBootstrapValues.POSTPROVISION_IDENTITY_ID)") -and -not $create.Contains('-mysql-directory')) 'Bootstrap must attach only bootstrap identity.'
        Assert-IdentityContract ($create.Contains('DEPLOY_RELAY_PROXY=true')) 'Relay SQL grant toggle must reach private initializer.'
        Assert-IdentityContract ($create.Contains('PG_ENTRA_ADMIN_LOGIN=deployer@example.invalid')) 'PG bootstrap must authenticate as the deploying user.'
        $secureIndex = $create.IndexOf('--secure-environment-variables')
        Assert-IdentityContract ($secureIndex -ge 0 -and $secureIndex -lt $create.IndexOf('PG_BOOTSTRAP_ACCESS_TOKEN=offline-short-lived-user-token')) 'PG token must be passed only after the secure environment switch.'
        if ($outcome -eq 'both-token-platforms') {
            Assert-IdentityContract ([regex]::Matches($create, '--secure-environment-variables').Count -eq 1) 'Both database tokens must share one secure environment argument list.'
            Assert-IdentityContract ($secureIndex -lt $create.IndexOf('MYSQL_BOOTSTRAP_ACCESS_TOKEN=offline-short-lived-user-token')) 'MySQL token must remain a secure environment value.'
            Assert-IdentityContract ($create.IndexOf('MYSQL_ENTRA_ADMIN_LOGIN=') -lt $secureIndex -and $create.IndexOf('PG_ENTRA_ADMIN_LOGIN=') -lt $secureIndex) 'Nonsecret database login names must precede the secure environment switch.'
        }
    }

    $entrypointEnvironment = @{}
    foreach ($name in @('ENV_NAME', 'RESOURCE_GROUP_NAME', 'SUBSCRIPTION_ID', 'POSTPROVISION_CLIENT_ID',
        'MYSQL_AUTH_MODE', 'DEPLOY_MYSQL', 'DEPLOY_POSTGRESQL', 'DEPLOY_SQLSERVER', 'DEPLOY_RELAY_PROXY',
        'MYSQL_BOOTSTRAP_ACCESS_TOKEN', 'PG_BOOTSTRAP_ACCESS_TOKEN', 'AZD_PROJECT_PATH')) {
        $entrypointEnvironment[$name] = [Environment]::GetEnvironmentVariable($name)
    }
    try {
        foreach ($name in @('ENV_NAME', 'RESOURCE_GROUP_NAME', 'SUBSCRIPTION_ID', 'POSTPROVISION_CLIENT_ID')) {
            [Environment]::SetEnvironmentVariable($name, 'offline')
        }
        foreach ($name in @('DEPLOY_MYSQL', 'DEPLOY_POSTGRESQL', 'DEPLOY_SQLSERVER', 'DEPLOY_RELAY_PROXY')) {
            [Environment]::SetEnvironmentVariable($name, 'true')
        }
        $env:MYSQL_AUTH_MODE = 'ManagedIdentity'
        function az {
            Assert-IdentityContract ([string]::IsNullOrEmpty($env:PG_BOOTSTRAP_ACCESS_TOKEN) -and [string]::IsNullOrEmpty($env:MYSQL_BOOTSTRAP_ACCESS_TOKEN)) 'ARM subprocesses must not inherit either database bootstrap token.'
            $global:LASTEXITCODE = 0
        }
        function pwsh {
            $command = $args -join ' '
            $global:LASTEXITCODE = 0
            if ($command.Contains('grant_mysql_identity_permissions.ps1')) {
                Assert-IdentityContract ([string]::IsNullOrEmpty($env:PG_BOOTSTRAP_ACCESS_TOKEN)) 'MySQL subprocess must not inherit the PG token.'
                Assert-IdentityContract ($env:MYSQL_BOOTSTRAP_ACCESS_TOKEN -eq 'offline-mysql-token') 'MySQL must receive its secure token.'
            }
            elseif ($command.Contains('grant_pg_identity_permissions.ps1')) {
                Assert-IdentityContract ($env:PG_BOOTSTRAP_ACCESS_TOKEN -eq 'offline-pg-token') 'Only PG subprocess must receive its secure token.'
                Assert-IdentityContract ([string]::IsNullOrEmpty($env:MYSQL_BOOTSTRAP_ACCESS_TOKEN)) 'MySQL token must be cleared before PG initialization.'
                if ($global:IdentityBootstrapOutcome -eq 'pg-failed') { $global:LASTEXITCODE = 1 }
            }
            elseif ($command.Contains('grant_identity_permissions.ps1')) {
                Assert-IdentityContract ([string]::IsNullOrEmpty($env:PG_BOOTSTRAP_ACCESS_TOKEN) -and [string]::IsNullOrEmpty($env:MYSQL_BOOTSTRAP_ACCESS_TOKEN)) 'SQL subprocesses must not inherit PostgreSQL/MySQL user tokens.'
            }
            else { throw "Unexpected bootstrap subprocess: $command" }
        }
        foreach ($outcome in @('success', 'pg-failed')) {
            $global:IdentityBootstrapOutcome = $outcome
            $env:MYSQL_BOOTSTRAP_ACCESS_TOKEN = 'offline-mysql-token'
            $env:PG_BOOTSTRAP_ACCESS_TOKEN = 'offline-pg-token'
            $failure = $null
            try { & (Join-Path $repoRoot 'infra\postprovision\run-private-postprovision.ps1') }
            catch { $failure = $_.Exception.Message }
            Assert-IdentityContract (($outcome -eq 'success') -eq ($null -eq $failure)) "Unexpected private entrypoint result: $failure"
            Assert-IdentityContract ([string]::IsNullOrEmpty($env:PG_BOOTSTRAP_ACCESS_TOKEN) -and [string]::IsNullOrEmpty($env:MYSQL_BOOTSTRAP_ACCESS_TOKEN)) 'Both user tokens must be cleared even when initialization fails.'
        }
    }
    finally {
        foreach ($name in $entrypointEnvironment.Keys) {
            [Environment]::SetEnvironmentVariable($name, $entrypointEnvironment[$name])
        }
    }
}
finally {
    $env:AZD_PROJECT_PATH = $savedProjectPath
    Remove-Variable IdentityTestSqlCalls -Scope Global -ErrorAction SilentlyContinue
    Remove-Variable IdentityBootstrapCalls, IdentityBootstrapValues, IdentityBootstrapOutcome -Scope Global -ErrorAction SilentlyContinue
}
Write-Host 'Identity infrastructure and mocked SQL grant assertions passed.' -ForegroundColor Green
