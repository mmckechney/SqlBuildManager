param prefix string


var resourceGroupNameVar = '${prefix}-rg'
var batchAccountNameVar = '${prefix}batchacct'
var storageAccountNameVar = '${prefix}storage'
var aciNameVar = '${prefix}aci'
var containerAppEnvNameVar = '${prefix}containerappenv'
var logAnalyticsWorkspaceVar = '${prefix}loganalytics'
var containerRegistryNameVar = '${prefix}containerregistry'
var keyVaultNameVar = '${prefix}keyvault'

var identityNameVar = '${prefix}identity'
var userAssignedIdentityVar = identityNameVar
var userAssignedIdentityNameVar = identityNameVar

var eventHubNamespaceNameVar = '${prefix}eventhubnamespace'
var eventHubNameVar = '${prefix}eventhub'
var serviceBusNamespaceNameVar = '${prefix}servicebus'
var aksClusterNameVar = '${prefix}aks'
var vnetVar = '${prefix}vnet'
var aksSubnetVar = '${prefix}akssubnet'
var nsgNameVar = '${prefix}nsg'
var containerAppSubnetVar = '${prefix}containerappsubnet'
var aciSubnetVar = '${prefix}acisubnet'
var batchSubnetVar = '${prefix}batchsubnet'

//Used with Kubernetes Workload Identity
var serviceAccountNameVar = '${prefix}serviceaccount'
var federatedIdNameVar = '${prefix}federatedidname'

output resourceGroupName string = resourceGroupNameVar
output batchAccountName string = batchAccountNameVar
output storageAccountName string = storageAccountNameVar
output aciName string = aciNameVar
output containerAppEnvName string = containerAppEnvNameVar
output logAnalyticsWorkspace string = logAnalyticsWorkspaceVar
output containerRegistryName string = containerRegistryNameVar
output keyVaultName string = keyVaultNameVar
output identityName string = identityNameVar
output userAssignedIdentity string = userAssignedIdentityVar
output userAssignedIdentityName string = userAssignedIdentityNameVar
output eventHubNamespaceName string = eventHubNamespaceNameVar
output eventHubName string = eventHubNameVar
output serviceBusNamespaceName string = serviceBusNamespaceNameVar
output aksClusterName string = aksClusterNameVar
output vnet string = vnetVar
output aksSubnet string = aksSubnetVar
output nsgName string = nsgNameVar
output containerAppSubnet string = containerAppSubnetVar
output aciSubnet string = aciSubnetVar
output batchSubnet string = batchSubnetVar
output serviceAccountName string = serviceAccountNameVar
output federatedIdName string = federatedIdNameVar
