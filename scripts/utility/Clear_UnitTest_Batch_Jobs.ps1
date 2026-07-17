<#
.SYNOPSIS
    Deletes completed Azure Batch jobs matching test naming conventions.
.DESCRIPTION
    Lists Batch jobs in the specified account and deletes those whose names start
    with "SqlBuild", "batch-", or "bat-". Optionally includes active (non-completed)
    jobs when includeActive is true.
.PARAMETER envName
    Azure Developer CLI environment name used to derive the Batch account name.
.PARAMETER resourceGroupName
    Azure resource group. Defaults to rg-{envName}.
.PARAMETER includeActive
    When true, deletes active jobs in addition to completed jobs. Default: false.
#>
param
(
    [string] $envName,
    [string] $resourceGroupName,
    [bool] $includeActive = $false
)

$resourceGroupNameOverride = $resourceGroupName
. (Join-Path (Split-Path $PSScriptRoot -Parent) "prefix_resource_names.ps1") -envName $envName
if (-not [string]::IsNullOrWhiteSpace($resourceGroupNameOverride)) {
    $resourceGroupName = $resourceGroupNameOverride
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
