# Pre-provision hook for Azure Developer CLI (azd)
# Sets up deployment environment with current user info and IP address

Write-Host "Setting up deployment environment..." -ForegroundColor Cyan

# Get current IP address
$currentIpAddress = (Invoke-WebRequest -Uri "https://api.ipify.org" -UseBasicParsing).Content.Trim()
Write-Host "Current IP Address: $currentIpAddress" -ForegroundColor DarkGreen
azd env set CURRENT_IP_ADDRESS $currentIpAddress

# Get current user info
$userIdGuid = az ad signed-in-user show --query id -o tsv
$userLoginName = az account show --query user.name -o tsv
Write-Host "Current User: $userLoginName" -ForegroundColor DarkGreen
Write-Host "User Object ID: $userIdGuid" -ForegroundColor DarkGreen
azd env set AZURE_PRINCIPAL_ID $userIdGuid
azd env set AZURE_PRINCIPAL_NAME $userLoginName
azd env set BUILD_BATCH_PACKAGES "true"
azd env set BUILD_CONTAINER_IMAGES "true"
azd env set GENERATE_MI_SETTINGS "true"

Write-Host "Environment configured successfully!" -ForegroundColor Green
