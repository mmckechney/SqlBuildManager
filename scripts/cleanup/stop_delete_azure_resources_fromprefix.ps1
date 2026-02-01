param
(
    [string] $prefix
)

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path

. "$scriptDir\..\prefix_resource_names.ps1" -prefix $prefix

Write-Host "Deleting test databases" -ForegroundColor DarkGreen
& "$scriptDir\delete_databases_fromprefix.ps1" -prefix $prefix

Write-Host "Deleting batch pools" -ForegroundColor DarkGreen
& "$scriptDir\delete_batch_pools_fromprefix.ps1" -prefix $prefix

Write-Host "Deleting Service Bus Queues" -ForegroundColor DarkGreen
& "$scriptDir\delete_servicebus_fromprefix.ps1" -prefix $prefix

Write-Host "Deleting Event Hub" -ForegroundColor DarkGreen
& "$scriptDir\delete_eventhub_fromprefix.ps1" -prefix $prefix

Write-Host "Stopping AKS Cluster" -ForegroundColor DarkGreen
& "$scriptDir\stop_aks_cluster_fromprefix.ps1" -prefix $prefix

