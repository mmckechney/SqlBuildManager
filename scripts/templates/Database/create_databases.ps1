param
(
    [string] $prefix,
    [string] $resourceGroupName,
    [string] $path,
    [int] $testDatabaseCount

)

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
$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
Write-Host "Creating Test databases. $testDatabaseCount per server" -ForegroundColor DarkGreen
az deployment group create --resource-group $resourceGroupName --template-file "$($scriptDir)/azuredeploy_db.bicep" --parameters namePrefix="$prefix" sqladminname="$sqlUserName" sqladminpassword="$sqlPassword" testDbCountPerServer=$testDatabaseCount  -o table

#Create local firewall rule
.$scriptDir/create_database_firewall_rule.ps1 -resourceGroupName $resourceGroupName