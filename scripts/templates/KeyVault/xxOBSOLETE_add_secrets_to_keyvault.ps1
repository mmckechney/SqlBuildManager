<#
.SYNOPSIS
    [OBSOLETE] Adds service connection secrets to Azure Key Vault.
.DESCRIPTION
    OBSOLETE: This script is no longer used. Previously stored connection strings and keys for
    Batch, Storage, Event Hub, and Service Bus in Azure Key Vault. Now displays a deprecation
    message indicating that Managed Identity with RBAC is used for all service-to-service
    communication instead.
#>
param
(
    [string] $path,
    [string] $resourceGroupName,
    [string] $keyVaultName,
    [string] $batchAccountName,
    [string] $storageAccountName,
    [string] $eventHubNamespaceName,
    [string] $serviceBusNamespaceName
)
Write-Host "Key Vault Secrets - DEPRECATED"  -ForegroundColor Yellow
Write-Host "This deployment uses Managed Identity with RBAC for all service-to-service communication."  -ForegroundColor Yellow
Write-Host "No connection strings or keys are stored in Key Vault."  -ForegroundColor Yellow
Write-Host ""  -ForegroundColor Yellow
Write-Host "Services use Managed Identity for authentication:"  -ForegroundColor Cyan
Write-Host "  - Azure SQL: Entra ID (Azure AD) only authentication"  -ForegroundColor Cyan
Write-Host "  - Event Hub: EventHubsDataReceiver/EventHubsDataSender RBAC roles"  -ForegroundColor Cyan
Write-Host "  - Service Bus: ServiceBusDataOwner RBAC role"  -ForegroundColor Cyan
Write-Host "  - Storage: StorageBlobDataContributor RBAC role"  -ForegroundColor Cyan
Write-Host "  - Batch: Uses user-assigned managed identity"  -ForegroundColor Cyan
Write-Host ""  -ForegroundColor Yellow
Write-Host "Application code should use DefaultAzureCredential for authentication."  -ForegroundColor Cyan
