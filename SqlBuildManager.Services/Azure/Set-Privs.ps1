Add-PSSnapin Microsoft.WindowsAzure.ServiceRuntime

while (!$?)
{
    echo "Failed, retrying after five seconds..."
    sleep 5

    Add-PSSnapin Microsoft.WindowsAzure.ServiceRuntime
}

echo "Added WA snapin."

$localresource = Get-LocalResource "RunLogFiles"
$folder = $localresource.RootPath

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

echo "Done changing ACLs."