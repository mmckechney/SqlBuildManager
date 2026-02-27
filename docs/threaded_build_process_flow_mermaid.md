# Threaded Build Process Flow (Mermaid Diagrams)

This document is a companion to [threaded_build_process_flow.md](threaded_build_process_flow.md), with ASCII flow diagrams replaced by Mermaid diagrams that render natively in GitHub and most markdown viewers.

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

```mermaid
graph TD
    H1["<b>ThreadedManager</b><br/>ExecuteAsync()"]:::header

    H1 --> V1["Validation &<br/>Path Setup"]:::process
    H1 --> V2["Script Source Config<br/>(SBM / DACPAC / Scripts)"]:::process
    H1 --> V3["Build Preparation<br/>& Script Loading"]:::process

    V2 --> |Execution Mode| EX1["ExecuteFromOverrideFile<br/>(Target file sourced)"]:::alt
    V2 --> |Execution Mode| EX2["ExecuteFromQueue<br/>(Service Bus sourced)"]:::alt

    EX1 --> PC["ProcessConcurrencyBucket<br/>(Parallel per bucket)"]:::exec
    EX2 --> PC

    PC --> TR["ThreadedRunner<br/>RunDatabaseBuild()"]:::exec
    TR --> SH["SqlBuildHelper<br/>ProcessBuild()"]:::exec
    SH --> SO["SqlBuildOrchestrator<br/>Execute()"]:::exec
    SO --> SR["SqlBuildRunner<br/>Run()"]:::exec
    SR --> SC["SqlCommandExecutor<br/>Execute()"]:::exec

    classDef header fill:#dae8fc,stroke:#6c8ebf,color:#333333,font-weight:bold
    classDef process fill:#d5e8d4,stroke:#82b366,color:#333333
    classDef alt fill:#fff2cc,stroke:#d6b656,color:#333333
    classDef exec fill:#e1d5e7,stroke:#9673a6,color:#333333
```

---

## Detailed Process Flow

### Phase 1: Initialization & Validation

**Location:** `ThreadedManager.ExecuteAsync()` (lines 58-87)

```mermaid
graph TD
    P1H["<b>ThreadedManager.ExecuteAsync()</b>"]:::header

    P1H --> P1A["Initialize ThreadedLogging<br/>with RunId"]:::process

    P1A --> P1B["Set root logging path"]:::process
    P1B --> P1B1["AZ_BATCH_TASK_WORKING_DIR<br/>(if in Batch)"]:::sub
    P1B --> P1B2["cmdLine.RootLoggingPath<br/>(if specified)"]:::sub
    P1B --> P1B3["Default:<br/>./tmp-sqlbuildlogging"]:::sub

    P1A --> P1C["SetRootAndWorkingPaths()"]:::process
    P1C --> P1C1["Create root logging directory"]:::sub
    P1C --> P1C2["Create Working subdirectory"]:::sub

    P1A --> P1D["Validation.ValidateCommon<br/>CommandLineArgs()"]:::process
    P1D --> P1D1["Validate required parameters"]:::sub
    P1D --> P1D2["Return error codes<br/>if validation fails"]:::error

    classDef header fill:#dae8fc,stroke:#6c8ebf,color:#333333,font-weight:bold
    classDef process fill:#d5e8d4,stroke:#82b366,color:#333333
    classDef sub fill:#f5f5f5,stroke:#666666,color:#333333
    classDef error fill:#f8cecc,stroke:#b85450,color:#333333
```

### Phase 2: Script Source Configuration

**Location:** `ThreadedManager.ConfigureScriptSource()` (lines 288-348)

The build can be sourced from multiple inputs. The method determines which source to use:

