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

.NOTES
    Azure Batch jobs are Batch data-plane resources and do not have an ARM cleanup
    equivalent. If Batch public network access is disabled, run this script from a
    host with connectivity to the Batch private endpoint.
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

az batch account login --name $batchAccountName --resource-group $resourceGroupName --only-show-errors
if ($LASTEXITCODE -ne 0) {
    throw "Unable to authenticate to Batch account '$batchAccountName'."
}

Write-Host "Retrieving list of completed Batch jobs for $batchAccountName " -ForegroundColor Green
if($includeActive)
{
    $jobs = az batch job list --output tsv --query "[].id" --only-show-errors
}
else {
    $jobs = az batch job list --output tsv --query "[?state=='completed'].id" --only-show-errors
}
if ($LASTEXITCODE -ne 0) {
    throw "Unable to list jobs in Batch account '$batchAccountName'. Verify data-plane network access and RBAC."
}

foreach ($job in $jobs) {
   
    if($job.StartsWith("SqlBuild") -or $job.StartsWith("batch-") -or $job.StartsWith("bat-"))
    {
        Write-Host "Removing job: $($job)" -ForegroundColor Green
        az batch job delete --job-id $job --yes --only-show-errors
        if ($LASTEXITCODE -ne 0) {
            throw "Unable to delete Batch job '$job'."
        }
    }else
    {
        Write-Host "Skipping job: $($job). Doesn't meet name convention." -ForegroundColor Cyan
    }
   
}
