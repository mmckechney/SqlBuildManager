param
(
    [Parameter(Mandatory=$true)]
    [string] $prefix
)

# Get the repo root
$repoRoot = $env:AZD_PROJECT_PATH
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path (Split-Path $script:MyInvocation.MyCommand.Path -Parent) -Parent) -Parent
}

#############################################
# Get set resource name variables from prefix
#############################################
. "$repoRoot\scripts\prefix_resource_names.ps1" -prefix $prefix

Write-Host "Stopping AKS Cluster" -ForegroundColor Green

az aks stop --resource-group $resourceGroupName --name $aksClusterName -o table