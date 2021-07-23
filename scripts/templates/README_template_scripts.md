# Template Scripts reference

**NOTE**: All scripts will require the Azure CLI be installed. See [here](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) for installation instructions.

----

## create_azure_resources.ps1

This is a one-stop script for creating all of the resources you will need to leverage SQL Build Manager. 

It will always create these Azure resources:

- Storage Account
- Batch Account
- Service Bus Topic
- Event Hub
- AKS Cluster
- Azure Key Vault
- Managed Identity

It will also create:

- 2 Azure SQL Servers
- 10 databases per server

You can change this behavior by providing a `-testDatabaseCount` value of 0 (zero) to skip the creation of the SQL resources or a different positive integer to change the number of databases per server

And finally it will:

- Build the code (both Windows and Linux targets) and publish the packages to the Azure Batch account (skip this step with `-build $false`)
- Create Batch settings files, Kubernetes secrets and runtime files, and database target configuration files (used in integration testing)

----

### azuredeploy.bicep

The Azure Bicep file that is used to generate the `azuredeploy.json` ARM template for the creation of the Storage Account. Batch Account, Service Bus Topic, Event Hub, Key Vault and Managed Identity

----

### azuredbdeploy.bicep

The Azure Bicep file that is used to generate the `azuredbdeploy.json` ARM template for the creation of the Azure SQL Servers, elastic pools and data bases

----

### add_secrets_to_keyvault_fromprefix.ps1

Collects secrets from resources in the specified resource group with the matching $prefix values and saves them to Azure Key Vault. To specify your own resource names, use `add_secrets_to_keyvault.ps1`

----

### build_and_upload_batch_fromprefix.ps1

Builds the `sbm.csproj` for both Windows and Linux and uploads the zip files into the proper Azure Batch application for the Azure Batch account in the resource group with the associated name $prefix. To specify your own resource names, use `build_and_upload_batch.ps1`. You can upload only if you have created zip packages on your own with the `-uploadonly $true` argument


----

### create_aks_cluster.ps1

Deploys an AKS cluster and associates it with the appropriate user assigned Managed Identity

----

### create_aks_keyvault_config_fromprefix.ps1

Creates the `podIdentityAndBinding.yaml` and `secretProviderClass.yaml` configuration files with the details from the Key Vault and Managed Identity found in  the specified resource group with the matching $prefix values.  To specify your own resource names, use `create_aks_keyvault_config.ps1`

----

### create_aks_secrets_and_rutime_files_fromprefix.ps1

Creates the `secrets.yaml` and `runtime.yaml` file used by a container/Kubernetes build  that include the secrets and values for the Azure resources created in the target resource group with the target name prefix. To specify your own resource names, use `ccreate_aks_secrets_and_rutime_files.ps1`

----

### create_batch_settingsfiles_fromprefix.ps1

Creates 4 Azure batch `settings-*.json` files that include the secrets and values for the Azure resources created in the target resource group with the target name prefix. To specify your own resource names, use `create_batch_settingsfiles.ps1`

----
### create_database_firewall_rule.ps1

By default, creates firewall rules for the Azure SQL databases created in the target resource group that match the current machine public IP address. Can also be used to create a rule for a specific IP address by adding the `-ipAddress` parameter

----

### create_database_override_files.ps1

Creates two database target override config files `databasetargets.cfg` and `clientdbtargets.cfg`, pulling that information from the Azure resources created in the target resource group. These files are used by the integration testing project.
