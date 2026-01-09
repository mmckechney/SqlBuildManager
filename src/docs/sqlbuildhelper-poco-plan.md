# SqlBuildHelper POCO Migration Plan

## Goals
- Remove active dependencies on legacy ADO.NET DataSet types in `SqlBuildHelper`.
- Ensure active fields use `SqlSync.SqlBuild.Models.SqlSyncBuildDataModel` (POCO).
- Refactor `myRunRow` usage to POCO or minimal temporary DataSet conversions.
- Update interconnected classes/tests.
- Add/validate unit tests.

## Tasks
1. Audit `SqlBuildHelper` for legacy fields/members (`buildData`, `myRunRow`, `Script*Row` usage).
2. Introduce `buildDataModel` field; ensure active methods use POCO.
3. Replace `myRunRow` with POCO-friendly state (e.g., track by `ScriptRunId` / index) and adapt logging helpers.
4. Update callers to set `BuildDataModel` instead of `BuildData` where applicable.
5. Update/add unit tests covering run lifecycle, script batching, logging, and committed scripts.
6. Run targeted tests (`SqlSync.SqlBuild.UnitTest`, `SqlBuildManager.Console.UnitTest`).

## Notes
- Keep legacy DataSet APIs `[Obsolete]` for compatibility; convert from POCO on demand when required.
- Use existing mappers (`SqlSyncBuildDataMappers`) for conversions.

## Status
- ✅ `myRunRow` replaced with POCO `ScriptRun`; history helper writes to model + DataSet.
- ✅ `BuildHistoryModel` exposed (internal) and synced from persisted history.
- ✅ Unit test: `BuildHistoryModelTest.AddScriptRunToHistory_AppendsToModelAndDataSet`.
- ✅ `buildData` replaced with `SqlSyncBuildDataModel` inside `SqlBuildHelper`; ephemeral DataSet derived via helpers.
- ✅ Unit test: `RecordCommittedScriptsObsoleteTest` ensures obsolete path updates model.
