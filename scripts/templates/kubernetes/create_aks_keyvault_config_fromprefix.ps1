param
(
    [string] $path = "..\..\..\src\TestConfig",
    [string] $resourceGroupName,
    [string] $prefix
)
if("" -eq $resourceGroupName)
{
    $resourceGroupName = "$prefix-rg"
}
Write-Host "Create AKS cluster key vault configuration from prefix: $prefix"  -ForegroundColor Cyan
$path = Resolve-Path $path

Write-Host "Retrieving resource names from resources in $resourceGroupName with prefix $prefix" -ForegroundColor DarkGreen

$keyVaultName = az keyvault list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
Write-Host "Using key vault name:'$keyVaultName'" -ForegroundColor DarkGreen

$userAssignedIdentityName = az identity list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
Write-Host "Using identity name: '$userAssignedIdentityName'" -ForegroundColor DarkGreen

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/create_aks_keyvault_config.ps1 -path $path -resourceGroupName $resourceGroupName -keyVaultName $keyVaultName -identityName $userAssignedIdentityName