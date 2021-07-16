param
(
    [string] $path,
    [string] $resourceGroupName,
    [string] $prefix
)
$path = Resolve-Path $path

$keyVaultName = az keyvault list --resource-group sbm4-rg -o tsv --query "[?contains(@.name '$prefix')].name"
$userAssignedIdentityName = az identity list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"

Write-Host "Retrieving resource names from resources in $resourceGroupName with prefix $prefix" -ForegroundColor DarkGreen

./create_aks_keyvault_config.ps1 -path $path -resourceGroupName $resourceGroupName -keyVaultName $keyVaultName -identityName $userAssignedIdentityName