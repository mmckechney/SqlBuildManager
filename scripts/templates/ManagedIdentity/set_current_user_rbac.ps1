param
(

    [string] $resourceGroupName,
    [string] $keyVaultName
)
Write-Host "Set current user identity RBAC"  -ForegroundColor Cyan
$userId = az ad signed-in-user show -o tsv --query id
$subscriptionId = az account show --query id --output tsv

Write-Host "Adding Role Assignments" -ForegroundColor DarkGreen

$roleAssignmentExists = az role assignment list --assignee $userId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName --query "[?roleDefinitionName=='Storage Blob Data Contributor'].principalName" -o tsv | Where-Object { $_ -eq $userId }
if (-not $roleAssignmentExists) {
    az role assignment create --role "Storage Blob Data Contributor" --assignee $userId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table
}
else {
    Write-Host "Role assignment 'Storage Blob Data Contributor' already exists" -ForegroundColor Cyan
}

$roleAssignmentExists = az role assignment list --assignee $userId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName --query "[?roleDefinitionName=='Key Vault Secrets User'].principalName" -o tsv | Where-Object { $_ -eq $userId }
if (-not $roleAssignmentExists) {
    az role assignment create --role "Key Vault Secrets User" --assignee $userId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table
}
else {
    Write-Host "Role assignment 'Key Vault Secrets User' already exists" -ForegroundColor Cyan
}

$roleAssignmentExists = az role assignment list --assignee $userId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName --query "[?roleDefinitionName=='Azure Service Bus Data Owner'].principalName" -o tsv | Where-Object { $_ -eq $userId }
if (-not $roleAssignmentExists) {
    az role assignment create --role "Azure Service Bus Data Owner" --assignee $userId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table
}
else {
    Write-Host "Role assignment 'Azure Service Bus Data Owner' already exists" -ForegroundColor Cyan
}

$roleAssignmentExists = az role assignment list --assignee $userId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName --query "[?roleDefinitionName=='Azure Event Hubs Data Receiver'].principalName" -o tsv | Where-Object { $_ -eq $userId }
if (-not $roleAssignmentExists) {
    az role assignment create --role "Azure Event Hubs Data Receiver" --assignee $userId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table
}
else {
    Write-Host "Role assignment 'Azure Event Hubs Data Receiver' already exists" -ForegroundColor Cyan
}

$roleAssignmentExists = az role assignment list --assignee $userId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName --query "[?roleDefinitionName=='Azure Event Hubs Data Sender'].principalName" -o tsv | Where-Object { $_ -eq $userId }
if (-not $roleAssignmentExists) {
    az role assignment create --role "Azure Event Hubs Data Sender" --assignee $userId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table
}
else {
    Write-Host "Role assignment 'Azure Event Hubs Data Sender' already exists" -ForegroundColor Cyan
}

$roleAssignmentExists = az role assignment list --assignee $userId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName --query "[?roleDefinitionName=='AcrPull'].principalName" -o tsv | Where-Object { $_ -eq $userId }
if (-not $roleAssignmentExists) {
    az role assignment create --role "AcrPull" --assignee $userId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table
}
else {
    Write-Host "Role assignment 'AcrPull' already exists" -ForegroundColor Cyan
}

$roleAssignmentExists = az role assignment list --assignee $userId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName --query "[?roleDefinitionName=='Azure Kubernetes Service Cluster Admin Role'].principalName" -o tsv | Where-Object { $_ -eq $userId }
if (-not $roleAssignmentExists) {
    az role assignment create --role "Azure Kubernetes Service Cluster Admin Role" --assignee $userId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table
}
else {
    Write-Host "Role assignment 'Azure Kubernetes Service Cluster Admin Role' already exists" -ForegroundColor Cyan
}

$roleAssignmentExists = az role assignment list --assignee $userId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName --query "[?roleDefinitionName=='Azure Kubernetes Service RBAC Admin'].principalName" -o tsv | Where-Object { $_ -eq $userId }
if (-not $roleAssignmentExists) {
    az role assignment create --role "Azure Kubernetes Service RBAC Admin" --assignee $userId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table
}
else {
    Write-Host "Role assignment 'Azure Kubernetes Service RBAC Admin' already exists" -ForegroundColor Cyan
}

$roleAssignmentExists = az role assignment list --assignee $userId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName --query "[?roleDefinitionName=='Azure Kubernetes Service RBAC Writer'].principalName" -o tsv | Where-Object { $_ -eq $userId }
if (-not $roleAssignmentExists) {
    az role assignment create --role "Azure Kubernetes Service RBAC Writer" --assignee $userId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table
}
else {
    Write-Host "Role assignment 'Azure Kubernetes Service RBAC Writer' already exists" -ForegroundColor Cyan
}


az keyvault set-policy -n $keyVaultName --secret-permissions get set --object-id $userId -o table
