param
(
    [string] $resourceGroupName
)
Write-Host "Container Apps that will be deleted:" -ForegroundColor Green
az containerapp list -g $resourceGroupName -o table

$apps = az containerapp list -g $resourceGroupName -o tsv --query [].name
foreach($app in $apps)
{
    Write-Host "Deleting Container App '$app'"
    az containerapp delete --resource-group $resourceGroupName --name $app --yes
}