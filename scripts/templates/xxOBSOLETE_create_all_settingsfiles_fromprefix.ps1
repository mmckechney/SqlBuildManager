[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]
    $prefix,
    [string]
    $outputPath = "..\..\src\TestConfig",
    [string] $sbmExe = "sbm.exe"

)

#############################################
# Get set resource name variables from prefix
#############################################
. ./prefix_resource_names.ps1 -prefix $prefix

./kubernetes/create_aks_settingsfile_fromprefix.ps1 -path $outputPath -resourceGroupName $resourceGroupName -prefix $prefix

./Batch/create_batch_settingsfiles_fromprefix.ps1 -sbmExe $sbmExe -path $outputPath -resourceGroupName $resourceGroupName -prefix $prefix

./ContainerApp/create_containerapp_settingsfile_fromprefix_all.ps1   -prefix $prefix 

./aci/create_aci_settingsfile_fromprefix.ps1 -sbmExe $sbmExe -path $outputPath -resourceGroupName $resourceGroupName -prefix $prefix  

. ./prefix_resource_names.ps1 -prefix $prefix
./Database/create_database_override_files.ps1 -path $outputPath -prefix $prefix 