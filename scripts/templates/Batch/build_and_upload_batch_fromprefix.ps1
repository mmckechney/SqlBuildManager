param
(
    [Parameter(Mandatory=$true)]
    [string] $prefix,
    [string] $resourceGroupName,
    [ValidateSet("BuildOnly", "UploadOnly", "BuildAndUpload")]
    [string] $action = "BuildAndUpload",
    [string] $path = "..\..\..\src\TestConfig"
)
#############################################
# Get set resource name variables from prefix
#############################################
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$prefixScript = Join-Path $scriptDir "..\prefix_resource_names.ps1"
$keyFileScript = Join-Path $scriptDir "..\key_file_names.ps1"

. $prefixScript -prefix $prefix
. $keyFileScript -prefix $prefix -path $path

Write-Host "Build and Upload Batch from prefix: $prefix" -ForegroundColor Cyan
Write-Host "Retrieving resource names from resources in $resourceGroupName with prefix $prefix" -ForegroundColor DarkGreen
Write-Host "Using batch account name: $batchAccountName"  -ForegroundColor DarkGreen

$batchScript = Join-Path $scriptDir "build_and_upload_batch.ps1"
& $batchScript -path $path -resourceGroupName $resourceGroupName -batchAcctName $batchAccountName -action $action
