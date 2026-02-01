param
(
    [string] $prefix
)

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
. "$scriptDir\..\templates\prefix_resource_names.ps1" -prefix $prefix

az servicebus namespace delete --name $serviceBusNamespaceName --resource-group $resourceGroupName