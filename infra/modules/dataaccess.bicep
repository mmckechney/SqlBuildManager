param workerPrincipalId string
param orchestratorPrincipalId string
param storageAccountName string
param containerRegistryName string
param serviceBusNamespaceName string
param eventHubNamespaceName string
param eventHubName string

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

var runtimePrincipals = [workerPrincipalId, orchestratorPrincipalId]
resource storageAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for principalId in runtimePrincipals: {
  name: guid(storage.id, principalId, 'StorageBlobDataContributor')
  scope: storage
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}]
resource imagePull 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for principalId in runtimePrincipals: {
  name: guid(registry.id, principalId, 'AcrPull')
  scope: registry
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}]
resource workerQueue 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(topic.id, workerPrincipalId, 'ServiceBusDataReceiver')
  scope: topic
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0')
    principalId: workerPrincipalId
    principalType: 'ServicePrincipal'
  }
}
resource orchestratorQueue 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(topic.id, orchestratorPrincipalId, 'ServiceBusDataOwner')
  scope: topic
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '090c5cfd-751d-490a-894a-3ce6f1109419')
    principalId: orchestratorPrincipalId
    principalType: 'ServicePrincipal'
  }
}
resource resultSend 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for principalId in runtimePrincipals: {
  name: guid(hub.id, principalId, 'EventHubsDataSender')
  scope: hub
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '2b629674-e913-4c01-ae53-ef4638d8f975')
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}]
resource resultReceive 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(hub.id, orchestratorPrincipalId, 'EventHubsDataReceiver')
  scope: hub
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'a638d3c7-ab3a-418d-83e6-5f17a39d4fde')
    principalId: orchestratorPrincipalId
    principalType: 'ServicePrincipal'
  }
}
resource consumerGroupRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' = {
  name: guid(resourceGroup().id, 'SbmConsumerGroups')
  properties: {
    roleName: '${resourceGroup().name}-consumer-groups'
    type: 'CustomRole'
    assignableScopes: [resourceGroup().id]
    permissions: [{
      actions: [
        'Microsoft.EventHub/namespaces/eventhubs/read'
        'Microsoft.EventHub/namespaces/eventhubs/consumergroups/read'
        'Microsoft.EventHub/namespaces/eventhubs/consumergroups/write'
        'Microsoft.EventHub/namespaces/eventhubs/consumergroups/delete'
      ]
      notActions: []
      dataActions: []
      notDataActions: []
    }]
  }
}
resource consumerGroupAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(hub.id, orchestratorPrincipalId, consumerGroupRole.name)
  scope: hub
  properties: {
    roleDefinitionId: consumerGroupRole.id
    principalId: orchestratorPrincipalId
    principalType: 'ServicePrincipal'
  }
}
