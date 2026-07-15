@description('Prefix to prepend to account names')
param namePrefix string

@description('Number of test databases to create per server')
param testDbCountPerServer int = 10

@description('Location for all resources.')
param location string = resourceGroup().location

@description('Object ID (GUID) of the Entra ID user or group to set as PG AAD admin')
param pgAdminObjectId string

@description('Login name (email) of the Entra ID admin')
param pgAdminLogin string

@secure()
@description('Administrator password for PostgreSQL (used for local/password-based auth)')
param pgAdminPassword string

@description('Object ID of the managed identity used for private post-provision initialization')
param postProvisionAdminObjectId string

@description('Name of the managed identity used for private post-provision initialization')
param postProvisionAdminName string

@description('VNet ID for the private DNS zone link')
param vnetId string

@description('Private endpoint subnet ID')
param privateEndpointSubnetId string

var pgServerNameA = '${namePrefix}pgserver-a'
var pgServerNameB = '${namePrefix}pgserver-b'
var pgAdminUser = '${namePrefix}pgadmin'
// ============================================================
// PostgreSQL Server 'A'
// ============================================================
resource pgFlexServerA 'Microsoft.DBforPostgreSQL/flexibleServers@2024-08-01' = {
  name: pgServerNameA
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '16'
    administratorLogin: pgAdminUser
    administratorLoginPassword: pgAdminPassword
    storage: {
      storageSizeGB: 32
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
    authConfig: {
      activeDirectoryAuth: 'Enabled'
      passwordAuth: 'Enabled'
      tenantId: subscription().tenantId
    }
  }
}

resource pgAadAdminA 'Microsoft.DBforPostgreSQL/flexibleServers/administrators@2024-08-01' = {
  parent: pgFlexServerA
  name: pgAdminObjectId
  properties: {
    principalType: 'User'
    principalName: pgAdminLogin
    tenantId: subscription().tenantId
  }
}

resource pgPostProvisionAdminA 'Microsoft.DBforPostgreSQL/flexibleServers/administrators@2024-08-01' = {
  parent: pgFlexServerA
  name: postProvisionAdminObjectId
  dependsOn: [
    pgAadAdminA
  ]
  properties: {
    principalType: 'ServicePrincipal'
    principalName: postProvisionAdminName
    tenantId: subscription().tenantId
  }
}

@batchSize(1)
resource pgDatabasesA 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2024-08-01' = [for i in range(1, testDbCountPerServer): {
  parent: pgFlexServerA
  name: 'sbm_pg_test${i}'
  dependsOn: [
    pgAadAdminA
  ]
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}]

// ============================================================
// PostgreSQL Server 'B'
// ============================================================
resource pgFlexServerB 'Microsoft.DBforPostgreSQL/flexibleServers@2024-08-01' = {
  name: pgServerNameB
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '16'
    administratorLogin: pgAdminUser
    administratorLoginPassword: pgAdminPassword
    storage: {
      storageSizeGB: 32
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
    authConfig: {
      activeDirectoryAuth: 'Enabled'
      passwordAuth: 'Enabled'
      tenantId: subscription().tenantId
    }
  }
}

resource pgAadAdminB 'Microsoft.DBforPostgreSQL/flexibleServers/administrators@2024-08-01' = {
  parent: pgFlexServerB
  name: pgAdminObjectId
  properties: {
    principalType: 'User'
    principalName: pgAdminLogin
    tenantId: subscription().tenantId
  }
}

resource pgPostProvisionAdminB 'Microsoft.DBforPostgreSQL/flexibleServers/administrators@2024-08-01' = {
  parent: pgFlexServerB
  name: postProvisionAdminObjectId
  dependsOn: [
    pgAadAdminB
  ]
  properties: {
    principalType: 'ServicePrincipal'
    principalName: postProvisionAdminName
    tenantId: subscription().tenantId
  }
}

@batchSize(1)
resource pgDatabasesB 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2024-08-01' = [for i in range(1, testDbCountPerServer): {
  parent: pgFlexServerB
  name: 'sbm_pg_test${i}'
  dependsOn: [
    pgAadAdminB
  ]
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}]

// ============================================================
// Private DNS Zone for PostgreSQL (shared by both servers)
// ============================================================
var pgPrivateDnsZoneName = 'privatelink.postgres.database.azure.com'

resource pgPrivateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: pgPrivateDnsZoneName
  location: 'global'
}

resource pgPrivateDnsZoneLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = {
  parent: pgPrivateDnsZone
  name: '${namePrefix}-pg-vnet-link'
  location: 'global'
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: vnetId
    }
  }
}

// Private Endpoint for PostgreSQL Server A
resource pgPrivateEndpointA 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: '${namePrefix}pg-a-pe'
  location: location
  properties: {
    subnet: {
      id: privateEndpointSubnetId
    }
    privateLinkServiceConnections: [
      {
        name: '${namePrefix}pg-a-plsc'
        properties: {
          privateLinkServiceId: pgFlexServerA.id
          groupIds: [
            'postgresqlServer'
          ]
        }
      }
    ]
  }
}

resource pgPrivateEndpointADnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  parent: pgPrivateEndpointA
  name: 'default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink-postgres-database-azure-com'
        properties: {
          privateDnsZoneId: pgPrivateDnsZone.id
        }
      }
    ]
  }
}

// Private Endpoint for PostgreSQL Server B
resource pgPrivateEndpointB 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: '${namePrefix}pg-b-pe'
  location: location
  properties: {
    subnet: {
      id: privateEndpointSubnetId
    }
    privateLinkServiceConnections: [
      {
        name: '${namePrefix}pg-b-plsc'
        properties: {
          privateLinkServiceId: pgFlexServerB.id
          groupIds: [
            'postgresqlServer'
          ]
        }
      }
    ]
  }
}

resource pgPrivateEndpointBDnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  parent: pgPrivateEndpointB
  name: 'default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink-postgres-database-azure-com'
        properties: {
          privateDnsZoneId: pgPrivateDnsZone.id
        }
      }
    ]
  }
}

// Outputs
output pgServerNameA string = pgFlexServerA.name
output pgServerFqdnA string = pgFlexServerA.properties.fullyQualifiedDomainName
output pgServerNameB string = pgFlexServerB.name
output pgServerFqdnB string = pgFlexServerB.properties.fullyQualifiedDomainName
output pgAdminUser string = pgAdminUser
output pgDatabaseCountPerServer int = testDbCountPerServer
