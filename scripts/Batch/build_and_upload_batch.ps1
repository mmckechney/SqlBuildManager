<#
.SYNOPSIS
    Publishes the sbm console app for Windows and Linux, then uploads as Azure Batch application packages.
.DESCRIPTION
    Builds self-contained publish artifacts for win-x64 and linux-x64 targets, creates zip
    packages from the publish output, and uploads them to an Azure Batch account as versioned
    application packages (SqlBuildManagerWindows and SqlBuildManagerLinux). The version is
    read from the built sbm.exe assembly.
.PARAMETER path
    Directory for zip output artifacts.
.PARAMETER resourceGroupName
    Azure resource group containing the Batch account.
.PARAMETER batchAcctName
    Name of the Azure Batch account.
.PARAMETER action
    BuildOnly, UploadOnly, or BuildAndUpload (default).
#>
param
(
    [string] $path,
    [string] $resourceGroupName,
    [string] $batchAcctName,
    [ValidateSet("BuildOnly", "UploadOnly", "BuildAndUpload")]
    [string] $action = "BuildAndUpload"
)
Write-Host "Build and Upload Batch" -ForegroundColor Cyan

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Get the repo root
$repoRoot = $env:AZD_PROJECT_PATH
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = Split-Path (Split-Path (Split-Path $script:MyInvocation.MyCommand.Path -Parent) -Parent) -Parent
}

$path = Resolve-Path $path
Write-Host "Code Publish output path set to $path" -ForegroundColor DarkGreen
$frameworkTarget = Invoke-Expression -Command (Join-Path $repoRoot "scripts\get_targetframework.ps1")

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
        dotnet publish (Join-Path $repoRoot "src\SqlBuildManager.Console\sbm.csproj") -r $env.BuildTarget --configuration Release -f $frameworkTarget --self-contained
        if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed for $($env.OSName) (exit code $LASTEXITCODE)" }
    }
    
    if($false -eq (Test-Path (Join-Path $repoRoot "src\SqlBuildManager.Console\bin\Release\$frameworkTarget\$($env.BuildTarget)\publish")))
    {
        throw "Expected Release publish output not found for $($env.OSName). Ensure dotnet publish ran with --configuration Release."
    }
    $source= Resolve-Path (Join-Path $repoRoot "src\SqlBuildManager.Console\bin\Release\$frameworkTarget\$($env.BuildTarget)\publish")
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
        if ($LASTEXITCODE -ne 0) { throw "az batch application create failed (exit code $LASTEXITCODE)" }
        
        Write-Host "Uploading application package $($env.ApplicationName) [$($env.BuildOutputZip)] to Azure Batch account"  -ForegroundColor DarkGreen

        ##  Work around -- the Azure CLI batch upload has been giving errors, so uploading with PowerShell 
        az batch application package create --name "$batchAcctName" --resource-group "$resourceGroupName" --application-name "$($env.ApplicationName)" --version "$version" --package-file "$($env.BuildOutputZip)"  -o table
        if ($LASTEXITCODE -ne 0) { throw "az batch application package create failed (exit code $LASTEXITCODE)" }
        
        Write-Host "Setting default application for  $($env.ApplicationName) version to $version"  -ForegroundColor DarkGreen
        az batch application set --name "$batchAcctName" --resource-group "$resourceGroupName" --application-name "$($env.ApplicationName)" --default-version "$version" -o table
        if ($LASTEXITCODE -ne 0) { throw "az batch application set failed (exit code $LASTEXITCODE)" }
    }
}

