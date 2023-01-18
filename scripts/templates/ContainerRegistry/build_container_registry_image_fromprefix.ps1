param
(
    [string] $prefix,
    [string] $resourceGroupName,
    [string] $path = "..\..\..\src\TestConfig",
    [bool] $wait = $true
)
#############################################
# Get set resource name variables from prefix
#############################################
. ./../prefix_resource_names.ps1 -prefix $prefix
. ./../key_file_names.ps1 -prefix $prefix -path $path

Write-Host "Upload and build Docker image in Container Registry from prefix: $prefix" -ForegroundColor Cyan
Write-Host "Retrieving resource names from resources in $resourceGroupName with prefix $prefix" -ForegroundColor DarkGreen
Write-Host "Using Azure Container Registry Name: $containerRegistryName  " -ForegroundColor DarkGreen

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/build_container_registry_image.ps1 -azureContainerRegistry $containerRegistryName -resourceGroupName $resourceGroupName -wait $wait
