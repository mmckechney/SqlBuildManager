param
(

    [string] $resourceGroupName,
    [string] $identityName,
    [string] $keyVaultName
)
Write-Host "Set managed identity RBAC"  -ForegroundColor Cyan
$clientId = az identity show --resource-group $resourceGroupName --name $identityName -o tsv --query clientId
$subscriptionId = az account show --query id --output tsv

Write-Host "Adding Role Assignments" -ForegroundColor DarkGreen
az role assignment create --role "Storage Blob Data Contributor" --assignee $clientId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table
az role assignment create --role "Key Vault Secrets User" --assignee $clientId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table
az role assignment create --role "Azure Service Bus Data Owner" --assignee $clientId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table
az role assignment create --role "Azure Event Hubs Data Receiver" --assignee $clientId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table
az role assignment create --role "Azure Event Hubs Data Sender" --assignee $clientId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table
az role assignment create --role "AcrPull" --assignee $clientId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table

az keyvault set-policy -n $keyVaultName --secret-permissions get --spn $clientId -o table
