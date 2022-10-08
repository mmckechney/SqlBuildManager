
param (
    $prefix,
    $path = "..\..\src\TestConfig"
)

if($false -eq (Test-Path $path))
{
    New-Item -Path $path -ItemType Directory
}

$keyFile = Join-Path $path "settingsfilekey.txt"
if($false -eq (Test-Path $keyFile))
{
    $AESKey = New-Object Byte[] 32
    [Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($AESKey)
    $settingsFileKey = [System.Convert]::ToBase64String($AESKey);
    $settingsFileKey |  Set-Content -Path $keyFile
}

$unFile = Join-Path $path "un.txt"
$pwFile = Join-Path $path "pw.txt"

if(Test-Path $unFile)
{
    $sqlUserName = (Get-Content -Path $unFile).Trim()
    $sqlPassword = (Get-Content -Path $pwFile).Trim()
}

if([string]::IsNullOrWhiteSpace($sqlUserName))
{
    $sqlUserName = -Join ((65..90) + (97..122) | Get-Random -Count 15 | % {[char]$_})

    $AESKey = New-Object Byte[] 32
    [Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($AESKey)
    $sqlPassword = [System.Convert]::ToBase64String($AESKey);

    $sqlUserName | Set-Content -Path $unFile
    $sqlPassword | Set-Content -Path $pwFile
}