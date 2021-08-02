param
(
    [string] $path,
    [string] $resourceGroupName,
    [string] $keyVaultName, 
    [string] $aciName,
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

$storageAcctKey = (az storage account keys list --account-name $storageAccountName -o tsv --query '[].value')[0]

$eventHubName = az eventhubs eventhub list  --resource-group $resourceGroupName --namespace-name $eventhubNamespaceName -o tsv --query "[?contains(@.name '$prefix')].name"
$eventHubAuthRuleName = az eventhubs eventhub authorization-rule list  --resource-group $resourceGroupName --namespace-name $eventhubNamespaceName --eventhub-name $eventHubName -o tsv --query [].name
$eventHubConnectionString = az eventhubs eventhub authorization-rule keys list --resource-group $resourceGroupName --namespace-name $eventHubNamespaceName --eventhub-name $eventHubName --name $eventHubAuthRuleName -o tsv --query "primaryConnectionString"

$serviceBusTopicAuthRuleName = az servicebus topic authorization-rule list --resource-group $resourceGroupName --namespace-name $serviceBusNamespaceName --topic-name "sqlbuildmanager" -o tsv --query "[].name"
$serviceBusConnectionString = az servicebus topic authorization-rule keys list --resource-group $resourceGroupName --namespace-name $serviceBusNamespaceName --topic-name "sqlbuildmanager" --name $serviceBusTopicAuthRuleName -o tsv --query "primaryConnectionString"

$settingsJsonLinuxQueueKv = Join-Path $path "settingsfile-linux-aci-queue-keyvault.json"

$subscriptionId = az account show --query id --output tsv


$AESKey = New-Object Byte[] 32
[Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($AESKey)
$settingsFileKey = [System.Convert]::ToBase64String($AESKey);

$keyFile = Join-Path $path "settingsfilekey_aci.txt"
$settingsFileKey |  Set-Content -Path $keyFile


if($haveSqlInfo)
{
    $tmpPath = $settingsJsonLinuxQueueKv
    Write-Host "Saving settings file to $tmpPath" -ForegroundColor DarkGreen
    ..\..\src\SqlBuildManager.Console\bin\Debug\net5.0\sbm.exe aci savesettings --aciname "$aciName" --identityname "$identityName" --idrg "$resourceGroupName" --acirg "$resourceGroupName"-sb "$serviceBusConnectionString"  -kv "$keyVaultName" --settingsfile "$tmpPath"  --settingsfilekey "$keyFile" --storageaccountname "$storageAccountName"  --storageaccountkey "$storageAcctKey" -eh "$eventHubConnectionString" --defaultscripttimeout 500 --username "$sqlUserName" --password "$sqlPassword" --subscriptionid "$subscriptionId" --force 
}
else 
{
    $tmpPath = $settingsJsonLinuxQueueKv
    Write-Host "Saving settings file to $tmpPath" -ForegroundColor DarkGreen
    ..\..\src\SqlBuildManager.Console\bin\Debug\net5.0\sbm.exe aci savesettings --aciname "$aciName" --identityname "$identityName" --idrg "$resourceGroupName" --acirg "$resourceGroupName"-sb "$serviceBusConnectionString"  -kv "$keyVaultName" --settingsfile "$tmpPath"  --settingsfilekey "$keyFile" --storageaccountname "$storageAccountName"  --storageaccountkey "$storageAcctKey" -eh "$eventHubConnectionString" --defaultscripttimeout 500 --subscriptionid "$subscriptionId" --force 

}

