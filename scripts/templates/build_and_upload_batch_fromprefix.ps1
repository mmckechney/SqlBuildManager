param
(
    [string] $path,
    [string] $resourceGroupName,
    [string] $prefix,
    [bool] $uploadonly = $false
)
Write-Host "Retrieving resource names from resources in $resourceGroupName with prefix $prefix" -ForegroundColor DarkGreen

$batchAccountName = az batch account list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
Write-Host "Using batch account name:'$batchAccountName'" -ForegroundColor DarkGreen

./build_and_upload_batch.ps1 -path $path -resourceGroupName $resourceGroupName -batchAcctName $batchAccountName -uploadonly $uploadonly
