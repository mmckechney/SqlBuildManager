param
(
    [string] $resourceGroupName,
    [string] $prefix
)
if("" -eq $resourceGroupName)
{
    $resourceGroupName = "$prefix-rg"
}
Write-Host "Create Batch Account from prefix: $prefix"  -ForegroundColor Cyan
$batchAccountName = $prefix + "batchacct"
$storageAccountName = $prefix + "storage"
$userAssignedIdentity = $prefix + "identity"
$location = az group show -n $resourceGroupName -o tsv --query location
Write-Host "Using location: $location" -ForegroundColor Green

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/create_batch_account.ps1 -resourceGroupName $resourceGroupName -batchAccountName $batchAccountName -storageAccountName $storageAccountName -location $location -userAssignedIdentity $userAssignedIdentity

