<#
.SYNOPSIS
    [OBSOLETE] Adds secrets to Azure Key Vault from a resource naming prefix.
.DESCRIPTION
    OBSOLETE: This script is no longer used. Resolves Azure resource names from a naming prefix
    and delegates to add_secrets_to_keyvault.ps1. Managed Identity with RBAC is now used instead
    of storing secrets in Key Vault.
#>
param
(
    [Parameter(Mandatory=$true)]
    [string] $prefix,
    [string] $path = "..\..\..\src\TestConfig",
    [string] $resourceGroupName
)

#############################################
# Get set resource name variables from prefix
#############################################
. ./../prefix_resource_names.ps1 -prefix $prefix
. ./../key_file_names.ps1 -prefix $prefix -path $path

Write-Host "Managed Identity Authentication - No secrets stored in Key Vault"  -ForegroundColor Cyan
$path = Resolve-Path $path
Write-Host "Path set to $path" -ForegroundColor DarkGreen

Write-Host "Retrieving resource names from resources in $resourceGroupName with prefix $prefix" -ForegroundColor DarkGreen
Write-Host "Using key vault name:'$keyVaultName'" -ForegroundColor DarkGreen
Write-Host "Using batch account name:'$batchAccountName'" -ForegroundColor DarkGreen
Write-Host "Using storage account name:'$storageAccountName'" -ForegroundColor DarkGreen
Write-Host "Using Event Hub Namespace name:'$eventHubNamespaceName'" -ForegroundColor DarkGreen
Write-Host "Using Service Bus Namespace name:'$serviceBusNamespaceName'" -ForegroundColor DarkGreen


$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/add_secrets_to_keyvault.ps1 -path $path -resourceGroupName $resourceGroupName -keyVaultName $keyVaultName -batchAccountName $batchAccountName -storageAccountName $storageAccountName -eventHubNamespaceName $eventHubNamespaceName -serviceBusNamespaceName $serviceBusNamespaceName

 