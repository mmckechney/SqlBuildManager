param
(
    [string] $sbmExe = "sbm.exe",
    [string] $path = "..\..\..\src\TestConfig",
    [string] $resourceGroupName,
    [string] $prefix,
    [string] $imageTag,
    [bool] $withContainerRegistry,
    [string] $sqlUserName,
    [string] $sqlPassword
)
Write-Host "Create ACI settings file from prefix: $prefix"  -ForegroundColor Cyan
$path = Resolve-Path $path

#############################################
# Get set resource name variables from prefix
#############################################
. ./../prefix_resource_names.ps1 -prefix $prefix
. ./../key_file_names.ps1 -prefix $prefix -path $path

Write-Host "Retrieving resource names from resources in $resourceGroupName with prefix $prefix" -ForegroundColor DarkGreen
if([string]::IsNullOrWhiteSpace($sqlUserName))
{
    $sqlUserName = (Get-Content -Path (Join-Path $path "un.txt")).Trim()
    $sqlPassword = (Get-Content -Path (Join-Path $path "pw.txt")).Trim()
}


if("" -eq $imageTag)
{
    $imageTag = "latest-vNext" #Get-Date -Format "yyyy-MM-dd"
    Write-Host "Using Image Tag: $imageTag" -ForegroundColor DarkGreen
}
 
$identityClientId = az identity show --resource-group $resourceGroupName --name $identityName -o tsv --query clientId
Write-Host "Using key vault name:'$keyVaultName'" -ForegroundColor DarkGreen
Write-Host "Using storage account name:'$storageAccountName'" -ForegroundColor DarkGreen
Write-Host "Using Event Hub Namespace name:'$eventHubNamespaceName'" -ForegroundColor DarkGreen
Write-Host "Using Service Bus Namespace name:'$serviceBusNamespaceName'" -ForegroundColor DarkGreen
Write-Host "Using Managed Identity name:'$identityName'" -ForegroundColor DarkGreen
Write-Host "Using Managed Identity ClientId:'$identityClientId'" -ForegroundColor DarkGreen
Write-Host "Using Container Registry Name :'$containerRegistryName'" -ForegroundColor DarkGreen
Write-Host "Using VNET Name :'$vnet'" -ForegroundColor DarkGreen
Write-Host "Using SubNet Name :'$aciSubnet'" -ForegroundColor DarkGreen


$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/create_aci_settingsfile.ps1 -sbmExe $sbmExe -path $path -resourceGroupName $resourceGroupName -keyVaultName $keyVaultName -aciname $aciName -imageTag $imageTag -containerRegistryName $containerRegistryName -storageAccountName $storageAccountName -eventHubNamespaceName $eventHubNamespaceName -serviceBusNamespaceName $serviceBusNamespaceName -identityName $identityName -sqlUserName $sqlUserName -sqlPassword $sqlPassword -identityClientId $identityClientId -vnetName $vnet -subnetName $aciSubnet
.$scriptDir/create_aci_settingsfile.ps1 -sbmExe $sbmExe -path $path -resourceGroupName $resourceGroupName -keyVaultName $keyVaultName -aciname $aciName -imageTag $imageTag -storageAccountName $storageAccountName -eventHubNamespaceName $eventHubNamespaceName -serviceBusNamespaceName $serviceBusNamespaceName -identityName $identityName -sqlUserName $sqlUserName -sqlPassword $sqlPassword -identityClientId $identityClientId -vnetName $vnet -subnetName $aciSubnet