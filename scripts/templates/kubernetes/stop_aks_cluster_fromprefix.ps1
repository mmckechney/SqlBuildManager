param
(
    [string] $prefix
)

#############################################
# Get set resource name variables from prefix
#############################################
. ./../prefix_resource_names.ps1 -prefix $prefix

Write-Host "Stopping AKS Cluster" -ForegroundColor Green

az aks stop --resource-group $resourceGroupName --name $aksClusterName -o table