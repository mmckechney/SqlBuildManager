param
(
    [string] $prefix,
    [string] $resourceGroupName,
    [string] $containerAppEnvName,
    [string] $logAnalyticsWorkspace,
    [string] $containerRegistryName,
    [string] $location,
    [string] $subnetId,
    [string] $path = "..\..\..\src\TestConfig"
)

if("" -ne $prefix)
{
    . ./../prefix_resource_names.ps1 -prefix $prefix
    . ./../key_file_names.ps1 -prefix $prefix -path $path
}
Write-Host "Creating Container App Environment: $containerAppEnvName" -ForegroundColor DarkGreen
if("" -eq $subnetId)
{
    $subnetId = az network vnet subnet show --resource-group $resourceGroupName --vnet-name $vnet --name $containerAppSubnet --query id -o tsv
}
$logAnalyticsClientId = az monitor log-analytics workspace show --query customerId -g $resourceGroupName -n $logAnalyticsWorkspace
$logAnalyticsKey = az monitor log-analytics workspace get-shared-keys --query primarySharedKey -g $resourceGroupName -n $logAnalyticsWorkspace

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
az deployment group create --resource-group $resourceGroupName --template-file "$($scriptDir)/../Modules/containerappenv.bicep" --parameters containerAppEnvName=$containerAppEnvName logAnalyticsClientId=$logAnalyticsClientId logAnalyticsKey=$logAnalyticsKey subnetId=$subnetId -o table
