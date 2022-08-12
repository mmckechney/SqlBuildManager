param aciName string
param identityName string


resource aci_resource_name 'Microsoft.ContainerInstance/containerGroups@2021-03-01' = {
  name: aciName
  location: resourceGroup().location
  identity:{
    type: 'UserAssigned'
    userAssignedIdentities:{
      '/subscriptions/${subscription().subscriptionId}/resourceGroups/${resourceGroup().name}/providers/Microsoft.ManagedIdentity/userAssignedIdentities/${identityName}' :{}
    }
  }
  properties:{
    containers:[
      {
        name: 'sqlbuildmanager1'
        properties: {
          image: 'ghcr.io/mmckechney/sqlbuildmanager:latest'
          command: [
            'sbm'
            'aci'
            'worker'
          ]
          resources:{
            requests:{
              cpu: 1
              memoryInGB: 2
            }
          }
        }
      }
    ]
    osType: 'Linux'
    restartPolicy: 'Never'
  }
}
