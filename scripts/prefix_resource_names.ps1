<#
.SYNOPSIS
    Sets standard Azure resource name variables derived from a deployment prefix.
.DESCRIPTION
    Defines variables for all Azure resource names used by the "*fromprefix.ps1"
    scripts (e.g. storage account, Batch account, AKS cluster, container registry,
    Event Hub, Service Bus, SQL servers, managed identity, and resource group).
    Designed to be dot-sourced by other scripts.
.PARAMETER prefix
    Environment name prefix appended to resource name conventions.
#>

param 
(
    $prefix
)

$resourceGroupName = $prefix + "-rg"
$batchAccountName = $prefix + "batchacct"
$storageAccountName = $prefix + "storage"
$aciName = $prefix + "aci"
$containerAppEnvName = $prefix + "containerappenv"
$logAnalyticsWorkspace = $prefix + "loganalytics"
$containerRegistryName = $prefix + "containerregistry"

$identityName = $prefix + "identity"
$userAssignedIdentity = $identityName
$userAssignedIdentityName = $identityName

$eventHubNamespaceName = $prefix + "eventhubnamespace" 
$eventHubName = $prefix + "eventhub" 
$serviceBusNamespaceName = $prefix + "servicebus" 
$aksClusterName = $prefix + "aks"
$vnet = $prefix + "vnet"
$aksSubnet = $prefix + "akssubnet"
$nsgName = $prefix + "nsg"
$containerAppSubnet = $prefix + "containerappsubnet"
$aciSubnet = $prefix + "acisubnet"
$batchSubnet = $prefix + "batchsubnet"

#Used with Kubernetes Workload Identity
$serviceAccountName= $prefix + "serviceaccount"
$federatedIdName = $prefix + "federatedidname"

#Used with PostgreSQL Flexible Server
$pgServerNameA = $prefix + "pgserver-a"
$pgServerNameB = $prefix + "pgserver-b"


