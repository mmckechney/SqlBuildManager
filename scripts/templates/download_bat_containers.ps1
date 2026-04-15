<#
.SYNOPSIS
    Downloads all blobs from Azure Storage containers whose names start with "bat".
.PARAMETER StorageAccount
    The storage account name (default: mwm544storage).
.PARAMETER DestinationPath
    Local folder to download into (default: testresults dated folder).
#>
param(
    [string]$StorageAccount = "mwm544storage",
    [string]$DestinationPath = "C:\Users\mimcke\source\repos\SqlBuildManager\scripts\tests\testresults\2026-04-14-141626"
)

# Ensure destination exists
if (-not (Test-Path $DestinationPath)) {
    New-Item -ItemType Directory -Path $DestinationPath -Force | Out-Null
}

Write-Host "Listing containers starting with 'bat' in storage account '$StorageAccount'..." -ForegroundColor Cyan

$containers = az storage container list --account-name $StorageAccount --auth-mode login --query "[?starts_with(name, 'bat')].name" -o tsv

if (-not $containers) {
    Write-Host "No containers found starting with 'bat'." -ForegroundColor Yellow
    exit 0
}

$containerList = $containers -split "`n" | ForEach-Object { $_.Trim() } | Where-Object { $_ -ne "" }
Write-Host "Found $($containerList.Count) container(s): $($containerList -join ', ')" -ForegroundColor Green

foreach ($container in $containerList) {
    $localDir = Join-Path $DestinationPath $container
    if (-not (Test-Path $localDir)) {
        New-Item -ItemType Directory -Path $localDir -Force | Out-Null
    }

    Write-Host "`nDownloading blobs from container '$container' to '$localDir'..." -ForegroundColor Cyan
    az storage blob download-batch --destination $localDir --source $container --account-name $StorageAccount --auth-mode login

    if ($LASTEXITCODE -eq 0) {
        Write-Host "  Done: $container" -ForegroundColor Green
    } else {
        Write-Host "  Failed: $container" -ForegroundColor Red
    }
}

Write-Host "`nAll downloads complete." -ForegroundColor Green
