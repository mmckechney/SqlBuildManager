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
   - **Environment name** - This becomes the environment component in each resource name (3-10 characters, lowercase)
   - **Azure subscription** - Select your target subscription
   - **Azure location** - Select the region for deployment

4. Provision the resources:
   ```bash
   azd up
   ```

### What happens during `azd up`

#### Pre-provision Hook (`infra/scripts/preprovision.ps1`)
- Captures your current IP address for deployment metadata
- Retrieves your Azure AD user info (for RBAC and SQL admin)
- Sets default environment variables for post-provision steps

#### Infrastructure Deployment (`infra/main.bicep`)
Creates the following Azure resources using the prefixes defined in `infra/resourcetypes.json`:
- **Resource Group** (`rg-{env}`)
- **Virtual Network** with subnets for AKS, Container Apps, ACI, Batch, and Private Endpoints
- **Worker identity** (`id-{env}-worker`) - Database migrations, image pulls and scoped data-plane access; no Azure compute-management or AKS administrator permissions
- **Orchestrator identity** (`id-{env}-orchestrator`) - Azure test-runner compute lifecycle, messaging setup, monitoring and cleanup
- **Infrastructure identities** - Separate bootstrap, Relay, AKS control-plane/kubelet and MySQL directory-lookup responsibilities (see [identity boundaries](azd-up-deployment.md#identity-summary))
- **Storage Account** (`st{env}`; hyphens removed) - For runtime logs and Kubernetes package staging
- **Service Bus Namespace** (`sbns-{env}`) - Topic-based message queue for database targets
- **Event Hub** (`evhns-{env}` / `evh-{env}`) - Progress event tracking
- **Log Analytics Workspace** (`log-{env}`)
- **Container Registry** (`cr{env}`; hyphens removed) - Private registry for SQL Build Manager images
- **Batch Account** (`ba{env}`; hyphens removed) - Pre-configured with Linux/Windows application slots
- **AKS Cluster** (`aks-{env}`) - Kubernetes cluster with workload identity federation
- **Container App Environment** (`cae-{env}`)
- **Azure SQL Servers** (`sql-{env}-a` and `sql-{env}-b`) - Each with test databases
- **PostgreSQL Flexible Servers** (`psql-{env}-a` and `psql-{env}-b`) - Each with test databases
- **MySQL Flexible Servers** (`mysql-{env}-a` and `mysql-{env}-b`) - Each with test databases

#### Post-provision Hook (`infra/scripts/postprovision.ps1`)
- Grants managed identity SQL permissions on all test databases
- Creates Kubernetes namespace and service account (if AKS deployed)
- Generates MI-only settings files for integration testing
- Creates database override configuration files
- Builds and pushes container images to ACR, including the Linux Batch runtime image (if
  `BUILD_CONTAINER_IMAGES=true`)

### Configuration Parameters

You can customize the deployment by setting environment variables before running `azd up`:

```bash
# Deployment toggles
azd env set DEPLOY_BATCH_ACCOUNT true       # Deploy Azure Batch (default: true)
azd env set DEPLOY_CONTAINER_REGISTRY true  # Deploy ACR (default: true)
azd env set DEPLOY_CONTAINERAPP_ENV true    # Deploy Container Apps (default: true)
azd env set DEPLOY_AKS true                 # Deploy AKS (default: true)
azd env set DEPLOY_SQLSERVER true           # Deploy SQL Server databases
azd env set DEPLOY_POSTGRESQL true          # Deploy PostgreSQL databases
azd env set DEPLOY_MYSQL true               # Deploy MySQL databases

# Database settings
azd env set TEST_DB_COUNT_PER_SERVER 10     # Test databases per server (default: 10)
azd env set MYSQL_AUTH_MODE ManagedIdentity # Default for Azure MySQL tests; local tests retain native credentials

# Post-provision options
azd env set BUILD_CONTAINER_IMAGES true     # Build and push Docker images
azd env set GENERATE_MI_SETTINGS true       # Generate settings files (default: true)

# Security options
azd env set USE_PRIVATE_ENDPOINT false      # Use private endpoints (default: false)
```

### Output Files

After successful deployment, the following files are generated in `src\TestConfig\<envName>`,
where `<envName>` is the current azd environment:
- `settingsfile-batch-*.json` - Batch settings with MI authentication
- `settingsfile-aci-*.json` - ACI settings 
- `settingsfile-containerapp-*.json` - Container App settings
- `settingsfile-k8s-*.json` - Kubernetes settings
- `settingsfile-*-mysql-mi-only.json` - MySQL managed-identity settings for Azure tests (see [MySQL prerequisites and migration](mysql.md#managed-identity-azure-mysql))
- `settingsfilekey.txt` - Encryption key for settings files
- `databasetargets.cfg` - Database listing for SBM file integration tests
- `clientdbtargets.cfg` - Database listing for DACPAC integration tests
- `mysql-databasetargets.cfg` - MySQL database listing for SBM integration tests
- `mysql-clientdbtargets.cfg` - MySQL client target mapping

Producer scripts and environment-aware build/test wrappers use `-envName` to select this directory.
An explicit `-path` is an exact directory override: no environment suffix is appended.
Existing flat files in `src\TestConfig` are left untouched and are **not** an Azure fallback.
Regenerate configuration with `azd up` for each environment, or deliberately move only files known
to belong to that environment into its folder, keeping settings and their encryption key together.

The test-image builder stages only the selected environment's top-level JSON, CFG, TXT and YAML
files; other environments, legacy root files, ZIP bundles and test results are excluded.
The compiled tests and container still read `TestConfig\<filename>`: only the selected environment
is flattened into that runtime directory. Use the image-build wrapper, not an unsanitized source
directory as a manual Docker build context:

```powershell
.\scripts\ContainerRegistry\build_external_test_image.ps1 -envName myenv
dotnet test .\src\SqlBuildManager.Console.MySQL.AzureTest\SqlBuildManager.Console.MySQL.AzureTest.csproj -p:AzdEnvironment=myenv
```

The same `AzdEnvironment` property applies to the SQL Server and PostgreSQL Azure test projects.
Alternatively set `$env:AZURE_ENV_NAME = 'myenv'`; an explicit property takes precedence.
Azure test execution requires a selection, including with `--no-build`, which refreshes the selected
runtime configuration. A plain build without a selection builds code without Azure configuration.
Native unit/local-container tests continue using their existing flat/native fixtures and do not
copy Azure environment subfolders. ACI Azure test downloads are stored in the selected environment's
`TestResults` subfolder.

---
## Notes on Unit Testing

**NOTE: There are currently some concurrency issues with the unit tests. You may get some failures in a full run that will then succeed after running aain, selecting only the failed tests** 

There are three types of Tests included in the solution:

1. True unit tests with no external dependency - found in the  `~UnitTest.csproj` projects
2. Those that are dependent on a local SQLEXPRESS database - found in the `~.Dependent.UnitTest.csproj` projects. If you want to be able to run the database dependent tests, you will need to install SQL Express as per the next section. \
**IMPORTANT**: If running the SQLEXPRESS dependent tests for the first time on your local machine, you need to run the tests in the `SqlBuildManager.SqlBuild.Dependent.SqlServer.UnitTest.csproj` _first_. This project has the scripts to create the necessary SQLEXPRESS databases.
3. Azure tests that leverage deployed resources. These are found in `SqlBuildManager.Console.SqlServer.AzureTest.csproj`, `SqlBuildManager.Console.PostgreSQL.AzureTest.csproj`, and `SqlBuildManager.Console.MySQL.AzureTest.csproj`. To run these tests, first run `azd up` from the repo root with the default test database count of 10. This creates the resources and configuration in `src\TestConfig\<envName>`. Select that environment for local test execution as described above, or pass `-envName` to the ACI test wrappers.

## SQL Express

In order to get some of the unit tests to succeed, you need to have a local install of SQLExpress. You can find the installer from here [https://www.microsoft.com/en-us/sql-server/sql-server-editions-express] (https://www.microsoft.com/en-us/sql-server/sql-server-editions-express). You should be able to leverage the basic install.

---
