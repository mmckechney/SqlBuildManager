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
- `*.Dependent.UnitTest.csproj` - Require local SQL Express; run `SqlSync.SqlBuild.Dependent.UnitTest` first on new machines to create databases
- `SqlBuildManager.Console.ExternalTest` - Integration tests requiring Azure resources (run `scripts/templates/create_azure_resources.ps1` first)

## Architecture Overview

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
SqlSync.Connection                      ← Database connections
SqlSync.DbInformation                   ← Database metadata
SqlSync.ObjectScript                    ← SQL object scripting
SqlSync.Constants                       ← Shared constants
```

### Console Application Pattern
The CLI uses **System.CommandLine** (beta 4) with .NET Generic Host:

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

## Infrastructure Provisioning

Azure resources can be provisioned two ways:
1. **Azure Developer CLI**: `azd up` (uses `infra/` Bicep templates)
2. **PowerShell scripts**: `scripts/templates/create_azure_resources.ps1`

Both create: Storage Account, Service Bus, Event Hub, Key Vault, Managed Identity, and optionally Batch/AKS/Container Apps/ACR/SQL databases.
