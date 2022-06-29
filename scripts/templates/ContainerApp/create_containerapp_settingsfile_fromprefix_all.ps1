param
(
    [string] $prefix
)

.\create_containerapp_settingsfile_fromprefix.ps1 -prefix $prefix -withContainerRegistry $true -withKeyVault $true
.\create_containerapp_settingsfile_fromprefix.ps1 -prefix $prefix -withContainerRegistry $false -withKeyVault $true
.\create_containerapp_settingsfile_fromprefix.ps1 -prefix $prefix -withContainerRegistry $true -withKeyVault $false
.\create_containerapp_settingsfile_fromprefix.ps1 -prefix $prefix -withContainerRegistry $false -withKeyVault $false
