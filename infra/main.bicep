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

@description('Number of test databases to create per server (0 to skip database deployment)')
param testDbCountPerServer int = 10

@allowed([
  'Basic'
  'Standard'
])
@description('The messaging tier for Event Hub namespace')
param eventhubSku string = 'Standard'

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
module databases './modules/database.bicep' = if(testDbCountPerServer > 0 && userIdGuid != '' && userLoginName != ''){
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
  }
}

// Service Bus
module serviceBusResource './modules/servicebus.bicep' = {
  name: 'serviceBus'
  scope: rg
  params: {
    serviceBusNamespaceName: serviceBusNamespaceNameVar
    location: location
  }
}

// Outputs for azd
output AZURE_LOCATION string = location
output AZURE_RESOURCE_GROUP string = resourceGroupName
output AZURE_NAME_PREFIX string = namePrefix
