# Dataset to POCO Migration Plan

## Goal
Replace legacy `System.Data` DataSet/DataTable/DataRow/DataColumn (including typed datasets) with strongly typed POCO models and mapping helpers, preserving behavior and keeping tests passing.

## Checklist
- [x] SqlSync.SqlBuild — `SqlSyncBuildDataModel` POCOs, mappers, `SqlBuildFileHelper`/`SqlBuildHelper` model APIs
- [x] ServerConnectConfig — POCOs, mappers, persistence helpers; UI module removed
- [x] AutoScriptingConfig — POCOs, mappers, persistence helpers
- [x] DbInformation — `SizeAnalysis` / `ServerSizeSummary` POCOs & mappers
- [x] Console adoption — model tests added; `ApplicationIcon` conditional
- [x] Unit tests updated — *only* UnitTest projects (Dependent/External excluded)
- [ ] Replace DataSet/XML persistence with POCO serialization (schema-compatible)
- [ ] Delete typed DataSet designer files (.Designer.cs/.xsd)
- [ ] Clean up Dependent/External tests or retire them
- [ ] Clean MSTest warnings (`TestContext` analyzer warnings)
- [ ] Optional: remove typed DataSet designer files if no longer required

## Next
- Sweep warnings: MSTEST0005 (avoid inheritance); nullable annotations in tests.
- Track legacy dataset call sites; plan staged removal once external contracts migrated.

## Milestones
1. **SqlSyncBuildData**
   - **Components**: `SqlSyncBuildDataModel` POCOs, mappers, `SqlBuildFileHelper`/`SqlBuildHelper` model APIs.
   - **Tests**: `dotnet test SqlSync.SqlBuild.UnitTest/SqlSync.SqlBuild.UnitTest.csproj`
   - **Notes**: Typed dataset retained for mapping/persistence; prefer model APIs for consumers.
   - **Suggested commit**: `refactor: add SqlSyncBuildData POCOs and model APIs`
2. **ServerConnectConfig**
   - **Components**: `ServerConnectConfigModels`, `ServerConnectConfigMappers`, `ServerConnectConfigPersistence`, `UtilityHelper` overloads; UI consumers updated.
   - **Tests**: Covered via SqlSync.SqlBuild.UnitTest
   - **Suggested commit**: `refactor: add ServerConnectConfig POCOs and mappers`

3. **AutoScriptingConfig**
   - **Components**: `AutoScriptingConfigModels`, `AutoScriptingConfigMappers`, `AutoScriptingConfigPersistence`.
   - **Tests**: `dotnet test SqlSync.ObjectScript.UnitTest/SqlSync.ObjectScript.UnitTest.csproj`
   - **Suggested commit**: `refactor: add AutoScriptingConfig POCOs and mappers`

4. **DbInformation (SizeAnalysis/ServerSizeSummary)**
   - **Components**: `SizeAnalysisModels`, `SizeAnalysisMappers`.
   - **Tests**: `dotnet test SqlSync.DbInformation.UnitTest/SqlSync.DbInformation.UnitTest.csproj`
   - **Suggested commit**: `refactor: add DbInformation POCOs and mappers`

5. **Console Adoption & Tests**
   - **Components**: Console UnitTests updated; model test added; `ApplicationIcon` conditional.
   - **Tests**: `dotnet test SqlBuildManager.Console.UnitTest/SqlBuildManager.Console.UnitTest.csproj`
   - **Suggested commit**: `test: add console model tests & fix icon dependency`

6. **SqlSyncBuildData XML Persistence (POCO-only)**
   - **Goal**: Eliminate `SqlSyncBuildData` DataSet usage; implement POCO XML serialization compatible with existing `.sbm` format.
   - **Components**: Replace `ReadXml`/`WriteXml` with custom serializer; update `SqlBuildFileHelper` consumers.
   - **Tests**: Extend `SqlSync.SqlBuild.UnitTest` with round-trip tests using the new serializer and legacy fixtures.
   - **Risk**: `.sbm` file format compatibility (external contract) — needs confirmation.
   - **Suggested commit**: `refactor: add POCO serializer for SqlSyncBuildData`

7. **ServerConnectConfig & AutoScriptingConfig Persistence (POCO-only)**
   - **Goal**: Replace DataSet-based XML load/save with POCO XML (or JSON) while reading legacy XML without `DataSet`.
   - **Components**: Implement `XDocument`/`XmlSerializer` loader; drop DataSet types.
   - **Tests**: Add fixtures to `SqlSync.SqlBuild.UnitTest` and `SqlSync.ObjectScript.UnitTest` for legacy XML import and POCO export.
   - **Suggested commit**: `refactor: persist ServerConnectConfig/AutoScriptingConfig via POCO`

