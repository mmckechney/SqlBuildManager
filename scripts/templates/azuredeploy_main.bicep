@description('Prefix to prepend to account names')
param namePrefix string = 'eztmwm'

@allowed([
  'Basic'
  'Standard'
])
@description('The messaging tier for service Bus namespace')
param eventhubSku string = 'Standard'

@allowed([
  1
  2
  4
])
@description('MessagingUnits for premium namespace')
param skuCapacity int = 1

@description('Location for all resources.')
param location string = resourceGroup().location

@description('The current IP address of the machine running the deployment')
param currentIpAddress string 

@description('The UserId GUID for the current user')
param userIdGuid string 

@description('Whether or not to deploy the Batch Account')
param deployBatchAccount bool = true

@description('Whether or not to deploy the container registry')
param deployContainerRegistry bool = true

@description('Whether or not to deploy the Container App Environment')
param deployContainerAppEnv bool = true

@description('Whether or not to deploy the AKS')
param deployAks bool = true

@description('Number of test databases to create per server')
param testDbCountPerServer int = 10

@description('Name for SQL Admin account')
param sqladminname string = 'sqladmin_user'

@description('Value for SQL Admin password')
@secure()
param sqladminpassword string = 'ERFSC#$%Ygvswer'



var resourceGroupName = '${namePrefix}-rg'
var batchAccountNameVar = '${namePrefix}batchacct'
var storageAccountNameVar = '${namePrefix}storage'
var containerAppEnvNameVar = '${namePrefix}containerappenv'
var logAnalyticsWorkspaceVar = '${namePrefix}loganalytics'
var containerRegistryNameVar = '${namePrefix}containerregistry'
var keyVaultNameVar = '${namePrefix}keyvault'
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

//Used with Kubernetes Workload Identity
var aksClusterNameVar = '${namePrefix}aks'
var serviceAccountNameVar = '${namePrefix}serviceaccount'
var federatedIdNameVar = '${namePrefix}federatedidname'


module networkResource './Modules/network.bicep' = {
  name: 'networkResource'
 scope: resourceGroup(resourceGroupName)
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

module identityResource './Modules/identity.bicep' = {
  name: 'identityResource'
  scope: resourceGroup(resourceGroupName)
  params: {
    identityName: identityNameVar
    location: location
  }
}

module userIdentityResource './Modules/useridentity.bicep' = if(userIdGuid != null){
  name: 'userIdentityResource'
  scope: resourceGroup(resourceGroupName)
  params: {
    userIdGuid: userIdGuid
  }
}

module keyVaultResource './Modules/keyvault.bicep' = {
  name: 'keyVaultResource'
  scope: resourceGroup(resourceGroupName)
  params: {
    keyvaultName: keyVaultNameVar
    location: location
    identityClientId: identityResource.outputs.clientId
    currentIpAddress: currentIpAddress
    subNet1Id: networkResource.outputs.aksSubnetId
    subNet2Id: networkResource.outputs.containerAppSubnetId
    subNet3Id: networkResource.outputs.aciSubnetId
    subNet4Id: networkResource.outputs.batchSubnetId
  }
}

module containerRegistry './Modules/containerregistry.bicep' = if(deployContainerRegistry){
  name: 'containerRegistry'
  params: {
    containerRegistryName: containerRegistryNameVar
    location: location
  }
}

module containerAppEnv './Modules/containerappenv.bicep' = if(deployContainerAppEnv){
  name: 'containerAppEnv'
  scope: resourceGroup(resourceGroupName)
  params: { 
    containerAppEnvName: containerAppEnvNameVar
    logAnalyticsClientId: logAnalyticsWorkspaceResource.properties.customerId
    logAnalyticsWorkspaceName: logAnalyticsWorkspaceVar
    subnetId: networkResource.outputs.containerAppSubnetId
    location: location

  }
}

module databases './Modules/database.bicep' = if(testDbCountPerServer > 0){
  name: 'databases'
  scope: resourceGroup(resourceGroupName)
  params: { 
    currentIpAddress: currentIpAddress
    location: location
    subnetNames: join(networkResource.outputs.subnetNames, ',')
    namePrefix: namePrefix
    sqladminname: sqladminname
    sqladminpassword: sqladminpassword
    testDbCountPerServer: testDbCountPerServer
  }
}

module batchAccount './Modules/batch.bicep' = if(deployBatchAccount){
  name: 'batchAccount'
  scope: resourceGroup(resourceGroupName)
  params: { 
    batchAccountName: batchAccountNameVar
    location: location
    namePrefix: namePrefix
    identityName: identityResource.outputs.name
    storageAccountName: storageAccountName.name
  }
}

module aks './Modules/aks.bicep' = if(deployAks){
  name: 'aks'
  scope: resourceGroup(resourceGroupName)
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
}
resource storageAccountName 'Microsoft.Storage/storageAccounts@2018-07-01' = {
  name: storageAccountNameVar
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    networkAcls: {
      bypass: 'AzureServices'
      virtualNetworkRules: []
      ipRules: []
      defaultAction: 'Allow'
    }
    supportsHttpsTrafficOnly: false
    encryption: {
      services: {
        file: {
          enabled: true
        }
        blob: {
          enabled: true
        }
      }
      keySource: 'Microsoft.Storage'
    }
    accessTier: 'Hot'
  }
}

