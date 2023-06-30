param
(
    [string] $prefix,
    [string] $resourceGroupName,
    [string] $path = "..\..\..\src\TestConfig",
    [bool] $includeContainerRegistry
)


#############################################
# Get set resource name variables from prefix
#############################################
. ./../prefix_resource_names.ps1 -prefix $prefix
. ./../key_file_names.ps1 -prefix $prefix -path $path

$exists = az aks show --name $aksClusterName --resource-group $resourceGroupName --query name -o tsv

if($aksClusterName -ne $exists)
{
  Write-Host "Create AKS cluster $aksClusterName"  -ForegroundColor Cyan

  $scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
  az deployment group create --resource-group $resourceGroupName --template-file "$($scriptDir)/../Modules/aks.bicep" --parameters `
    aksClusterName=$aksClusterName `
    identityName=$userAssignedIdentity `
    vnetName=$vnet `
    subnetName=$aksSubnet `
    logAnalyticsWorkspaceName=$logAnalyticsWorkspace `
    serviceAccountName=$serviceAccountName `
    federatedIdName=$federatedIdName `
    -o table
}
else
{
    Write-Host "AKS Cluster: $aksClusterName already exists" -ForegroundColor DarkGreen
}
Write-Host "Retrieving credentials for: $aksClusterName to be able to run kubectl commands" -ForegroundColor DarkGreen
az aks get-credentials --name $aksClusterName --resource-group $resourceGroupName --overwrite-existing --admin -o table

Write-Host "Create 'sqlbuildmanager' Kubernetes namespace" -ForegroundColor DarkGreen
kubectl create namespace sqlbuildmanager


$userAssignedClientId = az identity show --resource-group $resourceGroupName --name $userAssignedIdentity --query "clientId"

#create Kubernetes service principal
Write-Host "Creating Kubernetes Service Principal $serviceAccountName associated with $userAssignedIdentity having Client ID $userAssignedClientId" -ForegroundColor DarkGreen
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
