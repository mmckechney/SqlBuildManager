param
(
    [string] $resourceGroupName,
    [string] $batchAccountName,
    [string] $storageAccountName,
    [string] $userAssignedIdentity,
    [string] $location
)
Write-Host "Create Batch Account: $batchAccountName"  -ForegroundColor Cyan
# $identity = az identity show --name $userAssignedIdentity --resource-group $resourceGroupName -o tsv --query id

# Write-Host "Using User Assigned Identity ID : $identity"  -ForegroundColor Green
# az batch account create --name $batchAccountName --resource-group $resourceGroupName --storage-account $storageAccountName --location $location -o table --identity-type UserAssigned --ids $identity 
$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
az deployment group create --resource-group $resourceGroupName --template-file "$($scriptDir)/azuredeploy_batch.bicep" --parameters batchAccountName="$batchAccountName" identityName="$userAssignedIdentity" storageAccountName="$storageAccountName" -o table