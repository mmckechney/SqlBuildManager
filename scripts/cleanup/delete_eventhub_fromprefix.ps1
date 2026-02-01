param
(
    [string] $prefix
)

# Get the repo root
$repoRoot = $env:AZD_PROJECT_PATH
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path (Split-Path $script:MyInvocation.MyCommand.Path -Parent) -Parent) -Parent
}

. "$repoRoot\scripts\prefix_resource_names.ps1" -prefix $prefix

az eventhubs namespace delete --name $eventHubNamespaceName --resource-group $resourceGroupName