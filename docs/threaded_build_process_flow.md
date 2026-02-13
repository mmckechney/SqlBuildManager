# Threaded Build Process Flow

This document describes the internal process flow when executing a database build using the `sbm threaded run` command, starting from `ThreadedManager.ExecuteAsync()`.

## Overview

The threaded build execution allows SQL Build Manager to run scripts against multiple databases concurrently. The process involves several key phases:

1. **Initialization & Validation**
2. **Script Source Configuration**
3. **Build Preparation**
4. **Concurrent Execution**
5. **Script Execution per Database**
6. **Finalization**

---

## High-Level Architecture and Build Process Flow

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                              ThreadedManager                                 │
│                             ExecuteAsync()                                   │
└──────────────────────────────────────────────────────────────────────────────┘
                                      │
        ┌─────────────────────────────┼─────────────────────────────┐
        ▼                             ▼                             ▼
┌───────────────────┐    ┌────────────────────────┐    ┌────────────────────┐
│   Validation &    │    │  Script Source Config  │    │  Build Preparation │
│  Path Setup       │    │  (SBM/DACPAC/Scripts)  │    │  & Script Loading  │
└───────────────────┘    └────────────────────────┘    └────────────────────┘
                                      │
        ┌─────────────────────────────┴──────────────────────────────┐
        │                     Execution Mode                         │
        ▼                                                            ▼
┌───────────────────────────┐                    ┌───────────────────────────┐
│  ExecuteFromOverrideFile  │                    │   ExecuteFromQueue        │
│  (Target file sourced)    │                    │   (Service Bus sourced)   │
└───────────────────────────┘                    └───────────────────────────┘
        │                                                            │
        └──────────────────────────┬─────────────────────────────────┘
                                   ▼
                    ┌─────────────────────────────┐
                    │    ProcessConcurrencyBucket │
                    │    (Parallel per bucket)    │
                    └─────────────────────────────┘
                                   │
                                   ▼
                    ┌─────────────────────────────┐
                    │       ThreadedRunner        │
                    │      RunDatabaseBuild()     │
                    └─────────────────────────────┘
                                   │
                                   ▼
                    ┌─────────────────────────────┐
                    │        SqlBuildHelper       │
                    │       ProcessBuild()        │
                    └─────────────────────────────┘
                                   │
                                   ▼
                    ┌─────────────────────────────┐
                    │      SqlBuildOrchestrator   │
                    │          Execute()          │
                    └─────────────────────────────┘
                                   │
                                   ▼
                    ┌─────────────────────────────┐
                    │        SqlBuildRunner       │
                    │           Run()             │
                    └─────────────────────────────┘
                                   │
                                   ▼
                    ┌─────────────────────────────┐
                    │      SqlCommandExecutor     │
                    │         Execute()           │
                    └─────────────────────────────┘
```

---

## Detailed Process Flow

### Phase 1: Initialization & Validation

**Location:** `ThreadedManager.ExecuteAsync()` (lines 58-87)

```
ThreadedManager.ExecuteAsync()
    │
    ├── Initialize ThreadedLogging with RunId
    │
    ├── Set root logging path
    │   ├── Use AZ_BATCH_TASK_WORKING_DIR if in Batch
    │   ├── Use cmdLine.RootLoggingPath if specified
    │   └── Default to ./tmp-sqlbuildlogging
    │
    ├── SetRootAndWorkingPaths()
    │   ├── Create root logging directory
    │   └── Create Working subdirectory
    │
    └── Validation.ValidateCommonCommandLineArgs()
        ├── Validate required parameters
        └── Return error codes if validation fails
```

### Phase 2: Script Source Configuration

**Location:** `ThreadedManager.ConfigureScriptSource()` (lines 288-348)

The build can be sourced from multiple inputs. The method determines which source to use:

```
ConfigureScriptSource()
    │
    ├─► ScriptSrcDir specified?
    │   └── ConstructBuildFileFromScriptDirectory()
    │       ├── Enumerate .sql files in directory
    │       ├── Sort files alphabetically
    │       └── Build SBM package programmatically
    │
    ├─► BuildFileName (.sbm) specified?
    │   └── Use SBM file directly as build source
    │
    ├─► PlatinumDacpac specified?
    │   └── Use DACPAC to generate scripts via delta comparison
    │
    └─► PlatinumDbSource + PlatinumServerSource specified?
        └── DacPacHelper.ExtractDacPac()
            ├── Connect to platinum database
            └── Extract DACPAC from live database
