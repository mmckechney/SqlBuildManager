@description('Name of the Event Hub namespace')
param eventHubNamespaceName string

@description('Name of the Event Hub')
param eventHubName string

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
@description('Messaging units for premium namespace')
param skuCapacity int = 1

@description('Location for the Event Hub')
param location string = resourceGroup().location

resource eventHubNamespace 'Microsoft.EventHub/namespaces@2017-04-01' = {
  name: eventHubNamespaceName
  location: location
  sku: {
    name: eventhubSku
    tier: eventhubSku
    capacity: skuCapacity
  }
}

resource eventHub 'Microsoft.EventHub/namespaces/eventhubs@2017-04-01' = {
  parent: eventHubNamespace
  name: eventHubName
  properties: {
    messageRetentionInDays: 1
    partitionCount: 5
    status: 'Active'
  }
}

// Authorization rules removed - using Managed Identity with RBAC for Event Hub access
// The identity.bicep module assigns EventHubsDataReceiver and EventHubsDataSender roles

output namespaceId string = eventHubNamespace.id
output eventHubId string = eventHub.id
