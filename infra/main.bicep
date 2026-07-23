// Azure Developer CLI (azd) entry point for SqlBuildManager infrastructure

targetScope = 'subscription'

@description('Azure Developer CLI environment name used to derive resource names. Must be globally unique.')
@minLength(3)
@maxLength(10)
param envName string

@description('Primary location for all resources.')
param location string

@description('The current IP address of the machine running the deployment (for SQL Server firewall)')
param currentIpAddress string = ''

@description('The UserId GUID for the current user (for RBAC assignments)')
param userIdGuid string = ''

@description('The login name (email) of the current user for SQL admin')
param userLoginName string = ''

@description('Whether to deploy the Batch Account')
param deployBatchAccount bool = true

@description('Whether to deploy the Container App Environment')
param deployContainerAppEnv bool = true

@description('Whether to deploy AKS')
param deployAks bool = true

@description('Whether to deploy SQL Server databases')
param deploySqlServer bool = true

@description('Number of test databases to create per server (0 to skip database deployment)')
param testDbCountPerServer int = 10

@description('Whether to use private endpoints for SQL Server connectivity instead of public network access')
param usePrivateEndpoint bool = false

@description('Whether to deploy Azure Database for PostgreSQL Flexible Server')
param deployPostgreSQL bool = true

@description('Whether to deploy Azure Database for MySQL Flexible Server')
param deployMySQL bool = true

@description('Whether to deploy the ACI Relay proxy for private service access')
param deployRelayProxy bool = true

@secure()
@description('Administrator password for PostgreSQL Flexible Server')
param pgAdminPassword string = ''

@secure()
@description('Administrator password for MySQL Flexible Server')
param mySqlAdminPassword string = ''

@allowed([
  'Basic'
  'Standard'
])
@description('The messaging tier for Event Hub namespace')
param eventhubSku string = 'Standard'

@allowed([
  'Basic'
  'Standard'
  'Premium'
])
@description('The messaging tier for Service Bus namespace. Premium required for private endpoints.')
param serviceBusSku string = 'Standard'

@allowed([
  1
  2
  4
])
@description('MessagingUnits for premium namespace')
param skuCapacity int = 1

module resourceNames './modules/resourcenames.bicep' = {
  name: '${prefixes.deployment}${resourceEnvName}-${location}'
  params: {
    envName: resourceEnvName
  }
}

var prefixes = loadJsonContent('resourcetypes.json')
var resourceEnvName = toLower(envName)
// Resource group names must be known before module outputs are evaluated.
var resourceGroupName = '${prefixes.resourceGroup}${resourceEnvName}'
var batchAccountNameVar = resourceNames.outputs.batchAccountName
var storageAccountNameVar = resourceNames.outputs.storageAccountName
var containerAppEnvNameVar = resourceNames.outputs.containerAppEnvName
var logAnalyticsWorkspaceVar = resourceNames.outputs.logAnalyticsWorkspace
var containerRegistryNameVar = resourceNames.outputs.containerRegistryName
var identityNameVar = resourceNames.outputs.identityName
var postProvisionIdentityNameVar = resourceNames.outputs.postProvisionIdentityName
var relayProxyIdentityNameVar = resourceNames.outputs.relayProxyIdentityName
var relayNamespaceNameVar = resourceNames.outputs.relayNamespaceName
var relayPrivateEndpointNameVar = resourceNames.outputs.relayPrivateEndpointName
var relayPrivateLinkNameVar = resourceNames.outputs.relayPrivateLinkName
var relayConnectionNameVar = 'relayproxy'
var eventHubNamespaceNameVar = resourceNames.outputs.eventHubNamespaceName
var eventHubNameVar = resourceNames.outputs.eventHubName
var serviceBusNamespaceNameVar = resourceNames.outputs.serviceBusNamespaceName
var vnetVar = resourceNames.outputs.vnet
var aksSubnetVar = resourceNames.outputs.aksSubnet
var nsgNameVar = resourceNames.outputs.nsgName
var nsgBatchNameVar = resourceNames.outputs.nsgBatchName
var containerAppSubnetVar = resourceNames.outputs.containerAppSubnet
var aciSubnetVar = resourceNames.outputs.aciSubnet
var batchSubnetVar = resourceNames.outputs.batchSubnet
var privateEndpointSubnetVar = resourceNames.outputs.privateEndpointSubnet
var aksClusterNameVar = resourceNames.outputs.aksClusterName
var serviceAccountNameVar = resourceNames.outputs.serviceAccountName
var federatedIdNameVar = resourceNames.outputs.federatedIdName

