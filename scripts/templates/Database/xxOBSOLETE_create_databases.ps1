<#
.SYNOPSIS
    [OBSOLETE] Creates SQL Server databases and grants managed identity permissions.
.DESCRIPTION
    OBSOLETE: This script is no longer used. Deploys SQL Server databases, elastic pools, and
    firewall settings via Bicep templates (network.bicep and database.bicep), then grants managed
    identity permissions to the databases. Uses Entra ID (Azure AD) only authentication.
#>
param
(
    [string] $prefix,
    [string] $resourceGroupName,
    [string] $path = "..\..\..\src\TestConfig",
    [int] $testDatabaseCount = 10

)

#############################################
# Get set resource name variables from prefix
#############################################
. ./../prefix_resource_names.ps1 -prefix $prefix
. ./../key_file_names.ps1 -prefix $prefix -path $path

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path

$ipAddress = (Invoke-WebRequest ifconfig.me/ip).Content.Trim()
Write-Host "Using IP Address: $ipAddress" -ForegroundColor Green

Write-Host "Validating Network Setup" -ForegroundColor DarkGreen
az deployment group create --resource-group $resourceGroupName --template-file "$scriptDir/../../infra/modules/network.bicep" --parameters namePrefix="$prefix"  -o table

$subnetNames += (az network vnet subnet list --vnet-name $vnet --resource-group $resourceGroupName --query [].name -o tsv)
$subnetNames = $subnetNames -join "," 
Write-Host "Using Subnet Ids: $subnetNames" -ForegroundColor Green
Write-Host "Deploying SQL Servers, Elastic Pool, Firewall Settings and Databases" -ForegroundColor DarkGreen
Write-Host "Note: SQL Servers use Entra ID (Azure AD) only authentication" -ForegroundColor Cyan
az deployment group create --resource-group $resourceGroupName --template-file "$scriptDir/../../infra/modules/database.bicep" --parameters namePrefix="$prefix" testDbCountPerServer="$testDatabaseCount"  currentIpAddress=$ipAddress subnetNames=$subnetNames -o table

######################################################
# Grant Managed Identity permissions to SQL databases
######################################################
Write-Host "Granting Managed Identity permissions to SQL databases..." -ForegroundColor Cyan
.$scriptDir/grant_identity_permissions.ps1 -prefix $prefix -resourceGroupName $resourceGroupName -path $path
