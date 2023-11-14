
param (
    $prefix,
    $path = "..\..\src\TestConfig"
)

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