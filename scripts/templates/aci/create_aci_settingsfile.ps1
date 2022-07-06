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
    [string] $identityClientId,
    [string] $sqlUserName,
    [string] $sqlPassword, 
    [ValidateSet("Password", "ManagedIdentity", "Both")]
    [string] $authType = "Both"
)
Write-Host "Create ACI settings file"  -ForegroundColor Cyan

$path = Resolve-Path $path
Write-Host "Output path set to $path" -ForegroundColor DarkGreen
Write-Host "Retrieving keys from resources in $resourceGroupName" -ForegroundColor DarkGreen


if($authType -eq "Both")
{
    $authTypes = @("Password", "ManagedIdentity")
}
else
{
    $authTypes = @($authType)
}

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
if([string]::IsNullOrWhiteSpace($containerRegistryName))
{
    $baseFileName = Join-Path $path "settingsfile-aci-no-registry"
}
else {
    $baseFileName = Join-Path $path "settingsfile-aci"
}


$subscriptionId = az account show --query id --output tsv

if([string]::IsNullOrWhiteSpace($containerRegistryName) -eq $false)
{
    Write-Host "Getting info for Azure Conainer Registry: $containerRegistryName" -ForegroundColor DarkGreen
    $acrUserName = az acr credential show -g $resourceGroupName --name $containerRegistryName -o tsv --query username
    $acrPassword = az acr credential show -g $resourceGroupName --name  $containerRegistryName -o tsv --query passwords[0].value
    $acrServerName = az acr show -g $resourceGroupName --name $containerRegistryName -o tsv --query loginServer
}

$keyFile = Join-Path $path "settingsfilekey.txt"
if($false -eq (Test-Path $keyFile))
{
    $AESKey = New-Object Byte[] 32
    [Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($AESKey)
    $settingsFileKey = [System.Convert]::ToBase64String($AESKey);
    $settingsFileKey |  Set-Content -Path $keyFile
}



$sbAndEhArgs = 
foreach($auth in $authTypes)
{
    $params = @("aci", "savesettings")

    if($auth -eq "ManagedIdentity" )
    {
        $settingsAci  =$baseFileName + "-mi.json"
        $sbAndEhArgs = @("-sb", $serviceBusNamespaceName)
        $sbAndEhArgs += ("-eh","$($eventhubNamespaceName)|$($eventHubName)")
    }
    else 
    {
        $settingsAci  = $baseFileName + ".json"
        $sbAndEhArgs = @("-sb", """$serviceBusConnectionString""")
        $sbAndEhArgs += ("-eh","""$eventHubConnectionString""")
        $params += ("--storageaccountkey",$storageAcctKey)
    }
    Write-Host "Saving settings file to $settingsAci" -ForegroundColor DarkGreen

    
    $params += ("--settingsfile", "$($settingsAci)")
    $params += ("--aciname", $aciName)
    $params += ("--identityname", $identityName)
    $params += ("--clientid", $identityClientId)
    $params += ("--idrg", $resourceGroupName)
    $params += ("--acirg", $resourceGroupName)
    $params += ("-kv", $keyVaultName)
    $params += ("--storageaccountname", $storageAccountName)

    $params += ("--defaultscripttimeout", "500")
    $params += ("--subscriptionid",$subscriptionId)
    $params += ("--force")
    if($haveSqlInfo)
    {
        $params += ("--username", $sqlUserName)
        $params += ("--password", $sqlPassword)
    }
    if([string]::IsNullOrWhiteSpace($containerRegistryName) -eq $false)
    {
        $params += ("--registryserver", $acrServerName)
        $params += ("--registryusername", $acrUserName)
        $params += ("--registrypassword", $acrPassword)
    }
    if([string]::IsNullOrWhiteSpace($imageTag) -eq $false)
    {
        $params += ("--imagetag", $imageTag)
    }
    $params += ("--authtype", $auth)
    Write-Host $params $sbAndEhArgs -ForegroundColor DarkYellow
    Start-Process $sbmExe -ArgumentList ($params + $sbAndEhArgs) -Wait
}


