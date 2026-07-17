<#
.SYNOPSIS
    Resolves resource names from an environment name and delegates to build_runtime_image.ps1.
.DESCRIPTION
    Wrapper script that loads standard resource names from an azd environment name,
    then calls build_runtime_image.ps1 to build and push the production runtime
    container image to Azure Container Registry.
.PARAMETER envName
    Azure Developer CLI environment name used to derive resource names.
.PARAMETER resourceGroupName
    Azure resource group containing the container registry.
.PARAMETER path
    Output directory for key files. Defaults to src\TestConfig.
.PARAMETER wait
    Whether to wait for the ACR build to complete. Default: true.
#>
param
(
    [string] $envName,
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
# Get resource name variables from the environment name
#############################################
$prefixScript = Join-Path $repoRoot "scripts\prefix_resource_names.ps1"
$keyFileScript = Join-Path $repoRoot "scripts\key_file_names.ps1"

. $prefixScript -envName $envName
. $keyFileScript -envName $envName -path $path

Write-Host "Upload and build Docker image in Container Registry for environment: $envName" -ForegroundColor Cyan
Write-Host "Retrieving resource names from resources in $resourceGroupName for environment $envName" -ForegroundColor DarkGreen
Write-Host "Using Azure Container Registry Name: $containerRegistryName  " -ForegroundColor DarkGreen

$buildScript = Join-Path $repoRoot "scripts\ContainerRegistry\build_runtime_image.ps1"
& $buildScript -azureContainerRegistry $containerRegistryName -resourceGroupName $resourceGroupName -wait $wait
