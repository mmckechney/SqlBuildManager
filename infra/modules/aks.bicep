param aksClusterName string
param location string = resourceGroup().location
param identityName string
param vnetName string
param subnetName string
param logAnalyticsWorkspaceName string
param serviceAccountName string
param federatedIdName string
param controlPlaneIdentityName string
param kubeletIdentityName string
param containerRegistryName string
param orchestratorPrincipalId string
param operatorPrincipalId string

resource identityResource 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name : identityName
}

resource controlPlaneIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: controlPlaneIdentityName
  location: location
}
resource kubeletIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: kubeletIdentityName
  location: location
}
resource subnet 'Microsoft.Network/virtualNetworks/subnets@2023-05-01' existing = {
  name: '${vnetName}/${subnetName}'
}
resource networkAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(subnet.id, controlPlaneIdentity.id, 'NetworkContributor')
  scope: subnet
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4d97b98b-1d4f-4787-a291-c67834d212e7')
    principalId: controlPlaneIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}
resource kubeletAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(kubeletIdentity.id, controlPlaneIdentity.id, 'ManagedIdentityOperator')
  scope: kubeletIdentity
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'f1a07417-d97a-45cb-824c-7a7467783830')
    principalId: controlPlaneIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}
resource registry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: containerRegistryName
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

resource aksAcrPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(registry.id, kubeletIdentity.id, 'AcrPull')
  scope: registry
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
    principalId: kubeletIdentity.properties.principalId
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
      '${controlPlaneIdentity.id}': {}
    }
  }
  properties: {
    identityProfile: {
      kubeletidentity: {
        resourceId: kubeletIdentity.id
        clientId: kubeletIdentity.properties.clientId
        objectId: kubeletIdentity.properties.principalId
      }
    }
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
      minCount: 3
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
  dependsOn: [networkAccess, kubeletAssignment, aksAcrPull]
}

resource orchestratorClusterUser 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(aks.id, orchestratorPrincipalId, 'ClusterUser')
  scope: aks
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4abbcc35-e782-43d8-92c5-2d3f1bd2253f')
    principalId: orchestratorPrincipalId
    principalType: 'ServicePrincipal'
  }
}
// AKS uses this virtual ARM scope for namespace-scoped Azure RBAC.
#disable-next-line BCP081
resource workloadNamespace 'Microsoft.ContainerService/managedClusters/namespaces@2023-05-01' existing = {
  parent: aks
  name: 'sqlbuildmanager'
}
resource orchestratorNamespaceWriter 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(aks.id, 'sqlbuildmanager', orchestratorPrincipalId, 'RbacWriter')
  scope: workloadNamespace
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'a7ffa36f-339b-4b5c-8bdf-e2c188b2c0eb')
    principalId: orchestratorPrincipalId
    principalType: 'ServicePrincipal'
  }
}
resource operatorAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for role in [
  '4abbcc35-e782-43d8-92c5-2d3f1bd2253f'
  'b1ff04bb-8a4e-4dc4-8eb5-8693973ce19b'
]: if (operatorPrincipalId != '') {
  name: guid(aks.id, operatorPrincipalId, role)
  scope: aks
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', role)
    principalId: operatorPrincipalId
    principalType: 'User'
  }
}]
output clusterName string = aks.name
output clusterId string = aks.id
output federatedIdName string = federatedCredential.name
output serviceAccountName string = serviceAccountName
