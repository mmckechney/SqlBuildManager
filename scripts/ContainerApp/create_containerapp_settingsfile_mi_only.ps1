param
(
    [Parameter(Mandatory=$true)]
    [string] $prefix,

    [string] $sbmExe = "sbm.exe",
    [string] $path,
    [string] $resourceGroupName,
    [string] $imageTag = "latest-vNext"
)

<#
.SYNOPSIS
    Creates Container App settings files that use ONLY Managed Identity authentication (no keys or connection strings).

.DESCRIPTION
    This script generates settings files for Azure Container App deployments
    that use Managed Identity for ALL Azure service authentication:
    - Storage: Uses Managed Identity (no StorageAccountKey)
    - Event Hub: Uses namespace only (no connection string)
    - Service Bus: Uses namespace only (no connection string)
    - SQL: Uses Managed Identity (no username/password)
    - Container Registry: Uses Managed Identity (no password)

.PARAMETER prefix
    The resource name prefix used when deploying resources.

.PARAMETER sbmExe
    Path to the sbm.exe executable.

.PARAMETER path
    Output path for the generated settings files.

.PARAMETER resourceGroupName
    The Azure resource group name (defaults to {prefix}-rg).

.PARAMETER imageTag
    The container image tag to use.
#>

# Get the repo root
$repoRoot = $env:AZD_PROJECT_PATH
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path (Split-Path $script:MyInvocation.MyCommand.Path -Parent) -Parent) -Parent
}

if ([string]::IsNullOrWhiteSpace($path)) {
    $path = Join-Path $repoRoot "src\TestConfig"
}

#############################################
# Get set resource name variables from prefix
#############################################
$prefixScript = Join-Path $repoRoot "scripts\prefix_resource_names.ps1"
. $prefixScript -prefix $prefix

if ([string]::IsNullOrWhiteSpace($resourceGroupName)) {
    $resourceGroupName = "$prefix-rg"
}

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Creating Container App Managed Identity-Only Settings File" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "This settings file uses Managed Identity for ALL Azure services." -ForegroundColor Yellow
Write-Host "No keys, connection strings, or passwords are stored." -ForegroundColor Yellow
Write-Host ""

$path = Resolve-Path $path
Write-Host "Output path: $path" -ForegroundColor DarkGreen
Write-Host "Resource Group: $resourceGroupName" -ForegroundColor DarkGreen

# Get resource information
$subscriptionId = az account show --query id --output tsv
$tenantId = az account show -o tsv --query tenantId
$identity = az identity show --resource-group $resourceGroupName --name $identityName | ConvertFrom-Json

Write-Host "Using Storage Account: $storageAccountName" -ForegroundColor DarkGreen
Write-Host "Using Event Hub Namespace: $eventHubNamespaceName" -ForegroundColor DarkGreen
Write-Host "Using Service Bus Namespace: $serviceBusNamespaceName" -ForegroundColor DarkGreen
Write-Host "Using Container Registry: $containerRegistryName" -ForegroundColor DarkGreen
Write-Host "Using Container App Environment: $containerAppEnvName" -ForegroundColor DarkGreen
Write-Host "Using Managed Identity: $identityName (ClientId: $($identity.clientId))" -ForegroundColor DarkGreen

# Get Event Hub name
$eventHubName = az eventhubs eventhub list --resource-group $resourceGroupName --namespace-name $eventHubNamespaceName -o tsv --query "[0].name"

# Get Container App Environment info
$location = az containerapp env show -g $resourceGroupName -n $containerAppEnvName -o tsv --query location

# Get ACR server name
$acrServerName = az acr show -g $resourceGroupName --name $containerRegistryName -o tsv --query loginServer

# Output file path
$settingsContainerApp = Join-Path $path "settingsfile-containerapp-mi-only.json"

# Settings file key
$keyFile = Join-Path $path "settingsfilekey.txt"
if ($false -eq (Test-Path $keyFile)) {
    $AESKey = New-Object Byte[] 32
    [Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($AESKey)
    $settingsFileKey = [System.Convert]::ToBase64String($AESKey)
    $settingsFileKey | Set-Content -Path $keyFile
}

# Build parameters (NO KEYS!)
$params = @("containerapp", "savesettings")
$params += @("--tenantid", $tenantId)
$params += @("--environmentname", $containerAppEnvName)
$params += @("--location", """$location""")
$params += @("--resourcegroup", $resourceGroupName)
$params += @("--imagetag", $imageTag)
$params += @("--settingsfile", $settingsContainerApp)
$params += @("--settingsfilekey", $keyFile)
$params += @("--storageaccountname", $storageAccountName)
# NO --storageaccountkey - will use Managed Identity
$params += @("--ehrg", $resourceGroupName)
$params += @("--ehsub", $subscriptionId)
$params += @("--defaultscripttimeout", 500)
$params += @("--subscriptionid", $subscriptionId)
$params += @("--force", "true")
$params += @("--eventhublogging", "ScriptErrors")

# Identity parameters
$params += @("--identityname", $identityName)
$params += @("--idrg", $resourceGroupName)
$params += @("--clientid", $identity.clientId)

# Container Registry - use server name but NO credentials (MI will pull)
$params += @("--registryserver", $acrServerName)
# NO --registryusername or --registrypassword - will use Managed Identity

# Event Hub (namespace only, no connection string)
$ehValue = "$($eventHubNamespaceName)|$($eventHubName)"
$params += @("--eventhubconnection", $ehValue)

# Service Bus (namespace only, no connection string)
# NOTE: Container Apps KEDA scaler may still require connection string for Service Bus
# For MI-only, we use the namespace only and rely on MI for authentication
$params += @("--servicebustopicconnection", $serviceBusNamespaceName)

#############################################
# Generate Settings File
#############################################
if (Test-Path $settingsContainerApp) { Remove-Item $settingsContainerApp }
Write-Host "Saving MI-only settings file to $settingsContainerApp" -ForegroundColor DarkGreen
Write-Host $params -ForegroundColor DarkYellow
& $sbmExe $params

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "Container App Managed Identity-Only Settings File Created" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
Write-Host "This settings file contains NO secrets, keys, or connection strings." -ForegroundColor Cyan
Write-Host "All Azure services will authenticate using the Managed Identity:" -ForegroundColor Cyan
Write-Host "  Identity Name: $identityName" -ForegroundColor Cyan
Write-Host "  Client ID: $($identity.clientId)" -ForegroundColor Cyan
Write-Host ""
Write-Host "NOTE: Container Apps KEDA scaler for Service Bus may require additional" -ForegroundColor Yellow
Write-Host "      configuration for MI authentication." -ForegroundColor Yellow
Write-Host ""
Write-Host "File created:" -ForegroundColor Yellow
Write-Host "  - $settingsContainerApp" -ForegroundColor Yellow
