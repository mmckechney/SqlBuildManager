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

[bool]
 $build = $true,

 [bool]
 $deployBatch = $true,

 [bool]
 $deployAks = $true,

 [bool]
 $includeAci = $true,

 [bool]
 $deployContainerAppEnv = $true,

 [bool]
 $deployContainerRegistry = $true
 
 )

 if($false -eq (Test-Path  $outputPath))
 {
    New-Item -Path $outputPath -ItemType Directory
 }
$outputPath = Resolve-Path $outputPath
Write-Host "Will be saving output files to $outputPath" -ForegroundColor Green

#################
# Variables Setup
#################
$resourceGroupName = $prefix + "-rg"

################
# Resource Group
################
Write-Host "Creating Resourcegroup: $ResourceGroupName" -ForegroundColor Cyan
az group create --name $resourceGroupName --location $location -o table

##########################################################################################
# Storage Account, Event Hub and Service Bus Topic, Key Vault and Identity
##########################################################################################
Write-Host "Creating Azure resources: Storage Account, Event Hub, Service Bus Topic,  Key Vault and Identity" -ForegroundColor Cyan
az deployment group create --resource-group $resourceGroupName --template-file azuredeploy_base.bicep --parameters namePrefix="$prefix" eventhubSku="Standard" skuCapacity=1 location=$location -o table

####################
# Set Identity privs
####################
 ./ManagedIdentity/set_managedidentity_rbac_fromprefix.ps1 -prefix $prefix -resourceGroupName $resourceGroupName

#################
# Batch
#################
if($deployBatch)
{
    ./Batch/create_batch_account_fromprefix.ps1 -prefix $prefix -resourceGroupName $resourceGroupName
}
else 
{
    Write-Host "Skipping Batch deployment" -ForegroundColor DarkBlue
}

####################
# Container Registry
####################
if($deployContainerRegistry)
{
    ./ContainerRegistry/create_container_registry_fromprefix.ps1 -resourceGroupName $resourceGroupName -prefix $prefix
}
#################
# AKS
#################
if($deployAks)
{
    ./Kubernetes/create_aks_cluster.ps1 -prefix $prefix -resourceGroupName $resourceGroupName -includeContainerRegistry $deployContainerRegistry
}
else 
{
    Write-Host "Skipping AKS deployment" -ForegroundColor DarkBlue
}

#################
# Container App
#################
if($deployContainerAppEnv)
{
    ./ContainerApp/create_containerapp_env_fromprefix.ps1 -prefix $prefix -resourceGroupName $resourceGroupName 
}
else 
{
    Write-Host "Skipping Container App environment deployment" -ForegroundColor DarkBlue
}

#################
# Test Databases?
#################
if($testDatabaseCount -gt 0)
{
   ./Database/create_databases_from_prefix.ps1 -prefix $prefix -resourceGroupName $resourceGroupName -path $path -testDatabaseCount $testDatabaseCount
}
else 
{
    Write-Host "Skipping Test database deployment" -ForegroundColor DarkBlue
}


#########################
# Build Code?
#########################
if($build -and $deployBatch)
{
    ./Batch/build_and_upload_batch_fromprefix.ps1 -resourceGroupName $resourceGroupName -prefix $prefix -path $outputPath
}
else 
{
    Write-Host "Skipping code build" -ForegroundColor DarkBlue
}

##########################
# Add Secrets to Key Vault
##########################
./KeyVault/add_secrets_to_keyvault_fromprefix.ps1 -path $outputPath -resourceGroupName $resourceGroupName -prefix $prefix

if($deployAks)
{
    ##############################
    # Create AKS Key Vault configs
    ##############################
    ./Kubernetes/create_aks_keyvault_config_fromprefix.ps1 -path $outputPath -resourceGroupName $resourceGroupName -prefix $prefix

    ##################################
    # Secrets and Runtime File for AKS
    ##################################
    ./Kubernetes/create_aks_secrets_and_runtime_files_fromprefix.ps1 -path $outputPath -resourceGroupName $resourceGroupName -prefix $prefix

    #############################################
    # Copy sample K8s YAML files for test configs
    #############################################
    Copy-Item kubernetes/sample_job.yaml (Join-Path $outputPath basic_job.yaml)
    Copy-Item kubernetes/sample_job_keyvault.yaml (Join-Path $outputPath basic_job_keyvault.yaml)

}

$sbmExe = (Resolve-Path "..\..\src\SqlBuildManager.Console\bin\Debug\net6.0\sbm.exe").Path
#################################
# Settings File for Container App
#################################
if($deployContainerAppEnv)
{
    ./ContainerApp/create_containerapp_settingsfile_fromprefix.ps1 -sbmExe $sbmExe -path $outputPath -resourceGroupName $resourceGroupName -prefix $prefix -withContainerRegistry $deployContainerRegistry 
}


#########################
# Settings File for Batch
#########################
if($deployBatch)
{
    ./Batch/create_batch_settingsfiles_fromprefix.ps1 -sbmExe $sbmExe -path $outputPath -resourceGroupName $resourceGroupName -prefix $prefix
}
#######################
# Settings File for ACI
#######################
if($includeAci)
{
    ./aci/create_aci_settingsfile_fromprefix.ps1 -sbmExe $sbmExe -path $outputPath -resourceGroupName $resourceGroupName -prefix $prefix -withContainerRegistry $deployContainerRegistry 
}


#########################
# Database override files
#########################
if($testDatabaseCount -gt 0)
{
    ./Database/create_database_override_files.ps1 -sbmExe $sbmExe -path $outputPath -resourceGroupName $resourceGroupName
}

Write-Host "COMPLETED! - Azure resources have been created." -ForegroundColor DarkCyan