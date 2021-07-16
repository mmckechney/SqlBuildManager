@description('Prefix to prepend to account names')
param namePrefix string = 'eztmwm'

@description('Prefix to prepend to account names')
param sqladminname string = 'sqladmin_user'

@description('Prefix to prepend to account names')
param sqladminpassword string = 'ERFSC#$%Ygvswer'

@description('Prefix to prepend to account names')
param testDbCountPerServer int = 10

@description('Location for all resources.')
param location string = resourceGroup().location


var sqlserver_var = '${namePrefix}sql'
var sqlpool_var = '${namePrefix}pool'


resource sqlserver_a_name_resource 'Microsoft.Sql/servers@2021-02-01-preview' = {
  name: '${sqlserver_var}-a'
  location: location
  
  properties: {
    administratorLogin: sqladminname
    administratorLoginPassword: sqladminpassword
    publicNetworkAccess: 'Enabled'
    restrictOutboundNetworkAccess: 'Disabled'
  }
}

resource sqlserver_a_AllowAllWindowsAzureIps 'Microsoft.Sql/servers/firewallRules@2021-02-01-preview' = {
  parent: sqlserver_a_name_resource
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource sqlserver_a_pool_resource 'Microsoft.Sql/servers/elasticPools@2021-02-01-preview' = {
  parent: sqlserver_a_name_resource
  name: '${sqlpool_var}-a'  
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

resource sqlserver_a_database_resource 'Microsoft.Sql/servers/databases@2021-02-01-preview' = [for i in range(1,testDbCountPerServer):{
  parent: sqlserver_a_name_resource
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
    elasticPoolId: sqlserver_a_pool_resource.id
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    readScale: 'Disabled'
    requestedBackupStorageRedundancy: 'Geo'
    isLedgerOn: false
  }
}]

resource sqlserver_b_name_resource 'Microsoft.Sql/servers@2021-02-01-preview' = {
  name: '${sqlserver_var}-b'
  location: location
  
  properties: {
    administratorLogin: sqladminname
    administratorLoginPassword: sqladminpassword
    publicNetworkAccess: 'Enabled'
    restrictOutboundNetworkAccess: 'Disabled'
  }
}

resource sqlserver_b_AllowAllWindowsAzureIps 'Microsoft.Sql/servers/firewallRules@2021-02-01-preview' = {
  parent: sqlserver_b_name_resource
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}


resource sqlserver_b_pool_resource 'Microsoft.Sql/servers/elasticPools@2021-02-01-preview' = {
  parent: sqlserver_b_name_resource
  name: '${sqlpool_var}-b'  
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

resource sqlserver_b_database_resource 'Microsoft.Sql/servers/databases@2021-02-01-preview' = [for i in range(1,testDbCountPerServer):{
  parent: sqlserver_b_name_resource
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
    elasticPoolId: sqlserver_b_pool_resource.id
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    readScale: 'Disabled'
    requestedBackupStorageRedundancy: 'Geo'
    isLedgerOn: false
  }
}]
