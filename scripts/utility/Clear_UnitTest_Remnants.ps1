param
(
    [string] $prefix,
    [string] $resourceGroupName,
    [bool] $includeActiveBatchJobs = $true,
    [bool] $removeAllContainerApps = $true
)
if("" -eq $resourceGroupName)
{
    $resourceGroupName = "$prefix-rg"
}

.\Clear_UnitTest_Batch_Jobs.ps1 -prefix $prefix -includeActive $includeActiveBatchJobs
.\Clear_UnitTest_ServiceBus_Topics.ps1 -prefix $prefix
.\Clear_UnitTest_StorageAcct_Containers.ps1 -prefix $prefix
if($removeAllContainerApps)
{
    .\Clean-ContainerApps.ps1 -resourceGroupName $resourceGroupName
}
.\Clear_UnitTest_YamlFiles.ps1 
.\Clear_UnitTest_AciJson.ps1 

Write-Host "Deleting Kubernetes Unit Test resources in sqlbuildmanager namespace" -ForegroundColor Green
kubectl delete all --all -n sqlbuildmanager

Write-Host "Deleting Kubernetes Unit Test ConfigMaps" -ForegroundColor Green
$configMaps = (kubectl get configmap -o name)
foreach($map in $configMaps) 
{ 
    if($map -ne "configmap/kube-root-ca.crt") 
    { 
        kubectl delete $map
    }
}

Write-Host "Deleting Kubernetes Unit Test secrets" -ForegroundColor Green
kubectl delete secret  -n sqlbuildmanager --field-selector=type=Opaque