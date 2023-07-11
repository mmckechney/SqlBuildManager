param
(
    [Parameter(Mandatory=$true)]
    [string] $prefix,
    [string] $path = "..\..\..\src\TestConfig",
    [string] $resourceGroupName,
    [string] $sqlUserName,
    [string] $sqlPassword
)

#############################################
# Get set resource name variables from prefix
#############################################
. ./../prefix_resource_names.ps1 -prefix $prefix
. ./../key_file_names.ps1 -prefix $prefix -path $path

Write-Host "Adding secrets to Key Vault from prefix: $prefix "  -ForegroundColor Cyan
$path = Resolve-Path $path
Write-Host "Path set to $path" -ForegroundColor DarkGreen

Write-Host "Retrieving resource names from resources in $resourceGroupName with prefix $prefix" -ForegroundColor DarkGreen

if([string]::IsNullOrWhiteSpace($sqlUserName))
{
    if(Test-Path (Join-Path $path "un.txt"))
    {
        Write-Host "Reading sqlUserName from $path\un.txt" -ForegroundColor DarkGreen
        $sqlUserName = (Get-Content -Path (Join-Path $path "un.txt")).Trim()
    }
}

if([string]::IsNullOrWhiteSpace($sqlPassword))
{
    if(Test-Path (Join-Path $path "pw.txt"))
    {
        Write-Host "Reading sqlPassword from $path\pw.txt" -ForegroundColor DarkGreen
        $sqlPassword = (Get-Content -Path (Join-Path $path "pw.txt")).Trim()
    }
}

$haveSqlInfo = $true
if([string]::IsNullOrWhiteSpace($sqlUserName) -or [string]::IsNullOrWhiteSpace($sqlPassword))
{
    $haveSqlInfo = $false
}

Write-Host "Using key vault name:'$keyVaultName'" -ForegroundColor DarkGreen
Write-Host "Using batch account name:'$batchAccountName'" -ForegroundColor DarkGreen
Write-Host "Using storage account name:'$storageAccountName'" -ForegroundColor DarkGreen
Write-Host "Using Event Hub Namespace name:'$eventHubNamespaceName'" -ForegroundColor DarkGreen
Write-Host "Using Service Bus Namespace name:'$serviceBusNamespaceName'" -ForegroundColor DarkGreen


$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
.$scriptDir/add_secrets_to_keyvault.ps1 -path $path -resourceGroupName  $resourceGroupName -keyVaultName $keyVaultName -batchAccountName $batchAccountName -storageAccountName $storageAccountName -eventHubNamespaceName $eventHubNamespaceName -serviceBusNamespaceName $serviceBusNamespaceName -sqlUserName $sqlUserName -sqlPassword $sqlPassword

 