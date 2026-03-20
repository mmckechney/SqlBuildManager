// Azure Developer CLI (azd) entry point for SqlBuildManager infrastructure

targetScope = 'subscription'

@description('Name prefix for all resources. Must be unique.')
@minLength(3)
@maxLength(10)
param namePrefix string

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

@description('Whether to deploy the container registry')
param deployContainerRegistry bool = true

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

@secure()
@description('Administrator password for PostgreSQL Flexible Server')
param pgAdminPassword string = ''

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

// Resource naming variables
var resourceGroupName = '${namePrefix}-rg'
var batchAccountNameVar = '${namePrefix}batchacct'
var storageAccountNameVar = '${namePrefix}storage'
var containerAppEnvNameVar = '${namePrefix}containerappenv'
var logAnalyticsWorkspaceVar = '${namePrefix}loganalytics'
var containerRegistryNameVar = '${namePrefix}containerregistry'
var identityNameVar = '${namePrefix}identity'
var eventHubNamespaceNameVar = '${namePrefix}eventhubnamespace'
var eventHubNameVar = '${namePrefix}eventhub'
var serviceBusNamespaceNameVar = '${namePrefix}servicebus'
var vnetVar = '${namePrefix}vnet'
var aksSubnetVar = '${namePrefix}akssubnet'
var nsgNameVar = '${namePrefix}nsg'
var containerAppSubnetVar = '${namePrefix}containerappsubnet'
var aciSubnetVar = '${namePrefix}acisubnet'
var batchSubnetVar = '${namePrefix}batchsubnet'
var privateEndpointSubnetVar = '${namePrefix}pesubnet'

// Used with Kubernetes Workload Identity
var aksClusterNameVar = '${namePrefix}aks'
var serviceAccountNameVar = '${namePrefix}serviceaccount'
var federatedIdNameVar = '${namePrefix}federatedidname'

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

// User Identity RBAC
module userIdentityResource './modules/useridentity.bicep' = if(userIdGuid != ''){
  name: 'userIdentityResource'
  scope: rg
  params: {
    userIdGuid: userIdGuid
  }
}

// Container Registry
module containerRegistry './modules/containerregistry.bicep' = if(deployContainerRegistry){
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
  params: { 
    currentIpAddress: currentIpAddress
    location: location
    subnetNames: join(networkResource.outputs.subnetNames, ',')
    namePrefix: namePrefix
    testDbCountPerServer: testDbCountPerServer
    sqlAdminObjectId: userIdGuid
    sqlAdminLogin: userLoginName
    usePrivateEndpoint: usePrivateEndpoint
    vnetId: networkResource.outputs.vnetId
    privateEndpointSubnetId: networkResource.outputs.privateEndpointSubnetId
  }
}

// PostgreSQL Flexible Server
module postgresql './modules/postgresql.bicep' = if(deployPostgreSQL && userIdGuid != '' && userLoginName != '' && pgAdminPassword != ''){
  name: 'postgresql'
  scope: rg
  params: {
    namePrefix: namePrefix
    testDbCountPerServer: testDbCountPerServer
    location: location
    currentIpAddress: currentIpAddress
    subnetNames: join(networkResource.outputs.subnetNames, ',')
    pgAdminObjectId: userIdGuid
    pgAdminLogin: userLoginName
    pgAdminPassword: pgAdminPassword
    usePrivateEndpoint: usePrivateEndpoint
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
    namePrefix: namePrefix
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
    namePrefix: namePrefix
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
    namePrefix: namePrefix
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
    namePrefix: namePrefix
    currentIpAddress: currentIpAddress
    subnetNames: join(networkResource.outputs.subnetNames, ',')
    vnetName: networkResource.outputs.vnetName
  }
}

// Outputs for azd
output AZURE_LOCATION string = location
output AZURE_RESOURCE_GROUP string = resourceGroupName
output AZURE_NAME_PREFIX string = namePrefix

// Deployment parameter outputs
output DEPLOY_BATCH_ACCOUNT bool = deployBatchAccount
output DEPLOY_CONTAINER_REGISTRY bool = deployContainerRegistry
output DEPLOY_CONTAINERAPP_ENV bool = deployContainerAppEnv
output DEPLOY_AKS bool = deployAks
output DEPLOY_SQLSERVER bool = deploySqlServer
output TEST_DB_COUNT_PER_SERVER int = testDbCountPerServer
output EVENTHUB_SKU string = eventhubSku
output SERVICEBUS_SKU string = serviceBusSku
output SKU_CAPACITY int = skuCapacity
output USE_PRIVATE_ENDPOINT bool = usePrivateEndpoint
output DEPLOY_POSTGRESQL bool = deployPostgreSQL

