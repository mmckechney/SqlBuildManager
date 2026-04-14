@description('Prefix to prepend to account names')
param namePrefix string = 'eztmwm'

@description('Number of test databases to create per server')
param testDbCountPerServer int = 10

@description('Location for all resources.')
param location string = resourceGroup().location

@description('Current machine IP address to allow access to the SQL Server.')
param currentIpAddress string

@description('Array of subnet resource ids to allow access to the SQL Server.')
param subnetNames string

@description('Object ID (GUID) of the Entra ID user or group to set as SQL admin')
param sqlAdminObjectId string

@description('Login name (email or display name) of the Entra ID admin')
param sqlAdminLogin string

@description('Whether to use private endpoints instead of public network access')
param usePrivateEndpoint bool = false

@description('VNet ID for private DNS zone link (required when usePrivateEndpoint is true)')
param vnetId string = ''

@description('Private endpoint subnet ID (required when usePrivateEndpoint is true)')
param privateEndpointSubnetId string = ''

var sqlserverNameVar = '${namePrefix}sql'
var sqlpoolNameVar = '${namePrefix}pool'
var identityNameVar = '${namePrefix}identity'
var vnetNameVar = '${namePrefix}vnet'
var subnetNamesArray = split(subnetNames,',')

resource existingVnet 'Microsoft.Network/virtualNetworks@2023-05-01' existing = {
  name: vnetNameVar
}


// SQL Server 'A' resources - Entra ID Only Authentication
resource sqlserverAResource 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: '${sqlserverNameVar}-a'
  location: location
  
  properties: {
    // Always start with public access enabled so firewall rules can be created
    publicNetworkAccess: 'Enabled'
    restrictOutboundNetworkAccess: 'Disabled'
    minimalTlsVersion: '1.2'
    administrators: {
      administratorType: 'ActiveDirectory'
      principalType: 'User'
      login: sqlAdminLogin
      sid: sqlAdminObjectId
      tenantId: subscription().tenantId
      azureADOnlyAuthentication: true
    }
  }
}

// Output managed identity name for reference (used by grant_identity_permissions.ps1)
output identityName string = identityNameVar

resource sqlserverAFirewallRule 'Microsoft.Sql/servers/firewallRules@2021-11-01' = if(currentIpAddress != '') {
  parent: sqlserverAResource
  name: '${sqlserverNameVar}A_AllowIp'
  properties: {
    startIpAddress: currentIpAddress
    endIpAddress: currentIpAddress
  }
}

resource sqlserverA_VnetRule 'Microsoft.Sql/servers/virtualNetworkRules@2021-11-01' = [for subnet in subnetNamesArray: {
  parent: sqlserverAResource
  name: '${sqlserverNameVar}A_${subnet}'
  properties: {
    ignoreMissingVnetServiceEndpoint: false
    virtualNetworkSubnetId: resourceId('Microsoft.Network/virtualNetworks/subnets', existingVnet.name, subnet)

  }
}]

resource sqlserverAResource_Pool 'Microsoft.Sql/servers/elasticPools@2021-11-01' = {
  parent: sqlserverAResource
  name: '${sqlpoolNameVar}-a'  
  location: location
  sku: {
    name: 'BasicPool'
    tier: 'Basic'
    capacity: 50
  }
  properties: {
    maxSizeBytes: 5242880000
    perDatabaseSettings: {
      minCapacity: 0
      maxCapacity: 5
    }
    zoneRedundant: false
  }
}

resource sqlserverAResourceDatabase 'Microsoft.Sql/servers/databases@2021-11-01' = [for i in range(1,testDbCountPerServer):{
  parent: sqlserverAResource
  name: 'SqlBuildTest${i}'
  location: location
   sku: {
    name: 'ElasticPool'
    tier: 'Basic'
    capacity: 0
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 2147483648
    elasticPoolId: sqlserverAResource_Pool.id
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    readScale: 'Disabled'
    requestedBackupStorageRedundancy: 'Geo'
    isLedgerOn: false
  }
}]


// SQL Server 'B' resources - Entra ID Only Authentication
resource sqlserverBResource 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: '${sqlserverNameVar}-b'
  location: location  
  properties: {
    // Always start with public access enabled so firewall rules can be created
    publicNetworkAccess: 'Enabled'
    restrictOutboundNetworkAccess: 'Disabled'
    minimalTlsVersion: '1.2'
    administrators: {
      administratorType: 'ActiveDirectory'
      principalType: 'User'
      login: sqlAdminLogin
      sid: sqlAdminObjectId
      tenantId: subscription().tenantId
      azureADOnlyAuthentication: true
    } 
  } 
}
resource sqlserverBFirewallRule 'Microsoft.Sql/servers/firewallRules@2021-11-01' = if(currentIpAddress != '') {
  parent: sqlserverBResource
  name: '${sqlserverNameVar}B_AllowIp'
  properties: {
    startIpAddress: currentIpAddress
    endIpAddress: currentIpAddress
  }
}

