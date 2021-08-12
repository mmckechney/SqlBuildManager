param
(

    [string] $resourceGroupName,
    [string] $identityName,
    [string] $keyVaultName
)

$clientId = az identity show --resource-group $resourceGroupName --name $identityName -o tsv --query clientId
$subscriptionId = az account show --query id --output tsv

Write-Host "Adding Role Assignments" -ForegroundColor DarkGreen
az role assignment create --role "Storage Blob Data Contributor" --assignee $clientId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName
az role assignment create --role "Key Vault Reader" --assignee $clientId --scope /subscriptions/$subscriptionId/resourcegroups/$resourceGroupName

az keyvault set-policy -n $keyVaultName --secret-permissions get --spn $clientId
az keyvault set-policy -n $keyVaultName --key-permissions get --spn $clientId
az keyvault set-policy -n $keyVaultName --certificate-permissions get --spn $clientId