```mermaid
graph TD
    P2H["<b>ConfigureScriptSource()</b>"]:::header

    P2H --> P2D1{"ScriptSrcDir<br/>specified?"}:::alt
    P2D1 --> |Yes| P2D1A["ConstructBuildFileFrom<br/>ScriptDirectory()"]:::process
    P2D1A --> P2D1B["Enumerate .sql files"]:::sub
    P2D1B --> P2D1C["Sort alphabetically"]:::sub
    P2D1C --> P2D1D["Build SBM package"]:::sub

    P2H --> P2D2{"BuildFileName (.sbm)<br/>specified?"}:::alt
    P2D2 --> |Yes| P2D2A["Use SBM file directly<br/>as build source"]:::process

    P2H --> P2D3{"PlatinumDacpac<br/>specified?"}:::alt
    P2D3 --> |Yes| P2D3A["Use DACPAC to generate scripts<br/>via delta comparison"]:::process

    P2H --> P2D4{"PlatinumDbSource +<br/>PlatinumServerSource?"}:::alt
    P2D4 --> |Yes| P2D4A["DacPacHelper.ExtractDacPac()"]:::process
    P2D4A --> P2D4B["Connect to platinum DB"]:::sub
    P2D4B --> P2D4C["Extract DACPAC from<br/>live database"]:::sub

    classDef header fill:#dae8fc,stroke:#6c8ebf,color:#333333,font-weight:bold
    classDef process fill:#d5e8d4,stroke:#82b366,color:#333333
    classDef alt fill:#fff2cc,stroke:#d6b656,color:#333333
    classDef sub fill:#f5f5f5,stroke:#666666,color:#333333
```

### Phase 3: Build Preparation

**Location:** `ThreadedManager.PrepBuildAndScriptsAsync()` (lines 350-384)

```mermaid
graph TD
    P3H["<b>PrepBuildAndScriptsAsync()</b>"]:::header

    P3H --> P3A["ExtractAndLoadBuildFileAsync()"]:::process
    P3A --> P3A1["ExtractSqlBuildZipFileAsync()<br/>Extract .sbm to working dir"]:::sub
    P3A --> P3A2["LoadSqlBuildProjectFileAsync()<br/>Deserialize XML project file"]:::sub

    P3H --> P3B["_scriptBatcher.LoadAndBatchSqlScripts()"]:::process
    P3B --> P3B1["Parse scripts with<br/>GO batch separators"]:::sub
    P3B --> P3B2["Apply token replacements"]:::sub
    P3B --> P3B3["Store in BuildExecutionContext<br/>.BatchCollection"]:::sub

    classDef header fill:#dae8fc,stroke:#6c8ebf,color:#333333,font-weight:bold
    classDef process fill:#d5e8d4,stroke:#82b366,color:#333333
    classDef sub fill:#f5f5f5,stroke:#666666,color:#333333
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

```mermaid
graph TD
    P4H["<b>ExecuteFromOverrideFileAsync()</b>"]:::header

    P4H --> P4A["Concurrency.ConcurrencyByType()"]:::process
    P4A --> CT1["Count: Fixed # of parallel tasks"]:::sub
    P4A --> CT2["Server: Group by server"]:::sub
    P4A --> CT3["MaxPerServer: Limit per server"]:::sub
    P4A --> CT4["Tag: Group by concurrency tag"]:::sub

    P4H --> P4B["For each bucket - parallel:<br/>ProcessConcurrencyBucketAsync()"]:::exec
    P4B --> P4C["For each server, overrides:<br/>Create ThreadedRunner"]:::exec
    P4C --> P4D["ProcessThreadedBuildAsync()"]:::process

    P4D --> P4E["<b>await Task.WhenAll - tasks</b><br/>Aggregate results"]:::header

    classDef header fill:#dae8fc,stroke:#6c8ebf,color:#333333,font-weight:bold
    classDef process fill:#d5e8d4,stroke:#82b366,color:#333333
    classDef exec fill:#e1d5e7,stroke:#9673a6,color:#333333
    classDef sub fill:#f5f5f5,stroke:#666666,color:#333333
```

**Alternative: Queue-based Execution**

When using Service Bus (`ExecuteFromQueueAsync()`):

```mermaid
graph TD
    Q1["<b>ExecuteFromQueueAsync()</b>"]:::header

    Q1 --> Q2["QueueManager<br/>.GetDatabaseTargetFromQueue()<br/>Receive from Service Bus"]:::process

    Q2 --> Q3["For each message - parallel:<br/>Create ThreadedRunner"]:::exec

    Q3 --> Q4["ProcessThreadedBuild<br/>WithQueueAsync()"]:::process
    Q4 --> Q4A["ProcessThreadedBuildAsync()"]:::exec
    Q4 --> Q4B["Renew message lock<br/>every 30 seconds"]:::sub
    Q4 --> Q4C["CompleteMessage() or<br/>DeadletterMessage()"]:::alt

    Q4 --> Q5["Loop until no messages<br/>(with retry logic)"]:::alt

    classDef header fill:#dae8fc,stroke:#6c8ebf,color:#333333,font-weight:bold
    classDef process fill:#d5e8d4,stroke:#82b366,color:#333333
    classDef exec fill:#e1d5e7,stroke:#9673a6,color:#333333
    classDef alt fill:#fff2cc,stroke:#d6b656,color:#333333
    classDef sub fill:#f5f5f5,stroke:#666666,color:#333333
