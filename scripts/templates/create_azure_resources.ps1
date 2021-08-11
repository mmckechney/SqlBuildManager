param(
[Parameter(Mandatory=$True)]
[string]
$resourcePrefix,

 [Parameter(Mandatory=$True)]
 [string]
 $location = "East US",

 [string]
 $outputPath = "..\..\src\TestConfig",

 [int]
 $testDatabaseCount = 10,

[bool]
 $build = $true,

 [bool]
 $deployAks = $true
 
 )

$outputPath = Resolve-Path $outputPath
Write-Host "Will be saving output files to $outputPath"

#################
# Variables Setup
#################
$resourceGroupName = $resourcePrefix + "-rg"

################
# Resource Group
################
Write-Host "Creating Resourcegroup: $ResourceGroupName" -ForegroundColor DarkGreen
$result = az group create --name $resourceGroupName --location $location

##########################################################################################
# Storage Account, Batch Account, Event Hub and Service Bus Topic,  Key Vault and Identity
##########################################################################################
Write-Host "Creating Azure resources: Storage Account, Batch Account, Event Hub, Service Bus Topic,  Key Vault and Identity" -ForegroundColor DarkGreen
$result = az deployment group create --resource-group $resourceGroupName --template-file azuredeploy.json --parameters namePrefix="$resourcePrefix" eventhubSku="Standard" skuCapacity=1 location=$location

####################
# Set Identity privs
####################
./set_managedidentity_rbac_fromprefix.ps1 -prefix $resourcePrefix -resourceGroupName $resourceGroupName


#################
# AKS
#################
if($deployAks)
{
    ./create_aks_cluster.ps1 -prefix $resourcePrefix -resourceGroupName $resourceGroupName
}

#################
# Test Databases?
#################
if($testDatabaseCount -gt 0)
{
    $unFile = Join-Path $outputPath "un.txt"
    $pwFile = Join-Path $outputPath "pw.txt"

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

    Write-Host "Creating Test databases. $testDatabaseCount per server" -ForegroundColor DarkGreen
    $result = az deployment group create --resource-group $resourceGroupName --template-file azuredbdeploy.json --parameters namePrefix="$resourcePrefix" sqladminname="$sqlUserName" sqladminpassword="$sqlPassword" testDbCountPerServer=$testDatabaseCount

    #Create local firewall rule
    ./create_database_firewall_rule.ps1 -resourceGroupName $resourceGroupName

}

#########################
# Build Code?
#########################
if($build)
{
    ./build_and_upload_batch_fromprefix.ps1 -resourceGroupName $resourceGroupName -prefix $resourcePrefix -path $outputPath
}

##########################
# Add Secrets to Key Vault
##########################
./add_secrets_to_keyvault_fromprefix.ps1 -path $outputPath -resourceGroupName $resourceGroupName -prefix $resourcePrefix

if($deployAks)
{
    ##############################
    # Create AKS Key Vault configs
    ##############################
    ./create_aks_keyvault_config_fromprefix.ps1 -path $outputPath -resourceGroupName $resourceGroupName -prefix $resourcePrefix

    ##################################
    # Secrets and Runtime File for AKS
    ##################################
    ./create_aks_secrets_and_runtime_files_fromprefix.ps1 -path $outputPath -resourceGroupName $resourceGroupName -prefix $resourcePrefix
}

#########################
# Settings File for Batch
#########################
./create_batch_settingsfiles_fromprefix.ps1 -path $outputPath -resourceGroupName $resourceGroupName -prefix $resourcePrefix

#######################
# Settings File for ACI
#######################
./create_aci_settingsfile_fromprefix.ps1 -path $outputPath -resourceGroupName $resourceGroupName -prefix $resourcePrefix

#########################
# Database override files
#########################
if($testDatabaseCount -gt 0)
{
    ./create_database_override_files.ps1 -path $outputPath -resourceGroupName $resourceGroupName
}

#############################################
# Copy sample K8s YAML files for test configs
#############################################
Copy-Item kubernetes/sample_job.yaml (Join-Path $outputPath basic_job.yaml)
Copy-Item kubernetes/sample_job_keyvault.yaml (Join-Path $outputPath basic_job_keyvault.yaml)


Write-Host "COMPLETED! - Azure resources have been created." -ForegroundColor DarkGreen