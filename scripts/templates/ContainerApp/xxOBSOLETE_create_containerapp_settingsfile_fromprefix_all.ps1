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
