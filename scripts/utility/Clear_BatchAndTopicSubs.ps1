param
(
    [string] $prefix,
    [string] $resourceGroupName
)

$batchAccountName = $prefix + "batchacct"
$storageAccountName = $prefix + "storage"

$batchAcctKey  = az batch account keys list --name $batchAccountName --resource-group $resourceGroupName -o tsv --query 'primary'
$batchAcctEndpoint = az batch account show --name $batchAccountName --resource-group $resourceGroupName -o tsv --query "accountEndpoint"
$storageAcctKey = (az storage account keys list --account-name $storageAccountName -o tsv --query '[].value')[0]

$sbNamespaceName = $prefix + "servicebus"


$jobs = az batch job list --account-name $batchAccountName --account-endpoint $batchAcctEndpoint --account-key $batchAcctKey -o tsv --query "[?contains(@.state 'completed')].id"
foreach ($job in $jobs) {
   
    Write-Output "Removing job: $($job)"
    az batch job delete --account-name $batchAccountName --account-endpoint $batchAcctEndpoint --account-key $batchAcctKey  --job-id $job --yes

    $storageContainerNamePart = ($job -replace "SqlBuildManagerJobLinux_", "") -replace "SqlBuildManagerJobWindows_", ""
    $storageContainerName = a az storage container list --auth-mode key --account-key "$storageAcctKey" --account-name $storageAccountName  -o tsv --query "[?contains(@.name '$storageContainerNamePart')].name"
    Write-Output "Removing storage container : $($storageContainerName)"
    az storage container delete --name $storageContainerName --auth-mode key --account-key "$storageAcctKey" --account-name $storageAccountName
    
}

$subs = (az servicebus topic subscription list -g $resourceGroupName --namespace-name $sbNamespaceName --topic-name "sqlbuildmanager" ) | ConvertFrom-Json -AsHashtable
if($null -ne $subs)
{
    foreach($sub in $subs)
    {
        Write-Output "Removing service bus topic subscription : $($sub.name)"
        az servicebus topic subscription delete --ids $sub.id 
    }
}