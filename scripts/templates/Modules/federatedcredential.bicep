param federatedIdName string
param serviceAccountName string
param issuerUrl string
param identityName string



resource federatedCredential 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2022-01-31-preview' = {
  name: federatedIdName
  properties: {
    issuer: issuerUrl
    subject: 'system:serviceaccount:sqlbuildmanager:${serviceAccountName}'
    audiences: [
      'api://AzureADTokenExchange'
    ]
  }
}
