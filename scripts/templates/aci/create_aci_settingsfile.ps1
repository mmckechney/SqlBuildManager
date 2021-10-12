param
(
    [string] $sbmExe = "sbm.exe",
    [string] $path,
    [string] $resourceGroupName,
    [string] $keyVaultName, 
    [string] $aciName,
    [string] $imageTag,
    [string] $containerRegistryName,
    [string] $storageAccountName,
    [string] $eventHubNamespaceName,
    [string] $serviceBusNamespaceName,
    [string] $identityName,
    [string] $sqlUserName,
    [string] $sqlPassword
)
Write-Host "Create ACI settings file"  -ForegroundColor Cyan

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

if([string]::IsNullOrWhiteSpace($containerRegistryName))
{
    $acrUserName = az acr credential show -g $resourceGroupName --name $containerRegistryName -o tsv --query username
    $acrPassword = az acr credential show -g $resourceGroupName --name  $containerRegistryName -o tsv --query passwords[0].value
    $acrServerName = az acr show -g $resourceGroupName --name $containerRegistryName -o tsv --query loginServer
}

$params = @("aci", "savesettings")
$params += ("--settingsfile", $settingsJsonLinuxQueueKv)
$params += ("--aciname", $aciName)
$params += ("--identityname", $identityName)
$params += ("--idrg", $resourceGroupName)
$params += ("--acirg", $resourceGroupName)
$params += ("-sb", $serviceBusConnectionString)
$params += ("-kv", $keyVaultName)
$params += ("--storageaccountname", $storageAccountName)
$params += ("--storageaccountkey",$storageAcctKey)
$params += ("-eh",$eventHubConnectionString)
$params += ("--defaultscripttimeout", "500")
$params += ("--subscriptionid",$subscriptionId)
$params += ("--force")
if($haveSqlInfo)
{
    $params += ("--username", $sqlUserName)
    $params += ("--password", $sqlPassword)
}
if([string]::IsNullOrWhiteSpace($containerRegistryName))
{
    $params += ("--imagetag", $imageTag)
}
Write-Host $params
Start-Process $sbmExe -ArgumentList $params -Wait



