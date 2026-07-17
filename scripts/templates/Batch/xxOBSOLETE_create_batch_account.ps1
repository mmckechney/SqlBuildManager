<#
.SYNOPSIS
    [OBSOLETE] Creates an Azure Batch account via Bicep deployment.
.DESCRIPTION
    OBSOLETE: This script is no longer used. Creates an Azure Batch account by deploying the
    batch.bicep Bicep template to a resource group with the specified prefix, storage account,
    and managed identity parameters.
#>
param
(
    [string] $envName,
    [string] $resourceGroupName,
    [string] $batchAccountName,
    [string] $storageAccountName,
    [string] $userAssignedIdentity,
    [string] $location,
    [string] $path = "..\..\..\src\TestConfig"
)
Write-Host "Create Batch Account: $batchAccountName"  -ForegroundColor Cyan
if("" -ne $envName)
{
    . ./../prefix_resource_names.ps1 -envName $envName
    . ./../key_file_names.ps1 -envName $envName -path $path
}

$params = "{ ""envName"":{""value"":""$envName""},"
if("" -ne $batchAccountName) { $params += """batchAccountName"":{""value"":""$batchAccountName""}," }
if("" -ne $storageAccountName) { $params += """storageAccountName"":{""value"":""$storageAccountName""}," }
if("" -ne $userAssignedIdentity) { $params += """identityName"":{""value"":""$userAssignedIdentity""}," }
$params = $params.TrimEnd(",")
$params += "}"
$params = $params | ConvertTo-Json
Write-Host $params 

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
az deployment group create --resource-group $resourceGroupName --template-file "$($scriptDir)/../../infra/modules/batch.bicep" --parameters $params -o table