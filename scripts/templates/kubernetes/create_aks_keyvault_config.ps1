param
(
    [string] $path,
    [string] $resourceGroupName,
    [string] $keyVaultName,
    [string] $identityName
)
Write-Host "Create AKS key vault configuration"  -ForegroundColor Cyan
$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path

$path = Resolve-Path $path
Write-Host "Output path set to $path" -ForegroundColor DarkGreen

$context = (az account show)  | ConvertFrom-Json 
$subscriptionId = $context.id
$tenantId = $context.tenantId
$userAssignedIdentityName = $identityName
$userAssignedClientId = az identity list --resource-group $resourceGroupName  -o tsv --query "[?contains(@.name '$identityName')].clientId"

$fileName = Join-Path $path "k8s-podIdentity.yaml"
Write-Host("Writing $fileName");
$azureIdentityBinding= Get-Content "$($scriptDir)/podIdentity_template.yaml"
$azureIdentityBinding = $azureIdentityBinding.Replace("{{subscriptionId}}", $subscriptionId)
$azureIdentityBinding = $azureIdentityBinding.Replace("{{userAssignedIdentityName}}", $userAssignedIdentityName)
$azureIdentityBinding = $azureIdentityBinding.Replace("{{resourceGroupName}}", $resourceGroupName)
$azureIdentityBinding = $azureIdentityBinding.Replace("{{userAssignedClientId}}", $userAssignedClientId)
$azureIdentityBinding | Out-File -FilePath $fileName

$fileName = Join-Path $path "k8s-podIdentityBinding.yaml"
Write-Host("Writing $fileName");
$azureIdentityBinding= Get-Content "$($scriptDir)/podIdentity_binding_template.yaml"
$azureIdentityBinding = $azureIdentityBinding.Replace("{{userAssignedIdentityName}}", $userAssignedIdentityName)
$azureIdentityBinding | Out-File -FilePath $fileName

$fileName = Join-Path $path "k8s-secretProviderClass.yaml"
Write-Host("Writing $fileName");
$providerClass=  Get-Content "$($scriptDir)/secretProviderClass_template.yaml" 
$providerClass=  $providerClass.Replace("{{userAssignedClientId}}", $userAssignedClientId)
$providerClass=  $providerClass.Replace("{{keyVaultName}}", $keyVaultName)
$providerClass=  $providerClass.Replace("{{tenantId}}", $tenantId)
$providerClass=  $providerClass | Out-File -FilePath $fileName

