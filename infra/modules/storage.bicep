@description('Name of the storage account')
param storageAccountName string

@description('Location for the storage account')
param location string = resourceGroup().location

@description('Current machine IP address to allow access to the storage account')
param currentIpAddress string = ''

@description('Comma-separated list of subnet names to allow access to the storage account')
param subnetNames string = ''

@description('VNet name for subnet references')
param vnetName string = ''

@description('Whether to use private endpoints instead of public network access')
param usePrivateEndpoint bool = false

@description('VNet ID for private DNS zone link (required when usePrivateEndpoint is true)')
param vnetId string = ''

@description('Private endpoint subnet ID (required when usePrivateEndpoint is true)')
param privateEndpointSubnetId string = ''

@description('Name prefix for private endpoint resources')
param namePrefix string = ''

var subnetNamesArray = empty(subnetNames) ? [] : split(subnetNames, ',')

// Build virtual network rules array from subnet names
var virtualNetworkRules = [for subnet in subnetNamesArray: {
  id: resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, subnet)
  action: 'Allow'
  state: 'Succeeded'
}]

// Build IP rules array (only if currentIpAddress is provided)
var ipRules = empty(currentIpAddress) ? [] : [
  {
    value: currentIpAddress
    action: 'Allow'
  }
]

// Determine if we should use network restrictions
var useNetworkRestrictions = !empty(subnetNames) || !empty(currentIpAddress)

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    // Keep public access enabled - security is enforced via network rules (defaultAction: Deny)
    // This allows access from allowed IPs and VNet subnets while still supporting private endpoints
    publicNetworkAccess: 'Enabled'
    allowSharedKeyAccess: false
    networkAcls: {
      bypass: 'AzureServices'
      virtualNetworkRules: virtualNetworkRules
      ipRules: ipRules
      defaultAction: useNetworkRestrictions ? 'Deny' : 'Allow'
    }
    supportsHttpsTrafficOnly: true
    encryption: {
      services: {
        file: {
          enabled: true
        }
        blob: {
          enabled: true
        }
      }
      keySource: 'Microsoft.Storage'
    }
    accessTier: 'Hot'
  }
}

// Blob Services for the storage account
resource blobServices 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storageAccount
  name: 'default'
}

// Test results container
resource testResultsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: blobServices
  name: 'testresults'
  properties: {
    publicAccess: 'None'
  }
}

// Private DNS Zone for Storage (blob) - only when using private endpoints
resource privateDnsZoneBlob 'Microsoft.Network/privateDnsZones@2020-06-01' = if(usePrivateEndpoint) {
  name: 'privatelink.blob.${environment().suffixes.storage}'
  location: 'global'
}

// Link private DNS zone to VNet (blob)
resource privateDnsZoneLinkBlob 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = if(usePrivateEndpoint) {
  parent: privateDnsZoneBlob
  name: '${namePrefix}-storage-blob-vnet-link'
  location: 'global'
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: vnetId
    }
  }
}

// Private Endpoint for Storage (blob)
resource privateEndpointBlob 'Microsoft.Network/privateEndpoints@2023-05-01' = if(usePrivateEndpoint) {
  name: '${namePrefix}storage-blob-pe'
  location: location
  properties: {
    subnet: {
      id: privateEndpointSubnetId
    }
    privateLinkServiceConnections: [
      {
        name: '${namePrefix}storage-blob-plsc'
        properties: {
          privateLinkServiceId: storageAccount.id
          groupIds: [
            'blob'
          ]
        }
      }
    ]
  }
}

// DNS Zone Group for Storage blob private endpoint
resource privateEndpointBlobDnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = if(usePrivateEndpoint) {
  parent: privateEndpointBlob
  name: 'default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink-blob-storage'
        properties: {
          privateDnsZoneId: privateDnsZoneBlob.id
        }
      }
    ]
  }
}

// Private DNS Zone for Storage (queue) - only when using private endpoints
resource privateDnsZoneQueue 'Microsoft.Network/privateDnsZones@2020-06-01' = if(usePrivateEndpoint) {
  name: 'privatelink.queue.${environment().suffixes.storage}'
  location: 'global'
}

// Link private DNS zone to VNet (queue)
resource privateDnsZoneLinkQueue 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = if(usePrivateEndpoint) {
  parent: privateDnsZoneQueue
  name: '${namePrefix}-storage-queue-vnet-link'
  location: 'global'
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: vnetId
    }
  }
}

// Private Endpoint for Storage (queue)
resource privateEndpointQueue 'Microsoft.Network/privateEndpoints@2023-05-01' = if(usePrivateEndpoint) {
  name: '${namePrefix}storage-queue-pe'
  location: location
  properties: {
    subnet: {
      id: privateEndpointSubnetId
    }
    privateLinkServiceConnections: [
      {
        name: '${namePrefix}storage-queue-plsc'
        properties: {
          privateLinkServiceId: storageAccount.id
          groupIds: [
            'queue'
          ]
        }
      }
    ]
  }
}

// DNS Zone Group for Storage queue private endpoint
resource privateEndpointQueueDnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = if(usePrivateEndpoint) {
  parent: privateEndpointQueue
  name: 'default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink-queue-storage'
        properties: {
          privateDnsZoneId: privateDnsZoneQueue.id
        }
      }
    ]
  }
}

output name string = storageAccount.name
output id string = storageAccount.id
