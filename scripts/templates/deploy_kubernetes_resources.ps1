
param(
[Parameter(Mandatory=$True)]
[string]
$subscriptionId,

[Parameter(Mandatory=$True)]
[string]
$resourceGroupName,

[string]
$resourceGroupLocation,

[Parameter(Mandatory=$True)]
[string]
$resourcePrefix,

[Parameter(Mandatory=$True)]
[string]
$outputpath,

[Parameter(Mandatory=$False)]
[string]
$sqlServerUserName,

[Parameter(Mandatory=$False)]
[string]
$sqlServerPassword

)

if($null -ne $subscriptionId)
{
    Write-Host "Selecting subscription '$subscriptionId'";
    Set-AzContext -SubscriptionId $subscriptionId
}

Write-Host "Creating Resourcegroup : $resourceGroupName"
if($null -eq (Get-AzResourceGroup -ResourceGroupName $resourceGroupName -Location $resourceGroupLocation -ErrorAction SilentlyContinue))
{
    New-AzResourceGroup  -ResourceGroupName $resourceGroupName -Location $resourceGroupLocation 
}

$aksCluster = $resourcePrefix + "_aks"
Write-Host "Creating AKS cluster: $($aksCluster)";
New-AzAksCluster -ResourceGroupName $resourceGroupName -Name $aksCluster -NodeCount 1

#Write-Host "Installing kubectl";
#Install-AzAksKubectl

Write-Host "Importing Cluster credentials for kubectl";
Import-AzAksCredential -ResourceGroupName $resourceGroupName -Name $aksCluster -Force

$sFile =  Join-Path $outputpath "secrets.yaml"
$rFile =  Join-Path $outputpath "runtime.yaml"
./Create_SecretsFile.ps1 -subscriptionId $subscriptionId -resourceGroupName $resourceGroupName -resourcePrefix $resourcePrefix -secretsFileName $sFile -runtimeFile $rFile -username $sqlServerUserName -password $sqlServerPassword