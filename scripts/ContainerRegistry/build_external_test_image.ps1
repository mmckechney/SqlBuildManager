<#
.SYNOPSIS
    Builds and pushes the external (integration) test container image to ACR.
.DESCRIPTION
    Copies the src/ folder (excluding build artifacts and all TestConfig directories)
    to a temp directory, stages only the selected environment's configuration, and
    uses ACR Build to build the Dockerfile.tests image. The resulting
    image contains the test runner, Azure CLI, and kubectl for running integration
    tests in Azure Container Instances.
.PARAMETER envName
    Azure Developer CLI environment name used to derive the ACR name.
.PARAMETER resourceGroupName
    Azure resource group containing the container registry.
.PARAMETER wait
    Whether to wait for the ACR build to complete. Default: true.
.PARAMETER imageTag
    Tag for the built image. Default: "test-runner".
.PARAMETER path
    Exact configuration directory override. Defaults to src\TestConfig\<envName>.
#>
param
(
    [string] $envName,
    [string] $resourceGroupName,
    [bool] $wait = $true,
    [string] $imageTag = "test-runner",
    [string] $path
)
$ErrorActionPreference = 'Stop'
$repoRoot = $env:AZD_PROJECT_PATH
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path (Split-Path $script:MyInvocation.MyCommand.Path -Parent) -Parent) -Parent
}

$testImageName = "sqlbuildmanager-tests"

#############################################
# Get resource name variables from the environment name
#############################################
$prefixScript = Join-Path $repoRoot "scripts\prefix_resource_names.ps1"
$resourceGroupNameOverride = $resourceGroupName
. $prefixScript -envName $envName
if (-not [string]::IsNullOrWhiteSpace($resourceGroupNameOverride)) {
    $resourceGroupName = $resourceGroupNameOverride
}
. (Join-Path $PSScriptRoot '..\test_config_paths.ps1')
$configPath = Get-TestConfigPath -envName $envName -path $path -repoRoot $repoRoot
if (-not (Test-Path -LiteralPath (Join-Path $configPath 'settingsfilekey.txt') -PathType Leaf)) {
    throw "Missing settingsfilekey.txt in '$configPath'. Generate this environment's settings before building its test image."
}

$acrLoginServer = az acr show `
    --name $containerRegistryName `
    --resource-group $resourceGroupName `
    --query loginServer `
    --output tsv
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($acrLoginServer)) {
    throw "Unable to determine the login server for Azure Container Registry '$containerRegistryName'."
}

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
$tempContext = Join-Path ([IO.Path]::GetTempPath()) "sbm-test-build-$([guid]::NewGuid())"
Write-Host "Copying source to temp location (excluding .vs, bin, obj)..." -ForegroundColor DarkGray

try {
    $excludeDirs = ".vs", ".vs_backup", "bin", "obj", "TestResults", "TestConfig"
    robocopy $contextPath $tempContext /E /XD $excludeDirs /XF *.user /NFL /NDL /NJH /NJS /NC /NS /NP | Out-Null
    if ($LASTEXITCODE -ge 8) {
        throw "Unable to stage test image source (robocopy exit code $LASTEXITCODE)."
    }
    $stagedConfig = Join-Path $tempContext 'TestConfig' $envName
    New-Item -ItemType Directory -Path $stagedConfig -Force | Out-Null
    Get-ChildItem -LiteralPath $configPath -File |
        Where-Object { $_.Extension -in @('.json', '.cfg', '.txt', '.yaml', '.yml') } |
        Copy-Item -Destination $stagedConfig

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
        throw "Dockerfile.tests not found at '$dockerfilePath'."
    }

    Write-Host "Dockerfile found. Starting ACR build..." -ForegroundColor DarkGreen
    Write-Host "Temp context contents:" -ForegroundColor DarkGray
    Get-ChildItem $tempContext -Name | Where-Object { $_ -like "Dockerfile*" -or $_ -like "*.csproj" }

    # Use full path to dockerfile
    az acr build `
        --registry $containerRegistryName `
        --resource-group $resourceGroupName `
        --image "${testImageName}:${imageTag}" `
        --file "$tempContext\$dockerfileName" `
        --build-arg "AZD_ENV_NAME=$envName" `
        --no-logs `
        --output none `
        $tempContext
    
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to build test container image for '$envName'."
    }
}
finally {
    # Clean up temp folder
    if (Test-Path $tempContext) {
        Remove-Item -LiteralPath $tempContext -Recurse -Force
    }
}

Write-Host "Test image built successfully: $acrLoginServer/${testImageName}:${imageTag}" -ForegroundColor Green
Write-Host ""
