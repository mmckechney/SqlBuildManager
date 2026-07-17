<#
.SYNOPSIS
    Stops the AKS cluster for a given azd environment.
.DESCRIPTION
    Resolves the AKS cluster name from the environment name and stops it to save costs
    when the cluster is not in use.
.PARAMETER envName
    Azure Developer CLI environment name used to derive resource names.
#>
param
(
    [Parameter(Mandatory=$true)]
    [string] $envName
)
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path (Split-Path $script:MyInvocation.MyCommand.Path -Parent) -Parent) -Parent
}

#############################################
# Get resource name variables from the environment name
#############################################
. "$repoRoot\scripts\prefix_resource_names.ps1" -envName $envName

Write-Host "Stopping AKS Cluster" -ForegroundColor Green

az aks stop --resource-group $resourceGroupName --name $aksClusterName -o table