param
(
    [string] $prefix,
    [string] $resourceGroupName,
    [string] $path,
    [bool] $wait = $true
)

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
$keyFileScript = Join-Path $repoRoot "scripts\key_file_names.ps1"

. $prefixScript -prefix $prefix
. $keyFileScript -prefix $prefix -path $path

Write-Host "Upload and build Docker image in Container Registry from prefix: $prefix" -ForegroundColor Cyan
Write-Host "Retrieving resource names from resources in $resourceGroupName with prefix $prefix" -ForegroundColor DarkGreen
Write-Host "Using Azure Container Registry Name: $containerRegistryName  " -ForegroundColor DarkGreen

$buildScript = Join-Path $repoRoot "scripts\ContainerRegistry\build_runtime_image.ps1"
& $buildScript -azureContainerRegistry $containerRegistryName -resourceGroupName $resourceGroupName -wait $wait
