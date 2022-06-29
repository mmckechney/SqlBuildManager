param
(
    [string] $prefix,
    [string] $resourceGroupName,
    [bool] $includeActive = $false
)
if("" -eq $resourceGroupName)
{
    $resourceGroupName = "$prefix-rg"
}

.\Clear_UnitTest_Batch_Jobs.ps1 -prefix $prefix -includeActive $includeActive
.\Clear_UnitTest_ServiceBus_Topics.ps1 -prefix $prefix
.\Clear_UnitTest_StorageAcct_Containers.ps1 -prefix $prefix