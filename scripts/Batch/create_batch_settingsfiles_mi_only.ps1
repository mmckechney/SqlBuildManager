param
(
    [Parameter(Mandatory=$true)]
    [string] $prefix,

    [string] $sbmExe = "sbm.exe",
    [string] $path,
    [string] $resourceGroupName
)

<#
.SYNOPSIS
    Creates settings files that use ONLY Managed Identity authentication (no keys or connection strings).

.DESCRIPTION
    This script generates settings files for Azure Batch, AKS, ACI, and Container App deployments
    that use Managed Identity for ALL Azure service authentication:
    - Storage: Uses Managed Identity (no StorageAccountKey)
    - Batch: Uses Managed Identity (no BatchAccountKey)
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
Write-Host "Creating Managed Identity-Only Settings Files" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "These settings files use Managed Identity for ALL Azure services." -ForegroundColor Yellow
Write-Host "No keys, connection strings, or passwords are stored." -ForegroundColor Yellow
Write-Host ""

$path = Resolve-Path $path
Write-Host "Output path: $path" -ForegroundColor DarkGreen
Write-Host "Resource Group: $resourceGroupName" -ForegroundColor DarkGreen

# Get resource information
$batchAcctEndpoint = az batch account show --name $batchAccountName --resource-group $resourceGroupName -o tsv --query "accountEndpoint"
$identity = az identity show --resource-group $resourceGroupName --name $identityName | ConvertFrom-Json
$subscriptionId = az account show -o tsv --query id
$tenantId = az account show -o tsv --query tenantId

Write-Host "Using Batch Account: $batchAccountName" -ForegroundColor DarkGreen
Write-Host "Using Storage Account: $storageAccountName" -ForegroundColor DarkGreen
Write-Host "Using Event Hub Namespace: $eventHubNamespaceName" -ForegroundColor DarkGreen
Write-Host "Using Service Bus Namespace: $serviceBusNamespaceName" -ForegroundColor DarkGreen
Write-Host "Using Managed Identity: $identityName (ClientId: $($identity.clientId))" -ForegroundColor DarkGreen

# Output file paths
$settingsJsonLinuxMiOnly = Join-Path $path "settingsfile-batch-linux-mi-only.json"
$settingsJsonWindowsMiOnly = Join-Path $path "settingsfile-batch-windows-mi-only.json"
$settingsJsonLinuxQueueMiOnly = Join-Path $path "settingsfile-batch-linux-queue-mi-only.json"
$settingsJsonWindowsQueueMiOnly = Join-Path $path "settingsfile-batch-windows-queue-mi-only.json"

