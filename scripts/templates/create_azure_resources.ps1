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
 [ValidateSet("AKS", "ContainerApp", "Batch", "ACI", "All")]
 $deploy = "All"

)

 if($deploy.Contains("AKS") -or $deploy.Contains("All"))
 {
    $deployAks = $true
    $deployContainerRegistry = $true
 }

if($deploy.Contains("Batch") -or $deploy.Contains("All"))
{
    $deployBatch = $true
    $build = $true
}

if($deploy.Contains("ContainerApp") -or $deploy.Contains("All"))
{
    $deployContainerAppEnv = $true
    $deployContainerRegistry = $true
}

if($deploy.Contains("ACI") -or $deploy.Contains("All"))
{
    $includeAci = $true
    $deployContainerRegistry = $true
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

################
# Resource Group
################
Write-Host "Creating Resourcegroup: $ResourceGroupName" -ForegroundColor Cyan
az group create --name $resourceGroupName --location $location -o table

############################################################################################
# Storage Account, Event Hub and Service Bus Topic, Key Vault, Identity and RBAC Assignments
############################################################################################
$ipAddress = (Invoke-WebRequest ifconfig.me/ip).Content.Trim()
Write-Host "Using IP Address: $ipAddress" -ForegroundColor Green

$userIdGuid = az ad signed-in-user show -o tsv --query id
Write-Host "Using User Id GUID: $userIdGuid" -ForegroundColor Green

Write-Host "Deplpying Azure resources: Virtual Network, Subnets, Storage Account, Event Hub, Service Bus Topic, Key Vault, Identity and RBAC Assignments" -ForegroundColor Cyan

if($false -eq $deployAks) {Write-Host "Skipping AKS deployment" -ForegroundColor DarkBlue} else {Write-Host "Deploying AKS Cluster" -ForegroundColor Cyan}
if($false -eq $deployBatch) {Write-Host "Skipping Batch deployment" -ForegroundColor DarkBlue} else {Write-Host "Deploying Batch Account" -ForegroundColor Cyan}
if($false -eq $deployContainerRegistry) {Write-Host "Skipping Container Registry deployment" -ForegroundColor DarkBlue} else {Write-Host "Deploying Container Registry" -ForegroundColor Cyan}
if($false -eq $deployContainerAppEnv) {Write-Host "Skipping Container App Env deployment" -ForegroundColor DarkBlue} else {Write-Host "Deploying Container App Env" -ForegroundColor Cyan}
if($testDatabaseCount -le 0) {Write-Host "Skipping Test database deployment" -ForegroundColor DarkBlue} else {Write-Host "Deploying $testDatabaseCount Test Databases" -ForegroundColor Cyan}

$deployStatus = az deployment group create --resource-group $resourceGroupName --template-file azuredeploy_main.bicep `
    --parameters `
        namePrefix="$prefix" `
        currentIpAddress=$ipAddress `
        userIdGuid=$userIdGuid `
        sqladminname=$sqlUserName `
        sqladminpassword=$sqlPassword `
        deployBatchAccount=$deployBatch `
        deployContainerRegistry=$deployContainerRegistry `
        deployContainerAppEnv=$deployContainerAppEnv `
        testDbCountPerServer=$testDatabaseCount `
      
if($LASTEXITCODE){
    Write-Host "Deployment failed with status: $($deployStatus)" -ForegroundColor Red
    exit
}else 
{
    Write-Host "Azure resource deployment succeeded" -ForegroundColor Green
}

########################################
# Container Registry and Container Build
########################################
if($deployContainerRegistry)
{
     $scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
    .$scriptDir/ContainerRegistry/build_container_registry_image_fromprefix.ps1 -resourceGroupName $resourceGroupName -prefix $prefix -wait $false -path $outputPath
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

##########################
# Add Secrets to Key Vault
##########################
$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/KeyVault/add_secrets_to_keyvault_fromprefix.ps1 -path $outputPath -resourceGroupName $resourceGroupName -prefix $prefix

#########################
# Database override files
#########################
if($testDatabaseCount -gt 0)
{
    $scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
    .$scriptDir/Database/create_database_override_files.ps1 -prefix $prefix -path $outputPath -resourceGroupName $resourceGroupName
}


if($deployAks)
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
if($deployContainerAppEnv)
{
    # Create test file referencing the 
    $scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
    .$scriptDir/ContainerApp/create_containerapp_settingsfile_fromprefix.ps1 -sbmExe $sbmExe -path $outputPath -resourceGroupName $resourceGroupName -prefix $prefix -withContainerRegistry $deployContainerRegistry -withKeyVault $false
    
    $scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
    .$scriptDir/ContainerApp/create_containerapp_settingsfile_fromprefix.ps1 -sbmExe $sbmExe -path $outputPath -resourceGroupName $resourceGroupName -prefix $prefix -withContainerRegistry $deployContainerRegistry -withKeyVault $true
    
    $scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
    .$scriptDir/ContainerApp/create_containerapp_settingsfile_fromprefix.ps1 -sbmExe $sbmExe -path $outputPath -resourceGroupName $resourceGroupName -prefix $prefix -withContainerRegistry $false

}
else 
{
    Write-Host "Skipping Container App Environment settings files" -ForegroundColor DarkBlue
}

#########################
# Settings File for Batch
#########################
if($deployBatch)
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
if($includeAci)
{
    $scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
    .$scriptDir/aci/create_aci_settingsfile_fromprefix.ps1 -sbmExe $sbmExe -path $outputPath -resourceGroupName $resourceGroupName -prefix $prefix -withContainerRegistry $deployContainerRegistry 

    $scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
    .$scriptDir/aci/create_aci_settingsfile_fromprefix.ps1 -sbmExe $sbmExe -path $outputPath -resourceGroupName $resourceGroupName -prefix $prefix -withContainerRegistry $false

}
else 
{
    Write-Host "Skipping ACI settings files" -ForegroundColor DarkBlue
}



Write-Host "COMPLETED! - Azure resources have been created." -ForegroundColor DarkCyan