param
(
    [Parameter(Mandatory=$true)]
    [string] $envName,

    [string] $sbmExe = "sbm.exe",
    [string] $path,
    [string] $resourceGroupName,
    [int] $podCount = 2,
    [ValidateSet("AzureADDefault", "Password")]
    [string] $databaseAuthType = "AzureADDefault",
    [string] $databaseUserName = "",
    [string] $databasePassword = "",
    [string] $settingsFileSuffix = "mi-only"
)

<#
.SYNOPSIS
    Creates AKS settings files that use ONLY Managed Identity authentication (no keys or connection strings).

.DESCRIPTION
    This script generates settings files for Azure Kubernetes Service (AKS) deployments
    that use Managed Identity for ALL Azure service authentication:
    - Storage: Uses Managed Identity (no StorageAccountKey)
    - Event Hub: Uses namespace only (no connection string)
    - Service Bus: Uses namespace only (no connection string)
    - SQL: Uses Managed Identity (no username/password)
    - Container Registry: Uses Managed Identity (no password)

.PARAMETER envName
    The Azure Developer CLI environment name used when deploying resources.

.PARAMETER sbmExe
    Path to the sbm.exe executable.

.PARAMETER path
    Output path for the generated settings files.

.PARAMETER resourceGroupName
    The Azure resource group name (defaults to rg-{envName}).

.PARAMETER podCount
    Number of Kubernetes pods to create.
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
$resourceGroupNameOverride = $resourceGroupName
. $prefixScript -envName $envName
if (-not [string]::IsNullOrWhiteSpace($resourceGroupNameOverride)) {
    $resourceGroupName = $resourceGroupNameOverride
}

Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "Creating AKS Managed Identity-Only Settings File" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "This settings file uses Managed Identity for ALL Azure services." -ForegroundColor Yellow
Write-Host "No keys, connection strings, or passwords are stored." -ForegroundColor Yellow
Write-Host ""

$path = Resolve-Path $path
Write-Host "Output path: $path" -ForegroundColor DarkGreen
Write-Host "Resource Group: $resourceGroupName" -ForegroundColor DarkGreen

if ($databaseAuthType -eq "Password" -and ([string]::IsNullOrWhiteSpace($databaseUserName) -or [string]::IsNullOrWhiteSpace($databasePassword))) {
    throw "databaseUserName and databasePassword are required when databaseAuthType is Password."
}

# Get resource information
$subscriptionId = az account show --query id --output tsv
$tenantId = az account show -o tsv --query tenantId
$identity = az identity show --resource-group $resourceGroupName --name $identityName | ConvertFrom-Json

Write-Host "Using Storage Account: $storageAccountName" -ForegroundColor DarkGreen
Write-Host "Using Event Hub Namespace: $eventHubNamespaceName" -ForegroundColor DarkGreen
Write-Host "Using Service Bus Namespace: $serviceBusNamespaceName" -ForegroundColor DarkGreen
Write-Host "Using Container Registry: $containerRegistryName" -ForegroundColor DarkGreen
Write-Host "Using Service Account: $serviceAccountName" -ForegroundColor DarkGreen

# Get Event Hub name
$eventHubName = az eventhubs eventhub list --resource-group $resourceGroupName --namespace-name $eventHubNamespaceName -o tsv --query "[0].name"

# Output file path
$settingsFileName = Join-Path $path "settingsfile-k8s-$settingsFileSuffix.json"

# Settings file key
$keyFile = Join-Path $path "settingsfilekey.txt"
if ($false -eq (Test-Path $keyFile)) {
    $AESKey = New-Object Byte[] 32
    [Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($AESKey)
    $settingsFileKey = [System.Convert]::ToBase64String($AESKey)
    $settingsFileKey | Set-Content -Path $keyFile
}

# Build parameters (NO KEYS!)
$params = @("k8s", "savesettings")
$params += @("--concurrency", "5")
$params += @("--concurrencytype", "Count")
$params += @("--registry", $containerRegistryName)
$params += @("--tag", "latest-vNext")
$params += @("--tenantid", $tenantId)
$params += @("--serviceaccountname", $serviceAccountName)
$params += @("--identityname", $identityName)
$params += @("--clientid", $identity.clientId)
$params += @("--idrg", $resourceGroupName)
$params += @("--force")
$params += @("--podcount", $podCount)
$params += @("--eventhublogging", "ScriptErrors")
$params += @("--ehrg", $resourceGroupName)
$params += @("--ehsub", $subscriptionId)

# Storage (namespace only, no key)
$params += @("--storageaccountname", $storageAccountName)
$params += @("--relayproxyendpoint", "https://$relayNamespaceName.servicebus.windows.net/relayproxy")
# NO --storageaccountkey - will use Managed Identity

# Event Hub (namespace only, no connection string)
$ehValue = "$($eventHubNamespaceName)|$($eventHubName)"
$params += @("-eh", $ehValue)

# Service Bus (namespace only, no connection string)
$params += @("-sb", $serviceBusNamespaceName)

# Set auth type to Managed Identity

$params += @("--authtype", $databaseAuthType)
if ($databaseAuthType -eq "Password") {
    $params += @("--username", $databaseUserName)
    $params += @("--password", $databasePassword)
}

# Output file
$params += @("--settingsfile", $settingsFileName)

#############################################
# Generate Settings File
#############################################
if (Test-Path $settingsFileName) { Remove-Item $settingsFileName }
Write-Host "Saving MI-only settings file to $settingsFileName" -ForegroundColor DarkGreen
Write-Host $params -ForegroundColor DarkYellow
& $sbmExe $params

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "AKS Managed Identity-Only Settings File Created" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
Write-Host "This settings file contains NO secrets, keys, or connection strings." -ForegroundColor Cyan
Write-Host "All Azure services will authenticate using Workload Identity:" -ForegroundColor Cyan
Write-Host "  Service Account: $serviceAccountName" -ForegroundColor Cyan
Write-Host ""
Write-Host "File created:" -ForegroundColor Yellow
Write-Host "  - $settingsFileName" -ForegroundColor Yellow