```

### Phase 3: Build Preparation

**Location:** `ThreadedManager.PrepBuildAndScriptsAsync()` (lines 350-384)

```
PrepBuildAndScriptsAsync()
    │
    ├── ExtractAndLoadBuildFileAsync()
    │   ├── SqlBuildFileHelper.ExtractSqlBuildZipFileAsync()
    │   │   ├── Extract .sbm (zip) to working directory
    │   │   └── Return project file path
    │   │
    │   └── SqlBuildFileHelper.LoadSqlBuildProjectFileAsync()
    │       ├── Deserialize XML project file
    │       └── Return SqlSyncBuildDataModel
    │
    └── _scriptBatcher.LoadAndBatchSqlScripts()
        ├── Parse scripts with GO batch separators
        ├── Apply token replacements
        └── Store in BuildExecutionContext.BatchCollection
```

**Key Data Models:**

| Model | Purpose |
|-------|---------|
| `SqlSyncBuildDataModel` | Contains script metadata, execution order, tags |
| `BatchCollection` | Pre-parsed script batches ready for execution |
| `BuildExecutionContext` | Shared state: RunId, paths, batch collection |

### Phase 4: Concurrent Execution

**Location:** `ThreadedManager.ExecuteFromOverrideFileAsync()` (lines 157-217)

Databases are organized into concurrency "buckets" for parallel execution:

```
ExecuteFromOverrideFileAsync()
    │
    ├── Concurrency.ConcurrencyByType()
    │   ├── ConcurrencyType.Count: Fixed # of parallel tasks
    │   ├── ConcurrencyType.Server: Group by server
    │   ├── ConcurrencyType.MaxPerServer: Limit per server
    │   └── ConcurrencyType.Tag: Group by concurrency tag
    │
    ├── For each bucket (parallel):
    │   └── ProcessConcurrencyBucketAsync()
    │       └── For each (server, overrides) in bucket:
    │           ├── Create ThreadedRunner
    │           └── ProcessThreadedBuildAsync()
    │
    └── await Task.WhenAll(tasks)
        └── Aggregate results, log success/failure
```

**Alternative: Queue-based Execution**

When using Service Bus (`ExecuteFromQueueAsync()`):

```
ExecuteFromQueueAsync()
    │
    ├── QueueManager.GetDatabaseTargetFromQueue()
    │   └── Receive messages from Service Bus Topic
    │
    ├── For each message (parallel):
    │   ├── Create ThreadedRunner from TargetMessage
    │   └── ProcessThreadedBuildWithQueueAsync()
    │       ├── ProcessThreadedBuildAsync()
    │       ├── Renew message lock every 30 seconds
    │       └── CompleteMessage() or DeadletterMessage()
    │
    └── Loop until no messages (with retry logic)
```

### Phase 5: Script Execution per Database

**Location:** `ThreadedRunner.RunDatabaseBuildAsync()` → `SqlBuildHelper.ProcessBuildAsync()`

This is where scripts are actually executed against each target database:

```
ThreadedRunner.RunDatabaseBuildAsync()
    │
    ├── Build SqlBuildRunDataModel
    │   ├── Set IsTrial, IsTransactional flags
    │   ├── Set TargetDatabaseOverrides
    │   └── Set PlatinumDacPacFileName if DACPAC source
    │
    ├── If ForceCustomDacpac:
    │   └── DacPacHelper.UpdateBuildRunDataForDacPacSyncAsync()
    │       ├── Extract target database schema
    │       ├── Compare with platinum DACPAC
    │       └── Generate delta scripts specific to this target
    │
    └── SqlBuildHelper.ProcessBuildAsync()
        │
        ├── PrepareBuildForRunAsync()
        │   ├── Filter scripts based on tags/already run
        │   └── Apply database overrides
        │
        └── SqlBuildOrchestrator.ExecuteAsync()
            │
            └── SqlBuildRunner.RunAsync()
                │
                ├── Open database connection
                ├── Begin transaction (if transactional)
                │
                ├── For each script in batch collection:
                │   │
                │   ├── Create savepoint (for rollback granularity)
                │   │
                │   ├── SqlCommandExecutor.ExecuteAsync()
                │   │   ├── Set command timeout
                │   │   └── ExecuteNonQueryAsync()
                │   │
                │   ├── On success:
                │   │   └── Add to committedScripts list
                │   │
                │   └── On failure:
                │       ├── HandleSqlException()
                │       ├── Rollback to savepoint (if available)
                │       └── Set failure status
                │
                └── Return BuildStatus
