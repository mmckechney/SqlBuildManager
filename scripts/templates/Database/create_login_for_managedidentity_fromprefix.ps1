param
(
    [string] $prefix,
    [string] $resourceGroupName, 
    [string] $path = "..\..\..\src\TestConfig"
)

#############################################
# Get set resource name variables from prefix
#############################################
. ./../prefix_resource_names.ps1 -prefix $prefix
. ./../key_file_names.ps1 -prefix $prefix -path $path

Write-Host "Creating AAD Admin for User Assigned Managed Identity" -ForegroundColor Green

$servers= @("$($prefix)sql-a","$($prefix)sql-b")
Write-Host "Getting  managed identity information"  -ForegroundColor Cyan
$clientId = az identity show --resource-group $resourceGroupName --name $identityName --query clientId -o tsv
Write-Host "Using managed identity: $identityName" 
Write-Host "Using managed identity client id : $clientId" 

foreach($server in $servers)
{
    Write-Host "Adding $identityName as AAD Admin for server $server" -ForegroundColor Green
    az sql server ad-admin create --display-name $identityName --object-id $clientId --resource-group $resourceGroupName --server $server
}

