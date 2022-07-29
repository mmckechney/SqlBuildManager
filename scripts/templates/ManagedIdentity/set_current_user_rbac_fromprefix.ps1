param
(
    [string] $prefix,
    [string] $resourceGroupName
)


#############################################
# Get set resource name variables from prefix
#############################################
. ./../prefix_resource_names.ps1 -prefix $prefix


Write-Host "Set current user identity RBAC from prefix: $prefix"  -ForegroundColor Cyan
Write-Host "Retrieving resource names from resource group $resourceGroupName" -ForegroundColor DarkGreen
Write-Host "Using Keyvault name:'$keyVaultName'" -ForegroundColor DarkGreen

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/set_current_user_rbac.ps1 -resourceGroupName $resourceGroupName -keyVaultName $keyVaultName