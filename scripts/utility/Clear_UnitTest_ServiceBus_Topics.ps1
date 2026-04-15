<#
.SYNOPSIS
    Removes Service Bus topic subscriptions created by integration tests.
.DESCRIPTION
    Lists all subscriptions on the "sqlbuildmanager" Service Bus topic and deletes
    those matching test naming prefixes (aci-, k8s-, c-, ca-, batch-, bat-).
    Subscriptions that don't match are skipped.
.PARAMETER prefix
    Environment name prefix used to derive the Service Bus namespace name.
.PARAMETER resourceGroupName
    Azure resource group. Defaults to {prefix}-rg.
#>
param
(
    [string] $prefix,
    [string] $resourceGroupName
)

$sbNamespaceName = $prefix + "servicebus"
if("" -eq $resourceGroupName)
{
    $resourceGroupName = "$prefix-rg"
}

Write-Host "Cleaning up old Service Bus Topic subscriptions from $sbNamespaceName" -ForegroundColor Green
$subs = (az servicebus topic subscription list -g $resourceGroupName --namespace-name $sbNamespaceName --topic-name "sqlbuildmanager" ) | ConvertFrom-Json
if($null -ne $subs)
{
    foreach($sub in $subs)
    {
        if($sub.name.StartsWith("aci-") -or $sub.name.StartsWith("k8s-") -or $sub.name.StartsWith("aci-") -or $sub.name.StartsWith("c-") -or $sub.name.StartsWith("ca-") -or $sub.name.StartsWith("batch-") -or $sub.name.StartsWith("bat-"))
        {
            Write-Host "Removing Service Bus Topic subscription : $($sub.name)" -ForegroundColor Green
            az servicebus topic subscription delete --ids $sub.id 
        }
        else {
            Write-Host "Skippping Service Bus Topic subscription : $($sub.name). Didn't meet name criteria" -ForegroundColor Cyan
        }
    }
}

Write-Host "Complete!"