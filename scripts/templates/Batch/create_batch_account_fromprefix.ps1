param
(
    [string] $resourceGroupName,
    [string] $prefix
)

#############################################
# Get set resource name variables from prefix
#############################################
. ./../prefix_resource_names.ps1 -prefix $prefix
Write-Host "Create Batch Account from prefix: $prefix"  -ForegroundColor Cyan

$location = az group show -n $resourceGroupName -o tsv --query location
Write-Host "Using location: $location" -ForegroundColor Green

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/create_batch_account.ps1 -resourceGroupName $resourceGroupName -batchAccountName $batchAccountName -storageAccountName $storageAccountName -location $location -userAssignedIdentity $userAssignedIdentity

