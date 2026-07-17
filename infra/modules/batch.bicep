@description('Name of Batch Account to create')
param batchAccountName string

@description('Name of user-assigned managed identity to assign')
param identityName string

@description('Name of storage account associated with the Batch Account')
param storageAccountName string

@description('Location for all resources.')
param location string = resourceGroup().location

// Reference to the user-assigned identity
var userAssignedIdentityId = '/subscriptions/${subscription().subscriptionId}/resourceGroups/${resourceGroup().name}/providers/Microsoft.ManagedIdentity/userAssignedIdentities/${identityName}'

resource batchAccountResource 'Microsoft.Batch/batchAccounts@2024-07-01' = {
  name: batchAccountName
  location: location
  identity: {
    type: 'UserAssigned'

    userAssignedIdentities: {
      '${userAssignedIdentityId}': {}
    }
  }
  properties: {
    autoStorage: {
      storageAccountId: resourceId('Microsoft.Storage/storageAccounts',storageAccountName)
      authenticationMode: 'BatchAccountManagedIdentity'
      nodeIdentityReference: {
        resourceId: userAssignedIdentityId
      }
    }
    poolAllocationMode: 'BatchService'
  }

}

output name string = batchAccountResource.name
output id string = batchAccountResource.id
