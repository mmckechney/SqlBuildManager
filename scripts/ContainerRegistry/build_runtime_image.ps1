<#
.SYNOPSIS
    Builds and pushes the production SQL Build Manager runtime container image to ACR.
.DESCRIPTION
    Copies the src/ folder to a clean temp directory (excluding .vs, bin, obj, TestConfig,
    TestResults), reads the assembly version from AssemblyVersioning.cs, then builds the
    Dockerfile via ACR Build with date, version, and latest-vNext tags.
.PARAMETER azureContainerRegistry
    Name of the Azure Container Registry.
.PARAMETER resourceGroupName
    Azure resource group containing the container registry.
.PARAMETER wait
    Whether to wait for the ACR build to complete. Default: true.
#>
param
(
    [string] $azureContainerRegistry, 
    [string] $resourceGroupName,
    [bool] $wait = $true
)


Write-Host "Logging into Azure Container Registry" -ForegroundColor DarkGreen
$tmp = az acr login -n $azureContainerRegistry --expose-token

$dateTag = Get-Date -Format "yyyy-MM-dd"
$dateTag =  "sqlbuildmanager:$dateTag"
$vnextTag =  "sqlbuildmanager:latest-vNext"
Write-Host "Uploading and building Container image on registry $azureContainerRegistry." -ForegroundColor DarkGreen

# Get the repo root
$repoRoot = $env:AZD_PROJECT_PATH
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path (Split-Path $script:MyInvocation.MyCommand.Path -Parent) -Parent) -Parent
}
$originalSourcePath = Join-Path $repoRoot "src"

# Create a clean copy of source to avoid VS file locks
$tempBuildContext = Join-Path $env:TEMP "acr-build-context-$(Get-Random)"
Write-Host "Creating clean build context at $tempBuildContext (excluding .vs folder)..." -ForegroundColor DarkGreen

# Use robocopy to copy files excluding .vs, bin, obj folders (robocopy doesn't fail on locked files it skips)
$excludeDirs = ".vs", ".vs_backup", "bin", "obj", "TestConfig", "TestResults"
robocopy $originalSourcePath $tempBuildContext /E /XD $excludeDirs /NFL /NDL /NJH /NJS /NC /NS /NP | Out-Null

$dockerFile = Join-Path $tempBuildContext "Dockerfile"
$sourcePath = $tempBuildContext
Write-Host "Using DockerFile from $sourcePath" -ForegroundColor DarkGreen


$assemblyVersionFile = Join-Path $tempBuildContext "AssemblyVersioning.cs"
$success = ((Get-Content $assemblyVersionFile)  -match '\d{1,3}\.\d{1,3}\.\d{1,3}')[0] -match '\d{1,3}\.\d{1,3}\.\d{1,3}' 
if ($success)
{
    $ver = $Matches.0
    $verTag =  "sqlbuildmanager:$ver"
    Write-Host "Building with image tags: '$verTag' | '$dateTag' | '$vnextTag' (used in integration tests)" -ForegroundColor DarkGreen
    if($true -eq $wait)
    {
        az acr build --image $dateTag --image $vnextTag --image $verTag --registry $azureContainerRegistry --resource-group $resourceGroupName --file "$dockerFile" "$sourcePath" --no-logs --query outputimages
    }
    else {
        az acr build --image $dateTag --image $vnextTag --image $verTag --registry $azureContainerRegistry --resource-group $resourceGroupName --file "$dockerFile" "$sourcePath" --no-logs --query outputimages --no-wait
    }
}
else 
{
    Write-Host "Unable to read AssemblyVersion.cs file. Can not create a version tag" -ForegroundColor Yellow
    Write-Host "Building with image tags: '$dateTag' | '$vnextTag' (used in integration tests)" -ForegroundColor DarkGreen
    if($true -eq $wait)
    {
        az acr build --image $dateTag --image $vnextTag --registry $azureContainerRegistry --resource-group $resourceGroupName  --file "$dockerFile" "$sourcePath" --no-logs --query outputimages
    }
    else {
        az acr build --image $dateTag --image $vnextTag --registry $azureContainerRegistry --resource-group $resourceGroupName  --file "$dockerFile" "$sourcePath" --no-logs --query outputimages --no-wait
    }
}

# Clean up temp directory
Write-Host "Cleaning up temp build context..." -ForegroundColor DarkGreen
Remove-Item -Path $tempBuildContext -Recurse -Force -ErrorAction SilentlyContinue

