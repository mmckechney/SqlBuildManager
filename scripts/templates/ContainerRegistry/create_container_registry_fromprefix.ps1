param
(
    [string] $resourceGroupName,
    [string] $prefix

)

#############################################
# Get set resource name variables from prefix
#############################################
. ./../prefix_resource_names.ps1 -prefix $prefix

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/create_container_registry.ps1 -resourceGroupName $resourceGroupName -containerRegistryName $containerRegistryName -logAnalyticsWorkspace $logAnalyticsWorkspace

