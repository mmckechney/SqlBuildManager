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
