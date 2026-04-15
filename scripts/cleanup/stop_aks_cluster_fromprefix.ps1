<#
.SYNOPSIS
    Stops the AKS cluster for a given deployment prefix.
.DESCRIPTION
    Resolves the AKS cluster name from the prefix and stops it to save costs
    when the cluster is not in use.
.PARAMETER prefix
    Environment name prefix used to derive resource names.
#>
param
(
    [Parameter(Mandatory=$true)]
    [string] $prefix
)
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path (Split-Path $script:MyInvocation.MyCommand.Path -Parent) -Parent) -Parent
}

#############################################
# Get set resource name variables from prefix
#############################################
. "$repoRoot\scripts\prefix_resource_names.ps1" -prefix $prefix

Write-Host "Stopping AKS Cluster" -ForegroundColor Green

az aks stop --resource-group $resourceGroupName --name $aksClusterName -o table