# Settings file key
$keyFile = Join-Path $path "settingsfilekey.txt"
if ($false -eq (Test-Path $keyFile)) {
    $AESKey = New-Object Byte[] 32
    [Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($AESKey)
    $settingsFileKey = [System.Convert]::ToBase64String($AESKey)
    $settingsFileKey | Set-Content -Path $keyFile
}

# Base parameters (NO KEYS!)
$baseParams = @("batch", "savesettings")
$baseParams += @("--settingsfilekey", $keyFile)
$baseParams += @("--batchresourcegroup", $resourceGroupName)
$baseParams += @("--batchaccountname", $batchAccountName)
# NO --batchaccountkey - will use Managed Identity
$baseParams += @("--batchaccounturl", "https://$batchAcctEndpoint")
$baseParams += @("--batchnodecount", "2")
$baseParams += @("--batchvmsize", "STANDARD_D1_V2")
$baseParams += @("--rootloggingpath", "C:/temp")
$baseParams += @("--storageaccountname", $storageAccountName)
# NO --storageaccountkey - will use Managed Identity
$baseParams += @("--defaultscripttimeout", "500")
$baseParams += @("--concurrency", "5")
$baseParams += @("--concurrencytype", "Count")
$baseParams += @("--resourceid", $identity.id)
$baseParams += @("--idrg", $identity.resourceGroup)
$baseParams += @("--tenantid", $tenantId)
$baseParams += @("--subscriptionid", $subscriptionId)
$baseParams += @("--clientid", $identity.clientId)
$baseParams += @("--principalid", $identity.principalId)
$baseParams += @("--authtype", "ManagedIdentity")
$baseParams += @("--silent")
$baseParams += @("--eventhublogging", "ScriptErrors")
$baseParams += @("--ehrg", $resourceGroupName)
$baseParams += @("--ehsub", $subscriptionId)

# Network parameters
if ($vnet -ne "" -and $batchSubnet -ne "") {
    $baseParams += @("--vnetname", $vnet)
    $baseParams += @("--subnetname", $batchSubnet)
    $baseParams += @("--vnetrg", $resourceGroupName)
}

# OS-specific parameters
$winParams = @("--batchpoolname", "SqlBuildManagerPoolWindows", "-os", "Windows")
$linuxParams = @("--batchpoolname", "SqlBuildManagerPoolLinux", "-os", "Linux")

# Event Hub (namespace only, no connection string)
$eventHubName = az eventhubs eventhub list --resource-group $resourceGroupName --namespace-name $eventHubNamespaceName -o tsv --query "[0].name"
$ehValue = "$($eventHubNamespaceName)|$($eventHubName)"
$ehNameParam = @("-eh", $ehValue)

# Service Bus (namespace only, no connection string)
$sbNamespaceParam = @("-sb", $serviceBusNamespaceName)

#############################################
# Generate Settings Files
#############################################

# Linux with Event Hub logging only
if (Test-Path $settingsJsonLinuxMiOnly) { Remove-Item $settingsJsonLinuxMiOnly }
Write-Host "Saving MI-only settings file to $settingsJsonLinuxMiOnly" -ForegroundColor DarkGreen
$tmpPath = @("--settingsfile", $settingsJsonLinuxMiOnly)
$allArgs = $baseParams + $linuxParams + $tmpPath + $ehNameParam
& $sbmExe $allArgs

# Windows with Event Hub logging only
if (Test-Path $settingsJsonWindowsMiOnly) { Remove-Item $settingsJsonWindowsMiOnly }
Write-Host "Saving MI-only settings file to $settingsJsonWindowsMiOnly" -ForegroundColor DarkGreen
$tmpPath = @("--settingsfile", $settingsJsonWindowsMiOnly)
$allArgs = $baseParams + $winParams + $tmpPath + $ehNameParam
& $sbmExe $allArgs

# Linux with Service Bus Queue
if (Test-Path $settingsJsonLinuxQueueMiOnly) { Remove-Item $settingsJsonLinuxQueueMiOnly }
Write-Host "Saving MI-only settings file to $settingsJsonLinuxQueueMiOnly" -ForegroundColor DarkGreen
$tmpPath = @("--settingsfile", $settingsJsonLinuxQueueMiOnly)
$allArgs = $baseParams + $linuxParams + $tmpPath + $sbNamespaceParam + $ehNameParam
& $sbmExe $allArgs

# Windows with Service Bus Queue
if (Test-Path $settingsJsonWindowsQueueMiOnly) { Remove-Item $settingsJsonWindowsQueueMiOnly }
Write-Host "Saving MI-only settings file to $settingsJsonWindowsQueueMiOnly" -ForegroundColor DarkGreen
$tmpPath = @("--settingsfile", $settingsJsonWindowsQueueMiOnly)
$allArgs = $baseParams + $winParams + $tmpPath + $sbNamespaceParam + $ehNameParam
& $sbmExe $allArgs

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "Managed Identity-Only Settings Files Created" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
Write-Host "These settings files contain NO secrets, keys, or connection strings." -ForegroundColor Cyan
Write-Host "All Azure services will authenticate using the Managed Identity:" -ForegroundColor Cyan
Write-Host "  Identity Name: $identityName" -ForegroundColor Cyan
Write-Host "  Client ID: $($identity.clientId)" -ForegroundColor Cyan
Write-Host ""
Write-Host "Files created:" -ForegroundColor Yellow
Write-Host "  - $settingsJsonLinuxMiOnly" -ForegroundColor Yellow
Write-Host "  - $settingsJsonWindowsMiOnly" -ForegroundColor Yellow
Write-Host "  - $settingsJsonLinuxQueueMiOnly" -ForegroundColor Yellow
Write-Host "  - $settingsJsonWindowsQueueMiOnly" -ForegroundColor Yellow