resource logAnalyticsWorkspaceResource 'Microsoft.OperationalInsights/workspaces@2021-06-01' = {
  name:logAnalyticsWorkspaceVar
  location: location
  properties:{
    sku:{
      name:  'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource eventHubNamespaceResource 'Microsoft.EventHub/namespaces@2017-04-01' = {
  name: eventHubNamespaceNameVar
  location: location
  sku: {
    name: eventhubSku
    tier: eventhubSku
    capacity: skuCapacity
  }
}

resource eventHubNamespaceResource_eventHubName 'Microsoft.EventHub/namespaces/eventhubs@2017-04-01' = {
  parent: eventHubNamespaceResource
  name: eventHubNameVar
  properties: {
    messageRetentionInDays: 1
    partitionCount: 5
    status: 'Active'
  }
}

resource namespaceName_eventHubName_batchbuilder 'Microsoft.EventHub/namespaces/eventhubs/authorizationRules@2017-04-01' = {
  parent: eventHubNamespaceResource_eventHubName
  name: 'batchbuilder'
  properties: {
    rights: [
      'Listen'
      'Send'
    ]
  }
}

resource serviceBusResource 'Microsoft.ServiceBus/namespaces@2018-01-01-preview' = {
  name: serviceBusNamespaceNameVar
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
    capacity: 1
  }
  properties: {
    zoneRedundant: false
  }
}

resource serviceBusResource_RootManageSharedAccessKey 'Microsoft.ServiceBus/namespaces/AuthorizationRules@2017-04-01' = {
  parent: serviceBusResource
  name: 'RootManageSharedAccessKey'
  properties: {
    rights: [
      'Listen'
      'Manage'
      'Send'
    ]
  }
}

resource serviceBusResource_topic_sqlbuildmanager 'Microsoft.ServiceBus/namespaces/topics@2018-01-01-preview' = {
  parent: serviceBusResource
  name: 'sqlbuildmanager'
  properties: {
    defaultMessageTimeToLive: 'P14D'
    maxSizeInMegabytes: 4096
    requiresDuplicateDetection: false
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    enableBatchedOperations: true
    status: 'Active'
    supportOrdering: true
    autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
    enablePartitioning: true
    enableExpress: false
  }
}

resource serviceBusResource_topic_sqlbuildmanager_sbmtopicpolicy 'Microsoft.ServiceBus/namespaces/topics/authorizationRules@2018-01-01-preview' = {
  parent: serviceBusResource_topic_sqlbuildmanager
  name: 'sbmtopicpolicy'
  properties: {
    rights: [
      'Manage'
      'Listen'
      'Send'
    ]
  }
}



