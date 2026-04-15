<#
.SYNOPSIS
    Deletes bin, obj, and .vs directories from the src folder.
.DESCRIPTION
    Removes all build output (bin/obj) and Visual Studio cache (.vs) directories
    under the src folder to force a clean rebuild.
#>
Get-ChildItem ../../src -Include bin -Recurse -Force | Remove-Item -Recurse -Force
Get-ChildItem ../../src -Include obj -Recurse -Force | Remove-Item -Recurse -Force
Remove-Item ../../src/.vs -Recurse -Force 