// Resource Group
resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: resourceGroupName
  location: location
}

// Network
module networkResource './modules/network.bicep' = {
  name: 'networkResource'
  scope: rg
  params: {
    vnetName: vnetVar
    nsgName: nsgNameVar
    nsgBatchName: nsgBatchNameVar
    location: location
    aciSubnetName: aciSubnetVar
    batchSubnetName: batchSubnetVar
    containerAppSubnetName: containerAppSubnetVar
    aksSubnetName: aksSubnetVar
    privateEndpointSubnetName: privateEndpointSubnetVar
  }
}

// Managed Identity
module identityResource './modules/identity.bicep' = {
  name: 'identityResource'
  scope: rg
  params: {
    identityName: identityNameVar
    location: location
  }
}

// Dedicated identity for the VNet-integrated post-provision bootstrap container.
module postProvisionIdentity './modules/postprovisionidentity.bicep' = {
  name: 'postProvisionIdentity'
  scope: rg
  params: {
    identityName: postProvisionIdentityNameVar
    location: location
  }
}

// User Identity RBAC
module userIdentityResource './modules/useridentity.bicep' = if(userIdGuid != ''){
  name: 'userIdentityResource'
  scope: rg
  params: {
    userIdGuid: userIdGuid
  }
}

// Container Registry
module containerRegistry './modules/containerregistry.bicep' = {
  name: 'containerRegistry'
  scope: rg
  params: {
    containerRegistryName: containerRegistryNameVar
    location: location
  }
}

// Container App Environment
module containerAppEnv './modules/containerappenv.bicep' = if(deployContainerAppEnv){
  name: 'containerAppEnv'
  scope: rg
  params: { 
    containerAppEnvName: containerAppEnvNameVar
    logAnalyticsClientId: logAnalyticsWorkspaceResource.outputs.customerId
    logAnalyticsWorkspaceName: logAnalyticsWorkspaceVar
    subnetId: networkResource.outputs.containerAppSubnetId
    location: location
  }
}

// Databases
module databases './modules/database.bicep' = if(deploySqlServer && testDbCountPerServer > 0 && userIdGuid != '' && userLoginName != ''){
  name: 'databases'
  scope: rg
  dependsOn: [
    identityResource
  ]
  params: { 
    location: location
    sqlServerBaseName: resourceNames.outputs.sqlServerBaseName
    sqlElasticPoolBaseName: resourceNames.outputs.sqlElasticPoolBaseName
    identityName: identityNameVar
    envName: resourceEnvName
    testDbCountPerServer: testDbCountPerServer
    sqlAdminObjectId: userIdGuid
    sqlAdminLogin: userLoginName
    vnetId: networkResource.outputs.vnetId
    privateEndpointSubnetId: networkResource.outputs.privateEndpointSubnetId
  }
}

// PostgreSQL Flexible Server
module postgresql './modules/postgresql.bicep' = if(deployPostgreSQL && userIdGuid != '' && userLoginName != '' && pgAdminPassword != ''){
  name: 'postgresql'
  scope: rg
  params: {
    pgServerNameA: resourceNames.outputs.postgresqlServerNameA
    pgServerNameB: resourceNames.outputs.postgresqlServerNameB
    pgAdminUser: resourceNames.outputs.postgresqlAdminUser
    envName: resourceEnvName
    testDbCountPerServer: testDbCountPerServer
    location: location
    pgAdminObjectId: userIdGuid
    pgAdminLogin: userLoginName
    pgAdminPassword: pgAdminPassword
    postProvisionAdminObjectId: postProvisionIdentity.outputs.principalId
    postProvisionAdminName: postProvisionIdentity.outputs.name
    vnetId: networkResource.outputs.vnetId
    privateEndpointSubnetId: networkResource.outputs.privateEndpointSubnetId
  }
}

