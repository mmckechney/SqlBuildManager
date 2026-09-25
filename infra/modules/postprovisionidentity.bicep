param identityName string
param location string = resourceGroup().location
param containerRegistryName string

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: identityName
  location: location
}

// Discovery only: no keys, Graph, administrator writes or identity assignment.
resource discoveryRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' = {
  name: guid(resourceGroup().id, 'SbmBootstrapDiscovery')
  properties: {
    roleName: '${resourceGroup().name}-bootstrap-discovery'
    type: 'CustomRole'
    assignableScopes: [resourceGroup().id]
    permissions: [{
      actions: [
        'Microsoft.ManagedIdentity/userAssignedIdentities/read'
        'Microsoft.Sql/servers/read'
        'Microsoft.Sql/servers/databases/read'
        'Microsoft.DBforPostgreSQL/flexibleServers/read'
        'Microsoft.DBforPostgreSQL/flexibleServers/databases/read'
        'Microsoft.DBforMySQL/flexibleServers/read'
        'Microsoft.DBforMySQL/flexibleServers/databases/read'
      ]
      notActions: []
      dataActions: []
      notDataActions: []
    }]
  }
}

resource discovery 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, identity.id, discoveryRole.name)
  properties: {
    roleDefinitionId: discoveryRole.id
    principalId: identity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource registry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: containerRegistryName
}
resource acrPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(registry.id, identity.id, 'AcrPull')
  scope: registry
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
    principalId: identity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

output clientId string = identity.properties.clientId
output principalId string = identity.properties.principalId
output name string = identity.name
output id string = identity.id
