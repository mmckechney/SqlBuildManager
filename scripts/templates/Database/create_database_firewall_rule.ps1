param
(
    [string] $resourceGroupName,
    [string] $ipAddress
)
Write-Host "Create Database Firewall rules"  -ForegroundColor Cyan
if([string]::IsNullOrWhiteSpace($ipAddress))
{
    $ipAddress = (Invoke-WebRequest ifconfig.me/ip).Content.Trim()
}
Write-Host "Setting firewall rule for IP Address: $ipAddress"  -ForegroundColor Green

$servers = az sql server list --resource-group $resourceGroupName -o tsv --query "[].name"
$auto = -Join ((65..90) + (97..122) | Get-Random -Count 10 | % {[char]$_})

foreach ($server in $servers)
{
    $exists = az sql server firewall-rule list --resource-group $resourceGroupName --server $server -o tsv --query "[?contains(@.startIpAddress '$ipAddress')].startIpAddress"
    if($null -eq $exists)
    {
        Write-Host "Creating firewall rule on $server for $ipAddress" -ForegroundColor DarkGreen
        az sql server firewall-rule create --resource-group $resourceGroupName --server $server --name $auto --start-ip-address $ipAddress --end-ip-address $ipAddress -o table
    }
    else 
    {
        Write-Host "Firewall rule for $ipAddress on $server already exists!" -ForegroundColor DarkGreen
    }
}