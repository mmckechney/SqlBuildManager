<#
.SYNOPSIS
    Deletes non-system storage containers from the test storage account.
.DESCRIPTION
    Lists all blob containers in the prefix storage account and deletes those that
    do not start with "app-" or "eventhubcheckpoint". These are test-generated
    containers that can be safely removed after integration tests.
.PARAMETER prefix
    Environment name prefix used to derive the storage account name.
.PARAMETER resourceGroupName
    Azure resource group. Defaults to {prefix}-rg.
#>
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

$containers = az storage container list --auth-mode login --account-name $storageAccountName --query [].name -o tsv

foreach($container in $containers)
{
    
    if($container.StartsWith("app-") -eq $false -and $container.StartsWith("eventhubcheckpoint") -eq $false)
    {
        Write-Host "Deleting storage container: $container" -ForegroundColor Green
        az storage container delete --name $container --auth-mode  login --account-name $storageAccountName -o tsv
    }
    else
    {
        Write-Host "Preserving storage container: $container" -ForegroundColor Yellow
    }
}
Write-Host "Complete!"