```

### Phase 5: Script Execution per Database

**Location:** `ThreadedRunner.RunDatabaseBuildAsync()` → `SqlBuildHelper.ProcessBuildAsync()`

This is where scripts are actually executed against each target database:

```mermaid
graph TD
    S1["<b>ThreadedRunner<br/>.RunDatabaseBuildAsync()</b>"]:::header

    S1 --> S2["Build SqlBuildRunDataModel<br/>(IsTrial, IsTransactional, Overrides)"]:::process

    S2 --> S3{"ForceCustomDacpac?"}:::alt
    S3 --> |Yes| S3A["DacPacHelper<br/>.UpdateBuildRunDataForDacPacSync()<br/>Extract target schema<br/>Compare with platinum DACPAC<br/>Generate delta scripts"]:::sub

    S3 --> S4["SqlBuildHelper.ProcessBuildAsync()"]:::process
    S4 --> S5["PrepareBuildForRunAsync()<br/>Filter scripts, apply overrides"]:::process
    S5 --> S6["SqlBuildOrchestrator.ExecuteAsync()"]:::exec
    S6 --> S7["SqlBuildRunner.RunAsync()"]:::exec

    S7 --> S8["Open connection<br/>Begin transaction (if transactional)"]:::process

    S8 --> S9["For each script in<br/>batch collection:"]:::exec
    S9 --> S10["Create savepoint<br/>(for rollback granularity)"]:::process
    S10 --> S11["SqlCommandExecutor.ExecuteAsync()<br/>Set timeout, ExecuteNonQueryAsync()"]:::exec

    S11 --> |Success| S12["Add to<br/>committedScripts list"]:::process
    S11 --> |Failure| S13["HandleSqlException()<br/>Rollback to savepoint"]:::error

    S12 --> S14["<b>Return BuildStatus</b>"]:::header
    S13 --> S14

    classDef header fill:#dae8fc,stroke:#6c8ebf,color:#333333,font-weight:bold
    classDef process fill:#d5e8d4,stroke:#82b366,color:#333333
    classDef alt fill:#fff2cc,stroke:#d6b656,color:#333333
    classDef exec fill:#e1d5e7,stroke:#9673a6,color:#333333
    classDef error fill:#f8cecc,stroke:#b85450,color:#333333
    classDef sub fill:#f5f5f5,stroke:#666666,color:#333333
```

### Phase 6: Transaction Handling & Finalization

**Location:** `SqlBuildRunner` and `DefaultBuildFinalizer`

```mermaid
graph TD
    T1["<b>SqlBuildRunner.RunAsync()<br/>(continued)</b>"]:::header

    T1 --> T2{"All scripts<br/>succeeded?"}:::alt

    T2 --> |Yes| T3{"IsTrial = true?"}:::alt
    T3 --> |Yes| T3A["Transaction.Rollback()<br/>BuildSuccessTrialRolledBackEvent"]:::process
    T3 --> |No| T4["IsTrial = false"]:::alt

    T4 --> T4A["DefaultBuildFinalizer<br/>.PerformRunScriptFinalizationAsync()"]:::exec
    T4A --> F1["CommitBuild()<br/>Transaction.Commit()"]:::process
    T4A --> F2["RecordCommittedScripts()"]:::process
    T4A --> F3["LogCommittedScriptsToDatabase()<br/>Write to SqlBuild_Logging"]:::process
    T4A --> F4["SaveBuildDataModelAsync()"]:::process
    T4A --> F5["RaiseBuildCommittedEvent()"]:::process

    T2 --> |No| T5["Any script failed?"]:::error
    T5 --> T5A{"IsTransactional<br/>= true?"}:::alt
    T5A --> |Yes| T5B["Transaction.Rollback()<br/>BuildErrorRollBackEvent"]:::error
    T5A --> |No| T5C["Partial commit<br/>BuildErrorNonTransactionalEvent"]:::error

    classDef header fill:#dae8fc,stroke:#6c8ebf,color:#333333,font-weight:bold
    classDef process fill:#d5e8d4,stroke:#82b366,color:#333333
    classDef alt fill:#fff2cc,stroke:#d6b656,color:#333333
    classDef exec fill:#e1d5e7,stroke:#9673a6,color:#333333
    classDef error fill:#f8cecc,stroke:#b85450,color:#333333
