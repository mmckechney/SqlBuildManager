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

var storageAccountName_var = '${namePrefix}storage'
var namespaceName_var = '${namePrefix}eventhubnamespace'
var eventHubName = '${namePrefix}eventhub'
var serviceBusName_var = '${namePrefix}servicebus'
var keyvaultName_var = '${namePrefix}keyvault'
var identityName_var = '${namePrefix}identity'
var logAnalytics_var = '${namePrefix}loganalytics'


resource storageAccountName 'Microsoft.Storage/storageAccounts@2018-07-01' = {
  name: storageAccountName_var
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

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2021-06-01' = {
  name:logAnalytics_var
  location: location
  properties:{
    sku:{
      name:  'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource eventHubNamespace 'Microsoft.EventHub/namespaces@2017-04-01' = {
  name: namespaceName_var
  location: location
  sku: {
    name: eventhubSku
    tier: eventhubSku
    capacity: skuCapacity
  }
}

resource eventHubNamespace_eventHubName 'Microsoft.EventHub/namespaces/eventhubs@2017-04-01' = {
  name: '${eventHubNamespace.name}/${eventHubName}'
  properties: {
    messageRetentionInDays: 1
    partitionCount: 5
    status: 'Active'
  }
  dependsOn: [
    eventHubNamespace
  ]
}

resource namespaceName_eventHubName_batchbuilder 'Microsoft.EventHub/namespaces/eventhubs/authorizationRules@2017-04-01' = {
  name: '${eventHubNamespace_eventHubName.name}/batchbuilder'

  properties: {
    rights: [
      'Listen'
      'Send'
    ]
  }
  dependsOn: [
    eventHubNamespace
  ]
}

resource serviceBusName 'Microsoft.ServiceBus/namespaces@2018-01-01-preview' = {
  name: serviceBusName_var
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

resource serviceBusName_RootManageSharedAccessKey 'Microsoft.ServiceBus/namespaces/AuthorizationRules@2017-04-01' = {
  name: '${serviceBusName.name}/RootManageSharedAccessKey'
  properties: {
    rights: [
      'Listen'
      'Manage'
      'Send'
    ]
  }
  dependsOn: [
    serviceBusName
  ]
}

resource serviceBusName_sqlbuildmanager 'Microsoft.ServiceBus/namespaces/topics@2018-01-01-preview' = {
  name: '${serviceBusName.name}/sqlbuildmanager'
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
  dependsOn: [
    serviceBusName
  ]
}

resource serviceBusName_sqlbuildmanager_sbmtopicpolicy 'Microsoft.ServiceBus/namespaces/topics/authorizationRules@2018-01-01-preview' = {
  name: '${serviceBusName_sqlbuildmanager.name}/sbmtopicpolicy'
  properties: {
    rights: [
      'Manage'
      'Listen'
      'Send'
    ]
  }
  dependsOn: [
    serviceBusName
  ]
}

resource keyVault_resource 'Microsoft.KeyVault/vaults@2019-09-01' = {
  name: keyvaultName_var
  location: location
  properties: {
    enabledForTemplateDeployment: true
    enableRbacAuthorization: false
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }  
    accessPolicies:[
      {
        tenantId: subscription().tenantId
        objectId:identity_resource.properties.clientId
        permissions:  {
          secrets:[
            'set'
            'list'
            'get'
          ]
        }
        
      }
    ]
  }
}

resource identity_resource 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name : identityName_var
  location : location
 
}

