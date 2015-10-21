<#

Sql Build Manager (in DEV environment):
.\Push-DiagnosticsToCloudService.ps1 -subscription_name "stratustime DEV Environment" -deployment_slot Production -config_path "F:\Git\SqlBuildManager\Diagnostics\diagnostics.wadcfgx" -service_name sqlbuildmanager -storage_name sqlbuildlogs -remove_existing $false
#>
param(

    [string] 
    [ValidateSet('stratustime N2A Environment','stratustime PROD Environment','timesystemtest.com Environment','stratustime DEV Environment','stratustime PERF Hybrid Environment','cloudpayx NonProd Environment','stratustime PERF Environment','cloudpayx Prod Environment','stratustime N1 Environment','stratustime N2B Environment')] 
    $subscription_name = $(throw "-$subscription_name is required."),

    [string] 
    $config_path =  $(throw "-$config_path is required."),

    [string]
    $storage_name =  $(throw "-$storage_name is required."),

    [string]
    $service_name =  $(throw "-$service_name is required."),

    [string]
    [ValidateSet('Production','Staging')]
    $deployment_slot =  $(throw "-$deployment_slot is required."),

    [bool]
    $remove_existing = $false


)

Write-Host "Setting subscription to '$subscription_name'"
Select-AzureSubscription -SubscriptionName $subscription_name #-Location $serviceLocation 

    
$storageAccountKey = (Get-AzureStorageKey -StorageAccountName $storage_name).Primary
$storageContext = New-AzureStorageContext -StorageAccountName $storage_name -StorageAccountKey $storageAccountKey 

Write-Output $storageContext

$role_name = 'SqlBuildManager.Services'

try
{
	$diags = $null
	#See if there are diagnostics already installed for the service
	$diags = Get-AzureServiceDiagnosticsExtension -ServiceName $service_name -Slot $deployment_slot 

	if ($diags -ne $null -and $remove_existing-eq $true)
	{
		Write-Output "Diagnostics found associated with $service_name. Performing 'Remove'"
		Remove-AzureServiceDiagnosticsExtension -ServiceName $service_name -Role $role_name
	}
	elseif($diags -ne $null)
	{
	  Write-Output "Diagnostics found associated with $service_name. Skipping add"
	  continue
	}
	else
	{
		Write-Output "Diagnostics not found for $service_name."
	}
	
	Write-Output "Add diagnostics for $service_name. Performing 'set'"
	Set-AzureServiceDiagnosticsExtension -DiagnosticsConfigurationPath $config_path -ServiceName $service_name -StorageContext $storageContext -Role $role_name -Verbose -Slot $deployment_slot
   
}
catch
{
	$ErrorMessage = $_.Exception.Message

	if($ErrorMessage -match "Deployment not found")
	{
			Write-Output "No deployment found for $service_name. Can not push diagnostics."
	}
}




  



