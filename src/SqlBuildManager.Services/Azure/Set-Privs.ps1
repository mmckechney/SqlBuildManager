
[void]([System.Reflection.Assembly]::LoadWithPartialName("Microsoft.WindowsAzure.ServiceRuntime"))
$folder = ([Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment]::GetLocalResource("RunLogFiles")).RootPath

if($folder -eq $null -or $folder.length -eq 0)
{
	$folder = "C:\Resources\Directory"
}

Write-Output "Selected folder $folder"

$acl = Get-Acl $folder

Write-Output "acl:"
Write-Output $acl.Access


$rule1 = New-Object System.Security.AccessControl.FileSystemAccessRule(
  "Administrators", "FullControl", "ContainerInherit, ObjectInherit", "None", "Allow")

$rule2 = New-Object System.Security.AccessControl.FileSystemAccessRule(
  "Everyone", "FullControl", "ContainerInherit, ObjectInherit", "None", "Allow")

$rule3 = New-Object System.Security.AccessControl.FileSystemAccessRule(
  "BUILTIN\Users", "FullControl", "ContainerInherit, ObjectInherit", "None", "Allow")

$acl.AddAccessRule($rule1)
$acl.AddAccessRule($rule2)
$acl.AddAccessRule($rule3)


Set-Acl $folder $acl

$acl = Get-Acl $folder

Write-Output "updated acl"
Write-Output "-----------------------------------------------"
Write-Output $acl.Access

Write-Output "Done changing ACLs."


<#
Write-Output "Starting configuration of privs"
Add-PSSnapin Microsoft.WindowsAzure.ServiceRuntime 
$counter = -1
while (!$? -and $counter -lt 5)
{
	$counter = $counter + 1
    Write-Output "Failed, retrying after five seconds..."
    sleep 5

    Add-PSSnapin Microsoft.WindowsAzure.ServiceRuntime
}

if($counter -lt 5)
{
	Write-Output "Added WA snapin."
	$localresource = Get-LocalResource "RunLogFiles"
	$folder = $localresource.RootPath
}
#>