```

### Phase 6: Transaction Handling & Finalization

**Location:** `SqlBuildRunner` and `DefaultBuildFinalizer`

```
SqlBuildRunner.RunAsync() (continued)
    │
    ├── All scripts succeeded?
    │   │
    │   ├── IsTrial = true?
    │   │   └── Transaction.Rollback()
    │   │       └── Raise BuildSuccessTrialRolledBackEvent
    │   │
    │   └── IsTrial = false?
    │       └── DefaultBuildFinalizer.PerformRunScriptFinalizationAsync()
    │           │
    │           ├── CommitBuild()
    │           │   └── Transaction.Commit()
    │           │
    │           ├── RecordCommittedScripts()
    │           │   └── Populate CommittedScript list
    │           │
    │           ├── LogCommittedScriptsToDatabase()
    │           │   └── Write to SqlBuild_Logging table
    │           │
    │           ├── SaveBuildDataModelAsync()
    │           │   └── Persist to project file
    │           │
    │           └── RaiseBuildCommittedEvent()
    │
    └── Any script failed?
        │
        ├── IsTransactional = true?
        │   └── Transaction.Rollback()
        │       └── Raise BuildErrorRollBackEvent
        │
        └── IsTransactional = false?
            └── Partial commit (scripts run without transaction)
                └── Raise BuildErrorNonTransactionalEvent
```

---

## Build Commit and Database Logging

This section details what happens when a build is successfully committed.

### Commit Flow Architecture

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                    DefaultBuildFinalizer                                     │
│              PerformRunScriptFinalizationAsync()                             │
└──────────────────────────────────────────────────────────────────────────────┘
                                      │
        ┌─────────────────────────────┼─────────────────────────────┐
        ▼                             ▼                             ▼
┌───────────────────┐    ┌────────────────────────┐    ┌────────────────────┐
│   CommitBuild()   │    │ RecordCommittedScripts │    │  SaveBuildDataModel│
│ Transaction.Commit│    │  Populate POCO list    │    │  Persist to file   │
└───────────────────┘    └────────────────────────┘    └────────────────────┘
                                      │
                                      ▼
                    ┌─────────────────────────────────────────┐
                    │       DefaultSqlLoggingService          │
                    │      LogCommittedScriptsToDatabase()    │
                    └─────────────────────────────────────────┘
                                      │
        ┌─────────────────────────────┼─────────────────────────────┐
        ▼                             ▼                             ▼
┌────────────────────┐    ┌────────────────────────┐    ┌────────────────────┐
│EnsureLogTableExists│    │ Group scripts by DB    │    │  Batch INSERT or   │
│ Create if missing  │    │ connection             │    │  fallback to single│
└────────────────────┘    └────────────────────────┘    └────────────────────┘
```

### Detailed Commit Process

**Location:** `DefaultBuildFinalizer.PerformRunScriptFinalizationAsync()`

