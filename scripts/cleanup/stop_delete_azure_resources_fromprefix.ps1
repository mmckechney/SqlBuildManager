param
(
    [string] $prefix
)

# Get the repo root
$repoRoot = $env:AZD_PROJECT_PATH
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