// MySQL Flexible Server
module mysql './modules/mysql.bicep' = if(deployMySQL && mySqlAdminPassword != ''){
  name: 'mysql'
  scope: rg
  params: {
    mySqlServerNameA: resourceNames.outputs.mySqlServerNameA
    mySqlServerNameB: resourceNames.outputs.mySqlServerNameB
    mySqlAdminUser: resourceNames.outputs.mySqlAdminUser
    envName: resourceEnvName
    testDbCountPerServer: testDbCountPerServer
    location: location
    mySqlAdminPassword: mySqlAdminPassword
    vnetId: networkResource.outputs.vnetId
    privateEndpointSubnetId: networkResource.outputs.privateEndpointSubnetId
  }
}

// Batch Account
module batchAccount './modules/batch.bicep' = if(deployBatchAccount){
  name: 'batchAccount'
  scope: rg
  params: { 
    batchAccountName: batchAccountNameVar
    location: location
    identityName: identityResource.outputs.name
    storageAccountName: storageAccountResource.outputs.name
  }
}

// AKS
module aks './modules/aks.bicep' = if(deployAks){
  name: 'aks'
  scope: rg
  params:{
    aksClusterName: aksClusterNameVar
    location: location
    federatedIdName: federatedIdNameVar
    identityName: identityNameVar
    logAnalyticsWorkspaceName: logAnalyticsWorkspaceVar
    serviceAccountName: serviceAccountNameVar
    subnetName: aksSubnetVar
    vnetName: vnetVar
  }
  dependsOn: [
    networkResource
  ]
}

// Storage Account (inline module to access from subscription scope)
module storageAccountResource './modules/storage.bicep' = {
  name: 'storageAccount'
  scope: rg
  params: {
    storageAccountName: storageAccountNameVar
    location: location
    currentIpAddress: currentIpAddress
    subnetNames: join(networkResource.outputs.subnetNames, ',')
    vnetName: networkResource.outputs.vnetName
    usePrivateEndpoint: usePrivateEndpoint
    vnetId: networkResource.outputs.vnetId
    privateEndpointSubnetId: networkResource.outputs.privateEndpointSubnetId
    envName: resourceEnvName
  }
}

module relayProxy './modules/relayproxy.bicep' = if (deployRelayProxy) {
  name: 'relayProxy'
  scope: rg
  params: {
    relayNamespaceName: relayNamespaceNameVar
    hybridConnectionName: relayConnectionNameVar
    identityName: relayProxyIdentityNameVar
    storageAccountName: storageAccountResource.outputs.name
    eventHubNamespaceName: eventHubNamespaceResource.outputs.namespaceName
    containerRegistryName: containerRegistry.outputs.name
    usePrivateEndpoint: usePrivateEndpoint
    privateEndpointSubnetId: networkResource.outputs.privateEndpointSubnetId
    privateEndpointName: relayPrivateEndpointNameVar
    privateLinkServiceConnectionName: relayPrivateLinkNameVar
    location: location
  }
}

// Log Analytics Workspace (inline module to access from subscription scope)
module logAnalyticsWorkspaceResource './modules/loganalytics.bicep' = {
  name: 'logAnalyticsWorkspace'
  scope: rg
  params: {
    logAnalyticsWorkspaceName: logAnalyticsWorkspaceVar
    location: location
  }
}

// Event Hub Namespace
module eventHubNamespaceResource './modules/eventhub.bicep' = {
  name: 'eventHubNamespace'
  scope: rg
  params: {
    eventHubNamespaceName: eventHubNamespaceNameVar
    eventHubName: eventHubNameVar
    eventhubSku: eventhubSku
    skuCapacity: skuCapacity
    location: location
    usePrivateEndpoint: usePrivateEndpoint
    vnetId: networkResource.outputs.vnetId
    privateEndpointSubnetId: networkResource.outputs.privateEndpointSubnetId
    envName: resourceEnvName
    currentIpAddress: currentIpAddress
    subnetNames: join(networkResource.outputs.subnetNames, ',')
    vnetName: networkResource.outputs.vnetName
  }
}

