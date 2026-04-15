<#
.SYNOPSIS
    Deletes all SQL Server instances in the resource group for a given deployment prefix.
.DESCRIPTION
    Lists all SQL servers in the prefix resource group and deletes each one,
    including their elastic pools and databases. Used to tear down test databases.
.PARAMETER prefix
    Environment name prefix used to derive resource names.
#>
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

