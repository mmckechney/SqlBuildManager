# SqlBuildHelper & SqlBuildRunner Refactor Plan

## Overview
**Goals:** Improve maintainability, testability, and adherence to the Single Responsibility Principle (SRP) for `SqlBuildHelper` and `SqlBuildRunner` and related classes.

## Execution Log
- [ ] 2026-01-14: Scaffold execution log and refactor activity tracking.
- [ ] 2026-01-14: Phase 2 kickoff — introduce interfaces and seams.
 - [x] 2026-01-14: Phase 3 start — scaffold services (prep, batcher, token, logging).
- [ ] 2026-01-14: Phase 4 start — orchestrator extraction & retry wiring.
- [ ] 2026-01-14: Phase 5 start — async entry points & progress abstraction.
- [ ] 2026-01-14: Phase 5a start — fully async pipeline (runner/orchestrator/executor).
- [x] 2026-01-14: Phase 5a complete — fully async runner/orchestrator/executor; tests passing.
- [x] 2026-01-14: Phase 5b start — async executor path tests & cancellation tests.
- [ ] 2026-01-14: Phase 5b planned — async file/token IO abstractions.
- [x] 2026-01-14: Phase 5c start — async file/token abstractions (ScriptBatcher, IFileSystem, token service).
- [x] 2026-01-14: Phase 5d start — async helper consumers & service tests (helper static async, batcher/token async tests).
- [x] 2026-01-14: Phase 5e start — consider async variants in `SqlBuildFileHelper` consumers and add tests.
- [x] 2026-01-14: Phase 5f start — async packaging API variants & tests.
- [x] 2026-01-14: Phase 5g start — true async ZIP persistence & tests.
- [x] 2026-01-14: Phase 5h start — consolidate `ISqlBuildFileHelper` default implementation.
- [x] 2026-01-14: Phase 6 start — deprecate legacy APIs & prep DI seams.

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

### Phase 5: Progress & Async Wrappers
- Abstract progress reporting to `IProgressReporter`; adapt `BackgroundWorkerReporter` shim.
- Add async (`Task`-based) runner entry points compatible with UI console/service.

### Phase 5a: Fully Async Pipeline
- Make `RunAsync` async-first (no `Task.Run`), calling `ISqlCommandExecutor.ExecuteAsync` inside the script loop.
- Implement async SQL execution in `SqlCommandExecutor` using ADO.NET async APIs; propagate `CancellationToken`.
- Ensure orchestrator `ExecuteAsync` awaits runner async path and passes cancellation tokens through.
- Keep file/token operations sync for now; mark for future async IO extraction.

### Phase 5b: Async Tests & IO Abstractions
- ✅ Add async tests to assert `ExecuteAsync` path is invoked (`RunAsync_UsesExecutorAsyncPath`).
- ✅ Add cancellation test for `RunAsync` (`RunAsync_Cancellation_StopsExecution`).
- 🔜 Async IO: adapt `ReadBatchFromScriptFile` and token replacement to async via abstractions (`IFileSystem`/`IScriptBatcher` async variants), propagate through runner/orchestrator.

### Phase 5c: Async File/Token Operations
- ✅ Added async methods to `IScriptBatcher` and `DefaultScriptBatcher` (`ReadBatchFromScriptFileAsync`, `ReadBatchFromScriptTextAsync`).
- ✅ Added async methods to `IFileSystem` and `DotNetFileSystem` (`ReadAllTextAsync`, `WriteAllTextAsync`, `AppendAllTextAsync`).
- ✅ Added async methods to `ITokenReplacementService` and `DefaultTokenReplacementService` (`ReplaceTokensAsync`).
- ✅ Extended `ISqlBuildRunnerContext` with async `ReadBatchFromScriptFileAsync` and `PerformScriptTokenReplacementAsync` and wired `SqlBuildHelper` implementations.
- ✅ Updated `SqlBuildRunner.RunAsync` to use `LoadBatchScriptsAsync` and async token replacement.
- ✅ Updated test fakes to implement async methods.

### Phase 5d: Async Helper Consumers & Tests
- ✅ Added `SqlBuildHelper.ReadBatchFromScriptFileAsync` static helper.
- ✅ `DefaultScriptBatcherAsyncTests` for `ReadBatchFromScriptFileAsync` using temp file and GO delimiter.
- ✅ `DefaultTokenReplacementService.ReplaceTokensAsync` test added in `SqlBuildHelperTokenReplacementTests`.
- 🔜 Evaluate `SqlBuildFileHelper` async pathways if needed; add async variants where consumption exists.

### Phase 5e: SqlBuildFileHelper Async Variants
- ✅ Added `SqlBuildFileHelper.GetSHA1HashAsync` (tuple return) using async file hash + batching.
- ✅ Added `CopyIndividualScriptsToFolderAsync` and `CopyScriptsToSingleFileAsync` leveraging async batch/file IO.
- ✅ Added `SqlBuildFileHelperAsyncTests` to confirm async hash matches sync path.
- ✅ Tests passing.
- 🔜 Consider async variants for packaging APIs if/when consumers need it.

### Phase 5f: Async Packaging APIs
- ✅ Added `PackageSbxFilesIntoSbmFilesAsync`, `PackageSbxFileIntoSbmFileAsync` overloads.
- ✅ Async packaging tests added (`PackageSbxFileIntoSbmFileAsyncTest_InvalidSbx`, `PackageSbxFilesIntoSbmFilesAsyncTest_EmptyDirectory`).
- ✅ Tests passing.
- 🔜 When actual consumers emerge, consider replacing `Task.Run` wrapping `SaveSqlBuildProjectFile` with truly async ZIP persistence if .NET provides APIs.

