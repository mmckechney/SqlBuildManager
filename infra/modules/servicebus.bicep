@description('Name of the Service Bus namespace')
param serviceBusNamespaceName string

@description('Location for the Service Bus')
param location string = resourceGroup().location

@description('Whether to use private endpoints instead of public network access')
param usePrivateEndpoint bool = false

@description('VNet ID for private DNS zone link (required when usePrivateEndpoint is true)')
param vnetId string = ''

@description('Private endpoint subnet ID (required when usePrivateEndpoint is true)')
param privateEndpointSubnetId string = ''

@description('Name prefix for private endpoint resources')
param namePrefix string = ''

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: serviceBusNamespaceName
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
    capacity: 1
  }
  properties: {
    zoneRedundant: false
    publicNetworkAccess: usePrivateEndpoint ? 'Disabled' : 'Enabled'
  }
}

// Authorization rules removed - using Managed Identity with RBAC for Service Bus access
// The identity.bicep module assigns ServiceBusDataOwner role

resource topic 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = {
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

// Private DNS Zone for Service Bus - only when using private endpoints
resource privateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = if(usePrivateEndpoint) {
  name: 'privatelink.servicebus.windows.net'
  location: 'global'
}

// Link private DNS zone to VNet
resource privateDnsZoneLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = if(usePrivateEndpoint) {
  parent: privateDnsZone
  name: '${namePrefix}-servicebus-vnet-link'
  location: 'global'
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: vnetId
    }
  }
}

// Private Endpoint for Service Bus
resource privateEndpoint 'Microsoft.Network/privateEndpoints@2023-05-01' = if(usePrivateEndpoint) {
  name: '${namePrefix}servicebus-pe'
  location: location
  properties: {
    subnet: {
      id: privateEndpointSubnetId
    }
    privateLinkServiceConnections: [
      {
        name: '${namePrefix}servicebus-plsc'
        properties: {
          privateLinkServiceId: serviceBusNamespace.id
          groupIds: [
            'namespace'
          ]
        }
      }
    ]
  }
}

// DNS Zone Group for Service Bus private endpoint
resource privateEndpointDnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = if(usePrivateEndpoint) {
  parent: privateEndpoint
  name: 'default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink-servicebus-windows-net'
        properties: {
          privateDnsZoneId: privateDnsZone.id
        }
      }
    ]
  }
}

output namespaceId string = serviceBusNamespace.id
output topicId string = topic.id
output namespaceName string = serviceBusNamespace.name
