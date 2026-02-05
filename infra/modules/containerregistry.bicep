
param containerRegistryName string
param location string = resourceGroup().location

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' = {
  name: containerRegistryName
  location: location
  sku: {
    name: 'Standard'
  }
  properties: {
    adminUserEnabled: true
    anonymousPullEnabled: false
  }
}

output name string = containerRegistry.name
output id string = containerRegistry.id
output loginServer string = containerRegistry.properties.loginServer
