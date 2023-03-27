param
(
    [string] $prefix
)

. ./prefix_resource_names.ps1 -prefix $prefix

az eventhubs namespace delete --name $eventHubNamespaceName --resource-group $resourceGroupName