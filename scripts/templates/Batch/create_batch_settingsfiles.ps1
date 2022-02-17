param
(
    [string] $sbmExe = "sbm.exe",
    [string] $path,
    [string] $resourceGroupName,
    [string] $keyVaultName, 
    [string] $batchAccountName,
    [string] $storageAccountName,
    [string] $eventHubNamespaceName,
    [string] $serviceBusNamespaceName,
    [string] $identityName,
    [string] $sqlUserName,
    [string] $sqlPassword
)
Write-Host "Create batch settings files"  -ForegroundColor Cyan
$path = Resolve-Path $path
Write-Host "Output path set to $path" -ForegroundColor DarkGreen
Write-Host "Retrieving keys from resources in $resourceGroupName" -ForegroundColor DarkGreen

$haveSqlInfo = $true
if([string]::IsNullOrWhiteSpace($sqlUserName) -or [string]::IsNullOrWhiteSpace($sqlPassword))
{
    $haveSqlInfo = $false
}

$batchAcctKey  = az batch account keys list --name $batchAccountName --resource-group $resourceGroupName -o tsv --query 'primary'
$batchAcctEndpoint = az batch account show --name $batchAccountName --resource-group $resourceGroupName -o tsv --query "accountEndpoint"

$storageAcctKey = (az storage account keys list --account-name $storageAccountName -o tsv --query '[].value')[0]

$eventHubName = az eventhubs eventhub list  --resource-group $resourceGroupName --namespace-name $eventhubNamespaceName -o tsv --query "[?contains(@.name '$prefix')].name"
$eventHubAuthRuleName = az eventhubs eventhub authorization-rule list  --resource-group $resourceGroupName --namespace-name $eventhubNamespaceName --eventhub-name $eventHubName -o tsv --query [].name
$eventHubConnectionString = az eventhubs eventhub authorization-rule keys list --resource-group $resourceGroupName --namespace-name $eventHubNamespaceName --eventhub-name $eventHubName --name $eventHubAuthRuleName -o tsv --query "primaryConnectionString"

$serviceBusTopicAuthRuleName = az servicebus topic authorization-rule list --resource-group $resourceGroupName --namespace-name $serviceBusNamespaceName --topic-name "sqlbuildmanager" -o tsv --query "[].name"
$serviceBusConnectionString = az servicebus topic authorization-rule keys list --resource-group $resourceGroupName --namespace-name $serviceBusNamespaceName --topic-name "sqlbuildmanager" --name $serviceBusTopicAuthRuleName -o tsv --query "primaryConnectionString"

$identity =  az identity show --resource-group $resourceGroupName --name $identityName | ConvertFrom-Json -AsHashtable
$subscriptionId = az account show -o tsv --query id

$settingsJsonWindows = Join-Path $path "settingsfile-windows.json"
$settingsJsonLinux = Join-Path $path "settingsfile-linux.json"

$settingsJsonWindowsQueue = Join-Path $path "settingsfile-windows-queue.json"
$settingsJsonLinuxQueue = Join-Path $path "settingsfile-linux-queue.json"

$settingsJsonWindowsQueueKv = Join-Path $path "settingsfile-windows-queue-keyvault.json"
$settingsJsonLinuxQueueKv = Join-Path $path "settingsfile-linux-queue-keyvault.json"

$keyFile = Join-Path $path "settingsfilekey.txt"
if($false -eq (Test-Path $keyFile))
{
    $AESKey = New-Object Byte[] 32
    [Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($AESKey)
    $settingsFileKey = [System.Convert]::ToBase64String($AESKey);
    $settingsFileKey |  Set-Content -Path $keyFile
}


$winParams = @("--batchpoolname","SqlBuildManagerPoolWindows")
$winParams += ("-os", "Windows")

$linuxParams = @("--batchpoolname","SqlBuildManagerPoolLinux")
$linuxParams += ("-os", "Linux")

$params = @("batch", "savesettings")
$params += @("--settingsfilekey",$keyFile)
$params += @("--batchaccountname",$batchAccountName)
$params += @("--batchaccountkey",$batchAcctKey)
$params += @("--batchaccounturl", "https://$batchAcctEndpoint" )
$params += @("--batchnodecount ","2")  
$params += @("--batchvmsize", "STANDARD_DS1_V2")
$params += @("--rootloggingpath","C:/temp")
$params += @("--storageaccountname",$storageAccountName)
$params += @("--storageaccountkey",$storageAcctKey)
$params += @("-eh",$eventHubConnectionString) 
$params += @("--defaultscripttimeout", "500")
$params += @("--concurrency", "5")
$params += @("--concurrencytype", "Count")
$params += @("--clientid",$identity.clientId)
$params += @("--principalid",$identity.principalId)
$params += @("--resourceid",$identity.id)
$params += @("--idrg",$identity.resourceGroup)
$params += @("--subscriptionid", $subscriptionId)
$params += @("--silent")

if($haveSqlInfo)
{
    $params += ("--username",$sqlUserName)
    $params += ("--password",$sqlPassword)
}

Write-Host "Saving settings file to $settingsJsonWindows" -ForegroundColor DarkGreen
$tmpPath = @("--settingsfile", $settingsJsonWindows)
Start-Process $sbmExe -ArgumentList ($params + $winParams + $tmpPath)

Write-Host "Saving settings file to $settingsJsonLinux" -ForegroundColor DarkGreen 
$tmpPath = @("--settingsfile",$settingsJsonLinux)
Start-Process $sbmExe -ArgumentList ($params + $linuxParams + $tmpPath)  

$params += @("-sb", $serviceBusConnectionString)

Write-Host "Saving settings file to $settingsJsonWindowsQueue" -ForegroundColor DarkGreen
$tmpPath = @("--settingsfile",$settingsJsonWindowsQueue)
Start-Process $sbmExe -ArgumentList ($params + $winParams + $tmpPath)

Write-Host "Saving settings file to $settingsJsonWindowsQueueKv" -ForegroundColor DarkGreen 
$tmpPath = @("--settingsfile",$settingsJsonWindowsQueueKv)
Start-Process $sbmExe -ArgumentList ($params + $winParams + $tmpPath)  
 

Write-Host "Saving settings file to $settingsJsonLinuxQueue" -ForegroundColor DarkGreen 
$tmpPath = @("--settingsfile",$settingsJsonLinuxQueue)
Start-Process $sbmExe -ArgumentList ($params + $linuxParams + $tmpPath)  
 
Write-Host "Saving settings file to $settingsJsonLinuxQueueKv" -ForegroundColor DarkGreen 
$tmpPath = @("--settingsfile",$settingsJsonLinuxQueueKv)
Start-Process $sbmExe -ArgumentList ($params + $linuxParams + $tmpPath)  

    
