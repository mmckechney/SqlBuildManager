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
else
{
	$folder = "C:\Resources\Directory"
}

$acl = Get-Acl $folder

$rule1 = New-Object System.Security.AccessControl.FileSystemAccessRule(
  "Administrators", "FullControl", "ContainerInherit, ObjectInherit", 
  "None", "Allow")
$rule2 = New-Object System.Security.AccessControl.FileSystemAccessRule(
  "Everyone", "FullControl", "ContainerInherit, ObjectInherit", 
  "None", "Allow")

$acl.AddAccessRule($rule1)
$acl.AddAccessRule($rule2)

Set-Acl $folder $acl

Write-Output "Done changing ACLs."