<#
.SYNOPSIS
    Deletes the Event Hubs namespace for a given azd environment.
.DESCRIPTION
    Resolves the Event Hubs namespace name from the environment name and deletes it from
    the resource group. Used to tear down messaging resources after testing.
.PARAMETER envName
    Azure Developer CLI environment name used to derive resource names.
#>
param
(
    [string] $envName
)
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path (Split-Path $script:MyInvocation.MyCommand.Path -Parent) -Parent) -Parent
}

. "$repoRoot\scripts\prefix_resource_names.ps1" -envName $envName

az eventhubs namespace delete --name $eventHubNamespaceName --resource-group $resourceGroupName