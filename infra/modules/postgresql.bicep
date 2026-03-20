@description('Prefix to prepend to account names')
param namePrefix string

@description('Number of test databases to create per server')
param testDbCountPerServer int = 10

@description('Location for all resources.')
param location string = resourceGroup().location

@description('Current machine IP address to allow access to the PostgreSQL server.')
param currentIpAddress string

@description('Array of subnet resource ids to allow access via VNet rules.')
param subnetNames string

@description('Object ID (GUID) of the Entra ID user or group to set as PG AAD admin')
param pgAdminObjectId string

@description('Login name (email) of the Entra ID admin')
param pgAdminLogin string

@secure()
@description('Administrator password for PostgreSQL (used for local/password-based auth)')
param pgAdminPassword string

@description('Whether to use private endpoints instead of public network access')
param usePrivateEndpoint bool = false

@description('VNet ID for private DNS zone link (required when usePrivateEndpoint is true)')
param vnetId string = ''

@description('Private endpoint subnet ID (required when usePrivateEndpoint is true)')
param privateEndpointSubnetId string = ''

var pgServerNameA = '${namePrefix}pgserver-a'
var pgServerNameB = '${namePrefix}pgserver-b'
var pgAdminUser = '${namePrefix}pgadmin'
var subnetNamesArray = split(subnetNames, ',')

module networkResource 'network.bicep' = {
  name: 'pgNetworkResource'
  params: {
    namePrefix: namePrefix
    location: location
  }
}

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
  dependsOn: [
    pgFirewallAzureServicesA
    pgFirewallRuleA
  ]
  properties: {
    principalType: 'User'
    principalName: pgAdminLogin
    tenantId: subscription().tenantId
  }
}

resource pgFirewallRuleA 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2024-08-01' = if (currentIpAddress != '') {
  parent: pgFlexServerA
  name: '${namePrefix}_AllowCurrentIpA'
  dependsOn: [
    pgFirewallAzureServicesA
  ]
  properties: {
    startIpAddress: currentIpAddress
    endIpAddress: currentIpAddress
  }
}

resource pgFirewallAzureServicesA 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2024-08-01' = {
  parent: pgFlexServerA
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
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
  dependsOn: [
    pgFirewallAzureServicesB
    pgFirewallRuleB
  ]
  properties: {
    principalType: 'User'
    principalName: pgAdminLogin
    tenantId: subscription().tenantId
  }
}

resource pgFirewallRuleB 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2024-08-01' = if (currentIpAddress != '') {
  parent: pgFlexServerB
  name: '${namePrefix}_AllowCurrentIpB'
  dependsOn: [
    pgFirewallAzureServicesB
  ]
  properties: {
    startIpAddress: currentIpAddress
    endIpAddress: currentIpAddress
  }
}

resource pgFirewallAzureServicesB 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2024-08-01' = {
  parent: pgFlexServerB
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
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

resource pgPrivateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = if (usePrivateEndpoint) {
  name: pgPrivateDnsZoneName
  location: 'global'
}

resource pgPrivateDnsZoneLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = if (usePrivateEndpoint) {
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
resource pgPrivateEndpointA 'Microsoft.Network/privateEndpoints@2023-05-01' = if (usePrivateEndpoint) {
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

resource pgPrivateEndpointADnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = if (usePrivateEndpoint) {
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
resource pgPrivateEndpointB 'Microsoft.Network/privateEndpoints@2023-05-01' = if (usePrivateEndpoint) {
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

resource pgPrivateEndpointBDnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = if (usePrivateEndpoint) {
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
