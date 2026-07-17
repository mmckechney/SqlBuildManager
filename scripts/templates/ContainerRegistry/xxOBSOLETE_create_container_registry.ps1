<#
.SYNOPSIS
    [OBSOLETE] Creates an Azure Container Registry.
.DESCRIPTION
    OBSOLETE: This script is no longer used. Creates an Azure Container Registry with Standard SKU,
    admin access enabled, and Log Analytics workspace integration using az acr create.
#>
param
(
    [string] $envName,
    [string] $resourceGroupName,
    [string] $containerRegistryName,
    [string] $logAnalyticsWorkspace
)
if("" -ne $envName)
{
    . ./../prefix_resource_names.ps1 -envName $envName
}

Write-Host "Creating Container Registry: $containerRegistryName" -ForegroundColor DarkGreen
az acr create --resource-group "$resourceGroupName" --name $containerRegistryName --sku Standard --workspace $logAnalyticsWorkspace --admin-enabled true -o table     

