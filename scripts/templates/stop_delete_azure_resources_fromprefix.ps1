param
(
    [string] $prefix
)

Write-Host "Deleteing test databases" -ForegroundColor DarkGreen
./Database/delete_databases_fromprefix.ps1 -prefix $prefix

Write-Host "Deleting batch pools" -ForegroundColor DarkGreen
./Batch/delete_batch_pools_fromprefix.ps1 -prefix $prefix

Write-Host "Stopping AKS Cluster" -ForegroundColor DarkGreen
./kubernetes/stop_aks_cluster_fromprefix.ps1 -prefix $prefix

