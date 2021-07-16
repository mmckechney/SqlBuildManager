param
(
    [string] $path,
    [string] $resourceGroupName,
    [string] $keyVaultName,
    [string] $batchAccountName,
    [string] $storageAccountName,
    [string] $eventHubNamespaceName,
    [string] $serviceBusNamespaceName,
    [string] $sqlUserName,
    [string] $sqlPassword
)

$path = Resolve-Path $path
Write-Host "Path set to $path" -ForegroundColor DarkGreen

Write-Host "Setting current user Key Vault Access Policy" -ForegroundColor DarkGreen
$currentUser = az account show -o tsv --query "user.name"
$currentUserObjectId = az ad user show --id $currentUser -o tsv --query objectId
az keyvault set-policy --name $keyVaultName --object-id $currentUserObjectId --secret-permissions get list  set

Write-Host "Collecting Secret Information from resources" -ForegroundColor DarkGreen

$batchAcctKey  = az batch account keys list --name $batchAccountName --resource-group $resourceGroupName -o tsv --query 'primary'
$batchAcctEndpoint = az batch account show --name $batchAccountName --resource-group $resourceGroupName -o tsv --query "accountEndpoint"


$storageAcctKey = (az storage account keys list --account-name $storageAccountName -o tsv --query '[].value')[0]


$eventHubName = az eventhubs eventhub list  --resource-group $resourceGroupName --namespace-name $eventhubNamespaceName -o tsv --query "[?contains(@.name '$prefix')].name"
$eventHubAuthRuleName = az eventhubs eventhub authorization-rule list  --resource-group $resourceGroupName --namespace-name $eventhubNamespaceName --eventhub-name $eventHubName -o tsv --query [].name
$eventHubConnectionString = az eventhubs eventhub authorization-rule keys list --resource-group $resourceGroupName --namespace-name $eventHubNamespaceName --eventhub-name $eventHubName --name $eventHubAuthRuleName -o tsv --query "primaryConnectionString"


$serviceBusTopicAuthRuleName = az servicebus topic authorization-rule list --resource-group $resourceGroupName --namespace-name $serviceBusNamespaceName --topic-name "sqlbuildmanager" -o tsv --query "[].name"
$ServiceBusTopicConnectionString = az servicebus topic authorization-rule keys list --resource-group $resourceGroupName --namespace-name $serviceBusNamespaceName --topic-name "sqlbuildmanager" --name $serviceBusTopicAuthRuleName -o tsv --query "primaryConnectionString"

$keyName = "BatchAccountKey"
Write-Host "Adding $keyName to $keyVaultName" -ForegroundColor DarkGreen
az keyvault secret set --value $batchAcctKey --vault-name $keyVaultName --name $keyName -o tsv --query "name"

$keyName = "StorageAccountKey"
Write-Host "Adding $keyName to $keyVaultName" -ForegroundColor DarkGreen
az keyvault secret set --value $storageAcctKey --vault-name $keyVaultName --name $keyName -o tsv --query "name"

$keyName = "StorageAccountName"
Write-Host "Adding $keyName to $keyVaultName" -ForegroundColor DarkGreen
az keyvault secret set --value $storageAccountName  --vault-name $keyVaultName --name $keyName -o tsv --query "name"

$keyName = "EventHubConnectionString"
Write-Host "Adding $keyName to $keyVaultName" -ForegroundColor DarkGreen
az keyvault secret set --value $eventHubConnectionString --vault-name $keyVaultName --name $keyName -o tsv --query "name"

$keyName = "ServiceBusTopicConnectionString"
Write-Host "Adding $keyName to $keyVaultName" -ForegroundColor DarkGreen
az keyvault secret set --value $ServiceBusTopicConnectionString --vault-name $keyVaultName --name $keyName -o tsv --query "name"

$keyName = "UserName"
Write-Host "Adding $keyName to $keyVaultName" -ForegroundColor DarkGreen
az keyvault secret set --value $sqlUserName --vault-name $keyVaultName --name $keyName -o tsv --query "name"

$keyName = "Password"
Write-Host "Adding $keyName to $keyVaultName" -ForegroundColor DarkGreen
az keyvault secret set --value $sqlPassword --vault-name $keyVaultName --name $keyName -o tsv --query "name"
