param
(
    [string] $prefix
)

#############################################
# Get set resource name variables from prefix
#############################################
. ./../prefix_resource_names.ps1 -prefix $prefix

$servers = (az sql server list --resource-group $resourceGroupName  --query [].name ) | ConvertFrom-Json

foreach($server in $servers) 
{ 
    Write-Host "Deleting database server $server in resource group $resourceGroupName, its elastic pools and databases" -ForegroundColor DarkGreen
    az sql server delete --name $server --resource-group $resourceGroupName  --yes 
}

