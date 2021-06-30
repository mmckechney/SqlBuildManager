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

.PARAMETER  batchPoolOs
Optional. The operating system that the batch pools use: Windows (default), Linux

.PARAMETER  secretsFileName
The name for the settings file you want to create

#>

param(
 [string]
 $subscriptionId,

 [Parameter(Mandatory=$True)]
 [string]
 $resourceGroupName,

[string]
$resourcePrefix,

[Parameter(Mandatory=$True)]
[string]
$secretsFileName,

[Parameter(Mandatory=$True)]
[string]
$runtimeFileName,

[string]
$username,

[string]
$password 

)

if("" -ne $subscriptionId)
{
    Set-AzContext -Subscription $subscriptionId
}


$storageAcctName =$resourcePrefix + "storage"
$namespaceName =$resourcePrefix + "eventhubnamespace"
$eventHubName =$resourcePrefix + "eventhub"
$serviceBusName =$resourcePrefix + "servicebus"


##################################################
# Save settings files for environments
##################################################
$s = Get-AzStorageAccountKey -Name $storageAcctName -ResourceGroupName $resourceGroupName
$e = Get-AzEventHubKey -ResourceGroupName $resourceGroupName -NamespaceName $namespaceName -EventHubName $eventHubName -AuthorizationRuleName batchbuilder
$sb = Get-AzServiceBusKey -ResourceGroup $resourceGroupName -Namespace $serviceBusName -TopicName sqlbuildmanager -Name sbmtopicpolicy

$sb = Get-AzServiceBusKey -ResourceGroup $resourceGroupName -Namespace $serviceBusName -TopicName sqlbuildmanager -Name sbmtopicpolicy
$sbConn = $sb.PrimaryConnectionString

$secretsFile ="
apiVersion: v1
kind: Secret
metadata:
  name: connection-secrets
type: Opaque
data: 
  EventHubConnectionString: $([Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($e.PrimaryConnectionString)))
  ServiceBusTopicConnectionString: $([Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($sbConn))) 
  UserName: $([Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($username))) 
  Password: $([Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($password))) 
  StorageAccountName:  $([Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($storageAcctName))) 
  StorageAccountKey: $([Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($s.Value[0])))"

$secretsFile | Set-Content -Path $secretsFileName
Write-Host "Saved secrets file to " (Resolve-Path  $secretsFileName)

$runtimefile="
kind: ConfigMap 
apiVersion: v1 
metadata:
  name: runtime-properties
data:
  DacpacName: ''
  PackageName: ''
  JobName: ''
  Concurrency: '5'
  ConcurrencyType: 'Count'"

$runtimefile | Set-Content -Path $runtimeFileName
Write-Host "Saved runtime file to " (Resolve-Path  $runtimeFileName)



