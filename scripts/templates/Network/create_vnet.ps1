[CmdletBinding()]
param (

    [Parameter()]
    [string]
    $prefix,


    [Parameter()]
    [string]
    $resourceGroupName =$prefix + "-rg",
  
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

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
Write-Host "Creating VNET and subnets" -ForegroundColor DarkGreen

$params = "{ ""namePrefix"":{""value"":""$prefix""},"
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


az deployment group create --resource-group $resourceGroupName --template-file .$scriptDir../Modules/network.bicep --parameters $params  -o table 