```

---

## Build Commit and Database Logging

This section details what happens when a build is successfully committed.

### Commit Flow Architecture

```mermaid
graph TD
    CF1["<b>DefaultBuildFinalizer</b><br/>PerformRunScriptFinalizationAsync()"]:::header

    CF1 --> CF2["CommitBuild()<br/>Transaction.Commit"]:::process
    CF1 --> CF3["RecordCommittedScripts<br/>Populate POCO list"]:::process
    CF1 --> CF4["SaveBuildDataModel<br/>Persist to file"]:::process

    CF3 --> CF5["DefaultSqlLoggingService<br/>LogCommittedScriptsToDatabase()"]:::exec

    CF5 --> CF6["EnsureLogTableExists<br/>Create if missing"]:::process
    CF5 --> CF7["Group scripts by DB<br/>connection"]:::process
    CF5 --> CF8["Batch INSERT or<br/>fallback to single"]:::alt

    classDef header fill:#dae8fc,stroke:#6c8ebf,color:#333333,font-weight:bold
    classDef process fill:#d5e8d4,stroke:#82b366,color:#333333
    classDef exec fill:#e1d5e7,stroke:#9673a6,color:#333333
    classDef alt fill:#fff2cc,stroke:#d6b656,color:#333333
```

### Detailed Commit Process

**Location:** `DefaultBuildFinalizer.PerformRunScriptFinalizationAsync()`

```mermaid
graph TD
    DC1["<b>PerformRunScriptFinalizationAsync()</b>"]:::header

    DC1 --> DC2["CommitBuild()<br/>Transaction.Commit() + Close connection"]:::process

    DC2 --> DC3["RecordCommittedScripts(committedScripts)"]:::process
    DC3 --> DC3A["ScriptId (GUID as string)"]:::sub
    DC3 --> DC3B["ServerName"]:::sub
    DC3 --> DC3C["CommittedDate (DateTime.Now)"]:::sub
    DC3 --> DC3D["ScriptHash (FileHash)"]:::sub
    DC3 --> DC3E["SqlSyncBuildProjectId"]:::sub

    DC3 --> DC4["DefaultSqlLoggingService<br/>.LogCommittedScriptsToDatabase()"]:::exec

    DC4 --> DC4A["EnsureLogTablePresence()<br/>Check cache, CREATE TABLE if needed"]:::process
    DC4A --> DC4B["Group CommittedScripts by<br/>(Server, Database)"]:::process
    DC4B --> DC4C["Try batch INSERT<br/>(multi-row VALUES)"]:::process
    DC4C --> DC4D["On failure: fallback to<br/>individual INSERTs"]:::error

    DC4 --> DC5["SaveBuildDataModelAsync()<br/>Persist to project XML file"]:::process

    classDef header fill:#dae8fc,stroke:#6c8ebf,color:#333333,font-weight:bold
    classDef process fill:#d5e8d4,stroke:#82b366,color:#333333
    classDef exec fill:#e1d5e7,stroke:#9673a6,color:#333333
    classDef error fill:#f8cecc,stroke:#b85450,color:#333333
    classDef sub fill:#f5f5f5,stroke:#666666,color:#333333
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
Used during execution to track each script as it completes:
- ScriptId (Guid)
- FileHash (string)
- Sequence (int)
- ScriptText (string)
- Tag (string)
- ServerName (string)
- DatabaseTarget (string)
- RunStart (DateTime)
- RunEnd (DateTime)

**`Models.CommittedScript`** (Persistent model)
Stored in SqlSyncBuildDataModel for project history:
- ScriptId (string)
- ServerName (string)
- CommittedDate (DateTime)
- AllowScriptBlock (bool)
- ScriptHash (string)
- SqlSyncBuildProjectId (Guid)

### Logging to Alternate Database

If `--logtodatabasename` is specified, all logging writes go to that database instead of each target database:

```mermaid
graph TD
    AL1["<b>ThreadedRunner<br/>.RunDatabaseBuildAsync()</b>"]:::header

    AL1 --> AL2{"cmdArgs.LogToDatabaseName<br/>specified?"}:::alt
    AL2 --> |Yes| AL3["runDataModel.LogToDatabaseName<br/>= cmdArgs.LogToDatabaseName"]:::process
    AL3 --> AL4["DefaultSqlLoggingService uses<br/>alternate connection instead of<br/>target database connection"]:::exec

    classDef header fill:#dae8fc,stroke:#6c8ebf,color:#333333,font-weight:bold
    classDef process fill:#d5e8d4,stroke:#82b366,color:#333333
    classDef alt fill:#fff2cc,stroke:#d6b656,color:#333333
    classDef exec fill:#e1d5e7,stroke:#9673a6,color:#333333
```

