# SqlBuildHelper & SqlBuildRunner Refactor Plan

## Overview
**Goals:** Improve maintainability, testability, and adherence to the Single Responsibility Principle (SRP) for `SqlBuildHelper` and `SqlBuildRunner` and related classes.

## Current State
- `SqlBuildHelper.cs`: ~2356 LOC, multiple responsibilities (prep, execution, batching, logging, persistence, legacy conversions, DacPac, token replacement, FS IO, retries).
- `SqlBuildRunner.cs`: ~428 LOC, more cohesive but mixes execution, progress reporting, logging, and time handling.

### Key Responsibilities Observed
- Build preparation (paths, history files, hash calculation, filtering scripts).
- Script batching (regex-based `GO` detection, transaction stripping, file IO).
- Token replacement (`ScriptTokens`).
- Build execution orchestration (multi/single DB, retries, savepoints, cancellation).
- Connection & transaction management (`BuildConnectData`, `ConnectionData`).
- Logging: file log, SQL logging table, history persistence, committed scripts.
- Legacy DataSet compatibility (POCO ↔ DataSet conversions and mappers).
- DacPac delta handling.
- Progress reporting via `BackgroundWorker`.

## Maintainability Findings
| Hotspot | Symptoms | Risk | Direction |
| --- | --- | --- | --- |
| `SqlBuildHelper` (monolith) | 2356 LOC, stateful fields, many concerns | High complexity, regression risk | Split into services (prep, execution, logging, persistence, batching, token replace) |
| Legacy DataSet compatibility | Inline conversions, `buildDataCompat`, mappers | Noise, coupling, brittle | Wrap in `ILegacyBuildDataAdapter`; isolate/gradually deprecate |
| FS & IO inside core logic | `File.Exists`, `StreamWriter`, `ReadXml` | Unmockable paths, flaky tests | Introduce `IFileSystem` / abstraction |
| Static/global state | `SqlBuildRunnerFactory`, `OverrideData`, static logger | Hidden coupling, test interference | Inject via DI; avoid statics (or gate behind singleton interface) |
| Error handling & retries | Custom loops, `Polly` unused | Duplicated, inconsistent | Centralize retry policy (`IBuildRetryPolicy`), leverage `Polly` |
| Token replacement & batching | Complex regex, subtle bugs | Low coverage, brittle | Extract to `IScriptBatcher` + pure functions with tests |
| Commit/Rollback finalization | Multiple variants, duplicated code paths | Divergent behavior | Consolidate into `IBuildFinalizer` |
| Progress reporting | `BackgroundWorker` | Hard to test, legacy | Wrap in `IProgressReporter`, prepare async path |

## Testability Issues
- Tight coupling to ADO/Sql: inline `SqlCommand` creation. `SqlBuildRunner` uses `ISqlCommandExecutor`; extend with `ISqlConnectionFactory` for connection creation.
- Time/Random: `DateTime.Now`, `Guid.NewGuid()`; use `IClock`, `IGuidProvider`.
- File system: log writing & history persistence; use `IFileSystem` (e.g., `System.IO.Abstractions`).
- Progress/Events: `BackgroundWorker` is hard to assert; abstract to `IProgressReporter` with typed events.
- Static helpers: `SqlBuildFileHelper` static; wrap as `ISqlBuildFileHelper` for hashing/batching.

## SRP Violations → Extraction Targets
1. **BuildPreparationService**: project paths, history file, hash calculation, filtering scripts.
2. **ScriptRepository / ScriptBatcher**: load scripts, batching (regex), transaction stripping.
3. **TokenReplacementService**: encapsulate `PerformScriptTokenReplacement`.
4. **ExecutionOrchestrator**: current `ProcessBuild` orchestration; uses runner, retry policy, finalizer.
5. **ConnectionManager**: `ConnectionData` → `BuildConnectData`, transaction lifecycle.
6. **LoggingPersistenceService**: script log, SQL logging table, history persistence, committed scripts.
7. **LegacyBuildDataAdapter**: POCO ↔ DataSet conversions, backward compatibility.
8. **ProgressReporter**: events, `BackgroundWorker` bridge; async-friendly abstraction.

## Refactoring Plan Template

### Phase 1: Baselines & Safety Nets
- Add characterization tests for batching (`ReadBatchFromScriptText`), token replacement, `HandleSqlException`, finalization states, retry behavior.
- Capture complexity/coverage metrics (e.g., Sonar/NDepend).

### Phase 2: Introduce Seams (No Behavior Change)
- Define interfaces: `IFileSystem`, `IClock`, `IGuidProvider`, `IProgressReporter`, `ISqlBuildFileHelper`, `IBuildRetryPolicy`, `IBuildFinalizer`, `ILegacyBuildDataAdapter`.
- Wrap statics (logger, `SqlBuildFileHelper`, `OverrideData`) behind interfaces; inject via constructors/factories.

### Phase 3: Extract Services
- `BuildPreparationService`: responsibilities for project path, hash, filtering.
- `ScriptBatcher`: regex batching & file IO; pure functions for tests.
- `TokenReplacementService`: replace tokens deterministicly.
- `BuildFinalizer`: unify commit/rollback logic for single/multi-db.
- `SqlLoggingService`: ensure table, write committed scripts, blocking checks.
- `LegacyBuildDataAdapter`: encapsulate conversions.

### Phase 4: Orchestrate
- `SqlBuildOrchestrator` (former `ProcessBuild`): orchestrates prep → run → retry → finalize.
- Inject `ISqlCommandExecutor`, `IConnectionFactory` into `SqlBuildRunner`.
- Centralize retry policy with `Polly` (configurable).

### Phase 5: Progress & Async
- Abstract progress reporting to `IProgressReporter`; adapt `BackgroundWorkerReporter` shim.
- Add async (`Task`-based) runner entry points compatible with UI console/service.

### Phase 6: Deprecate & Clean
- Mark legacy APIs `[Obsolete]`; keep thin adapters.
- Remove static globals; wire via DI container.
- Simplify public surface to POCO models; isolate DataSet usage.

### Phase 7: Hardening & Docs
- Add unit tests per extracted service; integration tests with in-memory FS + fake SQL executor.
- Document module boundaries and extension points.

## Quick Wins
- Add tests for `HandleSqlException` and `ReadBatchFromScriptText` (edge cases: `GO` in comments, transaction stripping).
- Inject `IProgressReporter` into `SqlBuildRunner`; shim with `BackgroundWorker`.
- Wrap `SqlBuildFileHelper` behind `ISqlBuildFileHelper` interface.
- Consolidate commit/rollback logic into `IBuildFinalizer`.

## Artifacts & Ownership
- **Files:** `SqlBuildHelper.cs`, `SqlBuildRunner.cs` (split responsibilities into sub-files/classes).
- **Interfaces:** `ISqlBuildFileHelper`, `IProgressReporter`, `IFileSystem`, `IClock`, `IGuidProvider`, `IBuildRetryPolicy`, `IBuildFinalizer`, `ILegacyBuildDataAdapter`.
- **Services:** `BuildPreparationService`, `ScriptBatcher`, `TokenReplacementService`, `BuildFinalizer`, `SqlLoggingService`, `LegacyBuildDataAdapter`, `SqlBuildOrchestrator`.

## TODO (Starter List)
- [ ] Add characterization tests for batching, token replacement, exception handling, finalization.
- [ ] Introduce interfaces and inject dependencies (no behavior change).
- [ ] Extract services iteratively; wire orchestrator; maintain legacy adapter.
- [ ] Update docs and diagrams post-extraction.

---
*Generated: 2026-01-14*
