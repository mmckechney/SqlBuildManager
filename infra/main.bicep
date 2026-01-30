// Azure Developer CLI (azd) entry point for SqlBuildManager infrastructure
// This template references the existing modules in scripts/templates/Modules/

targetScope = 'subscription'

@description('Name prefix for all resources. Must be unique.')
@minLength(3)
@maxLength(10)
param namePrefix string

@description('Primary location for all resources.')
param location string

@description('The current IP address of the machine running the deployment (for SQL Server firewall)')
param currentIpAddress string = ''

@description('The UserId GUID for the current user (for RBAC assignments)')
param userIdGuid string = ''

@description('The login name (email) of the current user for SQL admin')
param userLoginName string = ''

@description('Whether to deploy the Batch Account')
param deployBatchAccount bool = true

@description('Whether to deploy the container registry')
param deployContainerRegistry bool = true

@description('Whether to deploy the Container App Environment')
param deployContainerAppEnv bool = true

@description('Whether to deploy AKS')
param deployAks bool = true

@description('Number of test databases to create per server (0 to skip database deployment)')
param testDbCountPerServer int = 10

@allowed([
  'Basic'
  'Standard'
])
@description('The messaging tier for Event Hub namespace')
param eventhubSku string = 'Standard'

@allowed([
  1
  2
  4
])
@description('MessagingUnits for premium namespace')
param skuCapacity int = 1

// Resource Group
var resourceGroupName = '${namePrefix}-rg'

resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: resourceGroupName
  location: location
}

// Deploy main infrastructure
module mainDeployment '../scripts/templates/azuredeploy_main.bicep' = {
  name: 'mainDeployment'
  scope: rg
  params: {
    namePrefix: namePrefix
    location: location
    currentIpAddress: currentIpAddress
    userIdGuid: userIdGuid
    userLoginName: userLoginName
    deployBatchAccount: deployBatchAccount
    deployContainerRegistry: deployContainerRegistry
    deployContainerAppEnv: deployContainerAppEnv
    deployAks: deployAks
    testDbCountPerServer: testDbCountPerServer
    eventhubSku: eventhubSku
    skuCapacity: skuCapacity
  }
}

// Outputs for azd
output AZURE_LOCATION string = location
output AZURE_RESOURCE_GROUP string = resourceGroupName
output AZURE_NAME_PREFIX string = namePrefix
