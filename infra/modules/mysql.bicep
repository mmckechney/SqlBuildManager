@description('Azure Developer CLI environment name used in child resource names')
param envName string

param mySqlServerNameA string
param mySqlServerNameB string
param mySqlAdminUser string

@secure()
@description('Administrator password for MySQL Flexible Server')
param mySqlAdminPassword string

@description('Object ID (GUID) of the managed identity used as the MySQL Entra administrator')
param postProvisionAdminObjectId string

@description('Login name of the managed identity used as the MySQL Entra administrator')
param postProvisionAdminName string

@description('Resource ID of the user-assigned identity used by MySQL Entra authentication')
param postProvisionIdentityResourceId string

@description('Number of test databases to create per server')
param testDbCountPerServer int = 10

@description('Location for all resources.')
param location string = resourceGroup().location

@description('VNet ID for the private DNS zone link')
param vnetId string

@description('Private endpoint subnet ID')
param privateEndpointSubnetId string

var prefixes = loadJsonContent('../resourcetypes.json')

resource mySqlFlexServerA 'Microsoft.DBforMySQL/flexibleServers@2023-12-30' = {
  name: mySqlServerNameA
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${postProvisionIdentityResourceId}': {}
    }
  }
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '8.0.21'
    administratorLogin: mySqlAdminUser
    administratorLoginPassword: mySqlAdminPassword
    storage: {
      storageSizeGB: 20
      iops: 360
      autoGrow: 'Disabled'
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    network: {
      publicNetworkAccess: 'Disabled'
    }
  }
}

resource mySqlAadAdminA 'Microsoft.DBforMySQL/flexibleServers/administrators@2023-12-30' = {
  parent: mySqlFlexServerA
  name: 'ActiveDirectory'
  properties: {
    administratorType: 'ActiveDirectory'
    identityResourceId: postProvisionIdentityResourceId
    login: postProvisionAdminName
    sid: postProvisionAdminObjectId
    tenantId: subscription().tenantId
  }
}

@batchSize(1)
resource mySqlDatabasesA 'Microsoft.DBforMySQL/flexibleServers/databases@2023-12-30' = [for i in range(1, testDbCountPerServer): {
  parent: mySqlFlexServerA
  name: 'sbm_mysql_test${i}'
  dependsOn: [
    mySqlAadAdminA
  ]
  properties: {
    charset: 'utf8mb4'
    collation: 'utf8mb4_0900_ai_ci'
  }
}]

resource mySqlFlexServerB 'Microsoft.DBforMySQL/flexibleServers@2023-12-30' = {
  name: mySqlServerNameB
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${postProvisionIdentityResourceId}': {}
    }
  }
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '8.0.21'
    administratorLogin: mySqlAdminUser
    administratorLoginPassword: mySqlAdminPassword
    storage: {
      storageSizeGB: 20
      iops: 360
      autoGrow: 'Disabled'
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    network: {
      publicNetworkAccess: 'Disabled'
    }
  }
}

resource mySqlAadAdminB 'Microsoft.DBforMySQL/flexibleServers/administrators@2023-12-30' = {
  parent: mySqlFlexServerB
  name: 'ActiveDirectory'
  properties: {
    administratorType: 'ActiveDirectory'
    identityResourceId: postProvisionIdentityResourceId
    login: postProvisionAdminName
    sid: postProvisionAdminObjectId
    tenantId: subscription().tenantId
  }
}

@batchSize(1)
resource mySqlDatabasesB 'Microsoft.DBforMySQL/flexibleServers/databases@2023-12-30' = [for i in range(1, testDbCountPerServer): {
  parent: mySqlFlexServerB
  name: 'sbm_mysql_test${i}'
  dependsOn: [
    mySqlAadAdminB
  ]
  properties: {
    charset: 'utf8mb4'
    collation: 'utf8mb4_0900_ai_ci'
  }
}]

var mySqlPrivateDnsZoneName = 'privatelink.mysql.database.azure.com'

resource mySqlPrivateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: mySqlPrivateDnsZoneName
  location: 'global'
}

resource mySqlPrivateDnsZoneLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = {
  parent: mySqlPrivateDnsZone
  name: '${prefixes.privateDnsZoneVirtualNetworkLink}${envName}-mysql'
  location: 'global'
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: vnetId
    }
  }
}

resource mySqlPrivateEndpointA 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: '${prefixes.privateEndpoint}${envName}-mysql-a'
  location: location
  properties: {
    subnet: {
      id: privateEndpointSubnetId
    }
    privateLinkServiceConnections: [
      {
        name: '${prefixes.privateLink}${envName}-mysql-a'
        properties: {
          privateLinkServiceId: mySqlFlexServerA.id
          groupIds: [
            'mysqlServer'
          ]
        }
      }
    ]
  }
}

resource mySqlPrivateEndpointADnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  parent: mySqlPrivateEndpointA
  name: 'default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink-mysql-database-azure-com'
        properties: {
          privateDnsZoneId: mySqlPrivateDnsZone.id
        }
      }
    ]
  }
}

resource mySqlPrivateEndpointB 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: '${prefixes.privateEndpoint}${envName}-mysql-b'
  location: location
  properties: {
    subnet: {
      id: privateEndpointSubnetId
    }
    privateLinkServiceConnections: [
      {
        name: '${prefixes.privateLink}${envName}-mysql-b'
        properties: {
          privateLinkServiceId: mySqlFlexServerB.id
          groupIds: [
            'mysqlServer'
          ]
        }
      }
    ]
  }
}

resource mySqlPrivateEndpointBDnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  parent: mySqlPrivateEndpointB
  name: 'default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink-mysql-database-azure-com'
        properties: {
          privateDnsZoneId: mySqlPrivateDnsZone.id
        }
      }
    ]
  }
}

output mySqlServerNameA string = mySqlFlexServerA.name
output mySqlServerFqdnA string = mySqlFlexServerA.properties.fullyQualifiedDomainName
output mySqlServerNameB string = mySqlFlexServerB.name
output mySqlServerFqdnB string = mySqlFlexServerB.properties.fullyQualifiedDomainName
output mySqlAdminUser string = mySqlAdminUser
output mySqlDatabaseCountPerServer int = testDbCountPerServer
