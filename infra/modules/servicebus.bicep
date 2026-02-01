@description('Name of the Service Bus namespace')
param serviceBusNamespaceName string

@description('Location for the Service Bus')
param location string = resourceGroup().location

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2018-01-01-preview' = {
  name: serviceBusNamespaceName
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

// Authorization rules removed - using Managed Identity with RBAC for Service Bus access
// The identity.bicep module assigns ServiceBusDataOwner role

resource topic 'Microsoft.ServiceBus/namespaces/topics@2018-01-01-preview' = {
  parent: serviceBusNamespace
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

// Topic authorization rule removed - using Managed Identity with RBAC for Service Bus access

output namespaceId string = serviceBusNamespace.id
output topicId string = topic.id
output namespaceName string = serviceBusNamespace.name
