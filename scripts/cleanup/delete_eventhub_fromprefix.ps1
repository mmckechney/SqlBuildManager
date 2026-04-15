<#
.SYNOPSIS
    Deletes the Event Hubs namespace for a given deployment prefix.
.DESCRIPTION
    Resolves the Event Hubs namespace name from the prefix and deletes it from
    the resource group. Used to tear down messaging resources after testing.
.PARAMETER prefix
    Environment name prefix used to derive resource names.
#>
param
(
    [string] $prefix
)
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path (Split-Path $script:MyInvocation.MyCommand.Path -Parent) -Parent) -Parent
}

. "$repoRoot\scripts\prefix_resource_names.ps1" -prefix $prefix

az eventhubs namespace delete --name $eventHubNamespaceName --resource-group $resourceGroupName