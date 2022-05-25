param
(
    [string] $path = "..\..\..\src\TestConfig",
    [string] $resourceGroupName,
    [string] $prefix,
    [ValidateSet("BuildOnly", "UploadOnly", "BuildAndUpload")]
    [string] $action = "BuildAndUpload"
)
if("" -eq $resourceGroupName)
{
    $resourceGroupName = "$prefix-rg"
}
Write-Host "Build and Upload Batch from prefix: $prefix" -ForegroundColor Cyan
Write-Host "Retrieving resource names from resources in $resourceGroupName with prefix $prefix" -ForegroundColor DarkGreen

$batchAccountName = az batch account list --resource-group $resourceGroupName -o tsv --query "[?contains(@.name '$prefix')].name"
Write-Host "Using batch account name: $batchAccountName"  -ForegroundColor DarkGreen

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/build_and_upload_batch.ps1 -path $path -resourceGroupName $resourceGroupName -batchAcctName $batchAccountName -action $action
