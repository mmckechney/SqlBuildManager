# Massively Parallel Database Builds

If you have a fleet of databases to update, it could take a very long time to run your build on a single machine - even if you leverage the threaded model. To solve this problem, SQL Build Manager offers two ways to parallelize database builds across many compute nodes: [Azure Batch](azure_batch.md) and [Kubernetes](kubernetes.md).

In both methods, each compute resource is able to manage concurrency to help you maximize throughput while not overloading your SQL Servers. See [Concurrency Options](concurrency_options.md).

----

- [Getting started - Building Azure Resources](#getting-started---building-azure-resources)
- [Batch Process Flow](#batch-process-flow)
- [Kubernetes Process Flow](#kubernetes-process-flow)

----


## Getting started - Building Azure Resources

To get started leveraging Batch or Kubernetes, you first need to create and configure resources in the Azure cloud. To automate as much of this as possible, there are PowerShell scripts in the `/scripts/templates` folder to leverage. See the associated [ReadMe](../scripts/templates/README.md) for full descriptions

**NOTE:** Before using these scripts, you will need to install both the [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) and Kubernetes [Helm](https://helm.sh/docs/intro/install/) installed on your machine.

### Steps

1. In a PowerShell window, navigate to the `scripts/templates` folder
2. Run the `az login` command to connect to your Azure account
3. Run the `create_azure_resources.ps1` file. You will be prompted for parameters:
    - `-resourcePrefix` - the prefix for the Azure resources. Must be 6 or less characters, all lowercase.
    - `-Location` - the Azure region to deploy the resources (you can get a list of available locations by running `az account list-locations -o tsv --query "[].name"`)
    - `-outputPath` - optional, will default to the `/scr/TestConfig` folder, the location that the integration tests will look for configuration files 
    - `-testDatabaseCount` - optional, default is 10. If 0, then the SQL Azure resources will not be created. If you plan on running any integration tests, this number should be greater than 1.

    There are additional optional parameters you can set:

    - `-build` - whether or not to build the `sbm` command line code in the `sbm.csproj` file and upload it to the Azure Batch account
    - `-deployAks` - whether or not to build the AKS cluster and create a `runtime.yaml`file and a `secrets.yaml` file (for use when using locally saved secrets) and `podIdentityAndBinding.yaml` and `secretProviderClass.yaml` files (when secrets are stored in Key Vault). If you don't plan on using Kubernetes, you can set this to `$false`

### What does the script do?

The `create_azure_resources.ps1` script will create the following resources which will be ready to start processing your database builds once it's complete. Each resources is prefixed with, you guessed it, the value you provided in the `-resourcePrefix` argument

- Storage Account (`{prefix}storage`) - this account is used for all of the runtime logs files and a staging location for the Kubernetes build package
- Service Bus Namespace and Topic (`{prefix}servicebus` and `sqlbuildmanager` respectively) - this is the Topic where the database target messages are sent and used by both Batch and Kubernetes
- EventHub Namespace and EventHub (`{prefix}eventhubnamespace` and `{prefix}eventhub` respectively) - used for progress event tracking in Kubernetes and can also be used for Batch
- Key Vault (`{prefix}keyvault`) - used to store the secrets to access the storage account, service bus, event hub and databases at runtime
- Managed Identity (`{prefix}identity`) - the identity used by both Kubernetes and Batch to access the secrets in the Key Vault
- AKS Cluster (`{prefix}aks`) - a managed Kubernetes cluster with 2 worker nodes for running Kubernetes pods database builds. You can increase the worker node count as needed.
- Batch Account (`{prefix}batchacct`) - a Batch account used to process database builds. Pre-configured with two applications `SqlBuildManagerLinux` and `SqlBuildManagerWindows` that have the local build of the console app uploaded to each respective OS target. Also pre-configured to use the Managed Identity
- 2 Azure SQL Servers (`{prefix}sql-a` and `{prefix}sql-b`) each with `-testDatabaseCount` number of databases.These can be used for integration testing from `SqlBuildManager.Console.ExternalTest.csproj`

In addition to creating the resources above it will create the following files in the `outputPath` location folder:

1. `settingsfile-*.json` - batch settings files that contains all of the SQL, Batch, Storage and Service Bus endpoints and connection keys for use in testing. There will also be two files ending with `-keyvault.json` that will not contain any secrets, but will instead contain the Key Vault name. The secrets will also have been saved to the Key Vault.
2. `settingsfilekey.txt` - a text file containing the encryption key for the settings files
3. `secrets.yaml` - secrets file containing the the Base64 encoded keys, connection strings and password used by Kubernetes. This file is not needed if using Key Vault.
4. `runtime.yaml` - runtime files template for Kubernetes builds
5. `secretsProviderClass.yaml` - the Azure Key vault `SecretProviderClass` configuration set up with the Key Vault name and Managed Identity information. Used by Kubernetes when leveraging Key Vault
6. `podIdentityAndBinding.yaml` - the Azure Key vault `AzureIdentity` and `AzureIdentityBinding` configuration set up  with the Key Vault name and Managed Identity information. Used by Kubernetes when leveraging Key Vault
7. `databasetargets.cfg` - a pre-configured database listing file for use in a Batch, Kubernetes or threaded execution targeting the SQL Azure databases just created. This is used by the integration tests

**IMPORTANT:** These files can be used _as is_ for the integration testing but are also great reference examples of how to create your own files for production use

----

## Batch Process Flow

For a step by step how-to, see the general [Batch documentation](azure_batch.md)

Running a build using Azure Batch follows the process below. If you do not leverage Azure Key Vault, then the secrets are instead passed to Azure batch from the command line or settings file in step #3. 
![Batch process flow](images/azure_batch_with_keyvault.png)



0. Start an Azure connection with the [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) via `az login`. This will create an authentication token that the `sbm` tooling will use to connect to Key Vault as well as be used to configure the Batch Pool nodes. 
1. Keys, Connection strings and passwords saved in Azure Key Vault with `sbm batch savesettings -kv`. You can also save the secrets to Key Vault in any other fashion you'd like. The secret names are: `StorageAccountKey`, `StorageAccountName`, `EventHubConnectionString`,`ServiceBusTopicConnectionString`, `UserName` (for SQL Server username), `Password` (for SQL Server password), `BatchAccountKey`. This will only need to be done once as long as your secrets do not change.
2. Database targets are sent to Service Bus Topic with `sbm batch enqueue`
3. Batch execution is started with `sbm batch run`. You can pre-stage the worker nodes with `sbm batch prestage`
4. The Batch nodes, leveraging the Managed Identity assigned to them when they were created, accessed the Key Vault and retrieves the secrets
5. The Batch nodes start processing messages from the Service Bus Topic...
6. And update the databases in parallel
7. Once complete, the logs are saved to Blob Storage
8. Status update is sent back to the originating `sbm` command line and processing is complete

----

## Kubernetes Process Flow

For a step by step how-to, see the general [Kubernetes documentation](kubernetes.md)

Running a build using Kubernetes follows the process below. If you do not leverage Azure Key Vault, then the secrets are instead passed to Kubernetes using the `secrets.yaml` file

![AKS process flow](images/aks_with_keyvault.png)



0. Start an Azure connection with the [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) via `az login`. This will create an authentication token that the `sbm` tooling will use to connect to Key Vault. 
1. Keys, Connection strings and passwords saved in Azure Key Vault with `sbm k8s savesettings -kv`. You can also save the secrets to Key Vault in any other fashion you'd like. The secret names are: `StorageAccountKey`, `StorageAccountName`, `EventHubConnectionString`,`ServiceBusTopicConnectionString`, `UserName` (for SQL Server username), `Password` (for SQL Server password). This will only need to be done once as long as your secrets do not change.
2. The `.sbm` package file is uploaded to Blob Storage via `sbm k8s prep`
3. Database targets are sent to Service Bus Topic with `sbm batch enqueue`
4. The pods are started via `kubectl`
   - `kubectl apply -f runtime.yaml` - this sets the runtime settings for the pods (.sbm package name, job name and concurrency settings)
   - `kubectl apply -f secretProviderClass.yaml` - configuration setting up the managed identity. Use [`create_aks_keyvault_config.ps1`](../scripts/templates/create_aks_keyvault_config.ps1) to create the config for you
   - `kubectl apply -f podIdentityAndBinding.yaml` - configuration to bind the managed identity to the pods. Use [`create_aks_keyvault_config.ps1`](../scripts/templates/create_aks_keyvault_config.ps1) to create the config for you
   - `kubectl apply -f basic_deploy_keyvault.yaml` - deployment to create the pods. You can find an example deployment configuration in [sample_deployment_keyvault.yaml](../scripts/templates/kubernetes/sample_deployment_keyvault.yaml) 
5. The pods, leveraging the Managed Identity assigned to them when they were created, accessed the Key Vault and retrieves the secrets
6. The pods start processing messages from the Service Bus Topic...
7. And update the databases in parallel
8. Once complete, the logs are saved to Blob Storage
9. Status update is sent back to the originating `sbm` command line and processing is complete

If you are not going to use Azure Key Vault, you would replace `kubectl apply -f secretProviderClass.yaml` and `kubectl apply -f podIdentityAndBinding.yaml` with `kubectl apply -f secrets.yaml` and leverage a deployment such as the example from [sample_deployment.yaml](../scripts/templates/kubernetes/sample_deployment.yaml)
