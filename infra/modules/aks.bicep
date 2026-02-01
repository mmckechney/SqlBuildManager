param aksClusterName string
param location string = resourceGroup().location
param identityName string
param vnetName string
param subnetName string
param logAnalyticsWorkspaceName string
param serviceAccountName string
param federatedIdName string


resource identityResource 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name : identityName
  location : location
}

resource federatedCredential 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2022-01-31-preview' = {
  parent: identityResource
  name: federatedIdName
  properties: {
    issuer: aks.properties.oidcIssuerProfile.issuerURL
    subject: 'system:serviceaccount:sqlbuildmanager:${serviceAccountName}'
    audiences: [
      'api://AzureADTokenExchange'
    ]
  }
}

resource aksAcrPull 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid('aksAcrPull', identityName)
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
    principalId: aks.properties.identityProfile.kubeletidentity.objectId
    principalType: 'ServicePrincipal'

  }
}

resource aks 'Microsoft.ContainerService/managedClusters@2023-05-01' = {
  name: aksClusterName
  location: location
  sku:{
    name: 'Base'
    tier: 'Free'
  }
  identity: {
    type:  'UserAssigned' 
    userAssignedIdentities: {
      '${identityResource.id}': {}
    }
  }
  properties: {
    enableRBAC: true
    dnsPrefix: aksClusterName
    addonProfiles: {
      azurepolicy: {
        enabled: true
      }
     omsagent: {
        enabled: true
        config: {
          logAnalyticsWorkspaceResourceID: resourceId('Microsoft.OperationalInsights/workspaces', logAnalyticsWorkspaceName)
        }
      }
    }
    agentPoolProfiles: [{
      name: 'agentpool'
      vmSize: 'Standard_D2s_v3'
      count: 1
      mode: 'System'
      enableAutoScaling: true
      minCount: 1
      maxCount: 5
      enableNodePublicIP: false
      osType: 'Linux' 
      osSKU: 'Ubuntu'
      vnetSubnetID: resourceId('Microsoft.Network/virtualNetworks/subnets',vnetName,subnetName)
      type: 'VirtualMachineScaleSets'
    }]
    networkProfile: {
      networkPlugin: 'azure'
      loadBalancerSku: 'standard'
      networkPolicy: 'azure'
    }
    aadProfile:{
      managed: true
      enableAzureRBAC: true
      tenantID: subscription().tenantId
    }
    autoUpgradeProfile:{
      upgradeChannel: 'patch'
    }
    securityProfile:{
      workloadIdentity:{
        enabled: true
      }
    }
    oidcIssuerProfile:{
        enabled: true
    }
  }
}


