param
(
    [string] $sbmExe = "sbm.exe",
    [string] $path,
    [string] $resourceGroupName,
    [string] $storageAccountName,
    [string] $eventHubNamespaceName,
    [string] $serviceBusNamespaceName,
    [string] $sqlUserName,
    [string] $sqlPassword,
    [ValidateSet("Password", "ManagedIdentity", "Both")]
    [string] $authType = "Both"
)
Write-Host "Create AKS secrets and runtime files"  -ForegroundColor Cyan

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


$eventHubName = az eventhubs eventhub list  --resource-group $resourceGroupName --namespace-name $eventhubNamespaceName -o tsv --query "[?contains(@.name '$prefix')].name"
$eventHubAuthRuleName = az eventhubs eventhub authorization-rule list  --resource-group $resourceGroupName --namespace-name $eventhubNamespaceName --eventhub-name $eventHubName -o tsv --query [].name
$eventHubConnectionString = az eventhubs eventhub authorization-rule keys list --resource-group $resourceGroupName --namespace-name $eventHubNamespaceName --eventhub-name $eventHubName --name $eventHubAuthRuleName -o tsv --query "primaryConnectionString"

$serviceBusTopicAuthRuleName = az servicebus topic authorization-rule list --resource-group $resourceGroupName --namespace-name $serviceBusNamespaceName --topic-name "sqlbuildmanager" -o tsv --query "[].name"
$serviceBusConnectionString = az servicebus topic authorization-rule keys list --resource-group $resourceGroupName --namespace-name $serviceBusNamespaceName --topic-name "sqlbuildmanager" --name $serviceBusTopicAuthRuleName -o tsv --query "primaryConnectionString"


$params = @("k8s", "savesettings")
$params += @("--path", $path)
$params += @("--storageaccountname",$storageAccountName)  
$params += @("--storageaccountkey","""$storageAcctKey""") 
$params += @("-eh","""$eventHubConnectionString""") 
$params += @("-sb","""$serviceBusConnectionString""")  
$params += @("--concurrency", "5")
$params += @("--concurrencytype", "Count")
$params += @("--prefix", "k8s")


if($authTypes -contains "Password")
{
    if([string]::IsNullOrWhiteSpace($sqlUserName) -eq $false)
    {
        $params += @("--username",$sqlUserName)  
        $params += @("--password",$sqlPassword)
    }
    Start-Process $sbmExe -ArgumentList $params
}   

if($authTypes -contains "ManagedIdentity")
{
    $params = @("k8s", "savesettings")
    $params += @("--path", $path)
    $params += @("--storageaccountname",$storageAccountName)  
    $params += @("-eh","$($eventhubNamespaceName).servicebus.windows.net|$($eventHubName)") 
    $params += @("-sb","$($serviceBusNamespaceName).servicebus.windows.net")  
    $params += @("--concurrency", "5")
    $params += @("--concurrencytype", "Count")
    $params += @("--authtype", "ManagedIdentity")
    $params += @("--prefix", "k8s-mi")

    Start-Process $sbmExe -ArgumentList $params
}  

