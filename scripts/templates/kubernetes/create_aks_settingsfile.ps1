param
(
    [string] $sbmExe = "sbm.exe",
    [string] $path,
    [string] $resourceGroupName,
    [string] $storageAccountName,
    [string] $eventHubNamespaceName,
    [string] $serviceBusNamespaceName,
    [string] $acrName, 
    [string] $keyVaultName,
    [string] $serviceAccountName,
    [int] $podCount = 2,
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

$keyFile = Join-Path $path "settingsfilekey.txt"
if($false -eq (Test-Path $keyFile))
{
    $AESKey = New-Object Byte[] 32
    [Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($AESKey)
    $settingsFileKey = [System.Convert]::ToBase64String($AESKey);
    $settingsFileKey |  Set-Content -Path $keyFile
}

Write-Host "Retrieving secrets from Azure resources" -ForegroundColor DarkGreen
$storageAcctKey = (az storage account keys list --account-name $storageAccountName -o tsv --query '[].value')[0]
$subscriptionId = az account show --query id --output tsv

Write-Host "Retrieving EventHub information" -ForegroundColor DarkGreen
$eventHubAuthRuleName = az eventhubs eventhub authorization-rule list  --resource-group $resourceGroupName --namespace-name $eventhubNamespaceName --eventhub-name $eventHubName -o tsv --query [].name
$eventHubConnectionString = az eventhubs eventhub authorization-rule keys list --resource-group $resourceGroupName --namespace-name $eventHubNamespaceName --eventhub-name $eventHubName --name $eventHubAuthRuleName -o tsv --query "primaryConnectionString"

Write-Host "Retrieving Service Bus information" -ForegroundColor DarkGreen
$serviceBusTopicAuthRuleName = az servicebus topic authorization-rule list --resource-group $resourceGroupName --namespace-name $serviceBusNamespaceName --topic-name "sqlbuildmanager" -o tsv --query "[].name"
$serviceBusConnectionString = az servicebus topic authorization-rule keys list --resource-group $resourceGroupName --namespace-name $serviceBusNamespaceName --topic-name "sqlbuildmanager" --name $serviceBusTopicAuthRuleName -o tsv --query "primaryConnectionString"

Write-Host "Retrieving Identity information" -ForegroundColor DarkGreen

$tenantId = az account show -o tsv --query tenantId


$settingsFileName = Join-Path $path "settingsfile-k8s-sec.json"

$saveSettingsShared =  @("k8s", "savesettings")
$saveSettingsShared += @("--settingsfilekey", """$keyFile""")
$saveSettingsShared += @("--concurrency", "5")
$saveSettingsShared += @("--concurrencytype", "Count")
$saveSettingsShared += @("--registry", $acrName)
$saveSettingsShared += @("--tag", "latest-vNext")
$saveSettingsShared += @("--tenantid", $tenantId)
$saveSettingsShared += @("--serviceaccountname", $serviceAccountName)
$saveSettingsShared += @("--force")
$saveSettingsShared += @("--podcount", $podCount)
$saveSettingsShared += @("--eventhublogging", "ScriptErrors")
$saveSettingsShared += @("--ehrg", $resourceGroupName)
$saveSettingsShared += @("--ehsub", $subscriptionId)


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
    $settingsFileName = Join-Path $path "settingsfile-k8s-sec.json"
    if(Test-Path $settingsFileName)
    {
        Remove-Item $settingsFileName
    }
    Write-Host ($params + @("--settingsfile", """$settingsFileName"""))-ForegroundColor DarkYellow
    Start-Process $sbmExe -ArgumentList ($params + @("--settingsfile", """$settingsFileName""")) -Wait -NoNewWindow

    #save with KeyVault settings
    $settingsFileName = Join-Path $path "settingsfile-k8s-kv.json"
    if(Test-Path $settingsFileName)
    {
        Remove-Item $settingsFileName
    }
    $params += @("--keyvaultname", $keyVaultName)
    Write-Host ($params + @("--settingsfile", """$settingsFileName""")) -ForegroundColor DarkYellow
    Start-Process $sbmExe -ArgumentList ($params + @("--settingsfile", """$settingsFileName""")) -Wait -NoNewWindow
}    
  


if($authTypes -contains "ManagedIdentity")
{
    $params = $saveSettingsShared
    $params += @("--storageaccountname",$storageAccountName)  
    $params += @("-eh","""$($eventhubNamespaceName)|$($eventHubName)""") 
    $params += @("-sb","$($serviceBusNamespaceName)")
    $params += @("--authtype", "ManagedIdentity")
    
    
    $settingsFileName = Join-Path $path "settingsfile-k8s-sec-mi.json"
    if(Test-Path $settingsFileName)
    {
        Remove-Item $settingsFileName
    }
    Write-Host $params  -ForegroundColor Yellow
    Write-Host ($params + @("--settingsfile", """$settingsFileName"""))-ForegroundColor DarkYellow
    Start-Process $sbmExe -ArgumentList ($params + @("--settingsfile", """$settingsFileName""")) -Wait -NoNewWindow

    $settingsFileName = Join-Path $path "settingsfile-k8s-kv-mi.json"
    if(Test-Path $settingsFileName)
    {
        Remove-Item $settingsFileName
    }
    $params += @("--keyvaultname", $keyVaultName)
    Write-Host $params  -ForegroundColor Yellow
    Write-Host ($params + @("--settingsfile", """$settingsFileName"""))-ForegroundColor DarkYellow
    Start-Process $sbmExe -ArgumentList ($params + @("--settingsfile", """$settingsFileName""")) -Wait -NoNewWindow
  
}  