```
PerformRunScriptFinalizationAsync()
    │
    ├── CommitBuild()
    │   ├── cData.Transaction.Commit()
    │   └── Close connection
    │
    ├── RecordCommittedScripts(committedScripts)
    │   │
    │   └── For each SqlLogging.CommittedScript:
    │       └── Create Models.CommittedScript
    │           ├── ScriptId (GUID as string)
    │           ├── ServerName
    │           ├── CommittedDate (DateTime.Now)
    │           ├── AllowScriptBlock = true
    │           ├── ScriptHash (FileHash)
    │           └── SqlSyncBuildProjectId
    │
    ├── DefaultSqlLoggingService.LogCommittedScriptsToDatabase()
    │   │
    │   ├── EnsureLogTablePresence()
    │   │   ├── Check static cache (avoid re-checking)
    │   │   ├── If table doesn't exist:
    │   │   │   ├── CREATE TABLE SqlBuild_Logging (...)
    │   │   │   └── CREATE INDEX on BuildFileName, CommitDate
    │   │   └── Add to static cache
    │   │
    │   ├── Group CommittedScripts by (Server, Database)
    │   │
    │   └── For each connection group:
    │       ├── Try batch INSERT (multi-row VALUES)
    │       └── On failure: fallback to individual INSERTs
    │
    └── SaveBuildDataModelAsync()
        └── Persist SqlSyncBuildDataModel to project XML file
```

### SqlBuild_Logging Table Schema

The logging table is automatically created in each target database (or alternate logging database if specified):

| Column | Type | Description |
|--------|------|-------------|
| `BuildFileName` | `varchar(300)` | Name of the SBM package file |
| `ScriptFileName` | `varchar(300)` | Name of the SQL script file |
| `ScriptId` | `uniqueidentifier` | Unique ID for the script |
| `ScriptFileHash` | `varchar(100)` | Hash of the script content |
| `CommitDate` | `datetime` | When the script was committed |
| `Sequence` | `int` | Execution order within the build |
| `ScriptText` | `text` | Full text of the SQL script |
| `Tag` | `varchar(200)` | Optional grouping tag |
| `TargetDatabase` | `varchar(200)` | Database the script ran against |
| `RunAs` | `varchar(50)` | User/identity that executed |
| `BuildProjectHash` | `varchar(100)` | Hash of the build project |
| `BuildRequestedBy` | `varchar(200)` | User who initiated the build |
| `ScriptRunStart` | `datetime` | Script execution start time |
| `ScriptRunEnd` | `datetime` | Script execution end time |
| `Description` | `varchar(500)` | Build description |
| `UserId` | `varchar(50)` | User ID |

**Indexes created:**
- `IX_SqlBuild_Logging_BuildFileName` on `BuildFileName`
- `IX_SqlBuild_Logging_CommitDate` on `CommitDate`

### CommittedScript Data Models

Two related models track committed scripts:

**`SqlLogging.CommittedScript`** (Runtime/Transaction object)
```
Used during execution to track each script as it completes:
├── ScriptId (Guid)
├── FileHash (string)
├── Sequence (int)
├── ScriptText (string)
├── Tag (string)
├── ServerName (string)
├── DatabaseTarget (string)
├── RunStart (DateTime)
└── RunEnd (DateTime)
```

**`Models.CommittedScript`** (Persistent model)
```
Stored in SqlSyncBuildDataModel for project history:
├── ScriptId (string)
├── ServerName (string)
├── CommittedDate (DateTime)
├── AllowScriptBlock (bool)
├── ScriptHash (string)
└── SqlSyncBuildProjectId (Guid)
```

### Logging to Alternate Database

If `--logtodatabasename` is specified, all logging writes go to that database instead of each target database:

```
ThreadedRunner.RunDatabaseBuildAsync()
    │
    └── If cmdArgs.LogToDatabaseName specified:
        └── runDataModel.LogToDatabaseName = cmdArgs.LogToDatabaseName
            │
            └── DefaultSqlLoggingService uses this connection
                instead of target database connection
```

### Error Handling in Logging

```
LogCommittedScriptsToDatabase()
    │
    ├── Try: Batch INSERT (all scripts in one statement)
    │   └── Multi-row VALUES clause for efficiency
    │
    └── Catch: Fallback to individual INSERTs
        │
        └── For each script:
            ├── Try: Single parameterized INSERT
            └── Catch: Log error, continue with next script
```

---

## Key Classes Reference

