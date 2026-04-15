<#
.SYNOPSIS
    Builds and pushes the external (integration) test container image to ACR.
.DESCRIPTION
    Copies the src/ folder (excluding build artifacts like .vs, bin, obj) to a temp
    directory and uses ACR Build to build the Dockerfile.tests image. The resulting
    image contains the test runner, Azure CLI, and kubectl for running integration
    tests in Azure Container Instances.
.PARAMETER prefix
    Environment name prefix used to derive the ACR name.
.PARAMETER resourceGroupName
    Azure resource group containing the container registry.
.PARAMETER wait
    Whether to wait for the ACR build to complete. Default: true.
.PARAMETER imageTag
    Tag for the built image. Default: "test-runner".
#>
param
(
    [string] $prefix,
    [string] $resourceGroupName,
    [bool] $wait = $true,
    [string] $imageTag = "test-runner"
)



$repoRoot = $env:AZD_PROJECT_PATH
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path (Split-Path $script:MyInvocation.MyCommand.Path -Parent) -Parent) -Parent
}

$testImageName = "sqlbuildmanager-tests"

#############################################
# Get set resource name variables from prefix
#############################################
$prefixScript = Join-Path $repoRoot "scripts\prefix_resource_names.ps1"
. $prefixScript -prefix $prefix

#############################################
# Build and push test image if requested
#############################################

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Building Test Container Image" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$contextPath = Join-Path $repoRoot "src"
$dockerfileName = "Dockerfile.tests"

Write-Host "Building image using ACR Build..." -ForegroundColor DarkGreen
Write-Host "  Context: $contextPath" -ForegroundColor DarkGray

# Copy source to temp folder excluding .vs and other problematic folders
$tempContext = Join-Path $env:TEMP "sbm-test-build-$(Get-Date -Format 'yyyyMMddHHmmss')"
Write-Host "Copying source to temp location (excluding .vs, bin, obj)..." -ForegroundColor DarkGray

$excludeDirs = ".vs", ".vs_backup", "bin", "obj", "TestResults"
robocopy $contextPath $tempContext /E /XD $excludeDirs /XF *.user /NFL /NDL /NJH /NJS /NC /NS /NP | Out-Null

# Remove .dockerignore if it exists (it might be excluding things we need)
$dockerignorePath = Join-Path $tempContext ".dockerignore"
if (Test-Path $dockerignorePath) {
    Write-Host "Removing .dockerignore to prevent exclusions..." -ForegroundColor DarkGray
    Remove-Item $dockerignorePath -Force
}

# Verify Dockerfile exists
$dockerfilePath = Join-Path $tempContext $dockerfileName
Write-Host "Checking for Dockerfile at: $dockerfilePath" -ForegroundColor DarkGray

if (-not (Test-Path $dockerfilePath)) {
    Write-Host "ERROR: Dockerfile.tests not found at $dockerfilePath" -ForegroundColor Red
    Write-Host "Top-level contents of temp folder:" -ForegroundColor Yellow
    Get-ChildItem $tempContext -Name
    exit 1
}

Write-Host "Dockerfile found. Starting ACR build..." -ForegroundColor DarkGreen
Write-Host "Temp context contents:" -ForegroundColor DarkGray
Get-ChildItem $tempContext -Name | Where-Object { $_ -like "Dockerfile*" -or $_ -like "*.csproj" }

try {
    # Use full path to dockerfile
    az acr build `
        --registry $containerRegistryName `
        --resource-group $resourceGroupName `
        --image "${testImageName}:${imageTag}" `
        --file "$tempContext\$dockerfileName" `
        --no-logs `
        $tempContext
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Failed to build test container image" -ForegroundColor Red
        exit 1
    }
}
finally {
    # Clean up temp folder
    if (Test-Path $tempContext) {
        Remove-Item $tempContext -Recurse -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "Test image built successfully: $acrLoginServer/${testImageName}:${imageTag}" -ForegroundColor Green
Write-Host ""
