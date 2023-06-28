param
(
    [string] $prefix,
    [string] $resourceGroupName,
    [string] $batchAccountName,
    [string] $storageAccountName,
    [string] $userAssignedIdentity,
    [string] $location,
    [string] $path = "..\..\..\src\TestConfig"
)
Write-Host "Create Batch Account: $batchAccountName"  -ForegroundColor Cyan
if("" -ne $prefix)
{
    . ./../prefix_resource_names.ps1 -prefix $prefix
    . ./../key_file_names.ps1 -prefix $prefix -path $path
}

$params = "{ ""namePrefix"":{""value"":""$prefix""},"
if("" -ne $batchAccountName) { $params += """batchAccountName"":{""value"":""$batchAccountName""}," }
if("" -ne $storageAccountName) { $params += """storageAccountName"":{""value"":""$storageAccountName""}," }
if("" -ne $userAssignedIdentity) { $params += """identityName"":{""value"":""$userAssignedIdentity""}," }
$params = $params.TrimEnd(",")
$params += "}"
$params = $params | ConvertTo-Json
Write-Host $params 

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
az deployment group create --resource-group $resourceGroupName --template-file "$($scriptDir)/../Modules/batch.bicep" --parameters $params -o table