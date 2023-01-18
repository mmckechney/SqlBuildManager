param
(
    [string] $prefix,
    [string] $resourceGroupName,
    [string] $path = "..\..\..\src\TestConfig",
    [bool] $includeContainerRegistry
)
Write-Host "Create AKS cluster"  -ForegroundColor Cyan

#############################################
# Get set resource name variables from prefix
#############################################
. ./../prefix_resource_names.ps1 -prefix $prefix
. ./../key_file_names.ps1 -prefix $prefix -path $path

$aksSubnet = "akssubnet"
#$virtualKubletSubnet = "virtualnodesubnet"

# Install the aks-preview extension
Write-Host "Adding AKS preview extension" -ForegroundColor DarkGreen
az extension add --name aks-preview
az extension update --name aks-preview

# Register the preview features
Write-Host "Registering AKS preview features" -ForegroundColor DarkGreen
az feature register --namespace "Microsoft.ContainerService" --name "EnableWorkloadIdentityPreview" -o table
az feature register --namespace "Microsoft.ContainerService" --name "EnableOIDCIssuerPreview" -o table
while(( az feature list -o table --query "[?contains(name, 'Microsoft.ContainerService/EnableWorkloadIdentityPreview')].properties.state" -o tsv) -eq "Registering")
{
    Write-Host "Waiting for 'EnableWorkloadIdentityPreview' feature registration to complete..."
    Start-Sleep -s 10
}
while(( az feature list -o table --query "[?contains(name, 'Microsoft.ContainerService/EnableOIDCIssuerPreview')].properties.state" -o tsv) -eq "Registering")
{
    Write-Host "Waiting for 'EnableOIDCIssuerPreview' feature registration to complete..."
    Start-Sleep -s 10
}
az provider register --namespace Microsoft.ContainerService

# Identity should alread exist, but just in case...
$userAssignedClientId = az identity show --resource-group $resourceGroupName --name $userAssignedIdentity --query "clientId"
if($null -eq $userAssignedClientId)
{
    Write-Host "Creating user assigned identity for: $aksClusterName" -ForegroundColor DarkGreen
    $userAssignedClientId = az identity create --resource-group $resourceGroupName --name $userAssignedIdentity -o tsv --query "clientId"
}

Write-Host "Creating Network Security Group $nsgName" -ForegroundColor DarkGreen
az network nsg create  --resource-group $resourceGroupName --name $nsgName -o table

Write-Host "Creating VNET $aksVnet and AKS subnet: $aksSubnet, for the AKS cluster $aksClusterName" -ForegroundColor DarkGreen
az network vnet create --resource-group $resourceGroupName --name $aksVnet  --address-prefixes 10.180.0.0/20 --subnet-name $aksSubnet --subnet-prefix 10.180.0.0/22 --network-security-group  $nsgName -o table

Write-Host "Retrieving ID values for VNET '$aksVnet' and Subnet '$aksSubnet '"

$aksSubnetId = az network vnet subnet show --resource-group $resourceGroupName --vnet-name $aksVnet --name $aksSubnet --query id -o tsv

# Virtual Node don't yet work for Keyvault sourced secrets
# $aksVnetId = az network vnet show --resource-group $resourceGroupName --name $aksVnet --query id -o tsv
#Write-Host "Creating $virtualKubletSubnet for the AKS cluster $aksClusterName" -ForegroundColor DarkGreen
#az network vnet subnet create --resource-group $resourceGroupName --vnet-name $aksVnet --name $virtualKubletSubnet --address-prefixes 10.180.4.0/22 --network-security-group  $nsgName -o table
# Write-Host "Creating AKS Cluster with virtual node add-on: $aksClusterName" -ForegroundColor DarkGreen
# az aks create --name $aksClusterName --resource-group $resourceGroupName --node-count 1 --enable-managed-identity --enable-pod-identity --network-plugin azure --enable-addons virtual-node --vnet-subnet-id $aksSubnetId --aci-subnet-name $virtualKubletSubnet --yes -o table

Write-Host "Creating AKS Cluster: $aksClusterName" -ForegroundColor DarkGreen
az aks create --name $aksClusterName --resource-group $resourceGroupName --node-count 1 --enable-oidc-issuer --enable-workload-identity --network-plugin azure --vnet-subnet-id $aksSubnetId  --generate-ssh-keys --node-osdisk-type Ephemeral --node-vm-size Standard_DS3_v2 --yes -o table

if($includeContainerRegistry)
{
    Write-Host "Attaching Azure Container Registry '$containerRegistryName' to AKS Cluster: $aksClusterName" -ForegroundColor DarkGreen
    az aks update --name $aksClusterName --resource-group $resourceGroupName --attach-acr $containerRegistryName -o table
}

Write-Host "Retrieving credentials for: $aksClusterName to be able to run kubectl commands" -ForegroundColor DarkGreen
az aks get-credentials --name $aksClusterName --resource-group $resourceGroupName --overwrite-existing -o table

Write-Host "Create 'sqlbuildmanager' Kubernetes namespace" -ForegroundColor DarkGreen
kubectl create namespace sqlbuildmanager

# Get OIDC issuer URL
Write-Host "Retrieving OIDC issuer URL" -ForegroundColor DarkGreen
$AKS_OIDC_Issuer= az aks show -n $aksClusterName -g $resourceGroupName --query "oidcIssuerProfile.issuerUrl" -o tsv

#create Kubernetes service principal
Write-Host "Creating Kubernetes Service Principal $serviceAccountName associated with $userAssignedIdentity" -ForegroundColor DarkGreen
$svcAcctYml = "
 apiVersion: v1
 kind: ServiceAccount
 metadata:
   annotations:
     azure.workload.identity/client-id: $userAssignedClientId 
   labels:
     azure.workload.identity/use: 'true'
   name: $serviceAccountName
   namespace: sqlbuildmanager"

$svcAcctYml | kubectl apply -f -

Write-Host "Setting OIDC issuer associated with $serviceAccountName and identity $userAssignedIdentity" -ForegroundColor DarkGreen
az identity federated-credential create --name $federatedIdName --identity-name $userAssignedIdentity --resource-group $resourceGroupName --issuer $AKS_OIDC_Issuer --subject system:serviceaccount:sqlbuildmanager:$($serviceAccountName)



# Set Policy
Write-Host "Setting Key Vault policy for : $keyVaultName" -ForegroundColor DarkGreen
az keyvault set-policy --name $keyVaultName --resource-group $resourceGroupName --secret-permissions get --spn $userAssignedClientId -o table
az keyvault set-policy --name $keyVaultName --resource-group $resourceGroupName --key-permissions get --spn $userAssignedClientId -o table
az keyvault set-policy --name $keyVaultName --resource-group $resourceGroupName --certificate-permissions get --spn $userAssignedClientId -o table