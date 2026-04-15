<#
.SYNOPSIS
    [OBSOLETE] Master script to provision all Azure resources and generate settings files.
.DESCRIPTION
    OBSOLETE: This script is no longer used. Provisions all Azure resources including Resource Group,
    VNet, Storage, Event Hub, Service Bus, Key Vault, Managed Identity, SQL databases, AKS cluster,
    Batch account, Container Registry, and Container App Environment via Bicep templates. Then
    generates all settings files for each execution target. Superseded by 'azd up'.
#>
param(
[Parameter(Mandatory=$True)]
[string]
$prefix,

 [Parameter(Mandatory=$True)]
 [string]
 $location = "East US",

 [string]
 $outputPath = "..\..\src\TestConfig",

 [int]
 $testDatabaseCount = 10,


 [Parameter()]
 [string[]]
 [ValidateSet("AKS", "ContainerApp", "Batch", "ACI", "All", "None")]
 $deploy = "All",

 [Parameter()]
 [string[]]
 [ValidateSet("AKS", "ContainerApp", "Batch", "ACI", "All", "None")]
 $settingsFile = ""

)
#############################################
# Set variable defaults to $false
#############################################
$deployAks = $false
$deployBatch = $false
$build = $false
$deployContainerAppEnv = $false
$deployContainerRegistry = $false
$settingsFileAks = $false
$settingsFileBatch = $false
$settingsFileContainerApp = $false
$settingsFileAci = $false
$shouldDeploy = $false

$shouldDeploy = (-Not $deploy.Contains("None"))
if($shouldDeploy)
{
    if($deploy.Contains("AKS") -or $deploy.Contains("All"))
    {
        $deployAks = $true
        $deployContainerRegistry = $true
        if(-Not $settingsFile.Contains("None")) { $settingsFileAks = $true}
    }

    if($deploy.Contains("Batch") -or $deploy.Contains("All"))
    {
        $deployBatch = $true
        $build = $true
        if(-Not $settingsFile.Contains("None")) { $settingsFileBatch = $true}
    }

    if($deploy.Contains("ContainerApp") -or $deploy.Contains("All"))
    {
        $deployContainerAppEnv = $true
        $deployContainerRegistry = $true
        if(-Not $settingsFile.Contains("None")) { $settingsFileContainerApp = $true}
    }

    if($deploy.Contains("ACI") -or $deploy.Contains("All"))
    {
        $deployContainerRegistry = $true
        if(-Not $settingsFile.Contains("None")) { $settingsFileAci = $true}
    }
}

if(-Not $settingsFile.Contains("None")) 
{
    if($settingsFile.Contains("AKS") -or $settingsFile.Contains("All"))
    {
        $settingsFileAks = $true
    }
    if($settingsFile.Contains("Batch") -or $settingsFile.Contains("All"))
    {
        $settingsFileBatch = $true
    }
    if($settingsFile.Contains("ContainerApp") -or $settingsFile.Contains("All"))
    {
        $settingsFileContainerApp = $true
    }
    if($settingsFile.Contains("ACI") -or $settingsFile.Contains("All"))
    {
        $settingsFileAci = $true
    }
}
#############################################
# Get set resource name variables from prefix
#############################################
. ./prefix_resource_names.ps1 -prefix $prefix
. ./key_file_names.ps1 -prefix $prefix -path $outputPath

$targetFramework = .\get_targetframework.ps1


 if($false -eq (Test-Path  $outputPath))
 {
    New-Item -Path $outputPath -ItemType Directory
 }
$outputPath = Resolve-Path $outputPath
Write-Host "Will be saving output files to $outputPath" -ForegroundColor Green

