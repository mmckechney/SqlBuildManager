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
    [string] $vnetName,
    [string] $subnetName,
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

$identity =  az identity show --resource-group $resourceGroupName --name $identityName | ConvertFrom-Json
$subscriptionId = az account show -o tsv --query id
$tenantId = az account show -o tsv --query tenantId

$settingsJsonWindows = Join-Path $path "settingsfile-batch-windows.json"
$settingsJsonWindowsMi = Join-Path $path "settingsfile-batch-windows-mi.json"

$settingsJsonLinux = Join-Path $path "settingsfile-batch-linux.json"
$settingsJsonLinuxMi = Join-Path $path "settingsfile-batch-linux-mi.json"

$settingsJsonWindowsQueue = Join-Path $path "settingsfile-batch-windows-queue.json"
$settingsJsonWindowsQueueMi = Join-Path $path "settingsfile-batch-windows-queue-mi.json"

$settingsJsonLinuxQueue = Join-Path $path "settingsfile-batch-linux-queue.json"
$settingsJsonLinuxQueueMi = Join-Path $path "settingsfile-batch-linux-queue-mi.json"

$settingsJsonWindowsQueueKv = Join-Path $path "settingsfile-batch-windows-queue-keyvault.json"
$settingsJsonWindowsQueueKvMi = Join-Path $path "settingsfile-batch-windows-queue-keyvault-mi.json"

$settingsJsonLinuxQueueKv = Join-Path $path "settingsfile-batch-linux-queue-keyvault.json"
$settingsJsonLinuxQueueKvMi = Join-Path $path "settingsfile-batch-linux-queue-keyvault-mi.json"

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
$params += @("--batchresourcegroup",$resourceGroupName)
$params += @("--batchaccountname",$batchAccountName)
$params += @("--batchaccountkey",$batchAcctKey)
$params += @("--batchaccounturl", "https://$batchAcctEndpoint" )
$params += @("--batchnodecount ","2")  
$params += @("--batchvmsize", "STANDARD_D1_V2")
$params += @("--rootloggingpath","C:/temp")
$params += @("--storageaccountname",$storageAccountName)
$params += @("--storageaccountkey",$storageAcctKey)
$params += @("--defaultscripttimeout", "500")
$params += @("--concurrency", "5")
$params += @("--concurrencytype", "Count")
$params += @("--resourceid",$identity.id)
$params += @("--idrg",$identity.resourceGroup)
$params += @("--tenantid", $tenantId)
$params += @("--subscriptionid", $subscriptionId)
$params += @("--silent")
$params += @("--eventhublogging", "ScriptErrors")
$params += @("--ehrg", $resourceGroupName)
$params += @("--ehsub", $subscriptionId)

if($vnetName -ne "" -and $subnetName -ne "")
{
    $params += @("--vnetname", $vnetName)
    $params += @("--subnetname", $subnetName)
    $params += ("--vnetrg", $resourceGroupName)
}

if($haveSqlInfo)
{
    $params += ("--username",$sqlUserName)
    $params += ("--password",$sqlPassword)
}


$sbConnParam = @("-sb", $serviceBusConnectionString)
$sbNamespaceParam = @("-sb", $serviceBusNamespaceName)

$ehConnParam = @("-eh",$eventHubConnectionString) 
$ehNameParam = @("-eh","""$($eventHubNamespaceName)|$($eventHubName)""")

$keyVaultParam = @("--keyvaultname", $keyVaultName)


# With Password auth (default)
if(Test-Path $settingsJsonWindows)
{
    Remove-Item $settingsJsonWindows
}
Write-Host "Saving settings file to $settingsJsonWindows" -ForegroundColor DarkGreen
$tmpPath = @("--settingsfile", $settingsJsonWindows)
Write-Host $params   $winParams   $tmpPath $tmpPath  $ehConnParam -ForegroundColor DarkYellow
Start-Process $sbmExe -ArgumentList ($params + $winParams + $tmpPath + $ehConnParam ) -Wait -NoNewWindow

if(Test-Path $settingsJsonLinux)
{
    Remove-Item $settingsJsonLinux
}
Write-Host "Saving settings file to $settingsJsonLinux" -ForegroundColor DarkGreen 
$tmpPath = @("--settingsfile",$settingsJsonLinux)
Write-Host $params  $linuxParams  $tmpPath  $ehConnParam -ForegroundColor DarkYellow
Start-Process $sbmExe -ArgumentList ($params + $linuxParams + $tmpPath  + $ehConnParam )  -Wait -NoNewWindow
 

#With Managed Identity auth
$authMi = @("--authtype", "ManagedIdentity");
$authMi += @("--clientid",$identity.clientId)
$authMi += @("--principalid",$identity.principalId)

if(Test-Path $settingsJsonWindowsMi)
{
    Remove-Item $settingsJsonWindowsMi
}
Write-Host "Saving settings file to $settingsJsonWindowsMi" -ForegroundColor DarkGreen
$tmpPath = @("--settingsfile", $settingsJsonWindowsMi)
Write-Host $params  $winParams   $tmpPath $authMi  $ehNameParam -ForegroundColor DarkYellow
Start-Process $sbmExe -ArgumentList ($params + $winParams + $tmpPath + $authMi + $ehNameParam) -Wait -NoNewWindow

if(Test-Path $settingsJsonLinuxMi)
{
    Remove-Item $settingsJsonLinuxMi
}
Write-Host "Saving settings file to $settingsJsonLinuxMi" -ForegroundColor DarkGreen 
$tmpPath = @("--settingsfile",$settingsJsonLinuxMi)
Write-Host $params  $linuxParams $tmpPath  $authMi  $ehNameParam -ForegroundColor DarkYellow
Start-Process $sbmExe -ArgumentList ($params + $linuxParams + $tmpPath + $authMi + $ehNameParam ) -Wait -NoNewWindow


