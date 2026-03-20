# SQL Build Manager - Copilot Instructions

## Build and Test Commands

```bash
# Build the console application
dotnet build ./src/SqlBuildManager.Console/sbm.csproj --configuration Release -f net10.0

# Run all unit tests (no external dependencies)
dotnet test ./src/SqlBuildManager.Console.UnitTest/SqlBuildManager.Console.UnitTest.csproj
dotnet test ./src/SqlBuildManager.Enterprise.UnitTest/SqlBuildManager.Enterprise.UnitTest.csproj
dotnet test ./src/SqlBuildManager.ScriptHandling.UnitTest/SqlBuildManager.ScriptHandling.UnitTest.csproj
dotnet test ./src/SqlSync.Connection.UnitTest/SqlSync.Connection.UnitTest.csproj
dotnet test ./src/SqlSync.ObjectScript.UnitTest/SqlSync.ObjectScript.UnitTest.csproj
dotnet test ./src/SqlSync.SqlBuild.UnitTest/SqlSync.SqlBuild.UnitTest.csproj

# Run a single test by name
dotnet test ./src/SqlBuildManager.Console.UnitTest/SqlBuildManager.Console.UnitTest.csproj --filter "FullyQualifiedName~TestMethodName"

# Build for specific runtime
dotnet publish ./src/SqlBuildManager.Console/sbm.csproj -r win-x64 --configuration Release -f net10.0 --self-contained
dotnet publish ./src/SqlBuildManager.Console/sbm.csproj -r linux-x64 --configuration Release -f net10.0 --self-contained
```

### Test Types
- `*.UnitTest.csproj` - True unit tests with no external dependencies
- `*.Dependent.UnitTest.csproj` - Require local SQL Express (SQL Server) or local PostgreSQL; run `SqlSync.SqlBuild.Dependent.UnitTest` first on new machines to create databases
- `*.Dependent.PostgreSQL.UnitTest.csproj` - PostgreSQL-specific dependent tests (require PostgreSQL instance)
- `SqlBuildManager.Console.ExternalTest` - Integration tests for SQL Server requiring Azure resources (run `azd up` first to provision resources)
- `SqlBuildManager.Console.PostgreSQL.ExternalTest` - Integration tests for PostgreSQL requiring Azure resources
- External tests run in ACI containers via `scripts/tests/run_all_external_tests_in_aci.ps1` using `src/Dockerfile.tests`

## Architecture Overview

### Database Platform Support
The application supports two database platforms, selected via `--platform` CLI option:
- **SqlServer** (default)
- **PostgreSQL**

Platform abstraction uses interfaces with per-platform implementations:
- `IDbConnectionFactory` → `SqlServerConnectionFactory` / `PostgresConnectionFactory`
- `ITransactionManager` → `SqlServerTransactionManager` / `PostgresTransactionManager`
- `IScriptSyntaxProvider` → `SqlServerSyntaxProvider` / `PostgresSyntaxProvider`
- `ISqlResourceProvider` → `SqlServerResourceProvider` / `PostgresResourceProvider`

Platform is propagated via `ConnectionData.DatabasePlatform` (set from `cmdLine.AuthenticationArgs.DatabasePlatform`).

### Project Layers
```
SqlBuildManager.Console (sbm.csproj)    ← Entry point, CLI
    ↓
SqlBuildManager.Enterprise              ← Azure services (Batch, ACI, K8s, Container Apps)
SqlBuildManager.ScriptHandling          ← Script parsing/handling
SqlBuildManager.Logging                 ← Serilog-based logging
SqlBuildManager.Interfaces              ← Shared interfaces
    ↓
SqlSync.SqlBuild                        ← Core build/package logic
SqlSync.Connection                      ← Database connections (SqlServer + PostgreSQL)
SqlSync.DbInformation                   ← Database metadata
SqlSync.ObjectScript                    ← SQL object scripting
SqlSync.Constants                       ← Shared constants
```

### Console Application Pattern
The CLI uses **System.CommandLine** (v2.0.3) with .NET Generic Host:

- **Program.cs** - Configures `IHostBuilder` with DI, logging, and `Worker` as `IHostedService`
- **Worker.cs** - Invokes the command parser and routes to handler methods
- **CommandLineBuilder*.cs** - Partial classes split by feature (Aci, Batch, ContainerApp, Kubernetes, etc.)
- **CommandLineArgs*.cs** - Partial classes aggregating feature-specific argument sets

### Execution Models
The `sbm` CLI supports multiple execution targets:
- `sbm build` - Single local execution
- `sbm threaded run` - Parallel execution from local machine
- `sbm batch run` - Azure Batch distributed execution
- `sbm k8s run` - Kubernetes pods execution
- `sbm containerapp run` - Azure Container Apps execution
- `sbm aci run` - Azure Container Instances execution

## Key Conventions

### Partial Classes for Modularity
Large classes are split across multiple files by feature area. When adding new features, follow the existing pattern:
- `CommandLineBuilder.{Feature}.cs` for new command definitions
- `CommandLineArgs.{Feature}.cs` for new argument groups

### Option Definition Pattern
Reusable command-line options are defined as static fields and added to commands:
```csharp
private static Option<string> serverOption = new Option<string>("--server", "SQL Server name");
command.AddRange(new Option[] { serverOption, databaseOption, ... });
```

### Nested Args Classes
`CommandLineArgs` aggregates feature-specific args as public properties:
```csharp
public AuthenticationArgs AuthenticationArgs { get; set; }
public BatchArgs BatchArgs { get; set; }
// etc.
```

### Settings File Pattern
Configuration can be saved to encrypted JSON settings files using `--settingsfile` and `--settingsfilekey`. Sensitive data is AES-256 encrypted or stored in Azure Key Vault.

### Batch Command Line Serialization
`CommandLineArgs` is serialized to a command string via `ToBatchString()` → `Extensions.ToStringExtension(StringType.Batch)`. `FileInfo` properties (`QueryFile`, `OutputFile`) require special handling in batch mode because `FileInfo.FullName` resolves relative paths against the orchestrator's CWD, producing invalid paths on batch nodes. Use `file.Name` (filename only) or `file.ToString()` (original path) instead of `file.FullName` for batch serialization.

## Docker / Container Images

- `src/Dockerfile` - Production runtime image (self-contained .NET 10.0)
- `src/Dockerfile.tests` - Integration test runner image (Azure CLI, kubectl); used by `scripts/tests/run_tests_in_aci.ps1`
- `src/Dockerfile.dependent-tests` - Dependent unit test image with SQL Server/PostgreSQL sidecars
- `src/Dockerfile-Windows` - Windows variant of the production image

## Infrastructure Provisioning

Azure resources can be provisioned via the Azure Developer CLI:
1. **Azure Developer CLI**: `azd up` (uses `infra/` Bicep templates, including `infra/modules/postgresql.bicep`)

This creates: Storage Account, Service Bus, Event Hub, Key Vault, Managed Identity, and optionally Batch/AKS/Container Apps/ACR/SQL databases/PostgreSQL databases.

### Database Setup Scripts
- `scripts/Database/create_database_override_files.ps1` - SQL Server override configs
- `scripts/Database/create_pg_database_override_files.ps1` - PostgreSQL override configs
- `scripts/Database/grant_identity_permissions.ps1` - Grant managed identity access to SQL Server databases
- `scripts/Database/grant_pg_identity_permissions.ps1` - Grant managed identity access to PostgreSQL databases
