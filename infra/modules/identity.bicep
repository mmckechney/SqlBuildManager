param identityName string
param location string = resourceGroup().location

// Identity creation deliberately carries no permissions. Each consumer scopes its grants.
resource identityResource 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: identityName
  location: location
}

output clientId string = identityResource.properties.clientId
output tenantId string = identityResource.properties.tenantId
output principalId string = identityResource.properties.principalId
output name string = identityResource.name
output id string = identityResource.id