resource sqlserverB_VnetRule 'Microsoft.Sql/servers/virtualNetworkRules@2021-11-01' = [for subnet in subnetNamesArray: {
  parent: sqlserverBResource
  name: '${sqlserverNameVar}B_${subnet}'
  properties: {
    ignoreMissingVnetServiceEndpoint: false
    virtualNetworkSubnetId: resourceId('Microsoft.Network/virtualNetworks/subnets', existingVnet.name, subnet)

  }
}]

resource sqlserverBResource_Pool 'Microsoft.Sql/servers/elasticPools@2021-11-01' = {
  parent: sqlserverBResource
  name: '${sqlpoolNameVar}-b'  
  location: location
  sku: {
    name: 'BasicPool'
    tier: 'Basic'
    capacity: 50
  }
  properties: {
    maxSizeBytes: 5242880000
    perDatabaseSettings: {
      minCapacity: 0
      maxCapacity: 5
    }
    zoneRedundant: false
  }
}

resource sqlserverBResource_Database 'Microsoft.Sql/servers/databases@2021-11-01' = [for i in range(1,testDbCountPerServer):{
  parent: sqlserverBResource
  name: 'SqlBuildTest${i}'
  location: location
   sku: {
    name: 'ElasticPool'
    tier: 'Basic'
    capacity: 0
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 2147483648
    elasticPoolId: sqlserverBResource_Pool.id
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    readScale: 'Disabled'
    requestedBackupStorageRedundancy: 'Geo'
    isLedgerOn: false
  }
}]

// Private DNS zone name for SQL Server using environment suffix
var sqlPrivateDnsZoneName = 'privatelink${environment().suffixes.sqlServerHostname}'

// Private DNS Zone for SQL Server (only when using private endpoints)
resource privateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = if(usePrivateEndpoint) {
  name: sqlPrivateDnsZoneName
  location: 'global'
}

// Link private DNS zone to VNet
resource privateDnsZoneLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = if(usePrivateEndpoint) {
  parent: privateDnsZone
  name: '${namePrefix}-sql-vnet-link'
  location: 'global'
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: vnetId
    }
  }
}

// Private Endpoint for SQL Server A
resource privateEndpointA 'Microsoft.Network/privateEndpoints@2023-05-01' = if(usePrivateEndpoint) {
  name: '${namePrefix}sql-a-pe'
  location: location
  properties: {
    subnet: {
      id: privateEndpointSubnetId
    }
    privateLinkServiceConnections: [
      {
        name: '${namePrefix}sql-a-plsc'
        properties: {
          privateLinkServiceId: sqlserverAResource.id
          groupIds: [
            'sqlServer'
          ]
        }
      }
    ]
  }
}

// DNS Zone Group for SQL Server A private endpoint
resource privateEndpointADnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = if(usePrivateEndpoint) {
  parent: privateEndpointA
  name: 'default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink-database-windows-net'
        properties: {
          privateDnsZoneId: privateDnsZone.id
        }
      }
    ]
  }
}

// Private Endpoint for SQL Server B
resource privateEndpointB 'Microsoft.Network/privateEndpoints@2023-05-01' = if(usePrivateEndpoint) {
  name: '${namePrefix}sql-b-pe'
  location: location
  properties: {
    subnet: {
      id: privateEndpointSubnetId
    }
    privateLinkServiceConnections: [
      {
        name: '${namePrefix}sql-b-plsc'
        properties: {
          privateLinkServiceId: sqlserverBResource.id
          groupIds: [
            'sqlServer'
          ]
        }
      }
    ]
  }
}

// DNS Zone Group for SQL Server B private endpoint
resource privateEndpointBDnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = if(usePrivateEndpoint) {
  parent: privateEndpointB
  name: 'default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink-database-windows-net'
        properties: {
          privateDnsZoneId: privateDnsZone.id
        }
      }
    ]
  }
}

// NOTE: Disabling public network access is handled in postprovision.ps1 via Azure CLI
// This allows both VNet rules and private endpoints to coexist for flexible switching
// The Bicep policy may block inline updates, but az sql server update works fine
