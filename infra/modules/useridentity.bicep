param userIdGuid string
param storageAccountName string
param containerRegistryName string
param serviceBusNamespaceName string
param eventHubNamespaceName string
param eventHubName string

// Provisioning permissions belong to the external deployer, not these data-plane grants.
resource storage 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
}
resource registry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: containerRegistryName
}
resource topic 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' existing = {
  name: '${serviceBusNamespaceName}/sqlbuildmanager'
}
resource hub 'Microsoft.EventHub/namespaces/eventhubs@2024-01-01' existing = {
  name: '${eventHubNamespaceName}/${eventHubName}'
}
resource storageAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage.id, userIdGuid, 'StorageBlobDataContributor')
  scope: storage
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
    principalId: userIdGuid
    principalType: 'User'
  }
}
resource queueAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(topic.id, userIdGuid, 'ServiceBusDataOwner')
  scope: topic
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '090c5cfd-751d-490a-894a-3ce6f1109419')
    principalId: userIdGuid
    principalType: 'User'
  }
}
resource eventAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for role in [
  'a638d3c7-ab3a-418d-83e6-5f17a39d4fde'
  '2b629674-e913-4c01-ae53-ef4638d8f975'
]: {
  name: guid(hub.id, userIdGuid, role)
  scope: hub
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', role)
    principalId: userIdGuid
    principalType: 'User'
  }
}]
resource imagePull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(registry.id, userIdGuid, 'AcrPull')
  scope: registry
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
    principalId: userIdGuid
    principalType: 'User'
  }
}
