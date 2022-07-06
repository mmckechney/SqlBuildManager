param
(
    [string] $prefix,
    [string] $resourceGroupName,
    [bool] $includeActiveBatchJobs = $true,
    [bool] $removeAllContainerApps = $false
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