// Service Bus
module serviceBusResource './modules/servicebus.bicep' = {
  name: 'serviceBus'
  scope: rg
  params: {
    serviceBusNamespaceName: serviceBusNamespaceNameVar
    location: location
    serviceBusSku: serviceBusSku
    usePrivateEndpoint: usePrivateEndpoint
    vnetId: networkResource.outputs.vnetId
    privateEndpointSubnetId: networkResource.outputs.privateEndpointSubnetId
    envName: resourceEnvName
    currentIpAddress: currentIpAddress
    subnetNames: join(networkResource.outputs.subnetNames, ',')
    vnetName: networkResource.outputs.vnetName
  }
}

// Outputs for azd
output AZURE_LOCATION string = location
output AZURE_RESOURCE_GROUP string = resourceGroupName
output ENVIRONMENT_NAME string = envName

// Deployment parameter outputs
output DEPLOY_BATCH_ACCOUNT bool = deployBatchAccount
output DEPLOY_CONTAINER_REGISTRY bool = true
output DEPLOY_CONTAINERAPP_ENV bool = deployContainerAppEnv
output DEPLOY_AKS bool = deployAks
output DEPLOY_SQLSERVER bool = deploySqlServer
output TEST_DB_COUNT_PER_SERVER int = testDbCountPerServer
output EVENTHUB_SKU string = eventhubSku
output SERVICEBUS_SKU string = serviceBusSku
output SKU_CAPACITY int = skuCapacity
output USE_PRIVATE_ENDPOINT bool = usePrivateEndpoint
output DEPLOY_POSTGRESQL bool = deployPostgreSQL
output DEPLOY_MYSQL bool = deployMySQL
output DEPLOY_RELAY_PROXY bool = deployRelayProxy

// Resource outputs
output RESOURCE_GROUP_NAME string = resourceGroupName

output VNET_NAME string = networkResource.outputs.vnetName
output NSG_NAME string = networkResource.outputs.nsgName
output ACI_SUBNET_NAME string = networkResource.outputs.aciSubnetName
output ACI_SUBNET_ID string = networkResource.outputs.aciSubnetId
output BATCH_SUBNET_NAME string = networkResource.outputs.batchSubnetName
output CONTAINERAPP_SUBNET_NAME string = networkResource.outputs.containerAppSubnetName
output AKS_SUBNET_NAME string = networkResource.outputs.aksSubnetName
output PRIVATE_ENDPOINT_SUBNET_NAME string = networkResource.outputs.privateEndpointSubnetName

output MANAGED_IDENTITY_NAME string = identityResource.outputs.name
output MANAGED_IDENTITY_ID string = identityResource.outputs.id
output MANAGED_IDENTITY_CLIENT_ID string = identityResource.outputs.clientId
output MANAGED_IDENTITY_PRINCIPAL_ID string = identityResource.outputs.principalId

output POSTPROVISION_IDENTITY_NAME string = postProvisionIdentity.outputs.name
output POSTPROVISION_IDENTITY_ID string = postProvisionIdentity.outputs.id
output POSTPROVISION_IDENTITY_CLIENT_ID string = postProvisionIdentity.outputs.clientId
output POSTPROVISION_IDENTITY_PRINCIPAL_ID string = postProvisionIdentity.outputs.principalId

output RELAY_PROXY_ENDPOINT string = deployRelayProxy ? relayProxy!.outputs.endpoint : ''

output STORAGE_ACCOUNT_NAME string = storageAccountResource.outputs.name
// output STORAGE_ACCOUNT_ID string = storageAccountResource.outputs.id

output LOG_ANALYTICS_WORKSPACE_NAME string = logAnalyticsWorkspaceResource.outputs.name
output LOG_ANALYTICS_WORKSPACE_ID string = logAnalyticsWorkspaceResource.outputs.id
output LOG_ANALYTICS_WORKSPACE_CUSTOMER_ID string = logAnalyticsWorkspaceResource.outputs.customerId

output EVENTHUB_NAMESPACE_NAME string = eventHubNamespaceResource.outputs.namespaceName
// output EVENTHUB_NAMESPACE_ID string = eventHubNamespaceResource.outputs.namespaceId
output EVENTHUB_NAME string = eventHubNamespaceResource.outputs.eventHubName

