<#
.SYNOPSIS
    Orchestrates full cleanup of Azure resources for a given deployment prefix.
.DESCRIPTION
    Sequentially deletes SQL databases, Batch pools, Service Bus namespace,
    Event Hub namespace, and stops the AKS cluster by calling the individual
    cleanup scripts for each resource type.
.PARAMETER prefix
    Environment name prefix used to derive resource names.
#>
param
(
    [string] $prefix
)
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path (Split-Path $script:MyInvocation.MyCommand.Path -Parent) -Parent) -Parent
}

. "$repoRoot\scripts\prefix_resource_names.ps1" -prefix $prefix

Write-Host "Deleting test databases" -ForegroundColor DarkGreen
& "$repoRoot\scripts\cleanup\delete_databases_fromprefix.ps1" -prefix $prefix

Write-Host "Deleting batch pools" -ForegroundColor DarkGreen
& "$repoRoot\scripts\cleanup\delete_batch_pools_fromprefix.ps1" -prefix $prefix

Write-Host "Deleting Service Bus Queues" -ForegroundColor DarkGreen
& "$repoRoot\scripts\cleanup\delete_servicebus_fromprefix.ps1" -prefix $prefix

Write-Host "Deleting Event Hub" -ForegroundColor DarkGreen
& "$repoRoot\scripts\cleanup\delete_eventhub_fromprefix.ps1" -prefix $prefix

Write-Host "Stopping AKS Cluster" -ForegroundColor DarkGreen
& "$repoRoot\scripts\cleanup\stop_aks_cluster_fromprefix.ps1" -prefix $prefix

