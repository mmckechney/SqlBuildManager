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
$aksVnet = $prefix + "vnet"
$nsgName = $prefix + "nsg"


