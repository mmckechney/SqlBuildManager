param nsgName string
param nsgBatchName string
param vnetName string
param aksSubnetName string
param containerAppSubnetName string
param aciSubnetName string
param batchSubnetName string
param privateEndpointSubnetName string

@description('Name of the location. Default is the resource group location')
param location string = resourceGroup().location

@description('IP range for the VNet. Default is 10.180.0.0/19')
param vnetIpRange string = '10.180.0.0/19'

@description('IP range for the AKS subnet. Default is 10.180.0.0/22')
param aksSubnetIpRange  string = '10.180.0.0/22'

@description('IP range for the container app subnet. Default is 10.180.4.0/22')
param containerAppSubnetIpRange  string = '10.180.4.0/22'

@description('IP range for the ACI subnet. Default is 10.180.8.0/22')
param aciSubnetIpRange  string = '10.180.8.0/22'

@description('IP range for the Batch subnet. Default is 10.180.12.0/22')
param batchSubnetIpRange  string = '10.180.12.0/22'

@description('IP range for the private endpoint subnet. Default is 10.180.16.0/24')
param privateEndpointSubnetIpRange string = '10.180.16.0/24'

resource nsg_resource 'Microsoft.Network/networkSecurityGroups@2021-02-01' = {
  name: nsgName
  location: location
  properties: {
    securityRules: []
  }
}

resource nsgBatchResource 'Microsoft.Network/networkSecurityGroups@2021-02-01' = {
  name: nsgBatchName
  location: location
  properties: {
    securityRules: [
      {
        name: 'BatchServiceRule'
        properties: {
          priority: 120
          access: 'Allow'
          direction: 'Inbound'
          sourceAddressPrefix: 'BatchNodeManagement'
          sourcePortRange: '*'
          destinationAddressPrefix: '*'
          destinationPortRange: '29876-29877'
          protocol: '*'
        }
      }
    ]
  }
}

resource virtualNetworkResource 'Microsoft.Network/virtualNetworks@2021-02-01' = {
  name: vnetName
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        vnetIpRange
      ]
    }
    subnets: [
      {
        name:  aksSubnetName
        properties: {
          addressPrefix: aksSubnetIpRange
          networkSecurityGroup: {
            id: nsg_resource.id
          }
          serviceEndpoints: [
            {
              service: 'Microsoft.Sql'
            }
            {
              service: 'Microsoft.KeyVault'
            }
            {
              service: 'Microsoft.Storage'
            }
            {
              service: 'Microsoft.EventHub'
            }
            {
              service: 'Microsoft.ServiceBus'
            }
          ]
        }
      }
      {
        name: containerAppSubnetName
        properties: {
          addressPrefix: containerAppSubnetIpRange
          networkSecurityGroup: {
            id: nsg_resource.id
          }
          serviceEndpoints: [
            {
              service: 'Microsoft.Sql'
            }
            {
              service: 'Microsoft.KeyVault'
            }
            {
              service: 'Microsoft.Storage'
            }
            {
              service: 'Microsoft.EventHub'
            }
            {
              service: 'Microsoft.ServiceBus'
            }
          ]
          delegations: [
            {
              name: 'Microsoft.App/environments'
              properties: {
                serviceName: 'Microsoft.App/environments'
              }
            }
          ]
        }
      }
      {
        name: aciSubnetName
        properties: {
          addressPrefix: aciSubnetIpRange
          networkSecurityGroup: {
            id: nsg_resource.id
          }
          serviceEndpoints: [
            {
              service: 'Microsoft.Sql'
            }
            {
              service: 'Microsoft.KeyVault'
            }
            {
              service: 'Microsoft.Storage'
            }
            {
              service: 'Microsoft.EventHub'
            }
            {
              service: 'Microsoft.ServiceBus'
            }
          ]
          delegations: [
            {
              name: 'Microsoft.ContainerInstance/containerGroups'
              properties: {
                serviceName: 'Microsoft.ContainerInstance/containerGroups'
              }
            }
          ]
        }
        
      }
      {
        name: batchSubnetName
        properties: {
          addressPrefix: batchSubnetIpRange
          networkSecurityGroup: {
            id: nsgBatchResource.id
          }
          serviceEndpoints: [
            {
              service: 'Microsoft.Sql'
            }
            {
              service: 'Microsoft.KeyVault'
            }
            {
              service: 'Microsoft.Storage'
            }
            {
              service: 'Microsoft.EventHub'
            }
            {
              service: 'Microsoft.ServiceBus'
            }
          ]
        }
      }
      {
        name: privateEndpointSubnetName
        properties: {
          addressPrefix: privateEndpointSubnetIpRange
          networkSecurityGroup: {
            id: nsg_resource.id
          }
          privateEndpointNetworkPolicies: 'Disabled'
        }
      }
    ]
  }
}

output nsgName string = nsg_resource.name
output vnetName string = virtualNetworkResource.name
output aksSubnetName string = aksSubnetName
output containerAppSubnetName string = containerAppSubnetName
output aciSubnetName string = aciSubnetName
output batchSubnetName string = batchSubnetName
output privateEndpointSubnetName string = privateEndpointSubnetName

output subnetNames array = [
  aksSubnetName
  containerAppSubnetName
  aciSubnetName
  batchSubnetName
]

output aksSubnetId string = virtualNetworkResource.properties.subnets[0].id
output containerAppSubnetId string = virtualNetworkResource.properties.subnets[1].id
output aciSubnetId string = virtualNetworkResource.properties.subnets[2].id
output batchSubnetId string = virtualNetworkResource.properties.subnets[3].id
output privateEndpointSubnetId string = virtualNetworkResource.properties.subnets[4].id

output nsgId string = nsg_resource.id
output vnetId string = virtualNetworkResource.id


