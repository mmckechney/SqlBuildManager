param
(
    [string] $prefix,
    [string] $resourceGroupName,
    [bool] $includeActive = $false
)

$batchAccountName = $prefix + "batchacct"
if("" -eq $resourceGroupName)
{
    $resourceGroupName = "$prefix-rg"
}

$batchAcctKey  = az batch account keys list --name $batchAccountName --resource-group $resourceGroupName -o tsv --query 'primary'
$batchAcctEndpoint = az batch account show --name $batchAccountName --resource-group $resourceGroupName -o tsv --query "accountEndpoint"

Write-Host "Retrieving list of completed Batch jobs for $batchAccountName " -ForegroundColor Green
if($includeActive)
{
    $jobs = az batch job list --account-name $batchAccountName --account-endpoint $batchAcctEndpoint --account-key $batchAcctKey -o tsv --query "[].id"
}
else {
    $jobs = az batch job list --account-name $batchAccountName --account-endpoint $batchAcctEndpoint --account-key $batchAcctKey -o tsv --query "[?contains(@.state 'completed')].id"
}

foreach ($job in $jobs) {
   
    if($job.StartsWith("SqlBuild") -or $job.StartsWith("batch-") -or $job.StartsWith("bat-"))
    {
        Write-Host "Removing job: $($job)" -ForegroundColor Green
        az batch job delete --account-name $batchAccountName --account-endpoint $batchAcctEndpoint --account-key $batchAcctKey  --job-id $job --yes
    }else
    {
        Write-Host "Skipping job: $($job). Doesn't meet name convention." -ForegroundColor Cyan
    }
   
}
