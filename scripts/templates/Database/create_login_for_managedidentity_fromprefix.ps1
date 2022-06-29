param
(
    [string] $prefix,
    [string] $resourceGroupName, 
    [string] $path = "..\..\..\src\TestConfig"
)

if("" -eq $resourceGroupName)
{
    $resourceGroupName =  "$($prefix)-rg"
}
Write-Host "Creating AAD Admin for User Assigned Managed Identity" -ForegroundColor Green

$servers= @("$($prefix)sql-a","$($prefix)sql-b")
Write-Host "Getting  managed identity information"  -ForegroundColor Cyan
$identityName = az identity list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
$clientId = az identity list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].clientId"
Write-Host "Using managed identity: $identityName" 

foreach($server in $servers)
{
    Write-Host "Adding $identityName as AAD Admin for server $server" -ForegroundColor Green
    az sql server ad-admin create --display-name $identityName --object-id $clientId --resource-group $resourceGroupName --server $server
}

