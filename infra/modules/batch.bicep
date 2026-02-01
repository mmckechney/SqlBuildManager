@description('Name of Batch Account to Create')
param namePrefix string 

@description('Name of Batch Account to Create. Default is <prefixName>batchacct')
param batchAccountName string = '${namePrefix}batchacct'

@description('Name of user assigned mangaged identy to assign. Default is <prefixName>identity')
param identityName string = '${namePrefix}identity'

@description('Name of storage account to use with Batch Account. Default is <prefixName>storage')
param storageAccountName string = '${namePrefix}storage'

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
