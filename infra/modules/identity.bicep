param identityName string
param location string = resourceGroup().location
param resourceGroupName string = resourceGroup().name

resource identityResource 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name : identityName
  location : location

 
}

resource storageBlobDataContributor 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroupName, 'storageBlobDataContributor', identityName)
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
    principalId: identityResource.properties.principalId
    principalType: 'ServicePrincipal'

  }
}

resource serviceBusDataOwner 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroupName, 'ServiceBusDataOwner', identityName)
  properties: {
    roleDefinitionId:  resourceId('Microsoft.Authorization/roleDefinitions', '090c5cfd-751d-490a-894a-3ce6f1109419')
    principalId: identityResource.properties.principalId
    principalType: 'ServicePrincipal'

  }
}

resource eventHubsDataReceiver 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroupName, 'EventHubsDataReceiver', identityName)
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', 'a638d3c7-ab3a-418d-83e6-5f17a39d4fde')
    principalId: identityResource.properties.principalId
    principalType: 'ServicePrincipal'

  }
}

resource eventHubsDataSender 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroupName, 'EventHubsDataSender', identityName)
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', '2b629674-e913-4c01-ae53-ef4638d8f975')
    principalId: identityResource.properties.principalId
    principalType: 'ServicePrincipal'

  }
}

resource acrPull 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroupName, 'AcrPull', identityName)
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
    principalId: identityResource.properties.principalId
    principalType: 'ServicePrincipal'

  }
}

resource contributor 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroupName, 'Contributor', identityName)
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', 'b24988ac-6180-42a0-ab88-20f7382dd24c')
    principalId: identityResource.properties.principalId
    principalType: 'ServicePrincipal'

  }
}


resource kubernetesClusterAdmin 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroupName, 'kubernetesClusterAdmin', identityName)
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', '0ab0b1a8-8aac-4efd-b8c2-3ee1fb270be8')
    principalId: identityResource.properties.principalId
    principalType: 'ServicePrincipal'

  }
}

resource kubernetesRbacAdmin 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroupName, 'kubernetesRbacAdmin', identityName)
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', '3498e952-d568-435e-9b2c-8d77e338d7f7')
    principalId: identityResource.properties.principalId
    principalType: 'ServicePrincipal'

  }
}

resource kubernetesServiceAdmin 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroupName, 'kubernetesServiceAdmin', identityName)
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', 'a7ffa36f-339b-4b5c-8bdf-e2c188b2c0eb')
    principalId: identityResource.properties.principalId
    principalType: 'ServicePrincipal'

  }
}

output clientId string = identityResource.properties.clientId
output tenantId string = identityResource.properties.tenantId
output principalId string = identityResource.properties.principalId
output name string = identityResource.name
output id string = identityResource.id
