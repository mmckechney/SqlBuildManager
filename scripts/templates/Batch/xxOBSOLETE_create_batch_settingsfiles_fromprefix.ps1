<#
.SYNOPSIS
    [OBSOLETE] Creates Batch settings files from a resource naming prefix.
.DESCRIPTION
    OBSOLETE: This script is no longer used. Resolves Azure resource names from a naming prefix
    and delegates to create_batch_settingsfiles.ps1 to generate Azure Batch settings files for
    SQL Build Manager.
#>
param
(
    [Parameter(Mandatory=$true)]
    [string] $prefix,
    [string] $sbmExe = "sbm.exe",
    [string] $path = "..\..\..\src\TestConfig",
    [string] $resourceGroupName,
    [string] $sqlUserName,
    [string] $sqlPassword
)
#############################################
# Get set resource name variables from prefix
#############################################
. ./../prefix_resource_names.ps1 -prefix $prefix
. ./../key_file_names.ps1 -prefix $prefix -path $path

Write-Host "Create batch settings files from prefix: $prefix"  -ForegroundColor Cyan
$path = Resolve-Path $path

Write-Host "Retrieving resource names from resources in $resourceGroupName with prefix $prefix" -ForegroundColor DarkGreen
if([string]::IsNullOrWhiteSpace($sqlUserName))
{
    $sqlUserName = (Get-Content -Path (Join-Path $path "un.txt")).Trim()
    $sqlPassword = (Get-Content -Path (Join-Path $path "pw.txt")).Trim()
}
 
Write-Host "Using key vault name:'$keyVaultName'" -ForegroundColor DarkGreen
Write-Host "Using batch account name:'$batchAccountName'" -ForegroundColor DarkGreen
Write-Host "Using storage account name:'$storageAccountName'" -ForegroundColor DarkGreen
Write-Host "Using Event Hub Namespace name:'$eventHubNamespaceName'" -ForegroundColor DarkGreen
Write-Host "Using Service Bus Namespace name:'$serviceBusNamespaceName'" -ForegroundColor DarkGreen
Write-Host "Using Managed Identity name:'$identityName'" -ForegroundColor DarkGreen
Write-Host "Using VNET name:'$vnet'" -ForegroundColor DarkGreen
Write-Host "Using Subnet name:'$batchSubnet'" -ForegroundColor DarkGreen

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/create_batch_settingsfiles.ps1 -sbmExe $sbmExe -path $path -resourceGroupName $resourceGroupName -keyVaultName $keyVaultName -batchAccountName $batchAccountName -storageAccountName $storageAccountName -eventHubNamespaceName $eventHubNamespaceName -serviceBusNamespaceName $serviceBusNamespaceName -identityName $identityName -sqlUserName $sqlUserName -sqlPassword $sqlPassword -vnetName $vnet -subnetName $batchSubnet 