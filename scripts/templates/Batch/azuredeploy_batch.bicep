@description('Name of Batch Account to Create')
param batchAccountName string 

@description('Name of user assigned mangaged identy to assign')
param identityName string

@description('Name of storage account to use with Batch')
param storageAccountName string

@description('Location for all resources.')
param location string = resourceGroup().location


resource batchAccountResource 'Microsoft.Batch/batchAccounts@2021-01-01' = {
  name: batchAccountName
  location: location
  identity: {
    type: 'UserAssigned'

    userAssignedIdentities: {
      '/subscriptions/${subscription().subscriptionId}/resourceGroups/${resourceGroup().name}/providers/Microsoft.ManagedIdentity/userAssignedIdentities/${identityName}': {
      }
    }
  }
  properties: {
    autoStorage: {
      storageAccountId: resourceId('Microsoft.Storage/storageAccounts',storageAccountName)
    }
    poolAllocationMode: 'BatchService'
  }

}
