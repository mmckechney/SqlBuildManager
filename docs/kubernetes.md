# Leveraging Kubernetes for database builds

- [Why use Kubernetes?](#why-use-kubernetes)
  - [Process Flow](massively_parallel.md#kubernetes-process-flow)
- [Getting Started](#getting-started)
  - [Container Image](#container-image)
  - [Environment Setup](#environment-setup)
  - [Basic Overview](#basic-overview)
- [Example and How To: Two-step](#two-step-deployment)
- [Example and How To: Multi-step](#multi-step-deployment)

----

## Why use Kubernetes?

If you have a fleet of databases to update, it could take a very long time to run your build on a single machine, even if you leverage the [threaded](threaded_build.md) model. Similar to leveraging [Azure Batch](azure_batch.md) or [Azure Container Instance](aci.md), to ensure you can complete your updates in a timely fashion, SQL Build Manager can target Kubernetes to distribute you build across multiple compute nodes and pods - each leveraging their own set of concurrent tasks. You can control the level of concurrency to maximize throughput while not overloading your SQL Servers (see [details on concurrency management](concurrency_options.md))

In this implementation, you could run a Kubernetes cluster just about anywhere, but the database targeting and logging leverage [Azure Service Bus](https://azure.microsoft.com/en-us/services/service-bus/) and [Azure Event Hub](https://azure.microsoft.com/en-us/services/event-hubs) respectively, so it would make sense to run Kubernetes in the [Azure Kubernetes Service (AKS)](https://azure.microsoft.com/en-us/services/kubernetes-service/).  To leverage AKS, you will need an [Azure subscription](https://azure.microsoft.com/) with several Azure resources deployed.

## Getting Started

This document assumes that you have a working knowledge of Kubernetes. If you do not, then I instead recommend that you leverage [Azure Batch](azure_batch.md) which is a bit more straightforward. If you are familiar with and already use Kubernetes for other workloads, then this should make sense!

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

As mentioned above, in addition to a Kubernetes cluster, the Kubernetes deployment leverages [Azure Service Bus](https://azure.microsoft.com/en-us/services/service-bus/) and [Azure Event Hub](https://azure.microsoft.com/en-us/services/event-hubs). You can create your own resources either through the [Azure portal](https://portal.azure.com), [az cli](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) or [Azure PowerShell](https://docs.microsoft.com/en-us/powershell/azure/). The only special configuration is with Azure Service Bus which requires a Topic named `sqlbuildmanager`.

It is recommended that you can create the resources via the included PowerShell [create_azure_resources.ps1](../scripts/templates/create_azure_resources.ps1). This script will create all of the resources you need for both Azure Batch and Kubernetes builds: Azure Batch Account, Kubernetes Cluster, Storage Account, Event Hub, Service Bus, Managed Identity and an option for 2 SQL servers and 20 databases in elastic pools. It will also create a new folder and pre-configured settings files in a folder `./src/TestConfig`. The settings files are needed for running integration tests but also serve as excellent references for you to create your own settings files.

### Basic Overview

**With version 14.5+, Kubernetes deployment has been greatly simplified and consolidated into a single `sbm k8s run` command.** The following stepwise manual YAML file deployment is still available if you need more control than offered by the one step method

## Two step deployment

To allow SQL Build Manager to handle all steps, you can leverage the `sbm k8s run` command which will leverage a settings file and/or command line arguments. It is recommended to use a settings file, to make things easier

### 1. Save Settings

The example below show secrets stored in the settings file. You can also leverage Key Vault to store secrets by providing a `--keyvaultname` value or leverage [Managed Identity](managed_identity.md) with `--clientid`, `--identityname`, `--identityresourcegroup`, `--tenantid`, `--subscriptionid`

``` bash 
sbm k8s savesettings --settingsfile "<settings file name>" --settingsfilekey "<settings file key name>" 
--imagename "<container image name>"--imagetag "<image tag>" --registry "<container registry>"
--storageaccountname "<Storage Account Name>" --storageaccountkey "<Storage Account Key>" 
--eh "<Event hub connection string>" --sb "<Event hub connection string>" 
--username "<Db username>"  --password "<Db password>"  
```

### 2. Execute the build:

Just the one command will orchestrate each of the steps, including making the `kubectl` calls for you. You do need to have `kubectl` in the path and you will need to be authenticated against the target Kubernetes cluster

``` bash 
sbm k8s run --settingsfile "<settings file name>" --settingsfilekey "<settings file key name>" --jobname "<job name>" --override "<override file>" --packagename "<sbm file name>"
```
----

## Multi-step deployment

The standard deployment definition for SQL Build Manger (see [sample_job.yaml](../scripts/templates/kubernetes/sample_job.yaml)) mounts two volumes - one for [secrets](../scripts/templates/kubernetes/sample_secrets.yaml) named `sbm` and one for [configmap runtime configuration](../scripts/templates/kubernetes/sample_runtime_configmap.yaml) named `runtime`. The secrets files contains the Base64 encoded values for your connection strings and passwords while the runtime configuration contains the parameters that will be used to execute the build. Both of these should be deployed to Kubernetes prior to creating your pods. Before you `kubetcl apply` the `runtime.yaml` file, you will need to add the `PackageName` and `JobName` values - this can be done for you with the [`sbm prep` command below](#3-upload-your-sbm-package-file-to-your-storage-account)



Once the pods are deployed, they will start up as `k8s worker` by:

1. Retrieving the secrets from the `sbm` volume
2. Retrieving the configuration settings from the `runtime` volume
3. Connect to the Azure Storage account and download the package file locally
4. Connect to and listen for messages on the Service Bus topic

If there are messages on the Service Bus Topic that match the `JobName` from the runtime config, it will start processing those messages and log its progress to the Event Hub. Once complete, it will wait for more messages matching the `JobName` on the Service Bus Topic until the pod is terminated.


### 1. Remove pre-existing pods

Each Kubernetes job is specific to particular settings (secrets, jobname and package file). To ensure the running pods are configured properly and ready to pull Service Bus Topic messages, you will need to remove any existing pods. **_This is true even if you are running the same build twice since the pods are deactivated after a run._**

``` bash
az login  #establish a connection to your Azure account
kubectl delete job sqlbuildmanager
```

### 2. Save the common settings to the config files

As explained above in the [Basic Overview](#basic-overview) the pods leverage both secrets and runtime configmap values. These commands will create those files for you. For ease of use, these files will also be leveraged in subsequent `sbm k8s` commands so you don't have to keep typing in all of the options again and again.

``` bash
sbm k8s savesettings  -u "<sql username>" -p "<sql password>" --storageaccountname "<storage acct name>" --storageaccountkey "<storage acct key>"  -eh "<event hub connection string>" -sb "<service bus topic connection string>"--concurrency "<int value>" --concurrencytype "<Count|Server|MaxServer>"

sbm k8s createyaml --settingsfile "<setting file name>" --settingsfilekey "<setting file key name>" --path "<dir to save files>" --prefix "prefix" --jobname "<name of build>" -packagename "<sbm file name>" --imagename "<container image name>"--imagetag "<image tag>" --registry "<container registry>"
```    
**Alternatively use the Key Vault PowerShell commands as highlighted above**
### 3. Upload your SBM Package file to your storage account

The Kubernetes pods retrieve the build package from Azure storage, this command will create a storage container with the name of the `--jobname` (it will be lower cased and any invalid characters removed) and upload the SBM file to the new container. If you provide the `--runtimefile` value for the runtime YAML file, it will also update the `PackageName` and `JobName` values of the YAML file for you.

``` bash
# For job using local secrets
sbm k8s prep --settingsfile "<setting file name>" --settingsfilekey "<setting file key name>" --secretsfile "secrets.yaml" --runtimefile "runtime.yaml" --jobname "Build1234" --packagename "db_update.sbm"
```

``` bash
# For job using Key Vault secrets
sbm k8s prep --settingsfile "<setting file name>" --settingsfilekey "<setting file key name>" --keyvaultname "<key vault name>" --runtimefile "runtime.yaml" --jobname "Build1234" --packagename "db_update.sbm"
```

### 4. Queue up the override targets in Service Bus

You can use the saved settings files created by `sbm k8s savesettings` or use the `--concurrencytype`,  `--servicebustopicconnection` and `--jobname` arguments.

**IMPORTANT:** If using arguments, the `jobname` and `concurrencytype` values _MUST_ match the values found in the `runtime.yaml` that was deployed to Kubernetes otherwise the messages will not get processed.

``` bash
# For job using local secrets
sbm k8s enqueue --settingsfile "<setting file name>" --settingsfilekey "<setting file key name>" --secretsfile "<secrets.yaml file>" --runtimefile "<runtime.yaml file>"  --override "<override.cfg file>"
```

``` bash
# For job using Key Vault secrets
sbm k8s enqueue --settingsfile "<setting file name>" --settingsfilekey "<setting file key name>" --keyvaultname "<key vault name>" --runtimefile "<runtime.yaml file>"  --override "<override.cfg file>"
```

### 5. Deploy the pods to Kubernetes Job

Leveraging the `kubetcl` command line interface, run the `apply` commands for the `secrets.yaml` (this will upload the values for the connection to Azure Service Bus, Event Grid, Storage and databases) and `runtime.yaml` (this will upload the values for the build package name, job name and runtime concurrency options).  Next apply the `deployment.yaml` to create the pods

``` bash
# For job using local secrets
kubectl apply -f secrets.yaml
kubectl apply -f runtime.yaml
kubectl apply -f sample_job.yaml
kubectl get pods
```

``` bash
# For job using Key Vault secrets
kubectl apply -f runtime.yaml
kubectl apply -f secretProviderClass.yaml
kubectl apply -f podIdentityAndBinding.yaml
kubectl apply -f sample_job_keyvault.yaml
kubectl get pods
```

You should see the pods start up and go to a `running` state. At this point, they will start processing messages from the Service Bus Topic!

``` bash
NAME                               READY   STATUS    RESTARTS   AGE
sqlbuildmanager-79fd65cf45-4s7tk   1/1     Running   0          10m
sqlbuildmanager-79fd65cf45-5nnnt   1/1     Running   0          10m
sqlbuildmanager-79fd65cf45-6hgbp   1/1     Running   0          10m
sqlbuildmanager-79fd65cf45-7llnz   1/1     Running   0          10m
sqlbuildmanager-79fd65cf45-9h6xd   1/1     Running   0          10m
sqlbuildmanager-79fd65cf45-hhg7g   1/1     Running   0          10m
sqlbuildmanager-79fd65cf45-hwjp4   1/1     Running   0          10m
sqlbuildmanager-79fd65cf45-twf7p   1/1     Running   0          10m
sqlbuildmanager-79fd65cf45-vrfgt   1/1     Running   0          10m
sqlbuildmanager-79fd65cf45-wg2c9   1/1     Running   0          10m
```

### 6. Monitor the progress and look for errors

This command will monitor the number of messages left in the Service Bus Topic and also monitor the Event Hub for error and commit messages.

``` bash
# For job using local secrets
sbm k8s monitor --settingsfile "<setting file name>" --settingsfilekey "<setting file key name>" --secretsfile "<secrets.yaml file>" --runtimefile "<runtime.yaml file>"  --override "<override.cfg file>"
```

``` bash
# For job using Key Vault secrets
sbm k8s monitor --settingsfile "<setting file name>" --settingsfilekey "<setting file key name>"--keyvaultname "<key vault name>" --runtimefile "<runtime.yaml file>"  --override "<override.cfg file>"
```

The `--override` argument is not necessary, it will allow the monitor to track the target database count and stop monitoring when all targets have been processed. 

 All of the run logs will be transferred from the pods to the storage container specified in the `jobname` argument. When monitoring is complete, it will output a Blob container SAS token that you can use in [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/) to easily view the logs.

 **IMPORTANT:** After the `sbm k8s monitor` completes, as part of the clean-up, it will remove the Service Bus Topic associated with the build. This will deactivate the running pods so all subsequent run will need to be reset as [specified above](#1-remove-pre-existing-pods).
