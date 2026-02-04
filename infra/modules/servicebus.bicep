@description('Name of the Service Bus namespace')
param serviceBusNamespaceName string

@description('Location for the Service Bus')
param location string = resourceGroup().location

@allowed([
  'Basic'
  'Standard'
  'Premium'
])
@description('The messaging tier for Service Bus namespace')
param serviceBusSku string = 'Standard'

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

// Private endpoints only supported on Premium SKU
var canUsePrivateEndpoint = usePrivateEndpoint && serviceBusSku == 'Premium'

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

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: serviceBusNamespaceName
  location: location
  sku: {
    name: serviceBusSku
    tier: serviceBusSku
    capacity: 1
  }
  properties: {
    zoneRedundant: false
    // Only disable public network access if we can actually use private endpoints (Premium SKU)
    publicNetworkAccess: canUsePrivateEndpoint ? 'Disabled' : 'Enabled'
  }
}

// Network rule set for Service Bus namespace (Premium SKU only)
resource serviceBusNetworkRuleSet 'Microsoft.ServiceBus/namespaces/networkRuleSets@2022-10-01-preview' = if(useNetworkRestrictions && serviceBusSku == 'Premium') {
  parent: serviceBusNamespace
  name: 'default'
  properties: {
    publicNetworkAccess: canUsePrivateEndpoint ? 'Disabled' : 'Enabled'
    defaultAction: useNetworkRestrictions ? 'Deny' : 'Allow'
    virtualNetworkRules: virtualNetworkRules
    ipRules: ipRules
    trustedServiceAccessEnabled: true
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

// Private DNS Zone for Service Bus - only when using private endpoints with Premium SKU
resource privateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = if(canUsePrivateEndpoint) {
  name: 'privatelink.servicebus.windows.net'
  location: 'global'
}

// Link private DNS zone to VNet
resource privateDnsZoneLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = if(canUsePrivateEndpoint) {
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

// Private Endpoint for Service Bus (Premium SKU only)
resource privateEndpoint 'Microsoft.Network/privateEndpoints@2023-05-01' = if(canUsePrivateEndpoint) {
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
resource privateEndpointDnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = if(canUsePrivateEndpoint) {
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
