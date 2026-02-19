@description('Prefix to prepend to account names')
param namePrefix string

@description('Number of test databases to create')
param testDbCount int = 10

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

var pgServerName = '${namePrefix}pgserver'
var pgAdminUser = '${namePrefix}pgadmin'
var subnetNamesArray = split(subnetNames, ',')

module networkResource 'network.bicep' = {
  name: 'pgNetworkResource'
  params: {
    namePrefix: namePrefix
    location: location
  }
}

// Azure Database for PostgreSQL Flexible Server
resource pgFlexServer 'Microsoft.DBforPostgreSQL/flexibleServers@2024-08-01' = {
  name: pgServerName
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

// Entra ID (AAD) administrator for the PG Flexible Server
// Must wait until the server is fully accessible (firewall rules applied) before setting Entra admin
resource pgAadAdmin 'Microsoft.DBforPostgreSQL/flexibleServers/administrators@2024-08-01' = {
  parent: pgFlexServer
  name: pgAdminObjectId
  dependsOn: [
    pgFirewallAzureServices
    pgFirewallRule
  ]
  properties: {
    principalType: 'User'
    principalName: pgAdminLogin
    tenantId: subscription().tenantId
  }
}

// Firewall rule for current machine IP
resource pgFirewallRule 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2024-08-01' = if (currentIpAddress != '') {
  parent: pgFlexServer
  name: '${namePrefix}_AllowCurrentIp'
  properties: {
    startIpAddress: currentIpAddress
    endIpAddress: currentIpAddress
  }
}

// Allow Azure services (e.g., ACI, Batch, Container Apps, AKS) to connect
resource pgFirewallAzureServices 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2024-08-01' = {
  parent: pgFlexServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Test databases: sbm_pg_test1 through sbm_pg_testN
resource pgDatabases 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2024-08-01' = [for i in range(1, testDbCount): {
  parent: pgFlexServer
  name: 'sbm_pg_test${i}'
  dependsOn: [
    pgAadAdmin
  ]
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}]

// Private DNS Zone for PostgreSQL (only when using private endpoints)
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

resource pgPrivateEndpoint 'Microsoft.Network/privateEndpoints@2023-05-01' = if (usePrivateEndpoint) {
  name: '${namePrefix}pg-pe'
  location: location
  properties: {
    subnet: {
      id: privateEndpointSubnetId
    }
    privateLinkServiceConnections: [
      {
        name: '${namePrefix}pg-plsc'
        properties: {
          privateLinkServiceId: pgFlexServer.id
          groupIds: [
            'postgresqlServer'
          ]
        }
      }
    ]
  }
}

resource pgPrivateEndpointDnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = if (usePrivateEndpoint) {
  parent: pgPrivateEndpoint
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
output pgServerName string = pgFlexServer.name
output pgServerFqdn string = pgFlexServer.properties.fullyQualifiedDomainName
output pgAdminUser string = pgAdminUser
output pgDatabaseCount int = testDbCount
