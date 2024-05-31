[CmdletBinding()]
param (
    [Parameter()]
    [bool] $update = $true
)

#get all the child documents in the current directory
$current = Get-Location
$srcPath = Join-Path $current.Path  "..\..\src\"
Push-Location $srcPath

if($update) {
    dotnet tool update --global dotnet-outdated-tool
    dotnet outdated -r --upgrade 
}


$directories = Get-ChildItem -Directory | Where-Object { $_.Name -notmatch 'Manual|SetUp|TestConfig|TestResults' }
foreach ($directory in $directories) {
    Write-Host "Building $directory"
    Push-Location $directory  
    dotnet build $file.FullName --verbosity quiet /property:WarningLevel=0
    Pop-Location
}


Pop-Location