output SERVICEBUS_NAMESPACE_NAME string = serviceBusResource.outputs.namespaceName
// output SERVICEBUS_NAMESPACE_ID string = serviceBusResource.outputs.namespaceId

output CONTAINER_REGISTRY_NAME string = containerRegistry.outputs.name
// output CONTAINER_REGISTRY_ID string = containerRegistry.outputs.id
output CONTAINER_REGISTRY_LOGIN_SERVER string = containerRegistry.outputs.loginServer

output CONTAINERAPP_ENVIRONMENT_NAME string = deployContainerAppEnv ? containerAppEnv!.outputs.name : ''
// output CONTAINERAPP_ENVIRONMENT_ID string = deployContainerAppEnv ? containerAppEnv!.outputs.id : ''

output BATCH_ACCOUNT_NAME string = deployBatchAccount ? batchAccount!.outputs.name : ''
// output BATCH_ACCOUNT_ID string = deployBatchAccount ? batchAccount!.outputs.id : ''

output AKS_CLUSTER_NAME string = deployAks ? aks!.outputs.clusterName : ''
// output AKS_CLUSTER_ID string = deployAks ? aks!.outputs.clusterId : ''
output AKS_FEDERATED_IDENTITY_NAME string = deployAks ? aks!.outputs.federatedIdName : ''
output AKS_SERVICE_ACCOUNT_NAME string = deployAks ? aks!.outputs.serviceAccountName : ''

output SQL_SERVER_NAME_A string = deploySqlServer && testDbCountPerServer > 0 && userIdGuid != '' && userLoginName != '' ? databases!.outputs.sqlServerNameA : ''
output SQL_SERVER_NAME_B string = deploySqlServer && testDbCountPerServer > 0 && userIdGuid != '' && userLoginName != '' ? databases!.outputs.sqlServerNameB : ''
output SQL_ELASTIC_POOL_NAME_A string = deploySqlServer && testDbCountPerServer > 0 && userIdGuid != '' && userLoginName != '' ? databases!.outputs.sqlElasticPoolNameA : ''
output SQL_ELASTIC_POOL_NAME_B string = deploySqlServer && testDbCountPerServer > 0 && userIdGuid != '' && userLoginName != '' ? databases!.outputs.sqlElasticPoolNameB : ''

output PG_SERVER_NAME_A string = deployPostgreSQL && pgAdminPassword != '' ? postgresql!.outputs.pgServerNameA : ''
output PG_SERVER_FQDN_A string = deployPostgreSQL && pgAdminPassword != '' ? postgresql!.outputs.pgServerFqdnA : ''
output PG_SERVER_NAME_B string = deployPostgreSQL && pgAdminPassword != '' ? postgresql!.outputs.pgServerNameB : ''
output PG_SERVER_FQDN_B string = deployPostgreSQL && pgAdminPassword != '' ? postgresql!.outputs.pgServerFqdnB : ''
output PG_ADMIN_USER string = deployPostgreSQL && pgAdminPassword != '' ? postgresql!.outputs.pgAdminUser : ''
output PG_DATABASE_COUNT_PER_SERVER int = deployPostgreSQL && pgAdminPassword != '' ? postgresql!.outputs.pgDatabaseCountPerServer : 0

output MYSQL_SERVER_NAME_A string = deployMySQL && mySqlAdminPassword != '' ? mysql!.outputs.mySqlServerNameA : ''
output MYSQL_SERVER_FQDN_A string = deployMySQL && mySqlAdminPassword != '' ? mysql!.outputs.mySqlServerFqdnA : ''
output MYSQL_SERVER_NAME_B string = deployMySQL && mySqlAdminPassword != '' ? mysql!.outputs.mySqlServerNameB : ''
output MYSQL_SERVER_FQDN_B string = deployMySQL && mySqlAdminPassword != '' ? mysql!.outputs.mySqlServerFqdnB : ''
output MYSQL_ADMIN_USER string = deployMySQL && mySqlAdminPassword != '' ? mysql!.outputs.mySqlAdminUser : ''
output MYSQL_DATABASE_COUNT_PER_SERVER int = deployMySQL && mySqlAdminPassword != '' ? mysql!.outputs.mySqlDatabaseCountPerServer : 0
