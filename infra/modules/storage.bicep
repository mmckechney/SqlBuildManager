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

output name string = storageAccount.name
output id string = storageAccount.id
