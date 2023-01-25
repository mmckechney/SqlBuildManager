#################################################################################
# Sets the resource name variables that are used in the "*fromprefix.ps1" scripts
#################################################################################

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
$keyVaultName =  $prefix + "keyvault"

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


