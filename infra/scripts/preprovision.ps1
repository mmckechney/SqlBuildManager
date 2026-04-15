# Pre-provision hook for Azure Developer CLI (azd)
# Sets up deployment environment with current user info and IP address

Write-Host "Setting up deployment environment..." -ForegroundColor Cyan

# Get current IP address
$currentIpAddress = (Invoke-WebRequest -Uri "https://api.ipify.org" -UseBasicParsing).Content.Trim()
Write-Host "Current IP Address: $currentIpAddress" -ForegroundColor DarkGreen
azd env set CURRENT_IP_ADDRESS $currentIpAddress

# Get current user info
$userIdGuid = az ad signed-in-user show --query id -o tsv
$userLoginName = az account show --query user.name -o tsv
Write-Host "Current User: $userLoginName" -ForegroundColor DarkGreen
Write-Host "User Object ID: $userIdGuid" -ForegroundColor DarkGreen
azd env set AZURE_PRINCIPAL_ID $userIdGuid
azd env set AZURE_PRINCIPAL_NAME $userLoginName
azd env set BUILD_BATCH_PACKAGES "true"
azd env set BUILD_CONTAINER_IMAGES "true"
azd env set GENERATE_MI_SETTINGS "true"

# -------------------------------------------------------------------
# Deployment service selection
# Check if selections have already been saved to the environment.
# If not, prompt the user interactively. Saved selections are honored
# on subsequent runs; edit the .env file to change them.
# -------------------------------------------------------------------

function Get-AzdEnvValueSafe {
    param([string]$Name)
    $val = azd env get-value $Name 2>$null
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($val) -or $val -like "ERROR:*") {
        return $null
    }
    return $val
}

$needsPrompt = $null -eq (Get-AzdEnvValueSafe "DEPLOY_BATCH")

if ($needsPrompt) {
    Write-Host ""
    Write-Host "=======================================================" -ForegroundColor Cyan
    Write-Host "  SQL Build Manager - First-Time Deployment Selection   " -ForegroundColor Cyan
    Write-Host "=======================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Select which services to deploy. These choices are saved" -ForegroundColor Yellow
    Write-Host "in your azd environment and reused on subsequent runs." -ForegroundColor Yellow
    Write-Host "To change later: edit .azure/<env>/.env or use 'azd env set'." -ForegroundColor Yellow
    Write-Host ""

    # --- Compute platforms ---
    Write-Host "Compute platforms:" -ForegroundColor White
    Write-Host "  [1] Azure Batch" -ForegroundColor Gray
    Write-Host "  [2] Azure Container Instances (ACI)" -ForegroundColor Gray
    Write-Host "  [3] Azure Container Apps" -ForegroundColor Gray
    Write-Host "  [4] Azure Kubernetes Service (AKS)" -ForegroundColor Gray
    Write-Host ""
    $computeInput = Read-Host "Enter compute selections (comma-separated, e.g. 1,2,3,4 or 'all')"
    if ([string]::IsNullOrWhiteSpace($computeInput)) { $computeInput = "all" }

    $computeChoices = if ($computeInput.Trim().ToLower() -eq "all") { @("1","2","3","4") }
                      else { $computeInput -split "," | ForEach-Object { $_.Trim() } }

    $deployBatch        = $computeChoices -contains "1"
    $deployAci          = $computeChoices -contains "2"
    $deployContainerApp = $computeChoices -contains "3"
    $deployAks          = $computeChoices -contains "4"

    Write-Host ""

    # --- Database platforms ---
    Write-Host "Database platforms:" -ForegroundColor White
    Write-Host "  [1] SQL Server" -ForegroundColor Gray
    Write-Host "  [2] PostgreSQL" -ForegroundColor Gray
    Write-Host ""
    $dbInput = Read-Host "Enter database selections (comma-separated, e.g. 1,2 or 'all')"
    if ([string]::IsNullOrWhiteSpace($dbInput)) { $dbInput = "all" }

    $dbChoices = if ($dbInput.Trim().ToLower() -eq "all") { @("1","2") }
                 else { $dbInput -split "," | ForEach-Object { $_.Trim() } }

    $deploySqlServer  = $dbChoices -contains "1"
    $deployPostgreSQL = $dbChoices -contains "2"

    # Save selections
    azd env set DEPLOY_BATCH          $(if ($deployBatch)        { "true" } else { "false" })
    azd env set DEPLOY_ACI            $(if ($deployAci)          { "true" } else { "false" })
    azd env set DEPLOY_CONTAINERAPP   $(if ($deployContainerApp) { "true" } else { "false" })
    azd env set DEPLOY_AKS            $(if ($deployAks)          { "true" } else { "false" })
    azd env set DEPLOY_SQLSERVER      $(if ($deploySqlServer)    { "true" } else { "false" })
    azd env set DEPLOY_POSTGRESQL     $(if ($deployPostgreSQL)   { "true" } else { "false" })

    Write-Host ""
    Write-Host "Selections saved to azd environment." -ForegroundColor Green
} else {
    # Load existing selections for display
    $deployBatch        = (Get-AzdEnvValueSafe "DEPLOY_BATCH") -eq "true"
    $deployAci          = (Get-AzdEnvValueSafe "DEPLOY_ACI") -eq "true"
    $deployContainerApp = (Get-AzdEnvValueSafe "DEPLOY_CONTAINERAPP") -eq "true"
    $deployAks          = (Get-AzdEnvValueSafe "DEPLOY_AKS") -eq "true"
    $deploySqlServer    = (Get-AzdEnvValueSafe "DEPLOY_SQLSERVER") -eq "true"
    $deployPostgreSQL   = (Get-AzdEnvValueSafe "DEPLOY_POSTGRESQL") -eq "true"

    Write-Host "Using saved deployment selections:" -ForegroundColor DarkGreen
}

# Display current selections
$computeList = @()
if ($deployBatch)        { $computeList += "Batch" }
if ($deployAci)          { $computeList += "ACI" }
if ($deployContainerApp) { $computeList += "Container Apps" }
if ($deployAks)          { $computeList += "AKS" }
$dbList = @()
if ($deploySqlServer)    { $dbList += "SQL Server" }
if ($deployPostgreSQL)   { $dbList += "PostgreSQL" }

Write-Host "  Compute: $($computeList -join ', ')" -ForegroundColor Cyan
Write-Host "  Database: $($dbList -join ', ')" -ForegroundColor Cyan

# Always deploy container registry — used by compute platforms and ad-hoc ACI test containers
azd env set DEPLOY_CONTAINER_REGISTRY "true"

# Generate a random PostgreSQL admin password if not already set
if ($deployPostgreSQL) {
    $pgPassword = azd env get-value PG_ADMIN_PASSWORD 2>$null
    if ([string]::IsNullOrWhiteSpace($pgPassword) -or $pgPassword -like "ERROR:*") {
        $bytes = New-Object Byte[] 24
        [Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($bytes)
        $pgPassword = [System.Convert]::ToBase64String($bytes)
        Write-Host "Generated PostgreSQL admin password" -ForegroundColor DarkGreen
        azd env set PG_ADMIN_PASSWORD $pgPassword
    } else {
        Write-Host "Using existing PostgreSQL admin password" -ForegroundColor DarkGreen
    }
} else {
    # Set empty password so Bicep param substitution doesn't fail
    $pgPassword = azd env get-value PG_ADMIN_PASSWORD 2>$null
    if ([string]::IsNullOrWhiteSpace($pgPassword) -or $pgPassword -like "ERROR:*") {
        azd env set PG_ADMIN_PASSWORD ""
    }
}

Write-Host "Environment configured successfully!" -ForegroundColor Green
