<#
.SYNOPSIS
    Lists and deletes all Azure Container Apps in a resource group.
.DESCRIPTION
    Enumerates all Container Apps in the specified resource group, displays them,
    then deletes each one. Used to clean up Container App resources after testing.
.PARAMETER resourceGroupName
    Azure resource group containing the Container Apps to delete.
#>
param
(
    [string] $resourceGroupName
)
Write-Host "Container Apps that will be deleted:" -ForegroundColor Green
az containerapp list -g $resourceGroupName -o table --only-show-errors
if ($LASTEXITCODE -ne 0) {
    throw "Unable to list Container Apps in resource group '$resourceGroupName'."
}

$apps = az containerapp list -g $resourceGroupName -o tsv --query "[].name" --only-show-errors
if ($LASTEXITCODE -ne 0) {
    throw "Unable to list Container App names in resource group '$resourceGroupName'."
}
foreach($app in $apps)
{
    Write-Host "Deleting Container App '$app'"
    az containerapp delete --resource-group $resourceGroupName --name $app --yes --only-show-errors
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to delete Container App '$app'."
    }
}