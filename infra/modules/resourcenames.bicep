targetScope = 'subscription'

param envName string

var prefixes = loadJsonContent('../resourcetypes.json')
var resourceEnvName = toLower(envName)
var normalizedEnvName = replace(resourceEnvName, '-', '')
var resourceGroupNameVar = '${prefixes.resourceGroup}${resourceEnvName}'
var batchAccountNameVar = '${replace(prefixes.batchAccounts, '-', '')}${normalizedEnvName}'
var storageAccountNameVar = '${replace(prefixes.storageAccount, '-', '')}${normalizedEnvName}'
var aciNameVar = '${prefixes.containerInstance}${resourceEnvName}'
var containerAppEnvNameVar = '${prefixes.containerAppsEnvironment}${resourceEnvName}'
var logAnalyticsWorkspaceVar = '${prefixes.logAnalyticsWorkspace}${resourceEnvName}'
var containerRegistryNameVar = '${replace(prefixes.containerRegistry, '-', '')}${normalizedEnvName}'
var keyVaultNameVar = '${prefixes.keyVault}${resourceEnvName}'

var identityNameVar = '${prefixes.managedIdentity}${resourceEnvName}'
var userAssignedIdentityVar = identityNameVar
var userAssignedIdentityNameVar = identityNameVar

var eventHubNamespaceNameVar = '${prefixes.eventHubsNamespace}${resourceEnvName}'
var eventHubNameVar = '${prefixes.eventHub}${resourceEnvName}'
var serviceBusNamespaceNameVar = '${prefixes.serviceBusNamespace}${resourceEnvName}'
var aksClusterNameVar = '${prefixes.aksCluster}${resourceEnvName}'
var vnetVar = '${prefixes.virtualNetwork}${resourceEnvName}'
var aksSubnetVar = '${prefixes.virtualNetworkSubnet}${resourceEnvName}-aks'
var nsgNameVar = '${prefixes.networkSecurityGroup}${resourceEnvName}'
var nsgBatchNameVar = '${prefixes.networkSecurityGroup}${resourceEnvName}-batch'
var containerAppSubnetVar = '${prefixes.virtualNetworkSubnet}${resourceEnvName}-cae'
var aciSubnetVar = '${prefixes.virtualNetworkSubnet}${resourceEnvName}-aci'
var batchSubnetVar = '${prefixes.virtualNetworkSubnet}${resourceEnvName}-batch'
var privateEndpointSubnetVar = '${prefixes.virtualNetworkSubnet}${resourceEnvName}-pe'

//Used with Kubernetes Workload Identity
var serviceAccountNameVar = '${prefixes.kubernetesServiceAccount}${resourceEnvName}'
var federatedIdNameVar = '${prefixes.federatedIdentityCredential}${resourceEnvName}'
var sqlServerBaseNameVar = '${prefixes.sqlDatabaseServer}${resourceEnvName}'
var sqlElasticPoolBaseNameVar = '${prefixes.sqlElasticPool}${resourceEnvName}'
var postgresqlServerNameA = '${prefixes.postgreSQLServer}${resourceEnvName}-a'
var postgresqlServerNameB = '${prefixes.postgreSQLServer}${resourceEnvName}-b'
var postgresqlAdminUser = '${prefixes.postgreSQLAdministrator}${normalizedEnvName}'

output resourceGroupName string = resourceGroupNameVar
output batchAccountName string = batchAccountNameVar
output storageAccountName string = storageAccountNameVar
output aciName string = aciNameVar
output containerAppEnvName string = containerAppEnvNameVar
output logAnalyticsWorkspace string = logAnalyticsWorkspaceVar
output containerRegistryName string = containerRegistryNameVar
output keyVaultName string = keyVaultNameVar
output identityName string = identityNameVar
output userAssignedIdentity string = userAssignedIdentityVar
output userAssignedIdentityName string = userAssignedIdentityNameVar
output eventHubNamespaceName string = eventHubNamespaceNameVar
output eventHubName string = eventHubNameVar
output serviceBusNamespaceName string = serviceBusNamespaceNameVar
output aksClusterName string = aksClusterNameVar
output vnet string = vnetVar
output aksSubnet string = aksSubnetVar
output nsgName string = nsgNameVar
output nsgBatchName string = nsgBatchNameVar
output containerAppSubnet string = containerAppSubnetVar
output aciSubnet string = aciSubnetVar
output batchSubnet string = batchSubnetVar
output privateEndpointSubnet string = privateEndpointSubnetVar
output serviceAccountName string = serviceAccountNameVar
output federatedIdName string = federatedIdNameVar
output sqlServerBaseName string = sqlServerBaseNameVar
output sqlElasticPoolBaseName string = sqlElasticPoolBaseNameVar
output postgresqlServerNameA string = postgresqlServerNameA
output postgresqlServerNameB string = postgresqlServerNameB
output postgresqlAdminUser string = postgresqlAdminUser