# With Password auth (default)

if(Test-Path $settingsJsonWindowsQueue)
{
    Remove-Item $settingsJsonWindowsQueue
}
Write-Host "Saving settings file to $settingsJsonWindowsQueue" -ForegroundColor DarkGreen
$tmpPath = @("--settingsfile",$settingsJsonWindowsQueue)
Write-Host $params  $winParams  $tmpPath  $sbConnParam   $ehConnParam  -ForegroundColor DarkYellow
Start-Process $sbmExe -ArgumentList ($params + $winParams + $tmpPath + $sbConnParam  + $ehConnParam ) -Wait -NoNewWindow

if(Test-Path $settingsJsonWindowsQueueKv)
{
    Remove-Item $settingsJsonWindowsQueueKv
}
Write-Host "Saving settings file to $settingsJsonWindowsQueueKv" -ForegroundColor DarkGreen 
$tmpPath = @("--settingsfile",$settingsJsonWindowsQueueKv)
Write-Host $params  $winParams  $tmpPath  $sbConnParam  $ehConnParam  $keyVaultParam  -ForegroundColor DarkYellow
Start-Process $sbmExe -ArgumentList ($params + $winParams + $tmpPath + $sbConnParam + $ehConnParam + $keyVaultParam )   -Wait -NoNewWindow

if(Test-Path $settingsJsonLinuxQueue)
{
    Remove-Item $settingsJsonLinuxQueue
}
Write-Host "Saving settings file to $settingsJsonLinuxQueue" -ForegroundColor DarkGreen 
$tmpPath = @("--settingsfile",$settingsJsonLinuxQueue)
Write-Host $params  $linuxParams  $tmpPath $sbConnParam  $ehConnParam  -ForegroundColor DarkYellow
Start-Process $sbmExe -ArgumentList ($params + $linuxParams + $tmpPath + $sbConnParam + $ehConnParam )  -Wait -NoNewWindow
 
if(Test-Path $settingsJsonLinuxQueueKv)
{
    Remove-Item $settingsJsonLinuxQueueKv
}
Write-Host "Saving settings file to $settingsJsonLinuxQueueKv" -ForegroundColor DarkGreen 
$tmpPath = @("--settingsfile",$settingsJsonLinuxQueueKv)
Write-Host $params  $linuxParams  $tmpPath  $sbConnParam   $ehConnParam  $keyVaultParam  -ForegroundColor DarkYellow
Start-Process $sbmExe -ArgumentList ($params + $linuxParams + $tmpPath + $sbConnParam  + $ehConnParam + $keyVaultParam )  -Wait -NoNewWindow

#With Managed Identity auth
if(Test-Path $settingsJsonWindowsQueueMi)
{
    Remove-Item $settingsJsonWindowsQueueMi
}
Write-Host "Saving settings file to $settingsJsonWindowsQueueMi" -ForegroundColor DarkGreen
$tmpPath = @("--settingsfile",$settingsJsonWindowsQueueMi)
Write-Host $params  $winParams  $tmpPath  $authMi  $sbNamespaceParam  $ehNameParam  -ForegroundColor DarkYellow
Start-Process $sbmExe -ArgumentList ($params + $winParams + $tmpPath + $authMi + $sbNamespaceParam  + $ehNameParam) -Wait -NoNewWindow

if(Test-Path $settingsJsonWindowsQueueKvMi)
{
    Remove-Item $settingsJsonWindowsQueueKvMi
}
Write-Host "Saving settings file to $settingsJsonWindowsQueueKvMi" -ForegroundColor DarkGreen 
$tmpPath = @("--settingsfile",$settingsJsonWindowsQueueKvMi)
Write-Host $params  $winParams  $tmpPath  $authMi  $sbNamespaceParam   $ehNameParam  $keyVaultParam -ForegroundColor DarkYellow
Start-Process $sbmExe -ArgumentList ($params + $winParams + $tmpPath + $authMi + $sbNamespaceParam  + $ehNameParam + $keyVaultParam)  -Wait -NoNewWindow

if(Test-Path $settingsJsonLinuxQueueMi)
{
    Remove-Item $settingsJsonLinuxQueueMi
}
Write-Host "Saving settings file to $settingsJsonLinuxQueueMi" -ForegroundColor DarkGreen 
$tmpPath = @("--settingsfile",$settingsJsonLinuxQueueMi)
Write-Host $params  $linuxParams  $tmpPath  $authMi $sbNamespaceParam $ehNameParam -ForegroundColor DarkYellow
Start-Process $sbmExe -ArgumentList ($params + $linuxParams + $tmpPath + $authMi + $sbNamespaceParam  + $ehNameParam) -Wait -NoNewWindow
 
if(Test-Path $settingsJsonLinuxQueueKvMi)
{
    Remove-Item $settingsJsonLinuxQueueKvMi
}
Write-Host "Saving settings file to $settingsJsonLinuxQueueKvMi" -ForegroundColor DarkGreen 
$tmpPath = @("--settingsfile",$settingsJsonLinuxQueueKvMi)
Write-Host $params $linuxParams  $tmpPath $authMi $sbNamespaceParam  $ehNameParam $keyVaultParam -ForegroundColor DarkYellow
Start-Process $sbmExe -ArgumentList ($params + $linuxParams + $tmpPath + $authMi + $sbNamespaceParam  + $ehNameParam + $keyVaultParam) -Wait -NoNewWindow
