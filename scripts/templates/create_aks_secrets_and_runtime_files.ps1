param
(
    [string] $path,
    [string] $resourceGroupName,
    [string] $storageAccountName,
    [string] $eventHubNamespaceName,
    [string] $serviceBusNamespaceName,
    [string] $sqlUserName,
    [string] $sqlPassword
)
$path = Resolve-Path $path
Write-Host "Output path set to $path" -ForegroundColor DarkGreen

$storageAcctKey = (az storage account keys list --account-name $storageAccountName -o tsv --query '[].value')[0]


$eventHubName = az eventhubs eventhub list  --resource-group $resourceGroupName --namespace-name $eventhubNamespaceName -o tsv --query "[?contains(@.name '$prefix')].name"
$eventHubAuthRuleName = az eventhubs eventhub authorization-rule list  --resource-group $resourceGroupName --namespace-name $eventhubNamespaceName --eventhub-name $eventHubName -o tsv --query [].name
$eventHubConnectionString = az eventhubs eventhub authorization-rule keys list --resource-group $resourceGroupName --namespace-name $eventHubNamespaceName --eventhub-name $eventHubName --name $eventHubAuthRuleName -o tsv --query "primaryConnectionString"

$serviceBusTopicAuthRuleName = az servicebus topic authorization-rule list --resource-group $resourceGroupName --namespace-name $serviceBusNamespaceName --topic-name "sqlbuildmanager" -o tsv --query "[].name"
$serviceBusConnectionString = az servicebus topic authorization-rule keys list --resource-group $resourceGroupName --namespace-name $serviceBusNamespaceName --topic-name "sqlbuildmanager" --name $serviceBusTopicAuthRuleName -o tsv --query "primaryConnectionString"

if([string]::IsNullOrWhiteSpace($sqlUserName) -eq $false)
{
    Write-Host "Saving settings file to $path" -ForegroundColor DarkGreen
    ..\..\src\SqlBuildManager.Console\bin\Debug\net5.0\sbm.exe k8s savesettings --path "$path" --username "$sqlUserName" --password "$sqlPassword" --storageaccountname "$storageAccountName"  --storageaccountkey "$storageAcctKey" -eh "$eventHubConnectionString" -sb "$serviceBusConnectionString "  --concurrency 5 --concurrencytype "Count"
}
else 
{

    Write-Host "Saving settings file to $path" -ForegroundColor DarkGreen
    ..\..\src\SqlBuildManager.Console\bin\Debug\net5.0\sbm.exe k8s savesettings --path "$path" --storageaccountname "$storageAccountName"  --storageaccountkey "$storageAcctKey" -eh "$eventHubConnectionString" -sb "$serviceBusConnectionString "  --concurrency 5 --concurrencytype "Count"
}

