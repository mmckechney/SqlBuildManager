param
(
    [string] $prefix,
    [string] $resourceGroupName
)

#############################################
# Get set resource name variables from prefix
#############################################
. ./../prefix_resource_names.ps1 -prefix $prefix

Write-Host "Set managed identity RBAC from prefix: $prefix"  -ForegroundColor Cyan
Write-Host "Using Managed Identity name:'$identityName'" -ForegroundColor DarkGreen
Write-Host "Using Keyvault name:'$keyVaultName'" -ForegroundColor DarkGreen

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/set_managedidentity_rbac.ps1 -resourceGroupName $resourceGroupName -identityname $identityName -keyVaultName $keyVaultName