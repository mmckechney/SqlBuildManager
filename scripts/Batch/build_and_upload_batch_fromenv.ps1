<#
.SYNOPSIS
    Resolves resource names from an environment name and delegates to build_and_upload_batch.ps1.
.DESCRIPTION
    Wrapper script that loads standard resource names from an azd environment name,
    then calls build_and_upload_batch.ps1 to publish, package, and upload the
    SQL Build Manager console application as Azure Batch application packages.
.PARAMETER envName
    Azure Developer CLI environment name used to derive resource names.
.PARAMETER resourceGroupName
    Azure resource group name. Derived from envName if not specified.
.PARAMETER action
    BuildOnly, UploadOnly, or BuildAndUpload (default).
.PARAMETER path
    Output directory for build artifacts. Defaults to src\TestConfig.
#>
param
(
    [Parameter(Mandatory=$true)]
    [string] $envName,
    [string] $resourceGroupName,
    [ValidateSet("BuildOnly", "UploadOnly", "BuildAndUpload")]
    [string] $action = "BuildAndUpload",
    [string] $path
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

Write-Host "Build and Upload Batch for environment: $envName" -ForegroundColor Cyan
Write-Host "Retrieving resource names from resources in $resourceGroupName for environment $envName" -ForegroundColor DarkGreen
Write-Host "Using batch account name: $batchAccountName"  -ForegroundColor DarkGreen

$batchScript = Join-Path $repoRoot "scripts\Batch\build_and_upload_batch.ps1"
& $batchScript -path $path -resourceGroupName $resourceGroupName -batchAcctName $batchAccountName -action $action
