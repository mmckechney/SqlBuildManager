param
(
    [string] $sbmExe = "sbm.exe",
    [string] $path,
    [string] $resourceGroupName,
    [string] $containerAppEnvironmentName,
    [string] $containerRegistryName,
    [string] $storageAccountName,
    [string] $eventHubNamespaceName,
    [string] $serviceBusNamespaceName,
    [string] $sqlUserName,
    [string] $sqlPassword,
    [string] $keyVaultName,
    [string] $identityName,
    [string] $identityClientId,
    [string] $imageTag,
    [bool] $withContainerRegistry
)
Write-Host "Create Container App Settings file"  -ForegroundColor Cyan
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

if($withContainerRegistry)
{
    $acrUserName = az acr credential show -g $resourceGroupName --name $containerRegistryName -o tsv --query username
    $acrPassword = az acr credential show -g $resourceGroupName --name  $containerRegistryName -o tsv --query passwords[0].value
    $acrServerName = az acr show -g $resourceGroupName --name $containerRegistryName -o tsv --query loginServer
    if("" -eq $keyVaultName)
    {
        $settingsContainerApp = Join-Path $path "settingsfile-containerapp.json"
    }
    else 
    {
        $settingsContainerApp = Join-Path $path "settingsfile-containerapp-kv.json"
    }
}
else 
{
    if("" -eq $keyVaultName)
    {
        $settingsContainerApp = Join-Path $path "settingsfile-containerapp-no-registry.json"
    }
    else 
    {
        $settingsContainerApp = Join-Path $path "settingsfile-containerapp-no-registry-kv.json"
    }
    
}

$location = az containerapp env show -g $resourceGroupName -n $containerAppEnvironmentName -o tsv --query location
$subscriptionId = az account show --query id --output tsv

$keyFile = Join-Path $path "settingsfilekey.txt"
if($false -eq (Test-Path $keyFile))
{
    $AESKey = New-Object Byte[] 32
    [Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($AESKey)
    $settingsFileKey = [System.Convert]::ToBase64String($AESKey);
    $settingsFileKey |  Set-Content -Path $keyFile
}

$tmpPath = $settingsContainerApp
Write-Host "Saving settings file to $tmpPath" -ForegroundColor DarkGreen

$params = @("containerapp", "savesettings")
$params +=("--environmentname",$containerAppEnvironmentName)
$params +=("--location",$location)
$params +=("--resourcegroup", $resourceGroupName)
$params +=("--imagetag",$imageTag)
$params +=("--servicebustopicconnection",$serviceBusConnectionString)
$params +=("--settingsfile",$tmpPath)
$params +=("--settingsfilekey",$keyFile)
$params +=("--storageaccountname",$storageAccountName)
$params +=("--storageaccountkey",$storageAcctKey)
$params +=("--eventhubconnection",$eventHubConnectionString)
$params +=("--defaultscripttimeout",500)
$params +=("--subscriptionid",$subscriptionId)
$params +=("--force","true")
if($haveSqlInfo)
{
    $params +=("--username",$sqlUserName)
    $params +=("--password",$sqlPassword)
}
if($withContainerRegistry)
{
    $params +=("--registryserver",$acrServerName)
    $params +=("--registryusername",$acrUserName)
    $params +=("--registrypassword",$acrPassword)
}
if("" -ne  $keyVaultName)
{
    $params += ("-kv", $keyVaultName)
}
if("" -ne  $identityName)
{
    $params += ("--identityname", $identityName)
}
if("" -ne  $identityName)
{
    $params += ("--idrg", $resourceGroupName)
}
if("" -ne  $identityName)
{
    $params += ("--clientid", $identityClientId)
}
 #Write-Host $params -ForegroundColor DarkYellow
Start-Process $sbmExe -ArgumentList $params -Wait

