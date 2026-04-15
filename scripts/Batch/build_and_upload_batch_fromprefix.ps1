<#
.SYNOPSIS
    Resolves resource names from a prefix and delegates to build_and_upload_batch.ps1.
.DESCRIPTION
    Wrapper script that loads standard resource names from a deployment prefix,
    then calls build_and_upload_batch.ps1 to publish, package, and upload the
    SQL Build Manager console application as Azure Batch application packages.
.PARAMETER prefix
    Environment name prefix used to derive resource names.
.PARAMETER resourceGroupName
    Azure resource group name. Derived from prefix if not specified.
.PARAMETER action
    BuildOnly, UploadOnly, or BuildAndUpload (default).
.PARAMETER path
    Output directory for build artifacts. Defaults to src\TestConfig.
#>
param
(
    [Parameter(Mandatory=$true)]
    [string] $prefix,
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
# Get set resource name variables from prefix
#############################################
$prefixScript = Join-Path $repoRoot "scripts\prefix_resource_names.ps1"
$keyFileScript = Join-Path $repoRoot "scripts\key_file_names.ps1"

. $prefixScript -prefix $prefix
. $keyFileScript -prefix $prefix -path $path

Write-Host "Build and Upload Batch from prefix: $prefix" -ForegroundColor Cyan
Write-Host "Retrieving resource names from resources in $resourceGroupName with prefix $prefix" -ForegroundColor DarkGreen
Write-Host "Using batch account name: $batchAccountName"  -ForegroundColor DarkGreen

$batchScript = Join-Path $repoRoot "scripts\Batch\build_and_upload_batch.ps1"
& $batchScript -path $path -resourceGroupName $resourceGroupName -batchAcctName $batchAccountName -action $action
