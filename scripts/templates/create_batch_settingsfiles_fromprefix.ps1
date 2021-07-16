param
(
    [string] $path,
    [string] $resourceGroupName,
    [string] $prefix,
    [string] $sqlUserName,
    [string] $sqlPassword
)

$path = Resolve-Path $path

Write-Host "Retrieving resource names from resources in $resourceGroupName with prefix $prefix" -ForegroundColor DarkGreen
if([string]::IsNullOrWhiteSpace($sqlUserName))
{
    $sqlUserName = (Get-Content -Path (Join-Path $path "un.txt")).Trim()
    $sqlPassword = (Get-Content -Path (Join-Path $path "pw.txt")).Trim()
}
 
$keyVaultName = az keyvault list --resource-group sbm4-rg -o tsv --query "[?contains(@.name '$prefix')].name"
$batchAccountName = az batch account list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
$storageAccountName =  az storage account list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
$eventHubNamespaceName = az eventhubs namespace list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
$serviceBusNamespaceName = az servicebus namespace list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
$identityName = az identity list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"

./create_batch_settingsfiles.ps1 -path $path -resourceGroupName $resourceGroupName -keyVaultName $keyVaultName -batchAccountName $batchAccountName -storageAccountName $storageAccountName -eventHubNamespaceName $eventHubNamespaceName -serviceBusNamespaceName $serviceBusNamespaceName -identityName $identityName -sqlUserName $sqlUserName -sqlPassword $sqlPassword 