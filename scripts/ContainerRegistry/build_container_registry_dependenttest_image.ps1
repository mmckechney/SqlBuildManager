
param
(
    [string] $prefix,
    [string] $resourceGroupName,
    [string] $imageTag = "dependent-test-runner"
)

<#
.SYNOPSIS
    Builds and pushes the dependent test container image to ACR.

.DESCRIPTION
    Copies the src/ folder (excluding build artifacts) to a temp directory and uses
    ACR Build to build the Dockerfile.dependent-tests image.

.PARAMETER prefix
    The resource name prefix used when deploying resources.

.PARAMETER resourceGroupName
    Optional resource group name override (defaults to "$prefix-rg").

.PARAMETER imageTag
    Tag for the container image (default: dependent-test-runner).
#>

$repoRoot = $env:AZD_PROJECT_PATH
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path (Split-Path $script:MyInvocation.MyCommand.Path -Parent) -Parent) -Parent
}

if([string]::IsNullOrWhiteSpace($resourceGroupName)) {
    $resourceGroupName = "$prefix-rg"
}

$testImageName = "sqlbuildmanager-dependent-tests"

#############################################
# Get set resource name variables from prefix
#############################################
$prefixScript = Join-Path $repoRoot "scripts\prefix_resource_names.ps1"
. $prefixScript -prefix $prefix

if ([string]::IsNullOrWhiteSpace($resourceGroupName)) {
    $resourceGroupName = "$prefix-rg"
}

#############################################
# Build and push dependent test image
#############################################

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Building Dependent Test Container Image" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$contextPath = Join-Path $repoRoot "src"
$dockerfileName = "Dockerfile.dependent-tests"

Write-Host "Building image using ACR Build..." -ForegroundColor DarkGreen
Write-Host "  Context: $contextPath" -ForegroundColor DarkGray

# Copy source to temp folder excluding .vs and other problematic folders
$tempContext = Join-Path $env:TEMP "sbm-dependent-tests-$(Get-Date -Format 'yyyyMMddHHmmss')"
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
    Write-Host "ERROR: $dockerfileName not found at $dockerfilePath" -ForegroundColor Red
    Write-Host "Top-level contents of temp folder:" -ForegroundColor Yellow
    Get-ChildItem $tempContext -Name
    exit 1
}

Write-Host "Dockerfile found. Starting ACR build..." -ForegroundColor DarkGreen
Write-Host "Temp context contents:" -ForegroundColor DarkGray
Get-ChildItem $tempContext -Name | Where-Object { $_ -like "Dockerfile*" -or $_ -like "*.csproj" }

try {
    az acr build `
        --registry $containerRegistryName `
        --resource-group $resourceGroupName `
        --image "${testImageName}:${imageTag}" `
        --file "$tempContext\$dockerfileName" `
        $tempContext
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Failed to build dependent test container image" -ForegroundColor Red
        exit 1
    }
}
finally {
    # Clean up temp folder
    if (Test-Path $tempContext) {
        Remove-Item $tempContext -Recurse -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "Dependent test image built successfully: ${testImageName}:${imageTag}" -ForegroundColor Green
Write-Host ""