8. **ScriptRunLog / SizeAnalysis / ServerSizeSummary**
   - ✅ **Done**: Replaced DataTable-derived classes with POCO collections/models.
   - **Components**: `ScriptRunLogEntry` model & reader mappers; `SizeAnalysisModel` & `ServerSizeInfo` lists; InfoHelper returns models.
   - **Tests**: Updated `SqlSync.SqlBuild.UnitTest` & `SqlSync.DbInformation.UnitTest`.
   - **Suggested commit**: `refactor: remove DataTable-derived log/info tables`

9. **Dependent/External Tests**
   - **Goal**: Remove DataSet references; adapt to POCO or retire tests if obsolete.
   - **Components**: Update test helpers; ensure no DataSet usage remains.
   - **Suggested commit**: `test: retire DataSet dependencies in external suites`

10. **Delete Legacy ADO.NET Assets**
    - **Goal**: Remove `.Designer.cs`, `.xsd`, and any DataSet-derived classes once unused.
    - **Components**: Update csproj to remove includes; delete files.
    - **Tests**: Run full UnitTest suite.
    - **Suggested commit**: `chore: remove legacy datasets`

## Tests Matrix
| Project | Command | Result |
|--------|---------|--------|
| SqlSync.SqlBuild.UnitTest | `dotnet test SqlSync.SqlBuild.UnitTest/SqlSync.SqlBuild.UnitTest.csproj -v minimal` | ✅ 56 tests |
| SqlSync.ObjectScript.UnitTest | `dotnet test SqlSync.ObjectScript.UnitTest/SqlSync.ObjectScript.UnitTest.csproj -v minimal` | ✅ 8 tests |
| SqlSync.DbInformation.UnitTest | `dotnet test SqlSync.DbInformation.UnitTest/SqlSync.DbInformation.UnitTest.csproj -v minimal` | ✅ 1 test |
| SqlBuildManager.Console.UnitTest | `dotnet test SqlBuildManager.Console.UnitTest/SqlBuildManager.Console.UnitTest.csproj -v minimal` | ✅ 91/93 (2 skipped) |

> **Excluded**: *Dependent* / *External* suites (require external resources)

## Known Exceptions
- Typed dataset designers retained for backward compatibility & runtime features:
   - `SqlSync.SqlBuild/SQLSyncBuildProject.Designer.cs`
   - `SqlSync.SqlBuild/ServerConnectConfig.Designer.cs`
   - `SqlSync.ObjectScript/AutoScriptingConfig.Designer.cs`
- **Removed**: `SqlSync.DbInformation/SizeAnalysis.cs`, `ServerSizeSummary.cs`, `SqlSync.SqlBuild/ScriptRunLog.cs`
- **Note**: POCO serializers are default; typed DataSets are kept for legacy APIs and tests.

## Legacy Dataset Call Sites (2026-01-09)
- `SqlSync.SqlBuild.SqlBuildFileHelper` — legacy XML load/save (`ReadXml`/`WriteXml`) and shell creation.
- `SqlSync.SqlBuild.Utility` — `ServerConnectConfig` DataSet for legacy conversions.
- `SqlSync.ObjectScript.UnitTest.AutoScriptingConfigModelTest` — compatibility test (`new AutoScriptingConfig()`).
- `SqlSync.SqlBuild.Dependent.UnitTest` — legacy DataSet tests (excluded from CI).

## Follow-Ups / Technical Debt
- Resolve MSTest `TestContext` analyzer warnings.
- Consider removing typed datasets entirely if no external contract depends on them.
- Add CI filters to skip Dependent/External projects.

## Log (Recent)
- 2026-01-09 — Designers restored for compatibility; all unit tests passing (SqlBuild/ObjectScript/DbInformation).
- 2026-01-08 — ScriptRunLog -> POCO entries; DbInformation DataTables removed; all unit tests passing.
- 2026-01-08 — All POCO migrations complete; model APIs added; UnitTests passing; Dependent/External excluded.
- 2026-01-08 — Console UnitTest passing; icon path conditional.
- 2026-01-08 — DbInformation POCO/mappers added; DbInformation UnitTest passing.
- 2026-01-08 — AutoScripting POCO/mappers/persistence added; ObjectScript UnitTest passing.
- 2026-01-08 — ServerConnectConfig POCO/mappers/persistence added; Utility overloads; UI module removed.
- 2026-01-08 — SqlSyncBuildData POCO/mappers; model APIs in helpers; SqlBuild UnitTests passing.
