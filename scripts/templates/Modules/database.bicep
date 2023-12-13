@description('Prefix to prepend to account names')
param namePrefix string = 'eztmwm'

@description('Name for SQL Admin account')
param sqladminname string = 'sqladmin_user'

@description('Value for SQL Admin password')
@secure()
param sqladminpassword string = 'ERFSC#$%Ygvswer'

@description('Number of test databases to create per server')
param testDbCountPerServer int = 10

@description('Location for all resources.')
param location string = resourceGroup().location

@description('Current manchine IP address to allow access to the SQL Server.')
param currentIpAddress string

@description('Array of subnet resource ids to allow access to the SQL Server.')
param subnetNames string

var resourceGroupNameVar = '${namePrefix}-rg'
var sqlserverNameVar = '${namePrefix}sql'
var sqlpoolNameVar = '${namePrefix}pool'
var identityNameVar = '${namePrefix}identity'
var subnetNamesArray = split(subnetNames,',')


module networkResource 'network.bicep' = {
  name: 'networkResource'
  scope: resourceGroup(resourceGroupNameVar)
  params: {
    namePrefix: namePrefix
    location: location
  }
}

module identityResource 'identity.bicep' = {
  name: 'identityResource'
  scope: resourceGroup(resourceGroupNameVar)
  params: {
    identityName: identityNameVar
    location: location
  }
}


// SQL Server 'A' resources
resource sqlserverAResource 'Microsoft.Sql/servers@2021-11-01' = {
  name: '${sqlserverNameVar}-a'
  location: location
  
  properties: {
    administratorLogin: sqladminname
    administratorLoginPassword: sqladminpassword
    publicNetworkAccess: 'Enabled'
    restrictOutboundNetworkAccess: 'Disabled'
    administrators: {
      administratorType: 'ActiveDirectory'
      principalType: 'Application'
      login: identityNameVar
      sid: identityResource.outputs.clientId
      tenantId: identityResource.outputs.tenantId
  }
    
  }
}

resource sqlserverAFirewallRule 'Microsoft.Sql/servers/firewallRules@2021-11-01' = if(currentIpAddress != '') {
  parent: sqlserverAResource
  name: '${sqlserverNameVar}A_AllowIp'
  properties: {
    startIpAddress: currentIpAddress
    endIpAddress: currentIpAddress
  }
}

resource sqlserverA_VnetRule 'Microsoft.Sql/servers/virtualNetworkRules@2021-11-01' = [for subnet in subnetNamesArray:{
  parent: sqlserverAResource
  name: '${sqlserverNameVar}A_${subnet}'
  properties: {
    ignoreMissingVnetServiceEndpoint: false
    virtualNetworkSubnetId: resourceId('Microsoft.Network/virtualNetworks/subnets',networkResource.outputs.vnetName, subnet)

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


// SQL Server 'B' resources
resource sqlserverBResource 'Microsoft.Sql/servers@2021-11-01' = {
  name: '${sqlserverNameVar}-b'
  location: location  
  properties: {
    administratorLogin: sqladminname
    administratorLoginPassword: sqladminpassword
    publicNetworkAccess: 'Enabled'
    restrictOutboundNetworkAccess: 'Disabled'
    administrators: {
      administratorType: 'ActiveDirectory'
      principalType: 'Application'
      login: identityNameVar
      sid: identityResource.outputs.clientId
      tenantId: identityResource.outputs.tenantId
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

resource sqlserverB_VnetRule 'Microsoft.Sql/servers/virtualNetworkRules@2021-11-01' = [for subnet in subnetNamesArray:{
  parent: sqlserverBResource
  name: '${sqlserverNameVar}B_${subnet}'
  properties: {
    ignoreMissingVnetServiceEndpoint: false
    virtualNetworkSubnetId: resourceId('Microsoft.Network/virtualNetworks/subnets',networkResource.outputs.vnetName, subnet)

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