if($shouldDeploy)
{
    ################
    # Resource Group
    ################
    Write-Host "Creating Resourcegroup: $ResourceGroupName" -ForegroundColor Cyan
    az group create --name $resourceGroupName --location $location -o table

    ############################################################################################
    # Storage Account, Event Hub and Service Bus Topic, Key Vault, Identity and RBAC Assignments
    ############################################################################################
    $ipAddress = (Invoke-WebRequest https://api.ipify.org/?format=text).Content.Trim()
    Write-Host "Using IP Address: $ipAddress" -ForegroundColor Green

    $userIdGuid = az ad signed-in-user show -o tsv --query id
    Write-Host "Using User Id GUID: $userIdGuid" -ForegroundColor Green


    Write-Host "Deploying Azure resources: Virtual Network, Subnets, Storage Account, Event Hub, Service Bus Topic, Key Vault, Identity and RBAC Assignments" -ForegroundColor Cyan
    Write-Host "Note: Azure SQL uses Entra ID (Azure AD) only authentication via Managed Identity" -ForegroundColor Cyan

    if($deployAks) {Write-Host "Deploying AKS Cluster" -ForegroundColor Cyan} else {Write-Host "Skipping AKS deployment" -ForegroundColor DarkBlue}
    if($deployBatch) {Write-Host "Deploying Batch Account" -ForegroundColor Cyan} else {Write-Host "Skipping Batch deployment" -ForegroundColor DarkBlue}
    if($deployContainerRegistry) {Write-Host "Deploying Container Registry" -ForegroundColor Cyan} else {Write-Host "Skipping Container Registry deployment" -ForegroundColor DarkBlue}
    if($deployContainerAppEnv) {Write-Host "Deploying Container App Env" -ForegroundColor Cyan} else {Write-Host "Skipping Container App Env deployment" -ForegroundColor DarkBlue}
    if($testDatabaseCount -ge 0) {Write-Host "Deploying $testDatabaseCount Test Databases (Entra ID auth only)"  -ForegroundColor Cyan} else {Write-Host  "Skipping Test database deployment" -ForegroundColor DarkBlue}


    $scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
    $deployStatus = az deployment sub create --location $location --template-file "$scriptDir/../../infra/main.bicep" `
        --parameters `
            namePrefix="$prefix" `
            location="$location" `
            currentIpAddress=$ipAddress `
            userIdGuid=$userIdGuid `
            deployBatchAccount=$deployBatch `
            deployContainerRegistry=$deployContainerRegistry `
            deployContainerAppEnv=$deployContainerAppEnv `
            deployAks=$deployAks `
            testDbCountPerServer=$testDatabaseCount `
        
    if($LASTEXITCODE){
        Write-Host "Deployment failed with status: $($deployStatus)" -ForegroundColor Red
        exit
    }else 
    {
        Write-Host "Azure resource deployment succeeded" -ForegroundColor Green
    }
}
else {
    Write-Host "Skipping Azure resource deployment" -ForegroundColor DarkBlue
}
########################################
# Container Registry and Container Build
########################################
if($deployContainerRegistry)
{
     $scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
    .$scriptDir/ContainerRegistry/build_runtime_image_fromprefix.ps1 -resourceGroupName $resourceGroupName -prefix $prefix -wait $false -path $outputPath
}
else 
{
    Write-Host "Skipping Container Build" -ForegroundColor DarkBlue
}
#################
# AKS
#################
if($deployAks)
{
    $scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
    .$scriptDir/Kubernetes/create_aks_cluster.ps1 -prefix $prefix -resourceGroupName $resourceGroupName -includeContainerRegistry $deployContainerRegistry -path $outputPath
}
else 
{
    Write-Host "Skipping AKS deployment" -ForegroundColor DarkBlue
}


#########################
# Build Code?
#########################
if($build -and $deployBatch)
{
    $scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
    .$scriptDir/Batch/build_and_upload_batch_fromprefix.ps1 -resourceGroupName $resourceGroupName -prefix $prefix -path $outputPath -action BuildAndUpload
}
else 
{
    Write-Host "Skipping code build" -ForegroundColor DarkBlue
}

#############################################################
# Save the settings files and config files for the unit tests
#############################################################
$sbmExe = (Resolve-Path "..\..\src\SqlBuildManager.Console\bin\Debug\$targetFramework\sbm.exe").Path

#########################
# Database override files
#########################
if($testDatabaseCount -gt 0)
{
    $scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
    .$scriptDir/Database/create_database_override_files.ps1 -prefix $prefix -path $outputPath -resourceGroupName $resourceGroupName

    ######################################################
    # Grant Managed Identity permissions to SQL databases
    ######################################################
    Write-Host "Granting Managed Identity permissions to SQL databases..." -ForegroundColor Cyan
    .$scriptDir/Database/grant_identity_permissions.ps1 -prefix $prefix -resourceGroupName $resourceGroupName -path $outputPath
}


if($settingsFileAks)
{
    ##############################
    # Create AKS Settings files
    ##############################
    $scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
    .$scriptDir/kubernetes/create_aks_settingsfile_fromprefix.ps1 -path $outputPath -resourceGroupName $resourceGroupName -prefix $prefix
 }
 else 
{
    Write-Host "Skipping AKS settings files" -ForegroundColor DarkBlue
}

#################################
# Settings File for Container App
#################################
if($settingsFileContainerApp)
{
    # Create test file referencing the 
    $scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
    .$scriptDir/ContainerApp/create_containerapp_settingsfile_fromprefix_all.ps1 -prefix $prefix
   
}
else 
{
    Write-Host "Skipping Container App Environment settings files" -ForegroundColor DarkBlue
}

#########################
# Settings File for Batch
#########################
if($settingsFileBatch)
{
    $scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
    .$scriptDir/Batch/create_batch_settingsfiles_fromprefix.ps1 -sbmExe $sbmExe -path $outputPath -resourceGroupName $resourceGroupName -prefix $prefix
}
else 
{
    Write-Host "Skipping Batch Account settings files" -ForegroundColor DarkBlue
}
#######################
# Settings File for ACI
#######################
if($settingsFileAci)
{
    $scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
    .$scriptDir/aci/create_aci_settingsfile_fromprefix.ps1 -sbmExe $sbmExe -path $outputPath -resourceGroupName $resourceGroupName -prefix $prefix 
}
else 
{
    Write-Host "Skipping ACI settings files" -ForegroundColor DarkBlue
}


###########################################
# Save to settings files to a prefix folder
###########################################
if(-Not $settingsFile.Contains("None")) 
{
    $prefixFolder = Join-Path $outputPath $prefix 

    if($false -eq (Test-Path  $prefixFolder))
    {
        New-Item -Path $prefixFolder -ItemType Directory
    }
    $prefixFolder = Resolve-Path $prefixFolder

    Write-Host "Copying settings and config files to $prefixFolder" -ForegroundColor DarkCyan
    Copy-Item -Path "$outputPath\*.json" -Destination $prefixFolder -Force
    Copy-Item -Path "$outputPath\*.cfg" -Destination $prefixFolder -Force
    Copy-Item -Path "$outputPath\*.txt" -Destination $prefixFolder -Force
    
}
Write-Host "COMPLETED! - Azure resources have been created." -ForegroundColor DarkCyan