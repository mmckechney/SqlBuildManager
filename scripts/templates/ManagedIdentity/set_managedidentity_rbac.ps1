param
(

    [string] $resourceGroupName,
    [string] $identityName,
    [string] $keyVaultName
)
Write-Host "Set managed identity RBAC"  -ForegroundColor Cyan
$clientId = az identity show --resource-group $resourceGroupName --name $identityName -o tsv --query clientId
$subscriptionId = az account show --query id --output tsv

Write-Host "Adding Role Assignments for $identityName" -ForegroundColor DarkGreen



$roleAssignmentExists = az role assignment list --assignee $clientId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName --query "[?roleDefinitionName=='Key Vault Secrets User'].principalName" -o tsv | Where-Object { $_ -eq $clientId }
if (-not $roleAssignmentExists) {
    az role assignment create --role "Key Vault Secrets User" --assignee $clientId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table
}
else {
    Write-Host "Role assignment 'Key Vault Secrets User' already exists." -ForegroundColor Cyan
}

$roleAssignmentExists = az role assignment list --assignee $clientId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName --query "[?roleDefinitionName=='Azure Service Bus Data Owner'].principalName" -o tsv | Where-Object { $_ -eq $clientId }
if (-not $roleAssignmentExists) {
    az role assignment create --role "Azure Service Bus Data Owner" --assignee $clientId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table
}
else {
    Write-Host "Role assignment 'Azure Service Bus Data Owner' already exists." -ForegroundColor Cyan
}

$roleAssignmentExists = az role assignment list --assignee $clientId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName --query "[?roleDefinitionName=='Azure Event Hubs Data Receiver'].principalName" -o tsv | Where-Object { $_ -eq $clientId }
if (-not $roleAssignmentExists) {
    az role assignment create --role "Azure Event Hubs Data Receiver" --assignee $clientId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table
}
else {
    Write-Host "Role assignment 'Azure Event Hubs Data Receiver' already exists." -ForegroundColor Cyan
}

$roleAssignmentExists = az role assignment list --assignee $clientId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName --query "[?roleDefinitionName=='Azure Event Hubs Data Sender'].principalName" -o tsv | Where-Object { $_ -eq $clientId }
if (-not $roleAssignmentExists) {
    az role assignment create --role "Azure Event Hubs Data Sender" --assignee $clientId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table
}
else {
    Write-Host "Role assignment 'Azure Event Hubs Data Sender' already exists." -ForegroundColor Cyan
}

$roleAssignmentExists = az role assignment list --assignee $clientId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName --query "[?roleDefinitionName=='AcrPull'].principalName" -o tsv | Where-Object { $_ -eq $clientId }
if (-not $roleAssignmentExists) {
    az role assignment create --role "AcrPull" --assignee $clientId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table
}
else {
    Write-Host "Role assignment 'AcrPull' already exists." -ForegroundColor Cyan
}

$roleAssignmentExists = az role assignment list --assignee $clientId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName --query "[?roleDefinitionName=='Contributor'].principalName" -o tsv | Where-Object { $_ -eq $clientId }
if (-not $roleAssignmentExists) {
    az role assignment create --role "Contributor" --assignee $clientId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName -o table
}
else {
    Write-Host "Role assignment 'Contributor' already exists." -ForegroundColor Cyan
}


Write-Host "Settings key vault policy for $keyVaultName" -ForegroundColor DarkGreen
az keyvault set-policy -n $keyVaultName --resource-group $resourceGroupName --secret-permissions get --spn $clientId -o table
