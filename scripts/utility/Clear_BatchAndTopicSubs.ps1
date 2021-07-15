#Connect-AzAccount

$jsondata = Get-Content -Raw -Path ../../src/TestConfig/settingsfile-windows.json | ConvertFrom-Json
$batchContext = Get-AzBatchAccountKey -AccountName $jsondata.BatchArgs.BatchAccountName
$rgName = $jsondata.BatchArgs.BatchAccountName -replace "BatchAcct", ""
$storageKey = Get-AzStorageAccountKey -ResourceGroupName $rgName -Name $jsondata.BatchArgs.StorageAccountName
$storageContext = New-AzStorageContext -StorageAccountName $jsondata.BatchArgs.StorageAccountName -StorageAccountKey $storageKey[0].Value

$jobs = Get-AzBatchJob -BatchContext $batchContext
foreach ($job in $jobs) {
    if($job.State -eq "Completed")
    {
        Write-Output "Removing job: $($job.Id)"
        Remove-AzBatchJob -Id $job.Id -BatchContext $batchContext -Force

        $storageContainerName = ($job.Id -replace "SqlBuildManagerJobLinux_", "") -replace "SqlBuildManagerJobWindows_", ""

        Write-Output "Removing storage container : $($storageContainerName)"
        $container = Get-AzStorageContainer -Name "$($storageContainerName)*" -Context $storageContext
        if($null -ne $container)
        {
            Remove-AzStorageContainer -Name $container[0].Name -Context $storageContext -Force
        }
    }
}

$sbNamespaceName = $rgName + "servicebus"
$subs = Get-AzServiceBusSubscription -ResourceGroupName $rgName -Namespace $sbNamespaceName -Topic sqlbuildmanager

if($null -ne $subs)
{
    foreach($sub in $subs)
    {
        Write-Output "Removing service bus topic subscription : $($sub.Name)"
        Remove-AzServiceBusSubscription -ResourceGroupName  $rgName -Namespace $sbNamespaceName -Topic sqlbuildmanager -Name $sub.Name -ErrorAction SilentlyContinue
    }
}