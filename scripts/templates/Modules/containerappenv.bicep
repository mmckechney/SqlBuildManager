param containerAppEnvName string
param logAnalyticsClientId string
param logAnalyticsWorkspaceName string
param location string = resourceGroup().location
param subnetId string = ''


resource logAnalyticsWorkspaceResource 'Microsoft.OperationalInsights/workspaces@2021-06-01' existing = {
  name:logAnalyticsWorkspaceName

}

resource containerAppEnvWithSubnet 'Microsoft.App/managedEnvironments@2022-11-01-preview' =  if(subnetId != '') {
  name: containerAppEnvName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsClientId
        sharedKey: logAnalyticsWorkspaceResource.listkeys().primarySharedKey

      }
    }
    workloadProfiles: [
      {
        name: 'Consumption'
        workloadProfileType: 'Consumption'
      }
    ]
    vnetConfiguration: {
      infrastructureSubnetId: subnetId
    }
  }
}

resource containerAppEnvNoSubnet 'Microsoft.App/managedEnvironments@2022-11-01-preview' =  if(subnetId == '') {
  name: containerAppEnvName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsClientId
        sharedKey: logAnalyticsWorkspaceResource.listkeys().primarySharedKey
      }
    }
    workloadProfiles: [
      {
        name: 'Consumption'
        workloadProfileType: 'Consumption'
      }
    ]
  }
}


