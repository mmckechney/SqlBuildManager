# Post-provision hook for Azure Developer CLI (azd)
# Grants managed identity SQL permissions and optionally generates settings files

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Post-Provision: Granting SQL Permissions" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Get the environment name (used as prefix)
$prefix = $env:AZURE_ENV_NAME
$resourceGroupName = "$prefix-rg"

Write-Host "Environment: $prefix" -ForegroundColor DarkGreen
Write-Host "Resource Group: $resourceGroupName" -ForegroundColor DarkGreen

# Get the repo root (where azure.yaml is located)
$repoRoot = $env:AZD_PROJECT_PATH
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Get-Location
}
Write-Host "Repo Root: $repoRoot" -ForegroundColor DarkGreen

# Run the grant identity permissions script
$scriptPath = Join-Path $repoRoot "scripts\templates\Database\grant_identity_permissions.ps1"
if (Test-Path $scriptPath) {
    & $scriptPath -prefix $prefix -resourceGroupName $resourceGroupName
} else {
    Write-Host "Script not found at: $scriptPath" -ForegroundColor Yellow
    Write-Host "Skipping SQL permissions grant. Run manually after deployment:" -ForegroundColor Yellow
    Write-Host "  .\scripts\templates\Database\grant_identity_permissions.ps1 -prefix $prefix -resourceGroupName $resourceGroupName" -ForegroundColor Yellow
}

# Build and upload Batch application packages if BUILD_BATCH_PACKAGES is set
$buildBatch = $env:BUILD_BATCH_PACKAGES
if ($buildBatch -eq "true") {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Post-Provision: Building Batch Application Packages" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    
    $batchScriptPath = Join-Path $repoRoot "scripts\templates\Batch\build_and_upload_batch_fromprefix.ps1"
    $outputPath = Join-Path $repoRoot "src\TestConfig"
    
    if (Test-Path $batchScriptPath) {
        & $batchScriptPath -prefix $prefix -resourceGroupName $resourceGroupName -path $outputPath -action "BuildAndUpload"
    } else {
        Write-Host "Batch build script not found at: $batchScriptPath" -ForegroundColor Yellow
        Write-Host "Run manually: .\scripts\templates\Batch\build_and_upload_batch_fromprefix.ps1 -prefix $prefix" -ForegroundColor Yellow
    }
} else {
    Write-Host ""
    Write-Host "Tip: Set BUILD_BATCH_PACKAGES=true to build and upload Batch application packages" -ForegroundColor DarkGray
    Write-Host "  azd env set BUILD_BATCH_PACKAGES true" -ForegroundColor DarkGray
}

# Build and push Docker container images if BUILD_CONTAINER_IMAGES is set
$buildContainers = $env:BUILD_CONTAINER_IMAGES
if ($buildContainers -eq "true") {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Post-Provision: Building Container Images" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    
    $containerScriptPath = Join-Path $repoRoot "scripts\templates\ContainerRegistry\build_container_registry_image_fromprefix.ps1"
    $outputPath = Join-Path $repoRoot "src\TestConfig"
    
    if (Test-Path $containerScriptPath) {
        & $containerScriptPath -prefix $prefix -resourceGroupName $resourceGroupName -path $outputPath -wait $true
    } else {
        Write-Host "Container build script not found at: $containerScriptPath" -ForegroundColor Yellow
        Write-Host "Run manually: .\scripts\templates\ContainerRegistry\build_container_registry_image_fromprefix.ps1 -prefix $prefix" -ForegroundColor Yellow
    }
} else {
    Write-Host ""
    Write-Host "Tip: Set BUILD_CONTAINER_IMAGES=true to build and push Docker container images" -ForegroundColor DarkGray
    Write-Host "  azd env set BUILD_CONTAINER_IMAGES true" -ForegroundColor DarkGray
}

# Generate MI-only settings files if GENERATE_MI_SETTINGS is set
$generateSettings = $env:GENERATE_MI_SETTINGS
if ($generateSettings -eq "true") {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Post-Provision: Generating MI-Only Settings Files" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    
    $settingsScriptPath = Join-Path $repoRoot "scripts\templates\create_all_settingsfiles_mi_only.ps1"
    $outputPath = Join-Path $repoRoot "src\TestConfig"
    
    # Ensure output directory exists
    if (-not (Test-Path $outputPath)) {
        New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
    }
    
    if (Test-Path $settingsScriptPath) {
        & $settingsScriptPath -prefix $prefix -resourceGroupName $resourceGroupName -path $outputPath
    } else {
        Write-Host "Settings script not found at: $settingsScriptPath" -ForegroundColor Yellow
        Write-Host "Run manually: .\scripts\templates\create_all_settingsfiles_mi_only.ps1 -prefix $prefix" -ForegroundColor Yellow
    }
} else {
    Write-Host ""
    Write-Host "Tip: Set GENERATE_MI_SETTINGS=true to auto-generate MI-only settings files" -ForegroundColor DarkGray
    Write-Host "  azd env set GENERATE_MI_SETTINGS true" -ForegroundColor DarkGray
}

# Generate database override config files for integration tests
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Post-Provision: Creating Database Config Files" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$dbConfigScriptPath = Join-Path $repoRoot "scripts\templates\Database\create_database_override_files.ps1"
$outputPath = Join-Path $repoRoot "src\TestConfig"

# Ensure output directory exists
if (-not (Test-Path $outputPath)) {
    New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
}

if (Test-Path $dbConfigScriptPath) {
    & $dbConfigScriptPath -prefix $prefix -path $outputPath
} else {
    Write-Host "Database config script not found at: $dbConfigScriptPath" -ForegroundColor Yellow
    Write-Host "Run manually: .\scripts\templates\Database\create_database_override_files.ps1 -prefix $prefix -path $outputPath" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Deployment Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "This deployment uses Managed Identity for all service-to-service authentication:" -ForegroundColor Cyan
Write-Host "  - Azure SQL: Entra ID (Azure AD) only authentication" -ForegroundColor Cyan
Write-Host "  - Event Hub: RBAC roles (EventHubsDataReceiver/EventHubsDataSender)" -ForegroundColor Cyan
Write-Host "  - Service Bus: RBAC role (ServiceBusDataOwner)" -ForegroundColor Cyan
Write-Host "  - Storage: RBAC role (StorageBlobDataContributor)" -ForegroundColor Cyan
Write-Host "  - Container Registry: RBAC role (AcrPull)" -ForegroundColor Cyan
Write-Host ""
Write-Host "Application code should use DefaultAzureCredential for authentication." -ForegroundColor Yellow
Write-Host ""
Write-Host "Environment variables to enable optional post-provision steps:" -ForegroundColor Cyan
Write-Host "  BUILD_BATCH_PACKAGES=true   - Build and upload Batch application packages" -ForegroundColor DarkGray
Write-Host "  BUILD_CONTAINER_IMAGES=true - Build and push Docker container images to ACR" -ForegroundColor DarkGray
Write-Host "  GENERATE_MI_SETTINGS=true   - Generate MI-only settings files for testing" -ForegroundColor DarkGray
