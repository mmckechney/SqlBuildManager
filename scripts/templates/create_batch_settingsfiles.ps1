param
(
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


$AESKey = New-Object Byte[] 32
[Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($AESKey)
$settingsFileKey = [System.Convert]::ToBase64String($AESKey);

$keyFile = Join-Path $path "settingsfilekey.txt"
$settingsFileKey |  Set-Content -Path $keyFile


if($haveSqlInfo)
{
    $tmpPath = $settingsJsonWindows
    Write-Host "Saving settings file to $tmpPath" -ForegroundColor DarkGreen
    ..\..\src\SqlBuildManager.Console\bin\Debug\net5.0\sbm.exe batch savesettings --batchpoolname "SqlBuildManagerPoolWindows" -os "Windows" --settingsfile "$tmpPath"  --settingsfilekey "$keyFile" --username "$sqlUserName" --password "$sqlPassword" --batchaccountname "$batchAccountName" --batchaccountkey "$batchAcctKey" --batchaccounturl "https://$batchAcctEndpoint" --batchnodecount 2  --batchvmsize "STANDARD_DS1_V2" --rootloggingpath "C:/temp" --storageaccountname "$storageAccountName"  --storageaccountkey "$storageAcctKey" -eh "$eventHubConnectionString" --defaultscripttimeout 500 --concurrency 5 --concurrencytype "Count" --clientid "$($identity.clientId)" --principalid "$($identity.principalId)" --resourceid "$($identity.id)" --resourcegroup "$($identity.resourceGroup)" --subscriptionid $subscriptionId --silent 

    $tmpPath = $settingsJsonWindowsQueue
    Write-Host "Saving settings file to $tmpPath" -ForegroundColor DarkGreen
    ..\..\src\SqlBuildManager.Console\bin\Debug\net5.0\sbm.exe batch savesettings --batchpoolname "SqlBuildManagerPoolWindows" -os "Windows" -sb "$serviceBusConnectionString"  --settingsfile "$tmpPath"  --settingsfilekey "$keyFile" --username "$sqlUserName" --password "$sqlPassword" --batchaccountname "$batchAccountName"  --batchaccountkey "$batchAcctKey" --batchaccounturl "https://$batchAcctEndpoint" --batchnodecount 2  --batchvmsize "STANDARD_DS1_V2" --rootloggingpath "C:/temp" --storageaccountname "$storageAccountName"  --storageaccountkey "$storageAcctKey" -eh "$eventHubConnectionString" --defaultscripttimeout 500 --concurrency 5 --concurrencytype "Count" --clientid "$($identity.clientId)" --principalid "$($identity.principalId)" --resourceid "$($identity.id)" --resourcegroup "$($identity.resourceGroup)" --subscriptionid $subscriptionId --silent 

    $tmpPath = $settingsJsonWindowsQueueKv
    Write-Host "Saving settings file to $tmpPath" -ForegroundColor DarkGreen
    ..\..\src\SqlBuildManager.Console\bin\Debug\net5.0\sbm.exe batch savesettings --batchpoolname "SqlBuildManagerPoolWindows" -os "Windows" -sb "$serviceBusConnectionString" -kv "$keyVaultName" --settingsfile "$tmpPath"  --settingsfilekey "$keyFile" --username "$sqlUserName" --password "$sqlPassword" --batchaccountname "$batchAccountName"  --batchaccountkey "$batchAcctKey" --batchaccounturl "https://$batchAcctEndpoint" --batchnodecount 2  --batchvmsize "STANDARD_DS1_V2" --rootloggingpath "C:/temp" --storageaccountname "$storageAccountName"  --storageaccountkey "$storageAcctKey" -eh "$eventHubConnectionString" --defaultscripttimeout 500 --concurrency 5 --concurrencytype "Count" --clientid "$($identity.clientId)" --principalid "$($identity.principalId)" --resourceid "$($identity.id)" --resourcegroup "$($identity.resourceGroup)" --subscriptionid $subscriptionId --silent 

    $tmpPath = $settingsJsonLinux
    Write-Host "Saving settings file to $tmpPath" -ForegroundColor DarkGreen
    ..\..\src\SqlBuildManager.Console\bin\Debug\net5.0\sbm.exe batch savesettings --batchpoolname "SqlBuildManagerPoolLinux" -os "Linux" --settingsfile "$tmpPath"  --settingsfilekey "$keyFile" --username "$sqlUserName" --password "$sqlPassword" --batchaccountname "$batchAccountName"  --batchaccountkey "$batchAcctKey" --batchaccounturl "https://$batchAcctEndpoint" --batchnodecount 2  --batchvmsize "STANDARD_DS1_V2" --rootloggingpath "C:/temp" --storageaccountname "$storageAccountName"  --storageaccountkey "$storageAcctKey" -eh "$eventHubConnectionString" --defaultscripttimeout 500 --concurrency 5 --concurrencytype "Count" --clientid "$($identity.clientId)" --principalid "$($identity.principalId)" --resourceid "$($identity.id)" --resourcegroup "$($identity.resourceGroup)" --subscriptionid $subscriptionId --silent 

    $tmpPath = $settingsJsonLinuxQueue
    Write-Host "Saving settings file to $tmpPath" -ForegroundColor DarkGreen
    ..\..\src\SqlBuildManager.Console\bin\Debug\net5.0\sbm.exe batch savesettings --batchpoolname "SqlBuildManagerPoolLinux" -os "Linux"  -sb "$serviceBusConnectionString" --settingsfile "$tmpPath"  --settingsfilekey "$keyFile" --username "$sqlUserName" --password "$sqlPassword" --batchaccountname "$batchAccountName"  --batchaccountkey "$batchAcctKey" --batchaccounturl "https://$batchAcctEndpoint" --batchnodecount 2  --batchvmsize "STANDARD_DS1_V2" --rootloggingpath "C:/temp" --storageaccountname "$storageAccountName"  --storageaccountkey "$storageAcctKey" -eh "$eventHubConnectionString" --defaultscripttimeout 500 --concurrency 5 --concurrencytype "Count" --clientid "$($identity.clientId)" --principalid "$($identity.principalId)" --resourceid "$($identity.id)" --resourcegroup "$($identity.resourceGroup)" --subscriptionid $subscriptionId --silent 

    $tmpPath = $settingsJsonLinuxQueueKv
    Write-Host "Saving settings file to $tmpPath" -ForegroundColor DarkGreen
    ..\..\src\SqlBuildManager.Console\bin\Debug\net5.0\sbm.exe batch savesettings --batchpoolname "SqlBuildManagerPoolLinux" -os "Linux"  -sb "$serviceBusConnectionString"  -kv "$keyVaultName" --settingsfile "$tmpPath"  --settingsfilekey "$keyFile" --username "$sqlUserName" --password "$sqlPassword" --batchaccountname "$batchAccountName"  --batchaccountkey "$batchAcctKey" --batchaccounturl "https://$batchAcctEndpoint" --batchnodecount 2  --batchvmsize "STANDARD_DS1_V2" --rootloggingpath "C:/temp" --storageaccountname "$storageAccountName"  --storageaccountkey "$storageAcctKey" -eh "$eventHubConnectionString" --defaultscripttimeout 500 --concurrency 5 --concurrencytype "Count" --clientid "$($identity.clientId)" --principalid "$($identity.principalId)" --resourceid "$($identity.id)" --resourcegroup "$($identity.resourceGroup)" --subscriptionid $subscriptionId --silent 

    

}
else 
{
    $tmpPath = $settingsJsonWindows
    Write-Host "Saving settings file to $tmpPath" -ForegroundColor DarkGreen
    ..\..\src\SqlBuildManager.Console\bin\Debug\net5.0\sbm.exe batch savesettings --batchpoolname "SqlBuildManagerPoolWindows" -os "Windows" --settingsfile "$tmpPath"  --settingsfilekey "$keyFile" --batchaccountname "$batchAccountName" --batchaccountkey "$batchAcctKey" --batchaccounturl "https://$batchAcctEndpoint" --batchnodecount 2  --batchvmsize "STANDARD_DS1_V2" --rootloggingpath "C:/temp" --storageaccountname "$storageAccountName"  --storageaccountkey "$storageAcctKey" -eh "$eventHubConnectionString" --defaultscripttimeout 500 --concurrency 5 --concurrencytype "Count" --clientid "$($identity.clientId)" --principalid "$($identity.principalId)" --resourceid "$($identity.id)" --resourcegroup "$($identity.resourceGroup)" --subscriptionid $subscriptionId --silent 

    $tmpPath = $settingsJsonWindowsQueue
    Write-Host "Saving settings file to $tmpPath" -ForegroundColor DarkGreen
    ..\..\src\SqlBuildManager.Console\bin\Debug\net5.0\sbm.exe batch savesettings --batchpoolname "SqlBuildManagerPoolWindows" -os "Windows" -sb "$serviceBusConnectionString"  --settingsfile "$tmpPath"  --settingsfilekey "$keyFile" --batchaccountname "$batchAccountName"  --batchaccountkey "$batchAcctKey" --batchaccounturl "https://$batchAcctEndpoint" --batchnodecount 2  --batchvmsize "STANDARD_DS1_V2" --rootloggingpath "C:/temp" --storageaccountname "$storageAccountName"  --storageaccountkey "$storageAcctKey" -eh "$eventHubConnectionString" --defaultscripttimeout 500 --concurrency 5 --concurrencytype "Count" --clientid "$($identity.clientId)" --principalid "$($identity.principalId)" --resourceid "$($identity.id)" --resourcegroup "$($identity.resourceGroup)" --subscriptionid $subscriptionId  --silent 

    $tmpPath = $settingsJsonWindowsQueueKv
    Write-Host "Saving settings file to $tmpPath" -ForegroundColor DarkGreen
    ..\..\src\SqlBuildManager.Console\bin\Debug\net5.0\sbm.exe batch savesettings --batchpoolname "SqlBuildManagerPoolWindows" -os "Windows" -sb "$serviceBusConnectionString" -kv "$keyVaultName" --settingsfile "$tmpPath"  --settingsfilekey "$keyFile"  --batchaccountname "$batchAccountName"  --batchaccountkey "$batchAcctKey" --batchaccounturl "https://$batchAcctEndpoint" --batchnodecount 2  --batchvmsize "STANDARD_DS1_V2" --rootloggingpath "C:/temp" --storageaccountname "$storageAccountName"  --storageaccountkey "$storageAcctKey" -eh "$eventHubConnectionString" --defaultscripttimeout 500 --concurrency 5 --concurrencytype "Count" --clientid "$($identity.clientId)" --principalid "$($identity.principalId)" --resourceid "$($identity.id)" --resourcegroup "$($identity.resourceGroup)" --subscriptionid $subscriptionId --silent 

    $tmpPath = $settingsJsonLinux
    Write-Host "Saving settings file to $tmpPath" -ForegroundColor DarkGreen
    ..\..\src\SqlBuildManager.Console\bin\Debug\net5.0\sbm.exe batch savesettings --batchpoolname "SqlBuildManagerPoolLinux" -os "Linux" --settingsfile "$tmpPath"  --settingsfilekey "$keyFile" --batchaccountname "$batchAccountName"  --batchaccountkey "$batchAcctKey" --batchaccounturl "https://$batchAcctEndpoint" --batchnodecount 2  --batchvmsize "STANDARD_DS1_V2" --rootloggingpath "C:/temp" --storageaccountname "$storageAccountName"  --storageaccountkey "$storageAcctKey" -eh "$eventHubConnectionString" --defaultscripttimeout 500 --concurrency 5 --concurrencytype "Count" --clientid "$($identity.clientId)" --principalid "$($identity.principalId)" --resourceid "$($identity.id)" --resourcegroup "$($identity.resourceGroup)" --subscriptionid $subscriptionId --silent 

    $tmpPath = $settingsJsonLinuxQueue
    Write-Host "Saving settings file to $tmpPath" -ForegroundColor DarkGreen
    ..\..\src\SqlBuildManager.Console\bin\Debug\net5.0\sbm.exe batch savesettings --batchpoolname "SqlBuildManagerPoolLinux" -os "Linux"  -sb "$serviceBusConnectionString" --settingsfile "$tmpPath"  --settingsfilekey "$keyFile" --batchaccountname "$batchAccountName"  --batchaccountkey "$batchAcctKey" --batchaccounturl "https://$batchAcctEndpoint" --batchnodecount 2  --batchvmsize "STANDARD_DS1_V2" --rootloggingpath "C:/temp" --storageaccountname "$storageAccountName"  --storageaccountkey "$storageAcctKey" -eh "$eventHubConnectionString" --defaultscripttimeout 500 --concurrency 5 --concurrencytype "Count" --clientid "$($identity.clientId)" --principalid "$($identity.principalId)" --resourceid "$($identity.id)" --resourcegroup "$($identity.resourceGroup)" --subscriptionid $subscriptionId --silent 

    $tmpPath = $settingsJsonLinuxQueueKv
    Write-Host "Saving settings file to $tmpPath" -ForegroundColor DarkGreen
    ..\..\src\SqlBuildManager.Console\bin\Debug\net5.0\sbm.exe batch savesettings --batchpoolname "SqlBuildManagerPoolLinux" -os "Linux"  -sb "$serviceBusConnectionString"  -kv "$keyVaultName" --settingsfile "$tmpPath"  --settingsfilekey "$keyFile" --batchaccountname "$batchAccountName"  --batchaccountkey "$batchAcctKey" --batchaccounturl "https://$batchAcctEndpoint" --batchnodecount 2  --batchvmsize "STANDARD_DS1_V2" --rootloggingpath "C:/temp" --storageaccountname "$storageAccountName"  --storageaccountkey "$storageAcctKey" -eh "$eventHubConnectionString" --defaultscripttimeout 500 --concurrency 5 --concurrencytype "Count" --clientid "$($identity.clientId)" --principalid "$($identity.principalId)" --resourceid "$($identity.id)" --resourcegroup "$($identity.resourceGroup)" --subscriptionid $subscriptionId --silent 

}
