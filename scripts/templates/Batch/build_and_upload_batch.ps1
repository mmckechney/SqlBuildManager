param
(
    [string] $path,
    [string] $resourceGroupName,
    [string] $batchAcctName,
    [bool] $uploadonly = $false
)
Write-Host "Build and Upload Batch" -ForegroundColor Cyan

$path = Resolve-Path $path
Write-Host "Code Publish output path set to $path" -ForegroundColor DarkGreen

$frameworkTarget = "net6.0"
Write-Output "Using Batch Account: $batchAcctName" -ForegroundColor DarkGreen

$winenv =@{
    ApplicationName = "SqlBuildManagerWindows"
    PoolName = "SqlBuildManagerPoolWindows"
    OSName = "Windows"
    BuildTarget = "win-x64"
    BuildOutputZip = ""
}

$linuxenv = @{
    ApplicationName = "SqlBuildManagerLinux"
    PoolName = "SqlBuildManagerPoolLinux"
    OSName = "Linux"
    BuildTarget = "linux-x64"
    BuildOutputZip = ""
}
$vars = $winenv, $linuxenv

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
foreach ($env in $vars) {

    Write-Host "Publishing for $($env.OSName)" -ForegroundColor DarkGreen

    if($uploadonly -eq $false)
    {
        dotnet publish  Resolve-Path (Join-Path $scriptDir  "..\..\..\src\SqlBuildManager.Console\sbm.csproj") -r $env.BuildTarget --configuration Debug -f $frameworkTarget
    }
    
    $source= Resolve-Path (Join-Path $scriptDir "..\..\..\src\SqlBuildManager.Console\bin\Debug\$frameworkTarget\$($env.BuildTarget)\publish")
    if($env.OSName -eq "Windows")
    {
        $version = (Get-Item "$($source)\sbm.exe").VersionInfo.ProductVersion  #Get version for Batch application
    }
    $buildOutput= Join-Path $path "sbm-$($env.OSName.ToLower())-$($version).zip"
    if($uploadonly -eq $false)
    {
        Add-Type -AssemblyName "system.io.compression.filesystem"
        If(Test-path $buildOutput) 
        {
            Remove-item $buildOutput
        }
        Write-Host "Creating Zip file for $($env.OSName) Release package" -ForegroundColor DarkGreen
        [io.compression.zipfile]::CreateFromDirectory($source,$buildOutput)
    }

    $env.BuildOutputZip =  $buildOutput
}


##################################################
# Upload zip application packages to batch account
##################################################

foreach ($env in $vars)
{
    Write-Host "Creating new Azure Batch Application named $($env.ApplicationName)"  -ForegroundColor DarkGreen
    az batch application create --name "$batchAcctName" --resource-group "$resourceGroupName" --application-name "$($env.ApplicationName)" -o table
    
    Write-Host "Uploading application package $($env.ApplicationName) [$($env.BuildOutputZip)] to Azure Batch account"  -ForegroundColor DarkGreen
    az batch application package create --name "$batchAcctName" --resource-group "$resourceGroupName" --application-name "$($env.ApplicationName)" --version "$version" --package-file "$($env.BuildOutputZip)"  -o table
    
    Write-Host "Setting default application for  $($env.ApplicationName) version to $version"  -ForegroundColor DarkGreen
    az batch application set --name "$batchAcctName" --resource-group "$resourceGroupName" --application-name "$($env.ApplicationName)" --default-version "$version" -o table
}
