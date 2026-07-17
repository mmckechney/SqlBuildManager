[CmdletBinding()]
param()

$repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
. (Join-Path $repoRoot "scripts\prefix_resource_names.ps1") -envName "dev-01"

$expectedNames = @{
    resourceGroupName       = "rg-dev-01"
    batchAccountName        = "badev01"
    storageAccountName      = "stdev01"
    aciName                 = "cidev-01"
    containerAppEnvName     = "cae-dev-01"
    logAnalyticsWorkspace   = "log-dev-01"
    containerRegistryName   = "crdev01"
    keyVaultName            = "kv-dev-01"
    identityName            = "id-dev-01"
    postProvisionIdentityName = "id-dev-01-postprovision"
    postProvisionContainerName = "cidev-01-postprovision"
    relayProxyIdentityName  = "id-dev-01-relayproxy"
    relayProxyContainerName = "cidev-01-relayproxy"
    relayNamespaceName      = "relay-dev-01"
    eventHubNamespaceName   = "evhns-dev-01"
    eventHubName            = "evh-dev-01"
    serviceBusNamespaceName = "sbns-dev-01"
    aksClusterName          = "aks-dev-01"
    vnet                    = "vnet-dev-01"
    aksSubnet               = "snet-dev-01-aks"
    containerAppSubnet      = "snet-dev-01-cae"
    aciSubnet               = "snet-dev-01-aci"
    batchSubnet             = "snet-dev-01-batch"
    privateEndpointSubnet   = "snet-dev-01-pe"
    serviceAccountName      = "sa-dev-01"
    federatedIdName         = "fic-dev-01"
    sqlServerNameA          = "sql-dev-01-a"
    sqlServerNameB          = "sql-dev-01-b"
    pgServerNameA           = "psql-dev-01-a"
    pgServerNameB           = "psql-dev-01-b"
    pgAdminUser             = "pgadmindev01"
}

$expectedPrefixes = @{
    resourceGroupName       = $resourceTypePrefixes.resourceGroup
    storageAccountName      = $resourceTypePrefixes.storageAccount
    aciName                 = $resourceTypePrefixes.containerInstance
    containerAppEnvName     = $resourceTypePrefixes.containerAppsEnvironment
    logAnalyticsWorkspace   = $resourceTypePrefixes.logAnalyticsWorkspace
    keyVaultName            = $resourceTypePrefixes.keyVault
    identityName            = $resourceTypePrefixes.managedIdentity
    postProvisionIdentityName = $resourceTypePrefixes.managedIdentity
    postProvisionContainerName = $resourceTypePrefixes.containerInstance
    relayProxyIdentityName  = $resourceTypePrefixes.managedIdentity
    relayProxyContainerName = $resourceTypePrefixes.containerInstance
    eventHubNamespaceName   = $resourceTypePrefixes.eventHubsNamespace
    eventHubName            = $resourceTypePrefixes.eventHub
    serviceBusNamespaceName = $resourceTypePrefixes.serviceBusNamespace
    aksClusterName          = $resourceTypePrefixes.aksCluster
    vnet                    = $resourceTypePrefixes.virtualNetwork
    aksSubnet               = $resourceTypePrefixes.virtualNetworkSubnet
}

foreach ($entry in $expectedNames.GetEnumerator()) {
    $actual = Get-Variable -Name $entry.Key -ValueOnly
    if ($actual -ne $entry.Value) {
        throw "Expected $($entry.Key) to be '$($entry.Value)', but found '$actual'."
    }
}

foreach ($entry in $expectedPrefixes.GetEnumerator()) {
    $actual = Get-Variable -Name $entry.Key -ValueOnly
    if (-not $actual.StartsWith($entry.Value)) {
        throw "Expected $($entry.Key) to use prefix '$($entry.Value)', but found '$actual'."
    }
}

$constrainedPrefixes = @{
    batchAccountName      = $resourceTypePrefixes.batchAccounts
    storageAccountName    = $resourceTypePrefixes.storageAccount
    containerRegistryName = $resourceTypePrefixes.containerRegistry
}
foreach ($entry in $constrainedPrefixes.GetEnumerator()) {
    $expectedPrefix = $entry.Value -replace '[^a-zA-Z0-9]', ''
    $actual = Get-Variable -Name $entry.Key -ValueOnly
    if (-not $actual.StartsWith($expectedPrefix)) {
        throw "Expected $($entry.Key) to use sanitized prefix '$expectedPrefix', but found '$actual'."
    }
}

if ($storageAccountName -notmatch '^[a-z0-9]{3,24}$') {
    throw "Storage account name '$storageAccountName' violates Azure naming constraints."
}

if ($containerRegistryName -notmatch '^[a-zA-Z0-9]{5,50}$') {
    throw "Container registry name '$containerRegistryName' violates Azure naming constraints."
}

if ($batchAccountName -notmatch '^[a-z0-9]{3,24}$') {
    throw "Batch account name '$batchAccountName' violates Azure naming constraints."
}

Write-Output "Azure resource naming tests passed."