### Error Handling in Logging

```mermaid
graph TD
    EH1["<b>LogCommittedScriptsToDatabase()</b>"]:::header

    EH1 --> EH2["Try: Batch INSERT<br/>(all scripts in one statement)<br/>Multi-row VALUES clause"]:::process

    EH2 --> |Exception| EH3["Catch: Fallback to<br/>individual INSERTs"]:::error

    EH3 --> EH4["For each script:"]:::exec
    EH4 --> EH5["Try: Single parameterized INSERT"]:::process
    EH5 --> |Exception| EH6["Catch: Log error,<br/>continue with next script"]:::error

    classDef header fill:#dae8fc,stroke:#6c8ebf,color:#333333,font-weight:bold
    classDef process fill:#d5e8d4,stroke:#82b366,color:#333333
    classDef exec fill:#e1d5e7,stroke:#9673a6,color:#333333
    classDef error fill:#f8cecc,stroke:#b85450,color:#333333
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

```mermaid
graph TD
    EV1["<b>Build Start</b>"]:::header

    EV1 --> EV2["ThreadedLogging.WriteToLog<br/>(Starting thread)"]:::process
    EV2 --> EV3["ScriptLogWriteEvent<br/>(per script, if EventHub enabled)"]:::process

    EV3 --> EV4["BuildCommittedEvent"]:::process
    EV3 --> EV5["BuildErrorRollBackEvent"]:::error
    EV3 --> EV6["BuildSuccess<br/>TrialRolledBackEvent"]:::alt

    EV4 --> EV7["ThreadedLogging.WriteToLog<br/>(Commit/Error log)"]:::process
    EV5 --> EV7
    EV6 --> EV7

    EV7 --> EV8["EventHub submission<br/>(if configured)"]:::exec

    EV8 --> EV9["Local log files"]:::sub
    EV8 --> EV10["Azure Event Hub"]:::sub
    EV8 --> EV11["Console output"]:::sub

    classDef header fill:#dae8fc,stroke:#6c8ebf,color:#333333,font-weight:bold
    classDef process fill:#d5e8d4,stroke:#82b366,color:#333333
    classDef alt fill:#fff2cc,stroke:#d6b656,color:#333333
    classDef exec fill:#e1d5e7,stroke:#9673a6,color:#333333
    classDef error fill:#f8cecc,stroke:#b85450,color:#333333
    classDef sub fill:#f5f5f5,stroke:#666666,color:#333333
```

Events can be streamed to:
- Local log files
- Azure Event Hub (for real-time monitoring)
- Console output

---

## Concurrency Model

```mermaid
graph TD
    CM1["<b>Database Target List</b><br/>Server1:DB1, Server1:DB2, Server2:DB3"]:::header

    CM1 --> CM2["Bucket 1<br/>Server1:DB1"]:::process
    CM1 --> CM3["Bucket 2<br/>Server1:DB2"]:::process
    CM1 --> CM4["Bucket 3<br/>Server2:DB3"]:::process

    CM2 --> CM5["Thread 1<br/>Runner"]:::exec
    CM3 --> CM6["Thread 2<br/>Runner"]:::exec
    CM4 --> CM7["Thread 3<br/>Runner"]:::exec

    CM5 --> CM8["<b>Task.WhenAll(tasks)</b><br/>Await all completions"]:::header
    CM6 --> CM8
    CM7 --> CM8

    classDef header fill:#dae8fc,stroke:#6c8ebf,color:#333333,font-weight:bold
    classDef process fill:#d5e8d4,stroke:#82b366,color:#333333
    classDef exec fill:#e1d5e7,stroke:#9673a6,color:#333333
```

Concurrency is controlled by:
- `--concurrency`: Number of parallel operations
- `--concurrencytype`: How to group databases (Count, Server, MaxPerServer, Tag)

---

## See Also

- [Original ASCII Diagram Version](threaded_build_process_flow.md)
- [Draw.io Visual Diagram Version](threaded_build_process_flow_visual.md)
- [Threaded Build Command Line](threaded_build.md)
- [Concurrency Options](concurrency_options.md)
- [Override Options](override_options.md)
- [Command Line Reference](commandline.md)
