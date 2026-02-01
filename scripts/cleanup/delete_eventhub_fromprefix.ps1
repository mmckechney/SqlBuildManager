param
(
    [string] $prefix
)

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
. "$scriptDir\..\prefix_resource_names.ps1" -prefix $prefix

az eventhubs namespace delete --name $eventHubNamespaceName --resource-group $resourceGroupName