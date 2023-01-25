param
(

    [string] $resourceGroupName,
    [string] $keyVaultName
)
Write-Host "Set current user identity RBAC"  -ForegroundColor Cyan
$userId = az ad signed-in-user show -o tsv --query id
$subscriptionId = az account show --query id --output tsv

Write-Host "Adding Role Assignments" -ForegroundColor DarkGreen
az role assignment create --role "Storage Blob Data Contributor" --assignee $userId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table
az role assignment create --role "Key Vault Secrets User" --assignee $userId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table
az role assignment create --role "Azure Service Bus Data Owner" --assignee $userId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table
az role assignment create --role "Azure Event Hubs Data Receiver" --assignee $userId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table
az role assignment create --role "Azure Event Hubs Data Sender" --assignee $userId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table
az role assignment create --role "AcrPull" --assignee $userId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table

az role assignment create --role "Azure Kubernetes Service Cluster Admin Role" --assignee $userId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table
az role assignment create --role "Azure Kubernetes Service RBAC Admin" --assignee $userId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table
az role assignment create --role "Azure Kubernetes Service RBAC Writer" --assignee $userId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table

az keyvault set-policy -n $keyVaultName --secret-permissions get set --object-id $userId -o table
