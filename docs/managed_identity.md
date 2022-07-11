# Using User Assigned Managed Identity 

Each of the remote execution options allows for varying use of Azure Managed Identities for authentication to supporting resources. This document shows:
- Which [execution option is capable of leveraging the identity](#managed-identity-support-between-resources-and-execution-type) to connect to which resource
- [How the identity gets associated](#managed-identity-to-compute-assignment) with the execution compute type
- The [configuration and/or arguments needed](#runtime-configuration-to-leverage-managed-identity) to instruct the use of the identity
- The [Azure RBAC roles requred](#azure-rbac-role-assignment-requirements) for the identity


## Managed Identity support between resources and execution type

 |                          | Azure Batch   | Kubernetes (AKS)                                                                                                              | Container Apps                    | Container Instance (ACI)  |
 | ------------------------ | :-----------: | :----------------------------------------------------------------------------------------------------------------------------:| :---------------:                 | :-----------------------: |
 | Azure Key Vault          |   Yes         |   Yes, with [Key Vault Identity Provider](https://docs.microsoft.com/en-us/azure/aks/csi-secrets-store-driver)                |   Yes                             |   Yes                     |
 | SQL databases            |   Yes         |   Yes                                                                                                                         |   No, see [note](#sql-database)   |   Yes                     |
 | Blob Storage             |   No, see [note](#blob-storage)|   Yes                                                                                                        |   Yes                             |   Yes                     |
 | Service Bus              |   Yes         |   Yes                                                                                                                         |   No, see [note](#service-bus)    |   Yes                     |
 | Event Hub                |   Yes         |   Yes                                                                                                                         |   Yes                             |   Yes                     |
 | Azure Container Registry |   N/A         |   Yes, with [`--attach-acr`](https://docs.microsoft.com/en-us/azure/aks/cluster-container-registry-integration?tabs=azure-cli)|   No                              |   No                      | 
 
## Managed Identity to Compute Assignment

Examples of each of these can be generated for you by running [create_azure_resources.ps1](../scripts/templates/create_azure_resources.ps1) which will create samples Azure resources (including a user assigned Managed Identity) as well as sample settings files with various options that are used in the test caes. You can also review the test methods in [SqlBuildManager.Console.ExternalTest](../src/SqlBuildManager.Console.ExternalTest/) to see working examples of various compute options and settings.

### Azure Batch

The identity is assigned at the creation of the Azure Batch account. For an example, see [azuredeploy_batch.bicep](../scripts/templates/Batch/azuredeploy_batch.bicep)

### Kubernetes

The identity first must be associated with the AKS VM Scale set. You can find an example of this assignment near the bottom of [this srcipt](../scripts/templates//kubernetes//create_aks_cluster.ps1)


The identity is assigned to the app at runtime in two steps. First `AzureIdentity` and `AzureIdentityBinding` resources are created in the cluster (see [podIdentityAndBinding_template.yaml](../scripts//templates/kubernetes/podIdentityAndBinding_template.yaml)). Then when the SQL Build Manger job is deployed, an `aadpodidbinding` label is added to the job spec (see [sample_job.yaml](../scripts/templates/kubernetes/sample_job.yaml)). This will tell Kubernetes to assign the identity to the job. 

### Container Apps

The Container Apps workload that is deployed via `sbm containerapp deploy` leverages ARM templates to deploy the workload. This ARM template contains the Managed Identity assignment (see [containerapp_identity_arm_template.json](../src/SqlBuildManager.Console/ContainerApp/containerapp_identity_arm_template.json). The identity information needs to be passed via the Identity options in command line or be saved in to a settings file via `sbm containerapp savesettings`

### Container Instance (ACI)

ACI also uses an ARM template to deploy the workload when running `sbm aci deploy`. This ARM template contains the Managed Identity assignment(see [aci_arm_template.json](../src/SqlBuildManager.Console/Aci/aci_arm_template.json)).  The identity information needs to be passed via the Identity options in command line or be saved in to a settings file via `sbm aci savesettings`

## Runtime Configuration to leverage Managed Identity

### Azure Key Vault

To instruct the app to pull secrets from Azure Key Vault, you need to provide the Key Vault name in the `--keyvault` argument of the appropriate `savesettings`, `prep` and/or `deploy`. Alternatively, you can simply save it to the settings file .json with the `savesetting` command and leverage the settings file for all subsequent commands

### SQL Database

By default, the app uses username/password database authentication. To enable Managed Identity authentication, you will first need to add the [Manged Identity as a user to the database](https://docs.microsoft.com/en-us/azure/azure-sql/database/authentication-azure-ad-user-assigned-managed-identity?view=azuresql#managing-a-managed-identity-for-a-server-or-instance). Once this has been done, you can direct the app to use the identity to authenticate with the `--authtype ManagedIdentity` flag. (As always, this can be saved in a settings file with `savesettings` for easier execution and reuse).

**NOTE:** Azre Container Apps does not currently allow for Managed Identity authentication to Azure SQL Databases


### Blob Storage

To use Managed Identity to access blob storage, simply don't provide a value for `--storageaccountkey`. If this is not provided, the app will default to connecting with the identity.

**NOTE:** Azure Batch requires the storage account key to manage storage and create SAS token URLs. The key must be provided as a settings file value, command line argument or be saved in Azure Key Vault. 

### Service Bus

To use Managed Identity to connect to Azure Service Bus, use the Service Bus namespace as the value (\<name>.servicebus.windows.net) for `--servicebustopicconnection`. 

**NOTE:** Azure Container Apps uses a [KEDA](https://keda.sh/) [service bus scaler](https://keda.sh/docs/2.7/scalers/azure-service-bus/) to manage scaling. This does not currently allow for Managed Identity authentication, so a full Service Bus connection string is still needed.

### Event Hub

To use Managed Identity to connect to Azure Event Hub, use the Event Hub namespace and Event Hub name as a pipe delimited value (<name>.servicebus.windows.net) for `--eventhubconnection`. For example "\<ehnamespace>.servicebus.windows.net|\<eh name>"

### Azure Container Registry

Only Kubernetes is able to natively connect to the container registry without an admin username and password. This is assigned at creation or update of the cluster using the [`--assign-acr`](https://docs.microsoft.com/en-us/azure/aks/cluster-container-registry-integration?tabs=azure-cli) flag

## Azure RBAC Role Assignment Requirements


The Managed Identity assigned to the runtime compute will need the following Azure RBAC roles assigned to the resource group or the specific services. _NOTE:_ You can also use an identity to authenticate for the `prep` and `enqueue` steps. The identity of that user or machine will just also need these roles.
- `Storage Blob Data Contributor` - to read the build package and save log files
- `Key Vault Secrets User` - to pull secrets from the Key Vault
- `Azure Service Bus Data Owner` - to read messages and delete compelted Service Bus Topic Subscription
- `Azure Event Hubs Data Receiver` - to read events from Event Hub
- `Azure Event Hubs Data Sender` - to send events to Event Hub
- `AcrPull` - to pull images from Azure Container Registry

You can see an example of the assignments in [`set_managedidentity_rbac.ps1`](../scripts/templates/ManagedIdentity/set_managedidentity_rbac.ps1)
