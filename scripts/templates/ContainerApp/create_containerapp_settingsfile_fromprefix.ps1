param
(
    [string] $sbmExe = "sbm.exe",
    [string] $path = "..\..\..\src\TestConfig",
    [string] $resourceGroupName,
    [string] $prefix,
    [string] $sqlUserName,
    [string] $sqlPassword,
    [string] $imageTag,
    [bool] $withContainerRegistry = $true,
    [bool] $withKeyVault = $true
)

if("" -eq $resourceGroupName)
{
    $resourceGroupName = "$prefix-rg"
}
Write-Host "Create Container App Settings file from prefix: $prefix"  -ForegroundColor Cyan
$path = Resolve-Path $path
$containerAppEnvironmentName = $prefix + "containerappenv"

Write-Host "Retrieving resource names from resources in $resourceGroupName with prefix $prefix" -ForegroundColor DarkGreen
if([string]::IsNullOrWhiteSpace($sqlUserName))
{
    $sqlUserName = (Get-Content -Path (Join-Path $path "un.txt")).Trim()
    $sqlPassword = (Get-Content -Path (Join-Path $path "pw.txt")).Trim()
}
 if($withKeyVault)
 {
    $keyVaultName = az keyvault list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
    Write-Host "Using key vault name:'$keyVaultName'" -ForegroundColor DarkGreen
}
else {
    Write-Host "Not using KeyVault"  -ForegroundColor DarkGreen
    
}

$identityName = az identity list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
Write-Host "Using Managed Identity name:'$identityName'" -ForegroundColor DarkGreen

$identityClientId = az identity list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].clientId"
Write-Host "Using Managed Identity ClientId:'$identityClientId'" -ForegroundColor DarkGreen
 
$storageAccountName =  az storage account list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
Write-Host "Using storage account name:'$storageAccountName'" -ForegroundColor DarkGreen

$eventHubNamespaceName = az eventhubs namespace list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
Write-Host "Using Event Hub Namespace name:'$eventHubNamespaceName'" -ForegroundColor DarkGreen

$serviceBusNamespaceName = az servicebus namespace list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
Write-Host "Using Service Bus Namespace name:'$serviceBusNamespaceName'" -ForegroundColor DarkGreen

if($withContainerRegistry)
{
    $containerRegistryName = az acr list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
    Write-Host "Using Container Registry name:'$containerRegistryName'" -ForegroundColor DarkGreen
}

if("" -eq $imageTag)
{
    $imageTag = "latest-vNext"
    Write-Host "Using Image Tag: $imageTag" -ForegroundColor DarkGreen
}

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/create_containerapp_settingsfile.ps1 -sbmExe $sbmExe -path $path -resourceGroupName $resourceGroupName -containerAppEnvironmentName $containerAppEnvironmentName -containerRegistryName $containerRegistryName -storageAccountName $storageAccountName -eventHubNamespaceName $eventHubNamespaceName -serviceBusNamespaceName $serviceBusNamespaceName -sqlUserName $sqlUserName -sqlPassword $sqlPassword -imageTag $imageTag -withContainerRegistry $withContainerRegistry -keyVaultName $keyVaultName -identityName $identityName -identityClientId $identityClientId

