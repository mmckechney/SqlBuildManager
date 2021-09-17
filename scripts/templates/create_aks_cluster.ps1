param
(
    [string] $prefix,
    [string] $resourceGroupName
)

$aksClusterName = $prefix + "aks"
$keyVaultName = $prefix + "keyvault"
$userAssignedIdentity = $prefix + "identity"
$aksVnet = $prefix + "vnet"
$aksSubnet = "akssubnet"
$virtualKubletSubnet = "virtualnodesubnet"

Write-Host "Making sure container provider is regustered" -ForegroundColor DarkGreen
az feature register --name EnablePodIdentityPreview --namespace Microsoft.ContainerService
az provider register -n Microsoft.ContainerService

Write-Host "Creating VNET $aksVnet and AKS subnet: $aksSubnet, for the AKS cluster $aksClusterName" -ForegroundColor DarkGreen
az network vnet create --resource-group $resourceGroupName --name $aksVnet  --address-prefixes 10.180.0.0/20 --subnet-name $aksSubnet --subnet-prefix 10.180.0.0/22

Write-Host "Creating $virtualKubletSubnet for the AKS cluster $aksClusterName" -ForegroundColor DarkGreen
az network vnet subnet create --resource-group $resourceGroupName --vnet-name $aksVnet --name $virtualKubletSubnet --address-prefixes 10.180.4.0/22

$aksVnetId = az network vnet show --resource-group $resourceGroupName --name $aksVnet --query id -o tsv
$aksSubnetId = az network vnet subnet show --resource-group $resourceGroupName --vnet-name $aksVnet --name $aksSubnet --query id -o tsv

Write-Host "Creating AKS Cluster: $aksClusterName" -ForegroundColor DarkGreen
az aks create --name $aksClusterName --resource-group $resourceGroupName --node-count 1 --enable-managed-identity --network-plugin kubenet # --enable-addons virtual-node --vnet-subnet-id $aksSubnetId --aci-subnet-name $virtualKubletSubnet --yes

Write-Host "Retrieving credentials for: $aksClusterName" -ForegroundColor DarkGreen
az aks get-credentials --name $aksClusterName --resource-group $resourceGroupName --overwrite-existing


# https://docs.microsoft.com/en-us/azure/key-vault/general/key-vault-integrate-kubernetes
##Get cluster information
Write-Host "Collecting cluster information for: $aksClusterName" -ForegroundColor DarkGreen
$clusterInfo = (az aks show --name  $aksClusterName --resource-group $resourceGroupName)  | ConvertFrom-Json -AsHashtable
$principalId = $clusterInfo.identity.principalId
$clientId = $clusterInfo.identityProfile.kubeletidentity.clientId
$nodeResourceGroup = $clusterInfo.nodeResourceGroup
$vaultResourceGroup = $resourceGroupName
$context = (az account show)  | ConvertFrom-Json -AsHashtable
$subscriptionId = $context.id
$tenantId = $context.tenantId
$userAssignedIdentity = $prefix + "identity"


##Install Key Vault CSI driver
Write-Host "Installing Key Vault CSI driver for: $aksClusterName" -ForegroundColor DarkGreen
helm repo add csi-secrets-store-provider-azure https://raw.githubusercontent.com/Azure/secrets-store-csi-driver-provider-azure/master/charts
helm install csi-secrets-store-provider-azure/csi-secrets-store-provider-azure --generate-name

##Add role assignments
Write-Host "Adding Role Assignments" -ForegroundColor DarkGreen
az role assignment create --role "Managed Identity Operator" --assignee $clientId --scope /subscriptions/$subscriptionId/resourcegroups/$vaultResourceGroup
az role assignment create --role "Managed Identity Operator" --assignee $clientId --scope /subscriptions/$subscriptionId/resourcegroups/$nodeResourceGroup
az role assignment create --role "Virtual Machine Contributor" --assignee $clientId --scope /subscriptions/$subscriptionId/resourcegroups/$nodeResourceGroup
az role assignment create --role "Contributor" --assignee $clientId --scope $aksVnetId

# Install AAD pod identity
Write-Host "Installing AAD pod identity for: $aksClusterName" -ForegroundColor DarkGreen
helm repo add aad-pod-identity https://raw.githubusercontent.com/Azure/aad-pod-identity/master/charts
helm install pod-identity aad-pod-identity/aad-pod-identity

# Identity should alread exist, but just in case...
$userAssignedClientId = az identity show --resource-group $resourceGroupName --name $userAssignedIdentity --query "clientId"
if($null -eq $userAssignedClientId)
{
    Write-Host "Creating user assigned identity for: $aksClusterName" -ForegroundColor DarkGreen
    $userAssignedClientId = az identity create -g $resourceGroupName -n $userAssignedIdentity -o tsv --query "clientId"
}

# Assign identity to the AKS VM Scale set
$vmScaleSetName = az vmss list -g $nodeResourceGroup -o tsv --query [].name
Write-Host "Assign identity to the AKS VM Scale set: $vmScaleSetName used by $aksClusterName" -ForegroundColor DarkGreen
$identityResourceId = az identity show --resource-group $resourceGroupName --name $userAssignedIdentity -o tsv --query "id"
az vmss identity assign  --name $vmScaleSetName --resource-group $nodeResourceGroup --identities $identityResourceId

# Set Policy
Write-Host "Setting Key Vault policy for : $keyVaultName" -ForegroundColor DarkGreen
az keyvault set-policy -n $keyVaultName --secret-permissions get --spn $userAssignedClientId
az keyvault set-policy -n $keyVaultName --key-permissions get --spn $userAssignedClientId
az keyvault set-policy -n $keyVaultName --certificate-permissions get --spn $userAssignedClientId