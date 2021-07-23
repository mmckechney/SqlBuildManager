param
(
    [string] $path,
    [string] $resourceGroupName,
    [string] $prefix,
    [bool] $uploadonly = $false
)
Write-Host "Retrieving resource names from resources in $resourceGroupName with prefix $prefix" -ForegroundColor DarkGreen

$batchAcctName = az batch account list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"

./build_and_upload_batch.ps1 -path $path -resourceGroupName $resourceGroupName -batchAcctName $batchAcctName -uploadonly $uploadonly
