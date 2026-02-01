@description('Name of the Log Analytics workspace')
param logAnalyticsWorkspaceName string

@description('Location for the Log Analytics workspace')
param location string = resourceGroup().location

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2021-06-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

output customerId string = logAnalyticsWorkspace.properties.customerId
output id string = logAnalyticsWorkspace.id
output name string = logAnalyticsWorkspace.name
