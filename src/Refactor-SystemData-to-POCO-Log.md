# Refactor System.Data to POCO – Work Log

## Goals
- Replace `DataSet`/`DataTable`/`DataRow`/`DataColumn` (including typed variants) with strongly typed POCO models and collections.
- Apply changes consistently across all projects.
- Preserve behavior; validate with unit tests and mappings.
- Maintain incremental, low-risk refactors with clear logging.

## Global Decisions
- **Naming**: Use singular POCO names aligned with table/row concepts (e.g., `Script`, `Build`, `ScriptRun`, `ServerConfiguration`). Keep names close to existing typed-row property names to reduce friction.
- **Nullability**: Map `AllowDBNull = false` to non-nullable properties; else nullable (`string?`, `int?`, `DateTime?`).
- **Type choices**: Preserve `DateTime` where used; prefer `int`, `bool`, `double` as per columns; use `Guid` where columns store GUIDs.
- **Shapes**: Prefer `record class` with init-only setters for value-like models; use immutable collections exposure (`IReadOnlyList<T>`), backed by `List<T>`.
- **Mapping**: Provide extension methods per module to map `DataRow`/typed rows to POCOs and `DataTable` to `List<T>`. Keep interim mappers to minimize breaking changes.
- **APIs**: Favor returning `IEnumerable<T>`/`List<T>`/single POCO instead of `DataSet`/`DataTable`. Where breaking changes are risky, add overloads/shims.

## Plan
1. Establish global decisions (naming, nullability, immutability) and mapping helpers (DataRow/DataTable→POCO).
2. Refactor `SqlSyncBuildData` typed dataset to POCOs and mappers; update consumers.
3. Refactor `ServerConnectConfig` typed dataset to POCOs and mappers; update consumers.
4. Refactor `AutoScriptingConfig` typed dataset to POCOs and mappers; update consumers.
5. Refactor remaining raw DataTable usages (`SizeAnalysis`, `ServerSizeSummary`, `LookUpTables`, `ScriptRunLog`, etc.).
6. Update tests and add mapping coverage; run relevant test suites incrementally.

## Task List
| Task ID | Area / Module | Description | Status | Notes |
|---------|----------------|-------------|--------|-------|
| 1 | Analysis | Inventory System.Data usages | Done | `SQLSyncBuildProject.Designer.cs` (288), `SQLSyncBuildProject.cs` (160), tests & helpers key hotspots |
| 2 | Analysis | Find typed DataSet/Table/Row classes | Done | `SqlSyncBuildData`, `ServerConnectConfig`, `AutoScriptingConfig` typed datasets & rows |
| 3 | Logging | Create and initialize refactor log | Done | This file |
| 4 | Reporting | Summarize analysis in chat and log | Done |  |
| 5 | Planning | Draft plan & tasks in log | Done |  |
| 6 | Design | Document global design decisions | Done |  |
| 7 | Execution | Select first module to refactor | Done |  |
| 8 | Design | Define mapping helpers (DataRow/DataTable→POCO) | Done | `ServerConnectConfigMappers` created |
| 9 | SqlSync.SqlBuild | Replace `SqlSyncBuildData` typed dataset with POCOs | Planned | Tables: SqlSyncBuildProject, Scripts, Script, Builds, Build, ScriptRun, CommittedScript, CodeReview |
| 10 | SqlSync.SqlBuild | Replace `ServerConnectConfig` typed dataset with POCOs | Done | POCOs + mappers + Utility updated; UI module removed |
| 11 | SqlSync.ObjectScript | Replace `AutoScriptingConfig` typed dataset with POCOs | Planned | Tables: AutoScripting, DatabaseScriptConfig, PostScriptingAction |
| 12 | SqlSync.DbInformation | Replace `SizeAnalysis`, `ServerSizeSummary` DataTables | Planned |  |
| 14 | Tests | Update console tests using DataTables/DataSets | Planned | Exclude *Dependent*/*External*; focus on UnitTest |

## Progress Updates
- 2026-01-08 00:40 UTC — Added `ServerConnectConfigModel` POCOs, mappers, and persistence helpers; updated `UtilityHelper` to prefer POCOs and added overloads.
- 2026-01-08 00:45 UTC — Updated `SettingsControl` & `SQLConnect` to use POCO-based APIs; legacy dataset API left as shim.
- 2026-01-08 01:00 UTC — `dotnet test SqlSync.SqlBuild.Dependent.UnitTest` (partial): warnings CS8632 (nullable) and MSTEST warnings; run canceled after ~300s (long-running tests). Added `#nullable enable` to models to resolve CS8632.
- 2026-01-08 01:30 UTC — User removed `SqlSync` UI module; will not modify that module further.
- 2026-01-08 01:35 UTC — Added `SqlSyncBuildDataModel` POCOs and `SqlSyncBuildDataMappers` for all typed tables.
- 2026-01-08 01:45 UTC — `dotnet test SqlSync.SqlBuild.UnitTest` succeeded (53 tests, 0 failures). Excluded Dependent/External suites.
- 2026-01-08 00:20 UTC — Decisions confirmed: **record class** preference, keep names unchanged, stick with `DateTime`.
- 2026-01-08 00:20 UTC — Selected **ServerConnectConfig** as first module; mapping helpers in progress.
- 2026-01-08 00:00 UTC — Initialized work log; scanned repository for `System.Data` usages. Top offenders: `SqlSync.SqlBuild/SQLSyncBuildProject.Designer.cs`, `SqlSync/SQLSyncBuildProject.cs`, `SqlBuildManager.Console.ExternalTest/BatchTests.cs`, `SqlSync.ObjectScript/AutoScriptingConfig.Designer.cs`, `SqlSync.SqlBuild/ScriptRunLog.cs`, `SqlSync.SqlBuild/ServerConnectConfig.Designer.cs`, `SqlSync.DbInformation/SizeAnalysis.cs`, `SqlSync/LookUpTables.cs`.
- 2026-01-08 00:00 UTC — Identified typed datasets: `SqlSyncBuildData` (tables: SqlSyncBuildProject, Scripts, Script, Builds, Build, ScriptRun, CommittedScript, CodeReview) in `SqlSync.SqlBuild/SQLSyncBuildProject.Designer.cs`; `ServerConnectConfig` in `SqlSync.SqlBuild/ServerConnectConfig.Designer.cs`; `AutoScriptingConfig` in `SqlSync.ObjectScript/AutoScriptingConfig.Designer.cs`.
- 2026-01-08 00:10 UTC — Published initial plan & tasks; documented global decisions (naming, nullability, mapping strategy) and requested confirmation.
