# Setting Up an Azure Environment

- [Getting Started - Azure Developer CLI (Recommended)](#getting-started---azure-developer-cli-recommended)
- [Alternative - PowerShell Scripts](#alternative---powershell-scripts)
- [Notes on Unit Testing](#notes-on-unit-testing)
- [SQL Express for local testing](#sql-express)
- [Visual Studio Installer Project](#Visual-studio-installer-project)

----

## Getting Started - Azure Developer CLI (Recommended)

The simplest way to provision all Azure resources is using the [Azure Developer CLI (azd)](https://learn.microsoft.com/azure/developer/azure-developer-cli/). This approach uses Bicep templates in the `/infra` folder for Infrastructure as Code deployment.

### Prerequisites

- [Azure Developer CLI (azd)](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- PowerShell 7+

### Steps

1. Open a terminal at the repository root (where `azure.yaml` is located)
2. Login to Azure:
   ```bash
   azd auth login
   az login
   ```
3. Initialize the environment (first time only):
   ```bash
   azd init
   ```
   You will be prompted for:
   - **Environment name** - This becomes the resource prefix (3-10 characters, lowercase)
   - **Azure subscription** - Select your target subscription
   - **Azure location** - Select the region for deployment

4. Provision the resources:
   ```bash
   azd up
   ```

### What happens during `azd up`

#### Pre-provision Hook (`infra/scripts/preprovision.ps1`)
- Captures your current IP address (for SQL firewall rules)
- Retrieves your Azure AD user info (for RBAC and SQL admin)
- Sets default environment variables for post-provision steps

#### Infrastructure Deployment (`infra/main.bicep`)
Creates the following Azure resources with the environment name as prefix:
- **Resource Group** (`{prefix}-rg`)
- **Virtual Network** with subnets for AKS, Container Apps, ACI, Batch, and Private Endpoints
- **Managed Identity** (`{prefix}identity`) - Used for all service-to-service authentication
- **Storage Account** (`{prefix}storage`) - For runtime logs and Kubernetes package staging
- **Service Bus Namespace** (`{prefix}servicebus`) - Topic-based message queue for database targets
- **Event Hub** (`{prefix}eventhubnamespace` / `{prefix}eventhub`) - Progress event tracking
- **Log Analytics Workspace** (`{prefix}loganalytics`)
- **Container Registry** (`{prefix}containerregistry`) - Private registry for SQL Build Manager images
- **Batch Account** (`{prefix}batchacct`) - Pre-configured with Linux/Windows application slots
- **AKS Cluster** (`{prefix}aks`) - Kubernetes cluster with workload identity federation
- **Container App Environment** (`{prefix}containerappenv`)
- **Azure SQL Servers** (`{prefix}sql-a` and `{prefix}sql-b`) - Each with test databases

#### Post-provision Hook (`infra/scripts/postprovision.ps1`)
- Grants managed identity SQL permissions on all test databases
- Creates Kubernetes namespace and service account (if AKS deployed)
- Generates MI-only settings files for integration testing
- Creates database override configuration files
- Builds and uploads Batch application packages (if `BUILD_BATCH_PACKAGES=true`)
- Builds and pushes container images to ACR (if `BUILD_CONTAINER_IMAGES=true`)

### Configuration Parameters

You can customize the deployment by setting environment variables before running `azd up`:

```bash
# Deployment toggles
azd env set DEPLOY_BATCH_ACCOUNT true       # Deploy Azure Batch (default: true)
azd env set DEPLOY_CONTAINER_REGISTRY true  # Deploy ACR (default: true)
azd env set DEPLOY_CONTAINERAPP_ENV true    # Deploy Container Apps (default: true)
azd env set DEPLOY_AKS true                 # Deploy AKS (default: true)

# Database settings
azd env set TEST_DB_COUNT_PER_SERVER 10     # Test databases per server (default: 10)

# Post-provision options
azd env set BUILD_BATCH_PACKAGES true       # Build and upload Batch packages
azd env set BUILD_CONTAINER_IMAGES true     # Build and push Docker images
azd env set GENERATE_MI_SETTINGS true       # Generate settings files (default: true)

# Security options
azd env set USE_PRIVATE_ENDPOINT false      # Use private endpoints (default: false)
```

### Output Files

After successful deployment, the following files are generated in `src/TestConfig`:
- `settingsfile-batch-*.json` - Batch settings with MI authentication
- `settingsfile-aci-*.json` - ACI settings 
- `settingsfile-containerapp-*.json` - Container App settings
- `settingsfile-k8s-*.json` - Kubernetes settings
- `settingsfilekey.txt` - Encryption key for settings files
- `databasetargets.cfg` - Database listing for SBM file integration tests
- `clientdbtargets.cfg` - Database listing for DACPAC integration tests

---
## Notes on Unit Testing

**NOTE: There are currently some concurrency issues with the unit tests. You may get some failures in a full run that will then succeed after running aain, selecting only the failed tests** 

There are three types of Tests included in the solution:

1. True unit tests with no external dependency - found in the  `~UnitTest.csproj` projects
2. Those that are dependent on a local SQLEXPRESS database - found in the `~.Dependent.UnitTest.csproj` projects. If you want to be able to run the database dependent tests, you will need to install SQL Express as per the next section. \
**IMPORTANT**: If running the SQLEXPRESS dependent tests for the first time on your local machine, you need to run the tests in the `SqlSync.SqlBuild.Dependent.UnitTest.csproj` _first_. This project has the scripts to create the necessary SQLEXPRESS databases.
3. Integration tests that leverage Azure resources for Batch and Kubernetes. These are found in the `SqlBuildManager.Console.ExternalTest.csproj` project. To run these tests, first run `azd up` from the repo root (see [Setting Up an Azure Environment](setup_azure_environment.md)) with the default test database count of 10. This will create the necessary resources and test config files (in `/src/TestConfig` folder) needed to run the tests.

## SQL Express

In order to get some of the unit tests to succeed, you need to have a local install of SQLExpress. You can find the installer from here [https://www.microsoft.com/en-us/sql-server/sql-server-editions-express] (https://www.microsoft.com/en-us/sql-server/sql-server-editions-express). You should be able to leverage the basic install.

---
