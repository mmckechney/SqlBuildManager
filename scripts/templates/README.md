# Template Scripts reference

**NOTE**: All scripts will require the Azure CLI be installed. See [here](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) for installation instructions.

----

## create_azure_resources.ps1

This is a one-stop script for creating all of the resources you will need to leverage SQL Build Manager. Uses the `azuredeploy_main.bicep` file which will leverage the bicep modules in the `Modules` folder as needed per the parameters passed in. 

It will always create these Azure resources:

- Virtual Network and 4 Subnets
- Storage Account
- Event Hub Namespace and Event Hub
- Service Bus Namespace and Topic
- Key Vault
- User Assigned Managed Identity with RBAC Assignments
- Current User RBAC Assignments

If you specify a `-testDatabaseCount` value of greater than 0 (zero), it will create:

- 2 Azure SQL Servers
- Designated number of databases per server

With the default `-deploy` value of `All`, it will also deploy:
- Azure Batch Account
- AKS Cluster Azure Container Registry
- Azure Container App Environment
- The settings files needed for external tests of Azure Batch, AKS, Azure Container Apps and ACI
- Run an Azure Container Registry Build to create the Docker image for SqlBuildManager
- Build the code (both Windows and Linux targets) and publish the packages to the Azure Batch account

Otherwise you can specify a `-deploy` value of `Batch`, `AKS`, `ContainerApp` or `ACI` (or any combination) to deploy only the resources needed for that environment along with the test settings files and builds.

----

## create_all_settingsfiles_fromprefix.ps1

A helper script that will create the settings files for all of the deployment types. If you run `create_azure_resources.ps1`, it will already create the settings files for you. 

## Subfolder scripts

Each subfolder has scripts and to individually create resources and settings files for those resources. 