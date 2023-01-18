param
(
    [string] $prefix
)

#############################################
# Get set resource name variables from prefix
#############################################
. ./../prefix_resource_names.ps1 -prefix $prefix


$batchAccts = az batch account list --resource-group $resourceGroupName  --query "[].{Name:name, AccountEndpoint:accountEndpoint}" | ConvertFrom-Json

foreach($batch in $batchAccts)
{
    $pools = az batch pool list --account-endpoint $batch.AccountEndpoint --account-name $batch.Name -o tsv --query [].id

    foreach($pool in $pools)
    {
        Write-Host "Deleting batch pool $pool from account $($batch.Name)" -ForegroundColor Cyan
        az batch pool delete --pool-id $pool --account-name $batch.Name  --account-endpoint $batch.AccountEndpoint --yes -o table
    }
}