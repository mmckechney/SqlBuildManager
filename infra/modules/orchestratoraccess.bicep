param orchestratorPrincipalId string
param workerIdentityName string
param deployContainerAppEnv bool
param containerAppEnvName string
param vnetName string
param subnetNames array

// Dynamic job names require RG scope, but only these compute lifecycle actions.
resource computeRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' = {
  name: guid(resourceGroup().id, 'SbmComputeOrchestrator')
  properties: {
    roleName: '${resourceGroup().name}-compute-orchestrator'
    type: 'CustomRole'
    assignableScopes: [resourceGroup().id]
    permissions: [{
      actions: concat([
        'Microsoft.Resources/subscriptions/resourceGroups/read'
        'Microsoft.Resources/subscriptions/resourceGroups/resources/read'
        'Microsoft.ContainerInstance/containerGroups/read'
        'Microsoft.ContainerInstance/containerGroups/write'
        'Microsoft.ContainerInstance/containerGroups/delete'
        'Microsoft.ContainerInstance/containerGroups/containers/logs/read'
        // The ACI runtime creates and removes a legacy network profile per job.
        'Microsoft.Network/networkProfiles/read'
        'Microsoft.Network/networkProfiles/write'
        'Microsoft.Network/networkProfiles/delete'
      ], deployContainerAppEnv ? [
        'Microsoft.App/containerApps/read'
        'Microsoft.App/containerApps/write'
        'Microsoft.App/containerApps/delete'
        'Microsoft.App/containerApps/revisions/read'
        'Microsoft.App/containerApps/revisions/replicas/read'
      ] : [])
      notActions: []
      dataActions: deployContainerAppEnv ? ['Microsoft.App/containerApps/logstream/action'] : []
      notDataActions: []
    }]
  }
}
resource computeAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, orchestratorPrincipalId, computeRole.name)
  properties: {
    roleDefinitionId: computeRole.id
    principalId: orchestratorPrincipalId
    principalType: 'ServicePrincipal'
  }
}
resource worker 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: workerIdentityName
}
resource workerAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(worker.id, orchestratorPrincipalId, 'ManagedIdentityOperator')
  scope: worker
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'f1a07417-d97a-45cb-824c-7a7467783830')
    principalId: orchestratorPrincipalId
    principalType: 'ServicePrincipal'
  }
}
resource subnetRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' = {
  name: guid(resourceGroup().id, 'SbmSubnetJoin')
  properties: {
    roleName: '${resourceGroup().name}-subnet-join'
    type: 'CustomRole'
    assignableScopes: [resourceGroup().id]
    permissions: [{
      actions: [
        'Microsoft.Network/virtualNetworks/subnets/read'
        'Microsoft.Network/virtualNetworks/subnets/join/action'
      ]
      notActions: []
      dataActions: []
      notDataActions: []
    }]
  }
}
resource subnets 'Microsoft.Network/virtualNetworks/subnets@2023-05-01' existing = [for name in subnetNames: {
  name: '${vnetName}/${name}'
}]
resource subnetAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for (name, i) in subnetNames: {
  name: guid(subnets[i].id, orchestratorPrincipalId, subnetRole.name)
  scope: subnets[i]
  properties: {
    roleDefinitionId: subnetRole.id
    principalId: orchestratorPrincipalId
    principalType: 'ServicePrincipal'
  }
}]
resource environmentRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' = if (deployContainerAppEnv) {
  name: guid(resourceGroup().id, 'SbmEnvironmentJoin')
  properties: {
    roleName: '${resourceGroup().name}-environment-join'
    type: 'CustomRole'
    assignableScopes: [resourceGroup().id]
    permissions: [{
      actions: [
        'Microsoft.App/managedEnvironments/read'
        'Microsoft.App/managedEnvironments/join/action'
      ]
      notActions: []
      dataActions: []
      notDataActions: []
    }]
  }
}
resource environment 'Microsoft.App/managedEnvironments@2023-05-01' existing = {
  name: containerAppEnvName
}
resource environmentAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (deployContainerAppEnv) {
  name: guid(environment.id, orchestratorPrincipalId, 'SbmEnvironmentJoin')
  scope: environment
  properties: {
    roleDefinitionId: environmentRole!.id
    principalId: orchestratorPrincipalId
    principalType: 'ServicePrincipal'
  }
}
