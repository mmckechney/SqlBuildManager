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
Write-Host "Adding secrets to Key Vault"  -ForegroundColor Cyan

$path = Resolve-Path $path
Write-Host "Path set to $path" -ForegroundColor DarkGreen

# Write-Host "Setting current user Key Vault Access Policy" -ForegroundColor DarkGreen
# $currentUser = az account show -o tsv --query "user.name"
# $currentUserObjectId = az ad signed-in-user show -o tsv --query id
# az keyvault set-policy --name $keyVaultName --object-id $currentUserObjectId --secret-permissions get set list  set -o table

if("" -ne $batchAccountName)
{
    $keyName = "BatchAccountKey"
    Write-Host "Adding $keyName to $keyVaultName" -ForegroundColor DarkGreen
    $batchAcctKey  = az batch account keys list --name $batchAccountName --resource-group $resourceGroupName -o tsv --query 'primary'
    az keyvault secret set --value $batchAcctKey --vault-name $keyVaultName --name $keyName -o tsv --query "name"
}
else 
{
    Write-Host "Skipping BatchAccountKey, no value provided" -ForegroundColor Cyan
}

if("" -ne $storageAccountName)
{
    $keyName = "StorageAccountKey"
    Write-Host "Adding $keyName to $keyVaultName" -ForegroundColor DarkGreen
    $storageAcctKey = (az storage account keys list --account-name $storageAccountName -o tsv --query '[].value')[0]
    az keyvault secret set --value $storageAcctKey --vault-name $keyVaultName --name $keyName -o tsv --query "name"
}
else
{
    Write-Host "Skipping StorageAccountKey, no value provided" -ForegroundColor Cyan
}

if("" -ne $storageAccountName)
{
    $keyName = "StorageAccountName"
    Write-Host "Adding $keyName to $keyVaultName" -ForegroundColor DarkGreen
    az keyvault secret set --value $storageAccountName  --vault-name $keyVaultName --name $keyName -o tsv --query "name"
}
else
{
    Write-Host "Skipping StorageAccountName, no value provided" -ForegroundColor Cyan
}

if("" -ne $eventhubNamespaceName)
{
    $keyName = "EventHubConnectionString"
    Write-Host "Adding $keyName to $keyVaultName" -ForegroundColor DarkGreen
    $eventHubAuthRuleName = az eventhubs eventhub authorization-rule list  --resource-group $resourceGroupName --namespace-name $eventhubNamespaceName --eventhub-name $eventHubName -o tsv --query [].name
    $eventHubConnectionString = az eventhubs eventhub authorization-rule keys list --resource-group $resourceGroupName --namespace-name $eventHubNamespaceName --eventhub-name $eventHubName --name $eventHubAuthRuleName -o tsv --query "primaryConnectionString"
    az keyvault secret set --value $eventHubConnectionString --vault-name $keyVaultName --name $keyName -o tsv --query "name"
}
else 
{
    Write-Host "Skipping EventHubConnectionString, no value provided" -ForegroundColor Cyan
}

if("" -ne $serviceBusNamespaceName)
{
    $keyName = "ServiceBusTopicConnectionString"
    Write-Host "Adding $keyName to $keyVaultName" -ForegroundColor DarkGreen
    $serviceBusTopicAuthRuleName = az servicebus topic authorization-rule list --resource-group $resourceGroupName --namespace-name $serviceBusNamespaceName --topic-name "sqlbuildmanager" -o tsv --query "[].name"
    $ServiceBusTopicConnectionString = az servicebus topic authorization-rule keys list --resource-group $resourceGroupName --namespace-name $serviceBusNamespaceName --topic-name "sqlbuildmanager" --name $serviceBusTopicAuthRuleName -o tsv --query "primaryConnectionString"
    az keyvault secret set --value $ServiceBusTopicConnectionString --vault-name $keyVaultName --name $keyName -o tsv --query "name"
}
else
{
    Write-Host "Skipping ServiceBusTopicConnectionString, no value provided" -ForegroundColor Cyan
}

if("" -ne $sqlUserName)
{
    $keyName = "UserName"
    Write-Host "Adding $keyName to $keyVaultName" -ForegroundColor DarkGreen
    az keyvault secret set --value $sqlUserName --vault-name $keyVaultName --name $keyName -o tsv --query "name"
}
else 
{
    Write-Host "Skipping SQL User Name, no value provided" -ForegroundColor Cyan
}

if("" -ne $sqlPassword)
{
    $keyName = "Password"
    Write-Host "Adding $keyName to $keyVaultName" -ForegroundColor DarkGreen
    az keyvault secret set --value $sqlPassword --vault-name $keyVaultName --name $keyName -o tsv --query "name"
}
else 
{
    Write-Host "Skipping SQL Password, no value provided" -ForegroundColor Cyan
}
