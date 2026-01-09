# ADO.NET Modernization Plan

## Goal
Replace remaining `System.Data` (`DataSet`/`DataTable`/`DataRow`/`DataView`) usage with POCO models and mapping helpers. Provide parallel POCO APIs and mark legacy methods `[Obsolete]` as an interim step. Keep behavior and tests green.

## Checklist
- [x] Inventory ADO.NET-dependent methods and entry points
- [x] Add initial POCO equivalents + mark legacy methods `[Obsolete]` (RecordCommittedScripts, SqlBuildFileHelper DataSet overloads)
- [ ] Update console/threaded flows to prefer POCO APIs
- [x] Add/extend tests for POCO pathways (RecordCommittedScripts)
- [x] Run targeted UnitTests (SqlSync.SqlBuild.UnitTest filter)
- [ ] Remove/retire legacy APIs (post-adoption)

## Inventory (initial)
| Area | Member | ADO.NET Types | Notes |
| --- | --- | --- | --- |
| `SqlSync.SqlBuild.SqlBuildHelper` | `ProcessBuild`, `ProcessMultiDbBuild` | `SqlSyncBuildData`, `DataView`, `DataRow` | Core engine uses typed DataSet
| `SqlSync.SqlBuild.SqlBuildHelper` | `RecordCommittedScripts(List<SqlLogging.CommittedScript>)` | `SqlSyncBuildData` | Writes CommittedScript rows
| `SqlSync.SqlBuild.SqlBuildHelper` | `PrepareBuildForRun`, `RunBuildScripts`, `ClearScriptBlocks` | `SqlSyncBuildData`, `DataView` | Execution flow
| `SqlSync.SqlBuild.SqlBuildHelper` | `buildData` fields/events | `SqlSyncBuildData` | State storage
| `SqlSync.SqlBuild.SqlBuildFileHelper` | `LoadSqlBuildProjectFile`, `CreateShellSqlSyncBuildDataObject`, `SaveSqlBuildProjectFile`, `AddScriptFileToBuild`, `PackageProjectFileIntoZip`, `CleanProjectFileForRemoteExecution` | `SqlSyncBuildData` | Legacy file IO
| `SqlSync.SqlBuild.Utility` | `ServerConnectConfig` converters | `ServerConnectConfig` DataSet | Legacy config persistence
| `SqlBuildManager.Console` | `Worker.Utility` packaging flows | `SqlSyncBuildData` | Create sbm packages
| `SqlBuildManager.Console` | `ThreadedManager`, `ThreadedRunner` | `SqlSyncBuildData` | Threaded execution

## Plan
1. **Inventory** (this doc) ✅ ongoing
2. **Mark legacy methods obsolete** and provide POCO overloads:
   - `SqlBuildHelper.RecordCommittedScripts(List<SqlLogging.CommittedScript>)`
   - `SqlBuildHelper.ProcessBuild/ProcessMultiDbBuild` (POCO entry)
   - `SqlBuildHelper.ClearScriptBlocks` (POCO)
   - `SqlBuildFileHelper.*` (POCO overloads exist; mark DataSet APIs `[Obsolete]`)
3. **Update console & threaded flows** to call POCO API first; convert to DataSet internally only where necessary (engine still DataSet).
4. **Add tests**:
   - `SqlSync.SqlBuild.UnitTest` for POCO `RecordCommittedScripts`
   - `SqlSync.SqlBuild.Dependent.UnitTest` targeted tests for POCO entry points (skip external dependencies)
5. **Run targeted tests** `dotnet test ... --filter` to validate new methods.
6. **Iterate**: migrate engine internals from DataSet to POCO (future), then remove legacy APIs.

## Progress Log
- 2026-01-09 — Added POCO `RecordCommittedScripts` overload; marked legacy DataSet APIs obsolete; added targeted test `RecordCommittedScripts_PocoModel_AppendsCommittedScript`.
- 2026-01-09 — Plan drafted; inventory seeded.

## Next Actions
- Complete inventory for `SqlBuildHelper` and `SqlBuildFileHelper` methods.
- Add POCO overload `RecordCommittedScripts(List<Models.CommittedScript>)` and mark legacy `[Obsolete]`.
- Update console flows to call POCO `SqlBuildRunDataModel` (existing) as entry; convert to DataSet internally.
- Add targeted unit tests; run with filters.
