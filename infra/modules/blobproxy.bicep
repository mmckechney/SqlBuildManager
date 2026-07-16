@description('Globally unique Azure Relay namespace name')
param relayNamespaceName string

@description('Hybrid Connection name used by the blob proxy')
param hybridConnectionName string

@description('User-assigned managed identity name for the proxy ACI')
param identityName string

@description('Storage account name the proxy is allowed to access')
param storageAccountName string

@description('Container Registry name containing the proxy image')
param containerRegistryName string

@description('Object ID of the deployment principal that sends through Relay')
param senderPrincipalId string = ''

@description('Object ID of the runtime managed identity that sends through Relay')
param runtimeSenderPrincipalId string

@description('Whether to create a Relay private endpoint for the ACI listener')
param usePrivateEndpoint bool = true

@description('Subnet resource ID for Relay private endpoint')
param privateEndpointSubnetId string = ''

@description('Resource name prefix')
param namePrefix string

param location string = resourceGroup().location

resource proxyIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: identityName
  location: location
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: containerRegistryName
}

resource relayNamespace 'Microsoft.Relay/namespaces@2024-01-01' = {
  name: relayNamespaceName
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {
    publicNetworkAccess: 'Enabled'
  }
}

resource hybridConnection 'Microsoft.Relay/namespaces/hybridConnections@2024-01-01' = {
  parent: relayNamespace
  name: hybridConnectionName
  properties: {
    requiresClientAuthorization: true
    userMetadata: 'SqlBuildManager private Blob Storage upload proxy'
  }
}

resource storageBlobDataContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: storageAccount
  name: guid(storageAccount.id, proxyIdentity.id, 'Storage Blob Data Contributor')
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
    )
    principalId: proxyIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource acrPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: containerRegistry
  name: guid(containerRegistry.id, proxyIdentity.id, 'AcrPull')
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '7f951dda-4ed3-4680-a7ca-43fe172d538d'
    )
    principalId: proxyIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource relayListener 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: hybridConnection
  name: guid(hybridConnection.id, proxyIdentity.id, 'Azure Relay Listener')
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '26e0b698-aa6d-4085-9386-aadae190014d'
    )
    principalId: proxyIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource relaySender 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(senderPrincipalId)) {
  scope: relayNamespace
  name: guid(relayNamespace.id, senderPrincipalId, 'Azure Relay Sender')
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '26baccc8-eea7-41f1-98f4-1762cc7f685d'
    )
    principalId: senderPrincipalId
  }
}

resource runtimeRelaySender 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: relayNamespace
  name: guid(relayNamespace.id, runtimeSenderPrincipalId, 'Azure Relay Sender')
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '26baccc8-eea7-41f1-98f4-1762cc7f685d'
    )
    principalId: runtimeSenderPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource relayPrivateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' existing = if (usePrivateEndpoint) {
  name: 'privatelink.servicebus.windows.net'
}

resource relayPrivateEndpoint 'Microsoft.Network/privateEndpoints@2023-05-01' = if (usePrivateEndpoint) {
  name: '${namePrefix}relay-pe'
  location: location
  properties: {
    subnet: {
      id: privateEndpointSubnetId
    }
    privateLinkServiceConnections: [
      {
        name: '${namePrefix}relay-plsc'
        properties: {
          privateLinkServiceId: relayNamespace.id
          groupIds: [
            'namespace'
          ]
        }
      }
    ]
  }
}

resource relayPrivateEndpointDnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = if (usePrivateEndpoint) {
  parent: relayPrivateEndpoint
  name: 'default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink-relay'
        properties: {
          privateDnsZoneId: relayPrivateDnsZone.id
        }
      }
    ]
  }
}

output relayNamespaceName string = relayNamespace.name
output relayNamespaceFqdn string = '${relayNamespace.name}.servicebus.windows.net'
output hybridConnectionName string = hybridConnection.name
output endpoint string = 'https://${relayNamespace.name}.servicebus.windows.net/${hybridConnection.name}'
output identityName string = proxyIdentity.name
output identityId string = proxyIdentity.id
output identityClientId string = proxyIdentity.properties.clientId
output identityPrincipalId string = proxyIdentity.properties.principalId
