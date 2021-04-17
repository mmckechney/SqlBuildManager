
<#
.SYNOPSIS
Creates a JSON settings file containing the relevent Azure Batch, Azure Storage and Azure EventHub information to run a sbm.exe batch execution. 

.DESCRIPTION
The JSON settings file created contains all of the paths, names and keys to run a sbm.exe batch execution. 
Is does not contain the SQL Server username and password, nor are the key values encrypted. 
To encrypt the values, simple run sbm batch savesettings --settingsfile <file name>. This will ingest the clear text file and output it back with encrypted text. You can also add the SQL values by including the --username and --password options. 

.PARAMETER subscriptionId
Optional, the subscription id where the resources are located. Will default to the users current default subscription

.PARAMETER resourceGroupName
The resource group where the resources are located

.PARAMETER batchprefix
The prefix characters used when creating the resources (if you used the deploy_batch.ps1 PowerShell or azuredeploy.json). This will be appended with the resource type for each created (batch account, storage account, eventhub namespace, eventhub)

.PARAMETER  batchPoolName
Optional. Name of the batch node pool Default: SqlBuildManagerPoolWindows or SqlBuildManagerPoolLinux

.PARAMETER  batchApplicationPackage
Optional. Name of the batch node pool Default: SqlBuildManagerWindows or SqlBuildManagerLinux

.PARAMETER  batchPoolOs
Optional. The operating system that the batch pools use: Windows (default), Linux

.PARAMETER  settingsFileName
The name for the settings file you want to create

#>

param(
 [string]
 $subscriptionId,

 [Parameter(Mandatory=$True)]
 [string]
 $resourceGroupName,

[string]
 $batchprefix,

 [string]
 $batchPoolName,

 [string]
 $batchApplicationPackage,

 [ValidateSet("Windows","Linux")]
 [string]
 $batchPoolOs = "Windows",

 [Parameter(Mandatory=$True)]
 [string]
 $settingsFileName
)

if("" -ne $subscriptionId)
{
    Set-AzContext -Subscription $subscriptionId
}


$batchAcctName = $batchprefix + "batchacct"
$storageAcctName = $batchprefix + "storage"
$namespaceName = $batchprefix + "eventhubnamespace"
$eventHubName = $batchprefix + "eventhub"
$serviceBusName = $batchprefix + "servicebus"


if("" -eq $batchPoolName)
{
    if($batchPoolOs.ToString().ToLower() -eq "windows")
    {
        $batchPoolName = "SqlBuildManagerPoolWindows"
    }
    else {
        $batchPoolName = "SqlBuildManagerPoolLinux"
    }
}

if("" -eq $batchApplicationPackage)
{
    if($batchPoolOs.ToString().ToLower() -eq "windows")
    {
        $batchApplicationPackage = "SqlBuildManagerWindows"
    }
    else {
        $batchApplicationPackage = "SqlBuildManagerLinux"
    }
}



##################################################
# Save settings files for environments
##################################################
$batch = Get-AzBatchAccountKey -AccountName $batchAcctName -ResourceGroupName $resourceGroupName
$t = Get-AzBatchAccount -AccountName $batchAcctName -ResourceGroupName $resourceGroupName
$s = Get-AzStorageAccountKey -Name $storageAcctName -ResourceGroupName $resourceGroupName
$e = Get-AzEventHubKey -ResourceGroupName $resourceGroupName -NamespaceName $namespaceName -EventHubName $eventHubName -AuthorizationRuleName batchbuilder
$sb = Get-AzServiceBusKey -ResourceGroup $resourceGroupName -Namespace $serviceBusName -Queue sqlbuildmanager -Name sbmpolicy

$settingsFile = [PSCustomObject]@{
    AuthenticationArgs = @{
        UserName = ""
        Password = ""
    }
    BatchArgs = @{
        BatchNodeCount = "2"
        BatchAccountName = $batchAcctName
        BatchAccountKey = "$($batch.PrimaryAccountKey)"
        BatchAccountUrl = "https://$($t.AccountEndpoint)"
        StorageAccountName = $storageAcctName
        StorageAccountKey = "$($s.Value[0])"
        BatchVmSize=  "STANDARD_DS1_V2"
        DeleteBatchPool = $false
        DeleteBatchJob = $false
        PollBatchPoolStatus = $True
        EventHubConnectionString = "$($e.PrimaryConnectionString)"
        BatchPoolOs =  $batchPoolOs
        BatchPoolName = $batchPoolName
        BatchApplicationPackage = $batchApplicationPackage
        ServiceBusConnectionString = "$(sb.PrimaryConnectionString)"
    }
    RootLoggingPath = "C:\temp"
    TimeoutRetryCount = 0
    DefaultScriptTimeout = 500
}

$settingsFile | ConvertTo-Json | Set-Content -Path $settingsFileName
Write-Host "Saved settings file to " (Resolve-Path  $settingsFileName)
