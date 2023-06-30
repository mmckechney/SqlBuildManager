param userIdGuid string
param resourceGroupName string = resourceGroup().name

resource keyVaultSecretsUser 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroupName, 'KeyVaultSecretsUser', userIdGuid)
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6')
    principalId: userIdGuid
    principalType: 'User'


  }
}

resource keyVaultSecretsOfficer 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroupName, 'keyVaultSecretsOfficer', userIdGuid)
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7')
    principalId: userIdGuid
    principalType: 'User'


  }
}

resource storageBlobDataContributor 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroupName, 'storageBlobDataContributor', userIdGuid)
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
    principalId: userIdGuid
    principalType: 'User'

  }
}

resource serviceBusDataOwner 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroupName, 'ServiceBusDataOwner', userIdGuid)
  properties: {
    roleDefinitionId:  resourceId('Microsoft.Authorization/roleDefinitions', '090c5cfd-751d-490a-894a-3ce6f1109419')
    principalId: userIdGuid
    principalType: 'User'

  }
}

resource eventHubsDataReceiver 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroupName, 'EventHubsDataReceiver', userIdGuid)
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', 'a638d3c7-ab3a-418d-83e6-5f17a39d4fde')
    principalId: userIdGuid
    principalType: 'User'

  }
}

resource eventHubsDataSender 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroupName, 'EventHubsDataSender', userIdGuid)
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', '2b629674-e913-4c01-ae53-ef4638d8f975')
    principalId: userIdGuid
    principalType: 'User'

  }
}

resource acrPull 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroupName, 'AcrPull', userIdGuid)
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
    principalId: userIdGuid
    principalType: 'User'

  }
}

resource kubernetesClusterAdmin 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroupName, 'kubernetesClusterAdmin', userIdGuid)
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', '0ab0b1a8-8aac-4efd-b8c2-3ee1fb270be8')
    principalId: userIdGuid
    principalType: 'User'

  }
}

resource kubernetesRbacAdmin 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroupName, 'kubernetesRbacAdmin', userIdGuid)
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', '3498e952-d568-435e-9b2c-8d77e338d7f7')
    principalId: userIdGuid
    principalType: 'User'

  }
}

resource kubernetesServiceAdmin 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroupName, 'kubernetesServiceAdmin', userIdGuid)
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', 'a7ffa36f-339b-4b5c-8bdf-e2c188b2c0eb')
    principalId: userIdGuid
    principalType: 'User'

  }
}
resource contributor 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroupName, 'Contributor', userIdGuid)
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', 'b24988ac-6180-42a0-ab88-20f7382dd24c')
    principalId: userIdGuid
    principalType: 'User'

  }
}

