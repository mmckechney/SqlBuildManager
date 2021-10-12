param
(
    [string] $prefix,
    [string] $resourceGroupName
)

if("" -eq $resourceGroupName)
{
    $resourceGroupName = "$prefix-rg"
}
Write-Host "Set managed identity RBAC from prefix: $prefix"  -ForegroundColor Cyan
Write-Host "Retrieving resource names from resource group $resourceGroupName" -ForegroundColor DarkGreen
$identityName = az identity list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
$keyVaultName = az keyvault list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
Write-Host "Using Managed Identity name:'$identityName'" -ForegroundColor DarkGreen
Write-Host "Using Keyvault name:'$keyVaultName'" -ForegroundColor DarkGreen

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/set_managedidentity_rbac.ps1 -resourceGroupName $resourceGroupName -identityname $identityName -keyVaultName $keyVaultName