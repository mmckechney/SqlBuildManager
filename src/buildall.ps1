dotnet tool update --global dotnet-outdated-tool

dotnet outdated -r --upgrade -ifs

#get all the child documents in the current directory
$directory = Get-Location
$directories = Get-ChildItem -Directory
foreach ($directory in $directories) {
    Write-Host "Building $directory"
    Push-Location $directory  
    dotnet build $file.FullName 
    Pop-Location
}
