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
    [bool] $withContainerRegistry,
    ## TODO: Enable Managed Identity. For now, ManagedIdentity for SQL Auth is not available on Container Apps. SB KEDA also requires a connection string. But EH can fully use MI
    [ValidateSet("Password", "ManagedIdentity", "Both")]
    [string] $authType = "Both"
)
Write-Host "Create Container App Settings file"  -ForegroundColor Cyan
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
$tenantId = az account show -o tsv --query tenantId
Write-Host "Retrieving keys from resources in $resourceGroupName... $storageAccountName" -ForegroundColor DarkGreen
$storageAcctKey = (az storage account keys list --account-name $storageAccountName -o tsv --query '[].value')[0]

Write-Host "Retrieving keys from resources in $resourceGroupName... $eventhubNamespaceName $eventHubName" -ForegroundColor DarkGreen
$eventHubName = az eventhubs eventhub list  --resource-group $resourceGroupName --namespace-name $eventhubNamespaceName -o tsv --query "[?contains(@.name '$prefix')].name"
$eventHubAuthRuleName = az eventhubs eventhub authorization-rule list  --resource-group $resourceGroupName --namespace-name $eventhubNamespaceName --eventhub-name $eventHubName -o tsv --query [].name
$eventHubConnectionString = az eventhubs eventhub authorization-rule keys list --resource-group $resourceGroupName --namespace-name $eventHubNamespaceName --eventhub-name $eventHubName --name $eventHubAuthRuleName -o tsv --query "primaryConnectionString"

Write-Host "Retrieving keys from resources in $resourceGroupName... $serviceBusNamespaceName sqlbuildmanager" -ForegroundColor DarkGreen
$serviceBusTopicAuthRuleName = az servicebus topic authorization-rule list --resource-group $resourceGroupName --namespace-name $serviceBusNamespaceName --topic-name "sqlbuildmanager" -o tsv --query "[].name"
$serviceBusConnectionString = az servicebus topic authorization-rule keys list --resource-group $resourceGroupName --namespace-name $serviceBusNamespaceName --topic-name "sqlbuildmanager" --name $serviceBusTopicAuthRuleName -o tsv --query "primaryConnectionString"

if($withContainerRegistry)
{
    Write-Host "Retrieving keys from resources in $resourceGroupName... $containerRegistryName" -ForegroundColor DarkGreen
    $acrUserName = az acr credential show -g $resourceGroupName --name $containerRegistryName -o tsv --query username
    $acrPassword = az acr credential show -g $resourceGroupName --name  $containerRegistryName -o tsv --query passwords[0].value
    $acrServerName = az acr show -g $resourceGroupName --name $containerRegistryName -o tsv --query loginServer
    if("" -eq $keyVaultName)
    {
        $settingsContainerApp = Join-Path $path "settingsfile-containerapp"
    }
    else 
    {
        $settingsContainerApp = Join-Path $path "settingsfile-containerapp-kv"
    }
}
else 
{
    if("" -eq $keyVaultName)
    {
        $settingsContainerApp = Join-Path $path "settingsfile-containerapp-no-registry"
    }
    else 
    {
        $settingsContainerApp = Join-Path $path "settingsfile-containerapp-no-registry-kv"
    }
    
}

Write-Host "Retrieving keys from resources in $resourceGroupName... $containerAppEnvironmentName" -ForegroundColor DarkGreen
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



foreach($auth in $authTypes)
{

    $params = @("containerapp", "savesettings")
    if($auth -eq "ManagedIdentity" )
    {
        $settingsContainerApp = $settingsContainerApp + "-mi"
        $sbAndEhArgs = @("--eventhubconnection","$($eventHubNamespaceName)|$($eventHubName)")
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
    }
    else 
    {
        $sbAndEhArgs = @("--eventhubconnection",$eventHubConnectionString)
        $params +=("--storageaccountkey",$storageAcctKey)
    }
    $sbAndEhArgs += @("--servicebustopicconnection",$serviceBusConnectionString)
    $tmpPath = "$($settingsContainerApp).json"
    Write-Host "Saving settings file to $tmpPath" -ForegroundColor DarkGreen

    $params += @("--tenantid", $tenantId)
    $params +=("--environmentname",$containerAppEnvironmentName)
    $params +=("--location","""$location""")
    $params +=("--resourcegroup", $resourceGroupName)
    $params +=("--imagetag",$imageTag)
    $params +=("--settingsfile",$tmpPath)
    $params +=("--settingsfilekey",$keyFile)
    $params +=("--storageaccountname",$storageAccountName)
    
    
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

    # Container apps don't take this yet!
    #$params += ("--authtype", "Password")
    

    Write-Host $params $sbAndEhArgs -ForegroundColor DarkYellow
    Start-Process $sbmExe -ArgumentList ($params + $sbAndEhArgs) -Wait -NoNewWindow
}

