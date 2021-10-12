param
(
    [string] $sbmExe = "sbm.exe",
    [string] $path,
    [string] $resourceGroupName,
    [string] $storageAccountName,
    [string] $eventHubNamespaceName,
    [string] $serviceBusNamespaceName,
    [string] $sqlUserName,
    [string] $sqlPassword
)
Write-Host "Create AKS secrets and runtime files"  -ForegroundColor Cyan

$path = Resolve-Path $path
Write-Host "Output path set to $path" -ForegroundColor DarkGreen

$storageAcctKey = (az storage account keys list --account-name $storageAccountName -o tsv --query '[].value')[0]


$eventHubName = az eventhubs eventhub list  --resource-group $resourceGroupName --namespace-name $eventhubNamespaceName -o tsv --query "[?contains(@.name '$prefix')].name"
$eventHubAuthRuleName = az eventhubs eventhub authorization-rule list  --resource-group $resourceGroupName --namespace-name $eventhubNamespaceName --eventhub-name $eventHubName -o tsv --query [].name
$eventHubConnectionString = az eventhubs eventhub authorization-rule keys list --resource-group $resourceGroupName --namespace-name $eventHubNamespaceName --eventhub-name $eventHubName --name $eventHubAuthRuleName -o tsv --query "primaryConnectionString"

$serviceBusTopicAuthRuleName = az servicebus topic authorization-rule list --resource-group $resourceGroupName --namespace-name $serviceBusNamespaceName --topic-name "sqlbuildmanager" -o tsv --query "[].name"
$serviceBusConnectionString = az servicebus topic authorization-rule keys list --resource-group $resourceGroupName --namespace-name $serviceBusNamespaceName --topic-name "sqlbuildmanager" --name $serviceBusTopicAuthRuleName -o tsv --query "primaryConnectionString"

$params = @("k8s", "savesettings")
$params += @("--path", $path)
$params += @("--username",$sqlUserName)
$params += @("--password",$sqlPassword) 
$params += @("--storageaccountname",$storageAccountName)  
$params += @("--storageaccountkey",$storageAcctKey) 
$params += @("-eh",$eventHubConnectionString) 
$params += @("-sb",$serviceBusConnectionString)  
$params += @("--concurrency", "5"
$params += @("--concurrencytype", "Count")
if([string]::IsNullOrWhiteSpace($sqlUserName) -eq $false)
{
    $params += @("--username",$sqlUserName)  
    $params += @("--password",$sqlPassword)
}

Start-Process $sbmExe -ArgumentList $params
