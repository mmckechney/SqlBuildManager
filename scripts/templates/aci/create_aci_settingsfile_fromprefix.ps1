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
$aciName = $prefix + "aci"
if("" -eq $resourceGroupName)
{
    $resourceGroupName = "$prefix-rg"
}
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
 
$keyVaultName = az keyvault list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
Write-Host "Using key vault name:'$keyVaultName'" -ForegroundColor DarkGreen

$storageAccountName =  az storage account list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
Write-Host "Using storage account name:'$storageAccountName'" -ForegroundColor DarkGreen

$eventHubNamespaceName = az eventhubs namespace list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
Write-Host "Using Event Hub Namespace name:'$eventHubNamespaceName'" -ForegroundColor DarkGreen

$serviceBusNamespaceName = az servicebus namespace list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
Write-Host "Using Service Bus Namespace name:'$serviceBusNamespaceName'" -ForegroundColor DarkGreen

$identityName = az identity list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
Write-Host "Using Managed Identity name:'$identityName'" -ForegroundColor DarkGreen

$identityClientId = az identity list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].clientId"
Write-Host "Using Managed Identity ClientId:'$identityClientId'" -ForegroundColor DarkGreen

$containerRegistryName = az acr list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/create_aci_settingsfile.ps1 -sbmExe $sbmExe -path $path -resourceGroupName $resourceGroupName -keyVaultName $keyVaultName -aciname $aciName -imageTag $imageTag -containerRegistryName $containerRegistryName -storageAccountName $storageAccountName -eventHubNamespaceName $eventHubNamespaceName -serviceBusNamespaceName $serviceBusNamespaceName -identityName $identityName -sqlUserName $sqlUserName -sqlPassword $sqlPassword -identityClientId $identityClientId
.$scriptDir/create_aci_settingsfile.ps1 -sbmExe $sbmExe -path $path -resourceGroupName $resourceGroupName -keyVaultName $keyVaultName -aciname $aciName -imageTag $imageTag -storageAccountName $storageAccountName -eventHubNamespaceName $eventHubNamespaceName -serviceBusNamespaceName $serviceBusNamespaceName -identityName $identityName -sqlUserName $sqlUserName -sqlPassword $sqlPassword -identityClientId $identityClientId