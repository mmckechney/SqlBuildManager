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

@description('Whether to use private endpoints instead of public network access')
param usePrivateEndpoint bool = false

@description('VNet ID for private DNS zone link (required when usePrivateEndpoint is true)')
param vnetId string = ''

@description('Private endpoint subnet ID (required when usePrivateEndpoint is true)')
param privateEndpointSubnetId string = ''

@description('Name prefix for private endpoint resources')
param namePrefix string = ''

@description('Current machine IP address to allow access')
param currentIpAddress string = ''

@description('Comma-separated list of subnet names to allow access')
param subnetNames string = ''

@description('VNet name for subnet references')
param vnetName string = ''

var subnetNamesArray = empty(subnetNames) ? [] : split(subnetNames, ',')

// Build virtual network rules array from subnet names
var virtualNetworkRules = [for subnet in subnetNamesArray: {
  subnet: {
    id: resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, subnet)
  }
  ignoreMissingVnetServiceEndpoint: false
}]

// Build IP rules array (only if currentIpAddress is provided)
var ipRules = empty(currentIpAddress) ? [] : [
  {
    ipMask: currentIpAddress
    action: 'Allow'
  }
]

// Determine if we should use network restrictions
var useNetworkRestrictions = !empty(subnetNames) || !empty(currentIpAddress)

resource eventHubNamespace 'Microsoft.EventHub/namespaces@2022-10-01-preview' = {
  name: eventHubNamespaceName
  location: location
  sku: {
    name: eventhubSku
    tier: eventhubSku
    capacity: skuCapacity
  }
  properties: {
    publicNetworkAccess: usePrivateEndpoint ? 'Disabled' : 'Enabled'
  }
}

// Network rule set for Event Hub namespace
resource eventHubNetworkRuleSet 'Microsoft.EventHub/namespaces/networkRuleSets@2022-10-01-preview' = if(useNetworkRestrictions) {
  parent: eventHubNamespace
  name: 'default'
  properties: {
    publicNetworkAccess: usePrivateEndpoint ? 'Disabled' : 'Enabled'
    defaultAction: useNetworkRestrictions ? 'Deny' : 'Allow'
    virtualNetworkRules: virtualNetworkRules
    ipRules: ipRules
    trustedServiceAccessEnabled: true
  }
}

resource eventHub 'Microsoft.EventHub/namespaces/eventhubs@2022-10-01-preview' = {
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

// Private DNS Zone for Event Hub - only when using private endpoints
resource privateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = if(usePrivateEndpoint) {
  name: 'privatelink.servicebus.windows.net'
  location: 'global'
}

// Link private DNS zone to VNet
resource privateDnsZoneLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = if(usePrivateEndpoint) {
  parent: privateDnsZone
  name: '${namePrefix}-eventhub-vnet-link'
  location: 'global'
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: vnetId
    }
  }
}

// Private Endpoint for Event Hub
resource privateEndpoint 'Microsoft.Network/privateEndpoints@2023-05-01' = if(usePrivateEndpoint) {
  name: '${namePrefix}eventhub-pe'
  location: location
  properties: {
    subnet: {
      id: privateEndpointSubnetId
    }
    privateLinkServiceConnections: [
      {
        name: '${namePrefix}eventhub-plsc'
        properties: {
          privateLinkServiceId: eventHubNamespace.id
          groupIds: [
            'namespace'
          ]
        }
      }
    ]
  }
}

// DNS Zone Group for Event Hub private endpoint
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

output namespaceId string = eventHubNamespace.id
output eventHubId string = eventHub.id
output namespaceName string = eventHubNamespace.name
output eventHubName string = eventHub.name
