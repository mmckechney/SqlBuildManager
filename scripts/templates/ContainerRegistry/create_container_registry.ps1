param
(
    [string] $prefix,
    [string] $resourceGroupName,
    [string] $containerRegistryName,
    [string] $logAnalyticsWorkspace
)
if("" -ne $prefix)
{
    . ./../prefix_resource_names.ps1 -prefix $prefix
}

Write-Host "Creating Container Registry: $containerRegistryName" -ForegroundColor DarkGreen
az acr create --resource-group "$resourceGroupName" --name $containerRegistryName --sku Standard --workspace $logAnalyticsWorkspace --admin-enabled true -o table     


