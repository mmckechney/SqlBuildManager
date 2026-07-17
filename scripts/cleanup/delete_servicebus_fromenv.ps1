<#
.SYNOPSIS
    Deletes the Service Bus namespace for a given azd environment.
.DESCRIPTION
    Resolves the Service Bus namespace name from the environment name and deletes it from
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

az servicebus namespace delete --name $serviceBusNamespaceName --resource-group $resourceGroupName