param
(
    [string] $prefix
)
. ./prefix_resource_names.ps1 -prefix $prefix

Write-Host "Deleting test databases" -ForegroundColor DarkGreen
./Database/delete_databases_fromprefix.ps1 -prefix $prefix

Write-Host "Deleting batch pools" -ForegroundColor DarkGreen
./Batch/delete_batch_pools_fromprefix.ps1 -prefix $prefix

Write-Host "Deleting Service Bus Queues" -ForegroundColor DarkGreen
./delete_servicebus_fromprefix.ps1 -prefix $prefix

Write-Host "Deleting Event Hub" -ForegroundColor DarkGreen
./delete_eventhub_fromprefix.ps1 -prefix $prefix

Write-Host "Stopping AKS Cluster" -ForegroundColor DarkGreen
./kubernetes/stop_aks_cluster_fromprefix.ps1 -prefix $prefix