### Phase 5g: True Async ZIP Persistence
- ✅ Added `ZipHelper.CreateZipPackageAsync` using async streams.
- ✅ Added `SaveSqlBuildProjectFileAsync` and `PackageProjectFileIntoZipAsync` consuming async zip helper.
- ✅ Updated `PackageSbxFileIntoSbmFileAsync` to use `SaveSqlBuildProjectFileAsync` (no `Task.Run`).
- ✅ Added `SaveSqlBuildProjectFileAsync_CreatesZip` test.
- ✅ Tests passing.
- ✅ Added `ZipHelper.UnpackZipPackageAsync` and `ZipHelper.AppendZipPackageAsync` with async streaming.
- ✅ Added `ZipHelperAsyncTests` covering async unpack/append.
- 🔜 Consider async unzip/append variants if needed (no direct BCL async APIs; can stream manually if required).

### Phase 5h: DefaultSqlBuildFileHelper Consolidation
- ✅ Implemented logic in `DefaultSqlBuildFileHelper` (hash/join logic localized).
- ✅ `SqlBuildFileHelper` static methods delegate to `DefaultFileHelper` for single source of truth.
- ✅ Added `DefaultSqlBuildFileHelperTests` to ensure parity with static helper.
- ✅ Tests passing.

### Phase 6: Deprecate & Clean
- ✅ Marked `SqlBuildHelper.SqlBuildRunnerFactory` `[Obsolete]`.
- ✅ Prior `[Obsolete]` decorations exist on DataSet-based APIs in `SqlBuildFileHelper` and `ClearScriptData`.
- 🔜 Additional `[Obsolete]` markings for any remaining DataSet overloads (review as needed).
- 🔜 Remove/reduce static globals; introduce DI seams for runner factory & logger.
- 🔜 Simplify public surface to POCO models; isolate DataSet usage behind adapters.
- Simplify public surface to POCO models; isolate DataSet usage.

### Phase 7: Hardening & Docs
- ✅ Added `SqlBuildOrchestratorTests` validating timeout retry → `CommittedWithTimeoutRetries`.
- ✅ `dotnet test SqlSync.SqlBuild.UnitTest` passing (84 passed, 3 skipped).
- ⚠️ Warnings remain (nullability + `[Obsolete]` usage in tests); address incrementally.
- 🔜 Add unit tests per extracted service; integration tests with in-memory FS + fake SQL executor.
- 🔜 Document module boundaries and extension points.

### Phase 8: Extract Methods to Default Abstractions
- **Goal:** Move implementation logic from `SqlBuildHelper.cs` into `SqlSync.SqlBuild\Abstractions\Default\*` classes.
- **Scope:** Review each Default class and extract appropriate methods, reducing SqlBuildHelper coupling.
- **Status:** Complete
- ✅ Created `IBuildFinalizerContext` interface to provide context for finalization operations
- ✅ Extracted finalization logic from `SqlBuildHelper.PerformRunScriptFinalization` to `DefaultBuildFinalizer`
- ✅ `SqlBuildHelper` implements `IBuildFinalizerContext` and delegates to injected `BuildFinalizer`
- ✅ All tests passing (84 passed, 3 skipped, 0 failed)
- ✅ Build successful with only pre-existing nullability warnings

**Files Modified:**
- Created: `SqlSync.SqlBuild\Abstractions\IBuildFinalizerContext.cs`
- Modified: `SqlSync.SqlBuild\Abstractions\IBuildFinalizer.cs` (added context parameter)
- Modified: `SqlSync.SqlBuild\Abstractions\Default\DefaultBuildFinalizer.cs` (full finalization implementation)
- Modified: `SqlSync.SqlBuild\SqlBuildHelper.cs` (implements IBuildFinalizerContext, delegates to BuildFinalizer)

**Impact:**
- Finalization logic now isolated in `DefaultBuildFinalizer`, testable independently
- `SqlBuildHelper` reduced by ~120 lines through delegation
- Cleaner separation of concerns: SqlBuildHelper manages state, DefaultBuildFinalizer handles finalization workflow

### Phase 9: Extract Methods to Services Classes
- **Goal:** Move implementation logic from `SqlBuildHelper.cs` into `SqlSync.SqlBuild\Services\*` classes.
- **Scope:** Extract methods from SqlBuildHelper into DefaultBuildPreparationService, DefaultScriptBatcher, DefaultSqlLoggingService, and DefaultTokenReplacementService.
- **Status:** In Progress
- 🔜 Extract batching logic to `DefaultScriptBatcher`
- 🔜 Extract token replacement logic to `DefaultTokenReplacementService`
- 🔜 Extract logging logic to `DefaultSqlLoggingService`
- 🔜 Extract preparation logic to `DefaultBuildPreparationService`
- 🔜 Remove extracted methods from `SqlBuildHelper.cs`
- 🔜 Validate all tests pass

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
- [ ] Phase 5a: async-first runner/orchestrator with async executor.
- [ ] Phase 5b: async IO abstractions for file/token operations.
- [ ] Phase 5c: async IO abstractions usage complete (follow-up: async file IO in helper/services).
- [ ] Phase 5d: extend async pathways to `SqlBuildFileHelper` consumers if/when required.
- [ ] Phase 5e: extend async variants to packaging APIs as needed.
- [ ] Phase 5f: async ZIP persistence once consumers require.

---
*Generated: 2026-01-14*
