param
(
    [string] $prefix
)

. ./prefix_resource_names.ps1 -prefix $prefix

az servicebus namespace delete --name $serviceBusNamespaceName --resource-group $resourceGroupName