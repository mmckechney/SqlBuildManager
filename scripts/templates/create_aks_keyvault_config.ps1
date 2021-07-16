param
(
    [string] $path,
    [string] $resourceGroupName,
    [string] $keyVaultName,
    [string] $identityName
)
$path = Resolve-Path $path
Write-Host "Output path set to $path" -ForegroundColor DarkGreen

$context = (az account show)  | ConvertFrom-Json 
$subscriptionId = $context.id
$tenantId = $context.tenantId
$userAssignedIdentityName = $identityName
$userAssignedClientId = az identity list --resource-group $resourceGroupName  -o tsv --query "[?contains(@.name '$identityName')].clientId"

$azureIdentityBinding= Get-Content "./podIdentityAndBinding_template.yaml"
$azureIdentityBinding = $azureIdentityBinding.Replace("{{subscriptionId}}", $subscriptionId)
$azureIdentityBinding = $azureIdentityBinding.Replace("{{userAssignedIdentityName}}", $userAssignedIdentityName)
$azureIdentityBinding = $azureIdentityBinding.Replace("{{resourceGroupName}}", $resourceGroupName)
$azureIdentityBinding = $azureIdentityBinding.Replace("{{userAssignedClientId}}", $userAssignedClientId)
$azureIdentityBinding | Out-File -FilePath (Join-Path $path "podIdentityAndBinding.yaml")


$providerClass=  Get-Content "./secretProviderClass_template.yaml" 
$providerClass=  $providerClass.Replace("{{userAssignedClientId}}", $userAssignedClientId)
$providerClass=  $providerClass.Replace("{{keyVaultName}}", $keyVaultName)
$providerClass=  $providerClass.Replace("{{tenantId}}", $tenantId)
$providerClass=  $providerClass | Out-File -FilePath (Join-Path $path "secretProviderClass.yaml")

