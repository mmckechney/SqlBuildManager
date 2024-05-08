param
(
    [string] $path,
    [string] $resourceGroupName,
    [string] $batchAcctName,
    [ValidateSet("BuildOnly", "UploadOnly", "BuildAndUpload")]
    [string] $action = "BuildAndUpload"
)
Write-Host "Build and Upload Batch" -ForegroundColor Cyan
$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path

$path = Resolve-Path $path
Write-Host "Code Publish output path set to $path" -ForegroundColor DarkGreen
$frameworkTarget = Invoke-Expression -Command (Join-Path $scriptDir ..\get_targetframework.ps1)

Write-Host "Target Framework:  $frameworkTarget" -ForegroundColor DarkGreen

Write-Host "Using Batch Account: $batchAcctName" -ForegroundColor DarkGreen

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


foreach ($env in $vars) {

    Write-Host "Publishing for $($env.OSName)" -ForegroundColor DarkGreen

    if($action -ne "UploadOnly")
    {
        dotnet publish (Resolve-Path (Join-Path $scriptDir  "..\..\..\src\SqlBuildManager.Console\sbm.csproj")) -r $env.BuildTarget --configuration Debug -f $frameworkTarget --self-contained
    }
    
    if($false -eq (Test-Path (Join-Path $scriptDir "..\..\..\src\SqlBuildManager.Console\bin\Debug\$frameworkTarget\$($env.BuildTarget)\publish")))
    {
        New-Item  (Join-Path $scriptDir "..\..\..\src\SqlBuildManager.Console\bin\Debug\$frameworkTarget\$($env.BuildTarget)\publish") -ItemType Directory
    }
    $source= Resolve-Path (Join-Path $scriptDir "..\..\..\src\SqlBuildManager.Console\bin\Debug\$frameworkTarget\$($env.BuildTarget)\publish")
    if($env.OSName -eq "Windows")
    {
        $version = (Get-Item "$($source)\sbm.exe").VersionInfo.ProductVersion  #Get version for Batch application
    }

    $buildOutput= Join-Path $path "sbm-$($env.OSName.ToLower())-$($version).zip"
    if($action -ne "UploadOnly")
    {
        Add-Type -AssemblyName "system.io.compression.filesystem"
        If(Test-path $buildOutput) 
        {
            Remove-item $buildOutput
        }
        Write-Host "Creating Zip file for $($env.OSName) Release package to [$buildOutput]" -ForegroundColor DarkGreen
        [io.compression.zipfile]::CreateFromDirectory($source,$buildOutput)
    }

    $env.BuildOutputZip =  $buildOutput
}


##################################################
# Upload zip application packages to batch account
##################################################
if($action -ne "BuildOnly")
{
    foreach ($env in $vars)
    {
        Write-Host "Creating new Azure Batch Application named $($env.ApplicationName)"  -ForegroundColor DarkGreen
        az batch application create --name "$batchAcctName" --resource-group "$resourceGroupName" --application-name "$($env.ApplicationName)" -o table
        
        Write-Host "Uploading application package $($env.ApplicationName) [$($env.BuildOutputZip)] to Azure Batch account"  -ForegroundColor DarkGreen

        ##  Work around -- the Azure CLI batch upload has been giving errors, so uploading with PowerShell 
        az batch application package create --name "$batchAcctName" --resource-group "$resourceGroupName" --application-name "$($env.ApplicationName)" --version "$version" --package-file "$($env.BuildOutputZip)"  -o table
        #New-AzBatchApplicationPackage -AccountName "$batchAcctName" -ResourceGroupName "$resourceGroupName" -ApplicationId "$($env.ApplicationName)" -ApplicationVersion "$version" -Format zip -FilePath "$($env.BuildOutputZip)"
        
        Write-Host "Setting default application for  $($env.ApplicationName) version to $version"  -ForegroundColor DarkGreen
        az batch application set --name "$batchAcctName" --resource-group "$resourceGroupName" --application-name "$($env.ApplicationName)" --default-version "$version" -o table
    }
}

