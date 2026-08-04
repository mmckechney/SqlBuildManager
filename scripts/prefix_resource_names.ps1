<#
.SYNOPSIS
    Sets standard Azure resource name variables derived from an azd environment name.
.DESCRIPTION
    Loads prefixes from infra/resourcetypes.json and defines variables for all Azure resource names used by the "*fromenv.ps1"
    scripts (e.g. storage account, Batch account, AKS cluster, container registry,
    Event Hub, Service Bus, SQL servers, managed identity, and resource group).
    Designed to be dot-sourced by other scripts.
.PARAMETER envName
    Azure Developer CLI environment name used in resource name conventions.
#>

param 
(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[a-z0-9-]{3,10}$')]
    [string] $envName
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$resourceEnvName = $envName.ToLowerInvariant()
$normalizedEnvName = $resourceEnvName.Replace("-", "")
$repoRoot = Split-Path $PSScriptRoot -Parent
$resourceTypesPath = Join-Path $repoRoot "infra\resourcetypes.json"
if (-not (Test-Path $resourceTypesPath -PathType Leaf)) {
    throw "Azure resource type prefix map was not found at '$resourceTypesPath'."
}
$resourceTypePrefixes = Get-Content $resourceTypesPath -Raw | ConvertFrom-Json

$resourceGroupName = "$($resourceTypePrefixes.resourceGroup)$resourceEnvName"
$batchAccountName = "$(($resourceTypePrefixes.batchAccounts -replace '[^a-zA-Z0-9]', ''))$normalizedEnvName"
$storageAccountName = "$(($resourceTypePrefixes.storageAccount -replace '[^a-zA-Z0-9]', ''))$normalizedEnvName"
$aciName = "$($resourceTypePrefixes.containerInstance)$resourceEnvName"
$containerAppEnvName = "$($resourceTypePrefixes.containerAppsEnvironment)$resourceEnvName"
$logAnalyticsWorkspace = "$($resourceTypePrefixes.logAnalyticsWorkspace)$resourceEnvName"
$containerRegistryName = "$(($resourceTypePrefixes.containerRegistry -replace '[^a-zA-Z0-9]', ''))$normalizedEnvName"
$keyVaultName = "$($resourceTypePrefixes.keyVault)$resourceEnvName"

$identityName = "$($resourceTypePrefixes.managedIdentity)$resourceEnvName"
$userAssignedIdentity = $identityName
$userAssignedIdentityName = $identityName
$postProvisionIdentityName = "$($resourceTypePrefixes.managedIdentity)$resourceEnvName-postprovision"
$postProvisionContainerName = "$($resourceTypePrefixes.containerInstance)$resourceEnvName-postprovision"
$relayProxyIdentityName = "$($resourceTypePrefixes.managedIdentity)$resourceEnvName-relayproxy"
$relayProxyContainerName = "$($resourceTypePrefixes.containerInstance)$resourceEnvName-relayproxy"
$relayNamespaceName = "relay-$resourceEnvName"

$eventHubNamespaceName = "$($resourceTypePrefixes.eventHubsNamespace)$resourceEnvName"
$eventHubName = "$($resourceTypePrefixes.eventHub)$resourceEnvName"
$serviceBusNamespaceName = "$($resourceTypePrefixes.serviceBusNamespace)$resourceEnvName"
$aksClusterName = "$($resourceTypePrefixes.aksCluster)$resourceEnvName"
$vnet = "$($resourceTypePrefixes.virtualNetwork)$resourceEnvName"
$aksSubnet = "$($resourceTypePrefixes.virtualNetworkSubnet)$resourceEnvName-aks"
$nsgName = "$($resourceTypePrefixes.networkSecurityGroup)$resourceEnvName"
$nsgBatchName = "$($resourceTypePrefixes.networkSecurityGroup)$resourceEnvName-batch"
$containerAppSubnet = "$($resourceTypePrefixes.virtualNetworkSubnet)$resourceEnvName-cae"
$aciSubnet = "$($resourceTypePrefixes.virtualNetworkSubnet)$resourceEnvName-aci"
$batchSubnet = "$($resourceTypePrefixes.virtualNetworkSubnet)$resourceEnvName-batch"
$privateEndpointSubnet = "$($resourceTypePrefixes.virtualNetworkSubnet)$resourceEnvName-pe"

#Used with Kubernetes Workload Identity
$serviceAccountName = "$($resourceTypePrefixes.kubernetesServiceAccount)$resourceEnvName"
$federatedIdName = "$($resourceTypePrefixes.federatedIdentityCredential)$resourceEnvName"

#Used with PostgreSQL Flexible Server
$sqlServerBaseName = "$($resourceTypePrefixes.sqlDatabaseServer)$resourceEnvName"
$sqlElasticPoolBaseName = "$($resourceTypePrefixes.sqlElasticPool)$resourceEnvName"
$sqlServerNameA = "$sqlServerBaseName-a"
$sqlServerNameB = "$sqlServerBaseName-b"
$pgServerNameA = "$($resourceTypePrefixes.postgreSQLServer)$resourceEnvName-a"
$pgServerNameB = "$($resourceTypePrefixes.postgreSQLServer)$resourceEnvName-b"
$pgAdminUser = "$($resourceTypePrefixes.postgreSQLAdministrator)$normalizedEnvName"
$mySqlServerNameA = "$($resourceTypePrefixes.mySQLServer)$resourceEnvName-a"
$mySqlServerNameB = "$($resourceTypePrefixes.mySQLServer)$resourceEnvName-b"
$mySqlAdminUser = "$($resourceTypePrefixes.mySQLAdministrator)$normalizedEnvName"
