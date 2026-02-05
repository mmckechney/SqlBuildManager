param keyvaultName string
param currentIpAddress string
param identityClientId string
param subNet1Id string
param subNet2Id string
param subNet3Id string
param subNet4Id string
param location string = resourceGroup().location

resource keyVaultResource 'Microsoft.KeyVault/vaults@2019-09-01' = {
  name: keyvaultName
  location: location
  properties: {
    enabledForTemplateDeployment: true
    enableRbacAuthorization: true
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }  
    accessPolicies:[
      {
        tenantId: subscription().tenantId
        objectId:identityClientId
        permissions:  {
          secrets:[
            'set'
            'list'
            'get'
          ]
        }
        
      }
    ]
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Deny'
      ipRules: [
        {
          value: currentIpAddress
        }
      ]
      virtualNetworkRules: [
        {
          id: subNet1Id
          ignoreMissingVnetServiceEndpoint: false
        }
        {
          id: subNet2Id
          ignoreMissingVnetServiceEndpoint: false
        }
        {
          id: subNet3Id
          ignoreMissingVnetServiceEndpoint: false

        }
        {
          id: subNet4Id
          ignoreMissingVnetServiceEndpoint: false
        }

      ]
    }
  }
}

output keyVaultResourceId string = keyVaultResource.id
output keyVaultUrl string = keyVaultResource.properties.vaultUri
output keyVaultName string = keyVaultResource.name