| Class | File | Responsibility |
|-------|------|----------------|
| `ThreadedManager` | `Threaded/ThreadedManager.cs` | Orchestrates entire threaded build |
| `ThreadedRunner` | `Threaded/ThreadedRunner.cs` | Executes build for single server/database set |
| `SqlBuildHelper` | `SqlSync.SqlBuild/SqlBuildHelper.cs` | Entry point for build execution |
| `SqlBuildOrchestrator` | `SqlSync.SqlBuild/SqlBuildOrchestrator.cs` | Handles timeout retries |
| `SqlBuildRunner` | `SqlSync.SqlBuild/SqlBuildRunner.cs` | Iterates through scripts |
| `SqlCommandExecutor` | `SqlSync.SqlBuild/SqlCommandExecutor.cs` | ADO.NET command execution |
| `DefaultBuildFinalizer` | `SqlSync.SqlBuild/Services/DefaultBuildFinalizer.cs` | Commit/rollback transactions, record scripts |
| `DefaultSqlLoggingService` | `SqlSync.SqlBuild/Services/DefaultSqlLoggingService.cs` | Database logging to SqlBuild_Logging table |
| `QueueManager` | `Queue/QueueManager.cs` | Service Bus message handling |
| `Concurrency` | `Threaded/Concurrency.cs` | Concurrency bucket calculation |
| `DacPacHelper` | `DacPac/DacPacHelper.cs` | DACPAC extraction and delta generation |

---

## Return Codes

The build process returns standardized exit codes:

| Code | Enum Value | Description |
|------|------------|-------------|
| 0 | `Successful` | All scripts executed successfully |
| 1 | `FinishingWithErrors` | Some databases had errors |
| 2 | `BuildFileExtractionError` | Could not extract SBM package |
| 3 | `LoadProjectFileError` | Could not load project XML |
| 4 | `NullBuildData` | Build data object is null |
| 5 | `DacpacDatabasesInSync` | DACPAC: target already matches platinum |
| 6 | `RunInitializationError` | Error setting up the run |
| 7 | `ProcessBuildError` | Error during script execution |

---

## Event Flow

Throughout execution, events are raised for monitoring and logging:

```
Build Start
    │
    ├── ThreadedLogging.WriteToLog (Message: "Starting thread")
    │
    ├── ScriptLogWriteEvent (per script, if EventHub logging enabled)
    │
    ├── BuildCommittedEvent OR BuildErrorRollBackEvent OR BuildSuccessTrialRolledBackEvent
    │
    └── ThreadedLogging.WriteToLog (Commit/Error log)
        │
        └── EventHub submission (if configured)
```

Events can be streamed to:
- Local log files
- Azure Event Hub (for real-time monitoring)
- Console output

---

## Concurrency Model

```
                        ┌───────────────────────────────────────┐
                        │        Database Target List           │
                        │  Server1:DB1, Server1:DB2, Server2:DB3│
                        └───────────────────────────────────────┘
                                          │
                        ┌─────────────────┼─────────────────┐
                        ▼                 ▼                 ▼
                  ┌──────────┐     ┌──────────┐     ┌──────────┐
                  │ Bucket 1 │     │ Bucket 2 │     │ Bucket 3 │
                  │Server1:DB1│    │Server1:DB2│    │Server2:DB3│
                  └──────────┘     └──────────┘     └──────────┘
                        │                 │                 │
                        ▼                 ▼                 ▼
                  ┌──────────┐     ┌──────────┐     ┌──────────┐
                  │ Thread 1 │     │ Thread 2 │     │ Thread 3 │
                  │  Runner  │     │  Runner  │     │  Runner  │
                  └──────────┘     └──────────┘     └──────────┘
                        │                 │                 │
                        └─────────────────┼─────────────────┘
                                          ▼
                              ┌───────────────────────┐
                              │ Task.WhenAll(tasks)   │
                              │ Await all completions │
                              └───────────────────────┘
```

Concurrency is controlled by:
- `--concurrency`: Number of parallel operations
- `--concurrencytype`: How to group databases (Count, Server, MaxPerServer, Tag)

---

## See Also

- [Threaded Build Command Line](threaded_build.md)
- [Concurrency Options](concurrency_options.md)
- [Override Options](override_options.md)
- [Command Line Reference](commandline.md)
