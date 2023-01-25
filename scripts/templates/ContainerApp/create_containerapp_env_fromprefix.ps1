

param
(
    [string] $prefix,
    [string] $resourceGroupName,
    [string] $path = "..\..\..\src\TestConfig"
)
#############################################
# Get set resource name variables from prefix
#############################################
. ./../prefix_resource_names.ps1 -prefix $prefix
. ./../key_file_names.ps1 -prefix $prefix -path $path

Write-Host "Create Container App Environment from prefix: $prefix"  -ForegroundColor Cyan
Write-Host "Using Container App Environment name: '$containerAppEnvName'"  -ForegroundColor Green
Write-Host "Using Log Analytics workspace name: '$logAnalyticsWorkspace'"  -ForegroundColor Green
Write-Host "Using Container Registry name: '$containerRegistryName'"  -ForegroundColor Green

$location =  az group show -n $resourceGroupName -o tsv --query location

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path

.$scriptDir/../Network/create_vnet_fromprefix.ps1 -prefix $prefix 
$subnetId = az network vnet subnet show -g $resourceGroupName --vnet-name $vnet -n $containerAppSubnet --query id -o tsv

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/create_containerapp_env.ps1 -containerAppEnvName $containerAppEnvName -logAnalyticsWorkspace $logAnalyticsWorkspace -location $location -resourceGroupName $resourceGroupName -containerRegistryName $containerRegistryName -subnetId $subnetId 
