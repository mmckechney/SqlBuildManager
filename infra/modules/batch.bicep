@description('Name of Batch Account to create')
param batchAccountName string

@description('Name of user-assigned managed identity to assign')
param identityName string
param storageIdentityName string
param orchestratorPrincipalId string

@description('Name of storage account associated with the Batch Account')
param storageAccountName string

@description('Location for all resources.')
param location string = resourceGroup().location

// Reference to the user-assigned identity
var userAssignedIdentityId = '/subscriptions/${subscription().subscriptionId}/resourceGroups/${resourceGroup().name}/providers/Microsoft.ManagedIdentity/userAssignedIdentities/${identityName}'

resource storageIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: storageIdentityName
  location: location
}
resource storage 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
}
resource accountStorageAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage.id, storageIdentity.id, 'StorageBlobDataContributor')
  scope: storage
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
    principalId: storageIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}
resource batchAccountResource 'Microsoft.Batch/batchAccounts@2024-07-01' = {
  name: batchAccountName
  location: location
  identity: {
    type: 'UserAssigned'

    userAssignedIdentities: {
      '${storageIdentity.id}': {}
    }
  }
  properties: {
    autoStorage: {
      storageAccountId: resourceId('Microsoft.Storage/storageAccounts',storageAccountName)
      authenticationMode: 'BatchAccountManagedIdentity'
      nodeIdentityReference: {
        // This is the POOL identity, not the account-side autoStorage identity.
        resourceId: userAssignedIdentityId
      }
    }
    poolAllocationMode: 'BatchService'
  }
  dependsOn: [accountStorageAccess]
}

resource orchestratorAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(batchAccountResource.id, orchestratorPrincipalId, 'AzureBatchDataContributor')
  scope: batchAccountResource
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '6aaa78f1-f7de-44ca-8722-c64a23943cae')
    principalId: orchestratorPrincipalId
    principalType: 'ServicePrincipal'
  }
}
output name string = batchAccountResource.name
output id string = batchAccountResource.id
