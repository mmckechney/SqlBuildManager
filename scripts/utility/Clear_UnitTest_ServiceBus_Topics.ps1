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