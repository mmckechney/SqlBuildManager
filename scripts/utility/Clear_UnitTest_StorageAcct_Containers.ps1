<#
.SYNOPSIS
    Deletes non-system storage containers from the test storage account.
.DESCRIPTION
    Lists all blob containers in the environment storage account and deletes those that
    do not start with "app-" or "eventhubcheckpoint". These are test-generated
    containers that can be safely removed after integration tests.
.PARAMETER envName
    Azure Developer CLI environment name used to derive the storage account name.
.PARAMETER resourceGroupName
    Azure resource group. Defaults to rg-{envName}.
#>
param
(
    [string] $envName,
    [string] $resourceGroupName
)

$resourceGroupNameOverride = $resourceGroupName
. (Join-Path (Split-Path $PSScriptRoot -Parent) "prefix_resource_names.ps1") -envName $envName
if (-not [string]::IsNullOrWhiteSpace($resourceGroupNameOverride)) {
    $resourceGroupName = $resourceGroupNameOverride
}

Write-Host "Deleting storage containers from $storageAccountName" -ForegroundColor Green
$storageAcctKey = (az storage account keys list --account-name $storageAccountName -o tsv --query '[].value')[0]

$containers = az storage container list --auth-mode login --account-name $storageAccountName --query [].name -o tsv

foreach($container in $containers)
{
    
    if($container.StartsWith("app-") -eq $false -and $container.StartsWith("eventhubcheckpoint") -eq $false)
    {
        Write-Host "Deleting storage container: $container" -ForegroundColor Green
        az storage container delete --name $container --auth-mode  login --account-name $storageAccountName -o tsv
    }
    else
    {
        Write-Host "Preserving storage container: $container" -ForegroundColor Yellow
    }
}
Write-Host "Complete!"
