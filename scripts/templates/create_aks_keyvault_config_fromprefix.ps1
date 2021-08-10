param
(
    [string] $path = "..\..\src\TestConfig",
    [string] $resourceGroupName,
    [string] $prefix
)
$path = Resolve-Path $path

Write-Host "Retrieving resource names from resources in $resourceGroupName with prefix $prefix" -ForegroundColor DarkGreen

$keyVaultName = az keyvault list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
Write-Host "Using key vault name:'$keyVaultName'" -ForegroundColor DarkGreen

$userAssignedIdentityName = az identity list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
Write-Host "Using identity name: '$userAssignedIdentityName'" -ForegroundColor DarkGreen

./create_aks_keyvault_config.ps1 -path $path -resourceGroupName $resourceGroupName -keyVaultName $keyVaultName -identityName $userAssignedIdentityName