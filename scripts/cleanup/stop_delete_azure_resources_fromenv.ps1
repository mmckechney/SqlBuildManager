<#
.SYNOPSIS
    Orchestrates full cleanup of Azure resources for a given azd environment.
.DESCRIPTION
    Sequentially deletes SQL databases, Batch pools, Service Bus namespace,
    Event Hub namespace, and stops the AKS cluster by calling the individual
    cleanup scripts for each resource type.
.PARAMETER envName
    Azure Developer CLI environment name used to derive resource names.
#>
param
(
    [string] $envName
)
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path (Split-Path $script:MyInvocation.MyCommand.Path -Parent) -Parent) -Parent
}

. "$repoRoot\scripts\prefix_resource_names.ps1" -envName $envName

Write-Host "Deleting test databases" -ForegroundColor DarkGreen
& "$repoRoot\scripts\cleanup\delete_databases_fromenv.ps1" -envName $envName

Write-Host "Deleting batch pools" -ForegroundColor DarkGreen
& "$repoRoot\scripts\cleanup\delete_batch_pools_fromenv.ps1" -envName $envName

Write-Host "Deleting Service Bus Queues" -ForegroundColor DarkGreen
& "$repoRoot\scripts\cleanup\delete_servicebus_fromenv.ps1" -envName $envName

Write-Host "Deleting Event Hub" -ForegroundColor DarkGreen
& "$repoRoot\scripts\cleanup\delete_eventhub_fromenv.ps1" -envName $envName

Write-Host "Stopping AKS Cluster" -ForegroundColor DarkGreen
& "$repoRoot\scripts\cleanup\stop_aks_cluster_fromenv.ps1" -envName $envName
