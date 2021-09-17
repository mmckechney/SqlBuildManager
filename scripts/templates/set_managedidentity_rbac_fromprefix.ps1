param
(
    [string] $prefix,
    [string] $resourceGroupName
)

Write-Host "Retrieving resource names from resource group $resourceGroupName" -ForegroundColor DarkGreen
$identityName = az identity list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
$keyVaultName = az keyvault list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
Write-Host "Using Managed Identity name:'$identityName'" -ForegroundColor DarkGreen


./set_managedidentity_rbac.ps1 -resourceGroupName $resourceGroupName -identityname $identityName -keyVaultName $keyVaultName