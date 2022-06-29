param
(
    [string] $prefix,
    [string] $resourceGroupName
)

$storageAccountName = $prefix + "storage"
if("" -eq $resourceGroupName)
{
    $resourceGroupName = "$prefix-rg"
}

Write-Host "Deleting storage containers from $storageAccountName" -ForegroundColor Green
$storageAcctKey = (az storage account keys list --account-name $storageAccountName -o tsv --query '[].value')[0]

$containers = az storage container list --auth-mode key --account-key "$storageAcctKey" --account-name $storageAccountName --query [].name -o tsv

foreach($container in $containers)
{
    
    if($container.StartsWith("app-") -eq $false -and $container.StartsWith("eventhubcheckpoint") -eq $false)
    {
        Write-Host "Deleting storage container: $container" -ForegroundColor Green
        az storage container delete --name $container --auth-mode key --account-key "$storageAcctKey" --account-name $storageAccountName -o tsv
    }
    else
    {
        Write-Host "Preserving storage container: $container" -ForegroundColor Yellow
    }
}
Write-Host "Complete!"


