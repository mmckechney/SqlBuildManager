param
(
    [string] $resourceGroupName,
    [string] $containerRegistryName,
    [string] $logAnalyticsWorkspace
)


Write-Host "Creating Container Registry: $containerRegistryName" -ForegroundColor DarkGreen
az acr create --resource-group "$resourceGroupName" --name $containerRegistryName --sku Standard --workspace $logAnalyticsWorkspace --admin-enabled true -o table     


