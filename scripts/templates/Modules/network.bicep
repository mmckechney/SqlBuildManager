@description('Prefix to prepend to resource names.  Should be 3-6 characters long.  Should not contain any special characters.  Should be unique across all deployments.')
param namePrefix string = ''

@description('Name of the NSG to create. Default is <namePrefix>nsg')
param nsgName string ='${namePrefix}nsg'

@description('Name of the Batch NSG to create. Default is <namePrefix>Batchnsg')
param nsgBatchName string ='${namePrefix}Batchnsg'

@description('Name of the VNet to create. Default is <namePrefix>vnet')
param vnetName string  = '${namePrefix}vnet'

@description('Name of the AKS subnet to create. Default is <namePrefix>akssubnet')
param aksSubnetName string = '${namePrefix}akssubnet'

@description('Name of the container app subnet to create. Default is <namePrefix>containerappsubnet')
param containerAppSubnetName string = '${namePrefix}containerappsubnet'

@description('Name of the ACI subnet to create. Default is <namePrefix>acisubnet')
param aciSubnetName string = '${namePrefix}acisubnet'

@description('Name of the Batch subnet to create. Default is <namePrefix>batchsubnet')
param batchSubnetName string = '${namePrefix}batchsubnet'

@description('Name of the location. Default is the resource group location')
param location string = resourceGroup().location

@description('IP range for the VNet. Default is 10.180.0.0/20')
param vnetIpRange string = '10.180.0.0/20'

@description('IP range for the AKS subnet. Default is 10.180.0.0/22')
param aksSubnetIpRange  string = '10.180.0.0/22'

@description('IP range for the container app subnet. Default is 10.180.4.0/22')
param containerAppSubnetIpRange  string = '10.180.4.0/22'

@description('IP range for the ACI subnet. Default is 10.180.8.0/22')
param aciSubnetIpRange  string = '10.180.8.0/22'

@description('IP range for the Batch subnet. Default is 10.180.12.0/22')
param batchSubnetIpRange  string = '10.180.12.0/22'

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
          ]
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

output nsgId string = nsg_resource.id
output vnetId string = virtualNetworkResource.id



