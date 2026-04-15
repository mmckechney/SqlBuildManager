<#
.SYNOPSIS
    [OBSOLETE] Creates all Container App settings file variants from a prefix.
.DESCRIPTION
    OBSOLETE: This script is no longer used. Orchestrates creation of all Container App settings
    file variants by calling create_containerapp_settingsfile_fromprefix.ps1 with all combinations
    of container registry (with/without) and Key Vault (with/without) options.
#>
param
(
    [Parameter(Mandatory=$true)]
    [string] $prefix
)
$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir\create_containerapp_settingsfile_fromprefix.ps1 -prefix $prefix -withContainerRegistry $true -withKeyVault $true 

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir\create_containerapp_settingsfile_fromprefix.ps1 -prefix $prefix -withContainerRegistry $false -withKeyVault $true 

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir\create_containerapp_settingsfile_fromprefix.ps1 -prefix $prefix -withContainerRegistry $true -withKeyVault $false 

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir\create_containerapp_settingsfile_fromprefix.ps1 -prefix $prefix -withContainerRegistry $false -withKeyVault $false 
