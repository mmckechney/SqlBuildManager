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

$servers = (az sql server list --resource-group $resourceGroupName  --query [].name ) | ConvertFrom-Json

foreach($server in $servers) 
{ 
    Write-Host "Deleting database server $server in resource group $resourceGroupName, its elastic pools and databases" -ForegroundColor DarkGreen
    az sql server delete --name $server --resource-group $resourceGroupName --yes 
}

