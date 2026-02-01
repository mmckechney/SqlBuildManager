param
(
    [string] $prefix,
    [string] $resourceGroupName,
    [string] $path = "..\..\src\TestConfig",
    [bool] $wait = $true
)
#############################################
# Get set resource name variables from prefix
#############################################
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$prefixScript = Join-Path $scriptDir "..\prefix_resource_names.ps1"
$keyFileScript = Join-Path $scriptDir "..\key_file_names.ps1"

. $prefixScript -prefix $prefix
. $keyFileScript -prefix $prefix -path $path

Write-Host "Upload and build Docker image in Container Registry from prefix: $prefix" -ForegroundColor Cyan
Write-Host "Retrieving resource names from resources in $resourceGroupName with prefix $prefix" -ForegroundColor DarkGreen
Write-Host "Using Azure Container Registry Name: $containerRegistryName  " -ForegroundColor DarkGreen

$buildScript = Join-Path $scriptDir "build_container_registry_image.ps1"
& $buildScript -azureContainerRegistry $containerRegistryName -resourceGroupName $resourceGroupName -wait $wait
