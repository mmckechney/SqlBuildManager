<#
.SYNOPSIS
    [OBSOLETE] Creates all settings files from an azd environment name.
.DESCRIPTION
    OBSOLETE: This script is no longer used. Orchestrates creation of all settings files for
    AKS, Batch, Container App, ACI, and Database from an environment name by invoking each service's
    respective settings file creation script.
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]
    $envName,
    [string]
    $outputPath = "..\..\src\TestConfig",
    [string] $sbmExe = "sbm.exe"

)

#############################################
# Get resource name variables from the environment name
#############################################
. ./prefix_resource_names.ps1 -envName $envName

./kubernetes/xxOBSOLETE_create_aks_settingsfile_fromenv.ps1 -path $outputPath -resourceGroupName $resourceGroupName -envName $envName

./Batch/xxOBSOLETE_create_batch_settingsfiles_fromenv.ps1 -sbmExe $sbmExe -path $outputPath -resourceGroupName $resourceGroupName -envName $envName

./ContainerApp/xxOBSOLETE_create_containerapp_settingsfile_fromenv_all.ps1 -envName $envName

./aci/xxOBSOLETE_create_aci_settingsfile_fromenv.ps1 -sbmExe $sbmExe -path $outputPath -resourceGroupName $resourceGroupName -envName $envName

. ./prefix_resource_names.ps1 -envName $envName
../Database/create_database_override_files.ps1 -path $outputPath -envName $envName