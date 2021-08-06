param
(
    [string] $path = "..\..\src\TestConfig",
    [string] $resourceGroupName,
    [string] $prefix,
    [string] $sqlUserName,
    [string] $sqlPassword
)

$path = Resolve-Path $path
$aciName = $prefix + "aci"

Write-Host "Retrieving resource names from resources in $resourceGroupName with prefix $prefix" -ForegroundColor DarkGreen
if([string]::IsNullOrWhiteSpace($sqlUserName))
{
    $sqlUserName = (Get-Content -Path (Join-Path $path "un.txt")).Trim()
    $sqlPassword = (Get-Content -Path (Join-Path $path "pw.txt")).Trim()
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

./create_aci_settingsfile.ps1 -path $path -resourceGroupName $resourceGroupName -keyVaultName $keyVaultName -aciname $aciName -storageAccountName $storageAccountName -eventHubNamespaceName $eventHubNamespaceName -serviceBusNamespaceName $serviceBusNamespaceName -identityName $identityName -sqlUserName $sqlUserName -sqlPassword $sqlPassword 