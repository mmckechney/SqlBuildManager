<#
.SYNOPSIS
    [OBSOLETE] Creates an Azure Virtual Network with subnets.
.DESCRIPTION
    OBSOLETE: This script is no longer used. Creates an Azure Virtual Network with subnets for
    AKS, Container Apps, ACI, and Batch by deploying the network.bicep Bicep template with
    configurable subnet names and IP address ranges.
#>
[CmdletBinding()]
param (

    [Parameter()]
    [string]
    $envName,


    [Parameter()]
    [string]
    $resourceGroupName,
  
    [Parameter()]
    [string]
    $nsgName,

    [Parameter()]
    [string]
    $vnet,

    [Parameter()]
    [string]
    $aksSubnet,

    [Parameter()]
    [string]
    $containerAppSubnet,

    [Parameter()]
    [string]
    $aciSubnet,

    [Parameter()]
    [string]
    $batchSubnet,
    
    [Parameter()]
    [string]
    $vnetPrefix,

    [Parameter()]
    [string]
    $aksSubnetPrefix,

    [Parameter()]
    [string]
    $containerAppSubnetPrefix,
    [Parameter()]
    [string]
    $aciSubnetPrefix ,

    [Parameter()]
    [string]
    $batchSubnetPrefix 


)

$repoRoot = Split-Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) -Parent
$resourceGroupNameOverride = $resourceGroupName
. (Join-Path $repoRoot "scripts\prefix_resource_names.ps1") -envName $envName
if (-not [string]::IsNullOrWhiteSpace($resourceGroupNameOverride)) {
    $resourceGroupName = $resourceGroupNameOverride
}

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
Write-Host "Creating VNET and subnets" -ForegroundColor DarkGreen

$params = "{ ""envName"":{""value"":""$envName""},"
if("" -ne $nsgName) { $params += """nsgName"":{""value"":""$nsgName""}," }
if("" -ne $vnet) { $params += """vnetName"":{""value"":""$vnet""},"}
if("" -ne $aksSubnet) { $params += """aksSubnetName"":{""value"":""$aksSubnet""},"}
if("" -ne $containerAppSubnet) { $params += """containerAppSubnetName"":{""value"":""$containerAppSubnet""},"}
if("" -ne $aciSubnet) { $params += """aciSubnetName"":{""value"":""$aciSubnet""},"}
if("" -ne $batchSubnet) { $params += """batchSubnetName"":{""value"":""$batchSubnet""},"}
if("" -ne $vnetPrefix) { $params += """vnetIpRange"":{""value"":""$vnetPrefix""},"}
if("" -ne $aksSubnetPrefix) { $params += """aksSubnetIpRange"":{""value"":""$aksSubnetPrefix""},"}
if("" -ne $containerAppSubnetPrefix) { $params += """containerAppSubnetIpRange"":{""value"":""$containerAppSubnetPrefix""},"}
if("" -ne $aciSubnetPrefix) { $params += """aciSubnetIpRange"":{""value"":""$aciSubnetPrefix""},"}
if("" -ne $batchSubnetPrefix) { $params += """batchSubnetIpRange"":{""value"":""$batchSubnetPrefix"""}
$params = $params.TrimEnd(",")
$params += "}"
$params = $params | ConvertTo-Json
Write-Host $params 


az deployment group create --resource-group $resourceGroupName --template-file $scriptDir/../../infra/modules/network.bicep --parameters $params  -o table 
