<#
.SYNOPSIS
    Creates or reads settings file key, SQL username, and SQL password files.
.DESCRIPTION
    Ensures that settingsfilekey.txt, un.txt, and pw.txt exist in the TestConfig
    output directory. If they do not exist, generates new values (AES-256 key and
    random password). These files are consumed by settings file generation scripts
    to encrypt test configuration.
.PARAMETER prefix
    Environment name prefix (unused directly, passed through for consistency).
.PARAMETER path
    Output directory for the key/credential files. Defaults to src\TestConfig.
#>
param (
    $prefix,
    $path
)

# Get the repo root if path not provided
if ([string]::IsNullOrWhiteSpace($path)) {
    $repoRoot = $env:AZD_PROJECT_PATH
    if ([string]::IsNullOrWhiteSpace($repoRoot)) {
        $repoRoot = Split-Path (Split-Path $script:MyInvocation.MyCommand.Path -Parent) -Parent
    }
    $path = Join-Path $repoRoot "src\TestConfig"
}

if($false -eq (Test-Path $path))
{
    New-Item -Path $path -ItemType Directory
}
$resolvedPath = Resolve-Path $path

$keyFile = (Join-Path $resolvedPath "settingsfilekey.txt")
if($false -eq (Test-Path $keyFile))
{
    Write-Host "Writing new key file $keyFile" -ForegroundColor DarkGreen
    $AESKey = New-Object Byte[] 32
    [Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($AESKey)
    $settingsFileKey = [System.Convert]::ToBase64String($AESKey);
    $settingsFileKey |  Set-Content -Path $keyFile
}

$unFile = (Join-Path $resolvedPath "un.txt")
$pwFile = (Join-Path $resolvedPath "pw.txt")

if(Test-Path $unFile)
{
    Write-Host "Using existing username file" -ForegroundColor DarkGreen
    Write-Host "Reading content of $unFile" -ForegroundColor DarkGreen
    $sqlUserName = (Get-Content -Path $unFile).Trim()
}

if(Test-Path $pwFile)
{
    Write-Host "Using existing password file" -ForegroundColor DarkGreen
    Write-Host "Reading content of $pwFile" -ForegroundColor DarkGreen
    $sqlPassword = (Get-Content -Path $pwFile).Trim()
}


if([string]::IsNullOrWhiteSpace($sqlUserName))
{
    $sqlUserName = "SqlBuildManagerSqlAdmin"
    Write-Host "Writing new username file" -ForegroundColor DarkGreen
    Write-Host "Saving content of $unFile" -ForegroundColor DarkGreen
    $sqlUserName | Set-Content -Path $unFile

}

if([string]::IsNullOrWhiteSpace($sqlPassword))
{
    Write-Host "Generating new SQL Password" -ForegroundColor DarkGreen
    $AESKey = New-Object Byte[] 32
    [Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($AESKey)
    $sqlPassword = [System.Convert]::ToBase64String($AESKey);
    
    Write-Host "Writing new password file" -ForegroundColor DarkGreen
    Write-Host "Saving content of $pwFile" -ForegroundColor DarkGreen
    $sqlPassword | Set-Content -Path $pwFile
}