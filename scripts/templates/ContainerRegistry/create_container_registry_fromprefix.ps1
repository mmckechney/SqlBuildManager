param
(
    [string] $resourceGroupName,
    [string] $prefix

)
if("" -eq $resourceGroupName)
{
    $resourceGroupName = "$prefix-rg"
}

$containerRegistryName = $prefix + "containerregistry"
$logAnalyticsWorkspace =  $prefix + "loganalytics"

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/create_container_registry.ps1 -resourceGroupName $resourceGroupName -containerRegistryName $containerRegistryName -logAnalyticsWorkspace $logAnalyticsWorkspace

