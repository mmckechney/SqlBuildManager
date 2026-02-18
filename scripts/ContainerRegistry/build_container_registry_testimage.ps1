
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
New-Item -ItemType Directory -Path $tempContext -Force | Out-Null

Write-Host "Copying source to temp location (excluding .vs, bin, obj)..." -ForegroundColor DarkGray

# Use Get-ChildItem with exclusions and Copy-Item
Get-ChildItem -Path $contextPath -Exclude '.vs','bin','obj','TestResults' | ForEach-Object {
    if ($_.PSIsContainer) {
        # For directories, use robocopy to handle subdirectory exclusions
        if ($_.Name -notin @('.vs', 'bin', 'obj', 'TestResults')) {
            $destDir = Join-Path $tempContext $_.Name
            robocopy $_.FullName $destDir /E /XD .vs bin obj TestResults /XF *.user /NFL /NDL /NJH /NJS /NC /NS /NP | Out-Null
        }
    } else {
        # For files, just copy them
        Copy-Item $_.FullName -Destination $tempContext
    }
}

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
