

param
(
    [string] $prefix,
    [string] $resourceGroupName
)
if("" -eq $resourceGroupName)
{
    $resourceGroupName = "$prefix-rg"
}
Write-Host "Create Container App Environment from prefix: $prefix"  -ForegroundColor Cyan
$containerAppEnvName = $prefix + "containerappenv"
Write-Host "Using Container App Environment name: '$containerAppEnvName'"  -ForegroundColor Green

$logAnalyticsWorkspace = $prefix + "loganalytics"
Write-Host "Using Log Analytics workspace name: '$logAnalyticsWorkspace'"  -ForegroundColor Green

$containerRegistryName = $prefix + "containerregistry"
Write-Host "Using Container Registry name: '$containerRegistryName'"  -ForegroundColor Green

$location =  az group show -n $resourceGroupName -o tsv --query location

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/create_containerapp_env.ps1 -containerAppEnvName $containerAppEnvName -logAnalyticsWorkspace $logAnalyticsWorkspace -location $location -resourceGroupName $resourceGroupName -containerRegistryName $containerRegistryName 
