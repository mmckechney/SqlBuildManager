# Leveraging Azure Container Instance (ACI) for database builds

- [Why use ACI?](#why-use-aci)
  - [Process Flow](massively_parallel.md#azure-container-instance-process-flow)
- [Getting Started](#getting-started)
  - [Container Image](#container-image)
  - [Environment Setup](#environment-setup)
- [Example and How To](#example-and-how-to)

----

## Why use ACI?

If you have a fleet of databases to update, it could take a very long time to run your build on a single machine, even if you leverage the [threaded](threaded_build.md) model. Similar to leveraging [Azure Batch](azure_batch.md) or [Kubernetes](kubernetes.md), to ensure you can complete your updates in a timely fashion, SQL Build Manager can target ACI to distribute you build across multiple serverless containers - each leveraging their own set of concurrent tasks. You can control the level of concurrency to maximize throughput while not overloading your SQL Servers (see [details on concurrency management](concurrency_options.md))
 To leverage ACI, you will need an [Azure subscription](https://azure.microsoft.com/) with several Azure resources deployed.

## Getting Started

### Container Image

The default container image can be found on Docker Hub at https://hub.docker.com/repository/docker/blueskydevus/sqlbuildmanager/general, or you could build your own from source using the following command from the `/src/` folder

``` bash
docker build -f Dockerfile .. -t sqlbuildmanager:latest
```
If you don't have Docker desktop or would rather off load your container builds, you can leverage Azure Container Registry build tasks with the Azure CLI from the `src` directory. This will build your image and save it to the registry:

``` bash
az acr build --image $nameAndTag --registry $azureContainerRegistryName --file Dockerfile .
```
### Environment Setup

While the ACI execution process will create the ACI for you, it also leverages [Azure Service Bus](https://azure.microsoft.com/en-us/services/service-bus/), [Azure Event Hub](https://azure.microsoft.com/en-us/services/event-hubs), [Azure Key Vault](https://azure.microsoft.com/en-us/services/key-vault) and [Azure Blob Storage](https://azure.microsoft.com/en-us/services/storage/blobs/). You can create your own resources either through the [Azure portal](https://portal.azure.com), [az cli](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) or [Azure PowerShell](https://docs.microsoft.com/en-us/powershell/azure/). The only special configuration is with Azure Service Bus which requires a Topic named `sqlbuildmanager`.

It is recommended that you can create the resources via the included PowerShell [create_azure_resources.ps1](../scripts/templates/create_azure_resources.ps1). This script will create all of the resources you need and an option for 2 SQL servers and 20 databases in elastic pools. It will also create a new folder and pre-configured settings files in a folder `./src/TestConfig`. The settings files are needed for running integration tests but also serve as excellent references for you to create your own settings files.

----

## Example and How To

### 1. Save the common settings to the config files

The recommended way to run an ACI deployment is to first save the settings that you will leverage in a `--settingsfile`. Since ACI always leverages Key Vault, not secrets are stored in the file and as such, no `--settingsfilekey` is required! Instead, the secrets will be saved directly into Key Vault.

``` bash
sbm aci savesettings --aciname "<ACI Name>" --acirg "<ACI resource group>" --identityname "<Managed Identity Name>" --idrg "<Managed identity resource group>" -sb "<service bus topic connection string>"  -kv "<Key Vault Name>" --settingsfile "<settings file name>" --storageaccountname "<storage acct name>" --storageaccountkey "<storage acct key>" -eh "<event hub connection string>" --defaultscripttimeout 500 --subscriptionid "<azure subscription id>" --force 
```

You can automate they collection and saving of secrets with the included PowerShell script:

- [create_aci_settingsfile.ps1](../scripts/templates/aci/create_aci_settingsfile.ps1) - saves secrets to Key Vault and creates the settings JSON file for you.

``` PowerShell
#Collects resource keys, saves them to Key Vault and creates settings file
create_aci_settingsfile.ps1 -path "<path to save the files>" -resourceGroupName "<resource group with the KV and identity>" -keyVaultName "<name of Key Vault>" -aciName "<name of ACI" -storageAccountName "<Name of storage account>" -eventHubNamespaceName "<Name of event hub namespace>" -serviceBusNamespaceName "<Name of service bus namespace>" -identityName "<Managed identity name>" -sqlUserName "<SQL user name" -sqlPassword "<SQL Password>"
```


### 2. Upload your SBM Package file to your storage account and create customized ARM template

The ACI containers retrieve the build package from Azure storage, this command will create a storage container with the name of the `--jobname` (it will be lower cased and any invalid characters removed) and upload the SBM file to the new container. It will also create a customized ARM template which will be used to deploy the ACI containers in the `deploy` step.

``` bash
sbm aci prep --settingsfile "<settings file name>" --tag "<container version tag>" --jobname "<job name>" -P "<sbm package name" --outputfile "<name for ARM template>" --containercount "<number of containers>" --concurrency "<concurrency value" --concurrencytype "<concurrency type>"
```

### 3. Queue up the override targets in Service Bus


**IMPORTANT:** If using arguments, the `jobname` and `concurrencytype` values _MUST_ match the values used in the `prep` steps otherwise the messages will not get processed.

``` bash
sbm aci enqueue --settingsfile "<settings file name>" --jobname "<job name>" --concurrencytype "<concurrency type>" --override "<override file name>"
```

### 4. Deploy Container and Monitor progress

Next is to deploy the ACI to create the containers. The `--templatefile` value is the file you created in the `prep` step as the `--outputfile`. By default, once the deployment is complete, it will start monitoring progress against the Service Bus and Event Hub. You can change this behavior by setting the `--monitor` argument to `false`. This step will extract the `jobname` and `concurrencytype` from the values already saved in the `--templatefile`. The `--override` argument is not required, but it will allow the monitor to track the target database count and stop monitoring when all targets have been processed.

``` bash
sbm aci deploy --settingsfile "<settings file name>" --templatefile "<ARM template file>" --override "<override file name>" --monitor 
```

if you would rather run an extra step (for whatever reason), you can run a separate `monitor` command:

``` bash
sbm aci monitor --settingsfile "<settings file name>" --jobname "<job name>" --concurrencytype "<concurrency type>" --override "<override file name>"
```

 All of the run logs will be transferred from the pods to the storage container specified in the `jobname` argument. When monitoring is complete, it will output a Blob container SAS token that you can use in [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/) to easily view the logs.

 **IMPORTANT:** After the `sbm aci deploy` (with monitoring) or `sbm aci monitor` completes, as part of the clean-up, it will remove the Service Bus Topic associated with the build. This will deactivate the running containers.