// Resource outputs
output RESOURCE_GROUP_NAME string = resourceGroupName
output RESOURCE_GROUP_ID string = rg.id

output VNET_NAME string = networkResource.outputs.vnetName
output VNET_ID string = networkResource.outputs.vnetId
output NSG_NAME string = networkResource.outputs.nsgName
output ACI_SUBNET_NAME string = networkResource.outputs.aciSubnetName
output BATCH_SUBNET_NAME string = networkResource.outputs.batchSubnetName
output CONTAINERAPP_SUBNET_NAME string = networkResource.outputs.containerAppSubnetName
output AKS_SUBNET_NAME string = networkResource.outputs.aksSubnetName
output PRIVATE_ENDPOINT_SUBNET_NAME string = networkResource.outputs.privateEndpointSubnetName

output MANAGED_IDENTITY_NAME string = identityResource.outputs.name
output MANAGED_IDENTITY_ID string = identityResource.outputs.id
output MANAGED_IDENTITY_CLIENT_ID string = identityResource.outputs.clientId
output MANAGED_IDENTITY_PRINCIPAL_ID string = identityResource.outputs.principalId

output STORAGE_ACCOUNT_NAME string = storageAccountResource.outputs.name
output STORAGE_ACCOUNT_ID string = storageAccountResource.outputs.id

output LOG_ANALYTICS_WORKSPACE_NAME string = logAnalyticsWorkspaceResource.outputs.name
output LOG_ANALYTICS_WORKSPACE_ID string = logAnalyticsWorkspaceResource.outputs.id
output LOG_ANALYTICS_WORKSPACE_CUSTOMER_ID string = logAnalyticsWorkspaceResource.outputs.customerId

output EVENTHUB_NAMESPACE_NAME string = eventHubNamespaceResource.outputs.namespaceName
output EVENTHUB_NAMESPACE_ID string = eventHubNamespaceResource.outputs.namespaceId
output EVENTHUB_NAME string = eventHubNamespaceResource.outputs.eventHubName

output SERVICEBUS_NAMESPACE_NAME string = serviceBusResource.outputs.namespaceName
output SERVICEBUS_NAMESPACE_ID string = serviceBusResource.outputs.namespaceId

output CONTAINER_REGISTRY_NAME string = deployContainerRegistry ? containerRegistry!.outputs.name : ''
output CONTAINER_REGISTRY_ID string = deployContainerRegistry ? containerRegistry!.outputs.id : ''
output CONTAINER_REGISTRY_LOGIN_SERVER string = deployContainerRegistry ? containerRegistry!.outputs.loginServer : ''

output CONTAINERAPP_ENVIRONMENT_NAME string = deployContainerAppEnv ? containerAppEnv!.outputs.name : ''
output CONTAINERAPP_ENVIRONMENT_ID string = deployContainerAppEnv ? containerAppEnv!.outputs.id : ''

output BATCH_ACCOUNT_NAME string = deployBatchAccount ? batchAccount!.outputs.name : ''
output BATCH_ACCOUNT_ID string = deployBatchAccount ? batchAccount!.outputs.id : ''

output AKS_CLUSTER_NAME string = deployAks ? aks!.outputs.clusterName : ''
output AKS_CLUSTER_ID string = deployAks ? aks!.outputs.clusterId : ''
output AKS_FEDERATED_IDENTITY_NAME string = deployAks ? aks!.outputs.federatedIdName : ''
output AKS_SERVICE_ACCOUNT_NAME string = deployAks ? aks!.outputs.serviceAccountName : ''

output PG_SERVER_NAME_A string = deployPostgreSQL && pgAdminPassword != '' ? postgresql!.outputs.pgServerNameA : ''
output PG_SERVER_FQDN_A string = deployPostgreSQL && pgAdminPassword != '' ? postgresql!.outputs.pgServerFqdnA : ''
output PG_SERVER_NAME_B string = deployPostgreSQL && pgAdminPassword != '' ? postgresql!.outputs.pgServerNameB : ''
output PG_SERVER_FQDN_B string = deployPostgreSQL && pgAdminPassword != '' ? postgresql!.outputs.pgServerFqdnB : ''
output PG_ADMIN_USER string = deployPostgreSQL && pgAdminPassword != '' ? postgresql!.outputs.pgAdminUser : ''
output PG_DATABASE_COUNT_PER_SERVER int = deployPostgreSQL && pgAdminPassword != '' ? postgresql!.outputs.pgDatabaseCountPerServer : 0
