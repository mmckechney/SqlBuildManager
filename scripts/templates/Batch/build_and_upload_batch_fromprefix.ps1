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
. ./../prefix_resource_names.ps1 -prefix $prefix
. ./../key_file_names.ps1 -prefix $prefix -path $path

Write-Host "Build and Upload Batch from prefix: $prefix" -ForegroundColor Cyan
Write-Host "Retrieving resource names from resources in $resourceGroupName with prefix $prefix" -ForegroundColor DarkGreen
Write-Host "Using batch account name: $batchAccountName"  -ForegroundColor DarkGreen

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/build_and_upload_batch.ps1 -path $path -resourceGroupName $resourceGroupName -batchAcctName $batchAccountName -action $action
