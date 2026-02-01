param
(
    [Parameter(Mandatory=$true)]
    [string] $prefix,

    [string] $sbmExe = "sbm.exe",
    [string] $path = "..\..\src\TestConfig",
    [string] $resourceGroupName,
    [string] $imageTag = "latest-vNext"
)

<#
.SYNOPSIS
    Creates ACI settings files that use ONLY Managed Identity authentication (no keys or connection strings).

.DESCRIPTION
    This script generates settings files for Azure Container Instance (ACI) deployments
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

#############################################
# Get set resource name variables from prefix
#############################################
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$prefixScript = Join-Path $scriptDir "..\prefix_resource_names.ps1"
. $prefixScript -prefix $prefix

if ([string]::IsNullOrWhiteSpace($resourceGroupName)) {
    $resourceGroupName = "$prefix-rg"
}

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Creating ACI Managed Identity-Only Settings File" -ForegroundColor Cyan
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
Write-Host "Using Managed Identity: $identityName (ClientId: $($identity.clientId))" -ForegroundColor DarkGreen
Write-Host "Using ACI Name: $aciName" -ForegroundColor DarkGreen
Write-Host "Using VNET: $vnet" -ForegroundColor DarkGreen
Write-Host "Using Subnet: $aciSubnet" -ForegroundColor DarkGreen

# Get Event Hub name
$eventHubName = az eventhubs eventhub list --resource-group $resourceGroupName --namespace-name $eventHubNamespaceName -o tsv --query "[0].name"

# Get ACR server name
$acrServerName = az acr show -g $resourceGroupName --name $containerRegistryName -o tsv --query loginServer

# Output file path
$settingsAci = Join-Path $path "settingsfile-aci-mi-only.json"

# Settings file key
$keyFile = Join-Path $path "settingsfilekey.txt"
if ($false -eq (Test-Path $keyFile)) {
    $AESKey = New-Object Byte[] 32
    [Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($AESKey)
    $settingsFileKey = [System.Convert]::ToBase64String($AESKey)
    $settingsFileKey | Set-Content -Path $keyFile
}

# Build parameters (NO KEYS!)
$params = @("aci", "savesettings")
$params += @("--settingsfile", $settingsAci)
# Note: ACI CLI requires -kv but we use a placeholder since MI mode doesn't need it
$params += @("-kv", "placeholder-not-used-with-mi")
$params += @("--aciname", $aciName)
$params += @("--identityname", $identityName)
$params += @("--clientid", $identity.clientId)
$params += @("--idrg", $resourceGroupName)
$params += @("--acirg", $resourceGroupName)
$params += @("--tenantid", $tenantId)
$params += @("--storageaccountname", $storageAccountName)
# NO --storageaccountkey - will use Managed Identity
$params += @("--defaultscripttimeout", "500")
$params += @("--subscriptionid", $subscriptionId)
$params += @("--force")
$params += @("--eventhublogging", "ScriptErrors")
$params += @("--ehrg", $resourceGroupName)
$params += @("--ehsub", $subscriptionId)

# Container Registry - use server name but NO credentials (MI will pull)
$params += @("--registryserver", $acrServerName)
# NO --registryusername or --registrypassword - will use Managed Identity

# Image tag
$params += @("--imagetag", $imageTag)

# Network settings
if ($vnet -ne "" -and $aciSubnet -ne "") {
    $params += @("--vnetname", $vnet)
    $params += @("--subnetname", $aciSubnet)
    $params += @("--vnetrg", $resourceGroupName)
}

# Auth type
$params += @("--authtype", "ManagedIdentity")

# Event Hub (namespace only, no connection string)
$ehValue = "$($eventHubNamespaceName)|$($eventHubName)"
$params += @("-eh", $ehValue)

# Service Bus (namespace only, no connection string)
$params += @("-sb", $serviceBusNamespaceName)

#############################################
# Generate Settings File
#############################################
if (Test-Path $settingsAci) { Remove-Item $settingsAci }
Write-Host "Saving MI-only settings file to $settingsAci" -ForegroundColor DarkGreen
Write-Host $params -ForegroundColor DarkYellow
& $sbmExe $params

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "ACI Managed Identity-Only Settings File Created" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
Write-Host "This settings file contains NO secrets, keys, or connection strings." -ForegroundColor Cyan
Write-Host "All Azure services will authenticate using the Managed Identity:" -ForegroundColor Cyan
Write-Host "  Identity Name: $identityName" -ForegroundColor Cyan
Write-Host "  Client ID: $($identity.clientId)" -ForegroundColor Cyan
Write-Host ""
Write-Host "File created:" -ForegroundColor Yellow
Write-Host "  - $settingsAci" -ForegroundColor Yellow
