param containerAppEnvName string
param logAnalyticsClientId string
param logAnalyticsKey string
param location string = resourceGroup().location
param subnetId string = ''

resource containerAppEnvWithSubnet 'Microsoft.App/managedEnvironments@2022-11-01-preview' =  if(subnetId != '') {
  name: containerAppEnvName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsClientId
        sharedKey: logAnalyticsKey
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
        sharedKey: logAnalyticsKey
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


