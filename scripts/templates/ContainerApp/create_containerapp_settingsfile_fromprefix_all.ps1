param
(
    [string] $prefix
)
$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path

.$scriptDir\create_containerapp_settingsfile_fromprefix.ps1 -prefix $prefix -withContainerRegistry $true -withKeyVault $true
.$scriptDir\create_containerapp_settingsfile_fromprefix.ps1 -prefix $prefix -withContainerRegistry $false -withKeyVault $true
.$scriptDir\create_containerapp_settingsfile_fromprefix.ps1 -prefix $prefix -withContainerRegistry $true -withKeyVault $false
.$scriptDir\create_containerapp_settingsfile_fromprefix.ps1 -prefix $prefix -withContainerRegistry $false -withKeyVault $false
