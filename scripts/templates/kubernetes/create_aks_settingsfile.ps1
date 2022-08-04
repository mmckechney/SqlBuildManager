param
(
    [string] $sbmExe = "sbm.exe",
    [string] $path,
    [string] $resourceGroupName,
    [string] $storageAccountName,
    [string] $eventHubNamespaceName,
    [string] $serviceBusNamespaceName,
    [string] $acrName, 
    [string] $identityName,
    [string] $keyVaultName,
    [string] $sqlUserName,
    [string] $sqlPassword,
    [ValidateSet("Password", "ManagedIdentity", "Both")]
    [string] $authType = "Both"
)
Write-Host "Create AKS settings file"  -ForegroundColor Cyan

$path = Resolve-Path $path
Write-Host "Output path set to $path" -ForegroundColor DarkGreen
if($authType -eq "Both")
{
    $authTypes = @("Password", "ManagedIdentity")
}
else
{
    $authTypes = @($authType)
}

Write-Host "Retrieving secrets from Azure resources" -ForegroundColor DarkGreen
$storageAcctKey = (az storage account keys list --account-name $storageAccountName -o tsv --query '[].value')[0]

$subscriptionId = az account show -o tsv --query id
Write-Host "Using subscription id: '$subscriptionId'" -ForegroundColor DarkGreen

Write-Host "Retrieving EventHub information" -ForegroundColor DarkGreen
$eventHubAuthRuleName = az eventhubs eventhub authorization-rule list  --resource-group $resourceGroupName --namespace-name $eventhubNamespaceName --eventhub-name $eventHubName -o tsv --query [].name
$eventHubConnectionString = az eventhubs eventhub authorization-rule keys list --resource-group $resourceGroupName --namespace-name $eventHubNamespaceName --eventhub-name $eventHubName --name $eventHubAuthRuleName -o tsv --query "primaryConnectionString"

Write-Host "Retrieving Service Bus information" -ForegroundColor DarkGreen
$serviceBusTopicAuthRuleName = az servicebus topic authorization-rule list --resource-group $resourceGroupName --namespace-name $serviceBusNamespaceName --topic-name "sqlbuildmanager" -o tsv --query "[].name"
$serviceBusConnectionString = az servicebus topic authorization-rule keys list --resource-group $resourceGroupName --namespace-name $serviceBusNamespaceName --topic-name "sqlbuildmanager" --name $serviceBusTopicAuthRuleName -o tsv --query "primaryConnectionString"

Write-Host "Retrieving Identity information" -ForegroundColor DarkGreen
$clientID = az identity show --name $identityName --resource-group $resourceGroupName -o tsv --query clientId
$tenantId = az identity show --name $identityName --resource-group $resourceGroupName -o tsv --query tenantId


$settingsFile = Join-Path $path "settingsfile-k8s-sec.json"
$keyFile = Join-Path $path "settingsfilekey.txt"

$saveSettingsShared =  @("k8s", "savesettings")
$saveSettingsShared += @("--settingsfilekey", """$keyFile""")
$saveSettingsShared += @("--concurrency", "5")
$saveSettingsShared += @("--concurrencytype", "Count")
$saveSettingsShared += @("--registry", $acrName)
$saveSettingsShared += @("--tag", "latest-vNext")
$saveSettingsShared += @("--identityname", $identityName)
$saveSettingsShared += @("--clientid", $clientID)
$saveSettingsShared += @("--tenantid", $tenantId)
$saveSettingsShared += @("--identityresourcegroup",$resourceGroupName )
$saveSettingsShared += @("--subscriptionid",$subscriptionId )
$saveSettingsShared += @("--force")


if($authTypes -contains "Password")
{
    $params = $saveSettingsShared
    $params += @("--storageaccountname",$storageAccountName)  
    $params += @("--storageaccountkey","""$storageAcctKey""") 
    $params += @("-eh","""$eventHubConnectionString""") 
    $params += @("-sb","""$serviceBusConnectionString""")  
    if([string]::IsNullOrWhiteSpace($sqlUserName) -eq $false)
    {
        $params += @("--username",$sqlUserName)  
        $params += @("--password",$sqlPassword)
    }
    
    #save with encrypted secrets
    $settingsFile = Join-Path $path "settingsfile-k8s-sec.json"
    Write-Host ($params + @("--settingsfile", """$settingsFile"""))-ForegroundColor Yellow
    Start-Process $sbmExe -ArgumentList ($params + @("--settingsfile", """$settingsFile""")) -Wait

    #save with KeyVault settings
    $settingsFile = Join-Path $path "settingsfile-k8s-kv.json"
    $params += @("--keyvaultname", $keyVaultName)
    Write-Host ($params + @("--settingsfile", """$settingsFile""")) -ForegroundColor Yellow
    Start-Process $sbmExe -ArgumentList ($params + @("--settingsfile", """$settingsFile""")) -Wait
}    
  


if($authTypes -contains "ManagedIdentity")
{
    $params = $saveSettingsShared
    $params += @("--storageaccountname",$storageAccountName)  
    $params += @("-eh","""$($eventhubNamespaceName).servicebus.windows.net|$($eventHubName)""") 
    $params += @("-sb","$($serviceBusNamespaceName).servicebus.windows.net")
    $params += @("--authtype", "ManagedIdentity")
    

    $settingsFile = Join-Path $path "settingsfile-k8s-sec-mi.json"
    Write-Host $params  -ForegroundColor Yellow
    Write-Host ($params + @("--settingsfile", """$settingsFile"""))-ForegroundColor Yellow
    Start-Process $sbmExe -ArgumentList ($params + @("--settingsfile", """$settingsFile""")) -Wait

    $settingsFile = Join-Path $path "settingsfile-k8s-kv-mi.json"
    $params += @("--keyvaultname", $keyVaultName)
    Write-Host $params  -ForegroundColor Yellow
    Write-Host ($params + @("--settingsfile", """$settingsFile"""))-ForegroundColor Yellow
    Start-Process $sbmExe -ArgumentList ($params + @("--settingsfile", """$settingsFile""")) -Wait
  
}  

