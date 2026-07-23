# Execution options and exit codes

## Validated defaults

Runtime defaults and polling policy are defined in
`src/SqlBuildManager.Console/ExecutionOptions.cs`.

| Contract | Default |
|---|---:|
| Concurrent targets per worker | 10 |
| Script timeout | 500 seconds |
| Timeout retries | 0 |
| Batch nodes | 10 |
| Kubernetes pods | 10 |
| Container Apps maximum replicas | 10 |
| Batch job monitoring timeout | 30 minutes |
| Batch pool name | `SqlBuildManagerPoolLinux` |
| Batch job name | 3-41 lowercase letters, numbers, or single dashes |
| Write SAS lifetime | 4 hours |
| Read SAS lifetime | 7 hours |
| SAS clock-skew allowance | Starts 1 hour before issuance |
| Fast resource polling | 1 second |
| Distributed queue polling | 2 seconds |
| Batch/ACI state polling | 15 seconds |
| Unchanged-status heartbeat | 30 seconds |

Concurrency, timeout, node, pod, and replica values must be greater than zero. Invalid values fail
before resource creation with `InvalidExecutionOption`.

## Process exit codes

`ExecutionReturn` in `SqlBuildManager.Interfaces` is the source of truth. Every value includes an
operator description and command handlers use names instead of private numeric values.

| Code | Name | Operator meaning |
|---:|---|---|
| -3 | `UserCancelled` | The user cancelled the operation. |
| -100 | `MissingOverrideFlag` | A required target override is missing. |
| -101 | `MissingBuildFlag` | A build source is missing. |
| -102 | `InvalidOverrideFlag` | The override value or file type is invalid. |
| -103 | `NullBuildData` | Build data could not be loaded. |
| -104 | `NullMultiDbConfig` | Multi-database configuration could not be loaded. |
| -105 | `InvalidScriptSourceDirectory` | The script source directory is invalid. |
| -106 | `InvalidBuildFileNameValue` | A build or DACPAC path is invalid. |
| -107 | `InvalidTransactionAndTrialCombo` | Trial mode requires transactional execution. |
| -108 | `MissingTargetDbOverrideSetting` | The target override file is missing. |
| -109 | `NegativeTimeoutRetryCount` | Timeout retries cannot be negative. |
| -110 | `BadRetryCountAndTransactionalCombo` | Timeout retries require transactional execution. |
| -111 | `InvalidExecutionOption` | An option is outside its supported range or a required operational option is missing. |
| -112 | `InvalidAuthenticationArguments` | Authentication arguments are incomplete. |
| -113 | `InvalidBatchArguments` | Batch configuration or its job name is invalid. |
| -114 | `InvalidOutputFile` | An output/settings file is missing, invalid, or already exists. |
| -115 | `InvalidSettingsKey` | The settings encryption key is invalid. |
| -200 | `BuildFileExtractionError` | Build package or DACPAC extraction failed. |
| -201 | `LoadProjectFileError` | A project/package file could not be loaded. |
| -300 | `RunInitializationError` | Settings decryption, Key Vault loading, or initialization failed. |
| -301 | `ProcessBuildError` | Build/package processing failed. |
| -302 | `UnhandledException` | An unexpected exception terminated the command. |
| -600 | `UnableToLoadBuildSettings` | Build settings could not be loaded. |
| -601 | `OneOrMoreRemoteServersHadError` | A remote worker or queue operation failed. |
| -602 | `BatchJobMonitorTimeout` | The Batch job exceeded its configured monitor timeout. |
| -603 | `BatchExecutionError` | Batch execution failed before a task result was available. |
| -604 | `BatchPoolCreationError` | The Batch pool could not be created. |
| -605 | `BatchNodeUnavailable` | Batch nodes could not become usable. |
| -606 | `BatchCleanupError` | Batch resources could not be deleted. |
| -698 | `UnassignedDatabaseServers` | Database targets were not assigned to workers. |
| 0 | `Successful` | The command completed successfully. |
| 1 | `FinishingWithErrors` | The command completed with one or more operation errors. |
| 5000/6000/7000 | Status values | Waiting, running, or checking connections; used by distributed status reporting. |
| 87598 | `DacpacDatabasesInSync` | DACPAC comparison found no required changes. |
| 100323 | `MissingOverrideTags` | Required concurrency tags are missing. |

On Linux and other POSIX shells, process exit status is limited to eight bits. Negative values are
reported modulo 256 by the shell. Automation that requires the full signed value should also parse
the final `Exiting with code ...` log entry.

## Operational error contracts

- Validation and initialization failures occur before cost-bearing resource creation.
- A Batch monitor timeout returns `BatchJobMonitorTimeout`; normal `finally` cleanup still follows
  the configured `--deletebatchjob` and `--deletebatchpool` values.
- Cleanup failures are logged and return `BatchCleanupError` only when cleanup is the requested
  operation. Cleanup errors after a completed build do not replace its build result.
- `FinishingWithErrors` means the command completed but one or more target/output operations failed;
  it is not equivalent to full success.
- Unhandled exceptions are logged as `UnhandledException`; callers must not treat absent output as
  success.
