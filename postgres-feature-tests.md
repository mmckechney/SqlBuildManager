# PostgreSQL Feature — Test Strategy Analysis

## Overview

This document analyzes the existing test projects to determine how they should be adapted to support PostgreSQL testing alongside the existing SQL Server tests. It evaluates each project for SQL Server-specific dependencies and recommends whether to integrate PostgreSQL tests into existing projects or create parallel test projects.

---

## Test Project Inventory

| Project | Test Count | Type | SQL Server Dependency |
|---------|-----------|------|----------------------|
| SqlSync.SqlBuild.UnitTest | ~1,173 | Pure unit | None — mocks/models only |
| SqlBuildManager.Console.UnitTest | ~65 | Pure unit | None — CLI parsing only |
| SqlSync.Connection.UnitTest | ~70 | Pure unit | None — model validation only |
| SqlSync.ObjectScript.UnitTest | ~43 | Pure unit | None — XML/sanitization only |
| SqlSync.DbInformation.UnitTest | ~73 | Pure unit | None — models only |
| SqlBuildManager.Enterprise.UnitTest | ~8 | Mixed | SQL in resources (policy checks) |
| SqlBuildManager.ScriptHandling.UnitTest | ~24 | Pure unit | None |
| SqlSync.SqlBuild.Dependent.UnitTest | ~62 | Integration | **Heavy** — live SQL Server |
| SqlBuildManager.Console.Dependent.UnitTest | ~21+ | Integration | **Heavy** — live SQL Server |
| SqlSync.ObjectScript.Dependent.UnitTest | 0 (stubs) | Integration | **Heavy** — SMO/scripting |
| SqlSync.DbInformation.Dependent.UnitTest | 0 (stubs) | Integration | **Heavy** — metadata queries |
| SqlSync.Connection.Dependent.UnitTest | ~2 | Integration | **Moderate** — connection tests |
| SqlBuildManager.Console.ExternalTest | ~62 | Cloud integration | **Heavy** — Azure + SQL Server |

---

## Analysis by Project

### 1. Pure Unit Test Projects (No Changes Needed)

These projects test models, parsing, and logic with no database interaction:

- **SqlSync.SqlBuild.UnitTest** — Tests services via mocking (DefaultScriptBatcher, DefaultBuildFinalizer, etc.). These tests will continue to work as-is since they mock `IConnectionsService`, `ISqlCommandExecutor`, etc.
- **SqlBuildManager.Console.UnitTest** — Tests CLI parsing, command routing, settings serialization. Adding a `--platform` option will require ~2-3 new tests for parsing validation.
- **SqlSync.Connection.UnitTest** — Tests `ConnectionData`, `DatabaseOverride`, extensions. Add tests for `DatabasePlatform` enum and new factory resolution.
- **SqlSync.ObjectScript.UnitTest** — Tests XML serialization, sanitization, backout scripts. No PostgreSQL impact.
- **SqlSync.DbInformation.UnitTest** — Tests models only. No impact.
- **SqlBuildManager.ScriptHandling.UnitTest** — Tests script wrapping, tag processing. No impact.

**Recommendation**: **Integrate** new `DatabasePlatform`-related unit tests directly into these existing projects. No separate projects needed.

#### Specific additions needed:
- `SqlSync.Connection.UnitTest`: Tests for `PostgresConnectionFactory.BuildConnectionString()`, `DatabasePlatform` enum serialization, `ConnectionData.DatabasePlatform` property
- `SqlBuildManager.Console.UnitTest`: Tests for `--platform` option parsing (SqlServer, PostgreSQL, invalid values)
- `SqlSync.SqlBuild.UnitTest`: Tests for `PostgresSyntaxProvider`, `PostgresResourceProvider`, `PostgresTransactionManager` (mocked)

---

### 2. SqlBuildManager.Enterprise.UnitTest (Minor Changes)

This project tests script policies with embedded SQL scripts as test data. Current policies include:
- `WithNoLockPolicy` — validates `WITH (NOLOCK)` presence (SQL Server-specific)
- `QualifiedNamesPolicy` — validates `[schema].[object]` quoting (SQL Server syntax)
- `GrantExecutePolicy`, `SelectStarPolicy`, etc. — more generic

**SQL Server-Specific Resources**:
- `ScriptResources/QualifiedNamesPolicy*.sql` — uses `[dbo].[TableName]` bracket syntax
- `ScriptResources/GrantExecuteToPublicPolicy*.sql` — uses `GRANT EXECUTE TO PUBLIC`
- Policy check logic is syntax-pattern matching (regex), not database execution

**Recommendation**: **Integrate** into existing project.
- Add PostgreSQL-equivalent test scripts for platform-specific policies
- Add tests verifying policies skip when `ApplicablePlatforms` doesn't include PostgreSQL
- No need for a separate project since these are pure unit tests with string matching

---

### 3. SqlSync.SqlBuild.Dependent.UnitTest (Significant Changes)

This is the most impacted integration test project. It connects to a live SQL Server and executes real scripts.

#### Test Categories and PostgreSQL Compatibility:

| Test Class | Tests | SQL Server-Specific? | PostgreSQL Strategy |
|-----------|-------|---------------------|-------------------|
| **SqlBuildHelperTest** | 24 | **Yes** — executes T-SQL INSERT/SELECT, uses SqlBuild_Logging table | Needs parallel PostgreSQL tests with PL/pgSQL scripts |
| **DacPacHelperTest** | 19 | **Yes** — DacPac extraction/comparison is SQL Server-only | Skip for PostgreSQL (DacPac N/A) |
| **QueryCollectorTest** | 14 | **Yes** — T-SQL queries, master DB reference, SQL Server syntax | Needs PostgreSQL versions with pg_catalog queries |
| **QueryCollectionRunnerTest** | 15 | **Yes** — uses masterConnData, SQL Server ad-hoc queries | Needs PostgreSQL versions |
| **RebuilderTest** | 11 | **Yes** — rebuilds SqlBuild_Logging, T-SQL DDL | Needs PostgreSQL DDL versions |
| **DatabaseDifferTest** | 3 | **Yes** — schema comparison via ConnectionData | Needs PostgreSQL versions |
| **DatabaseSyncerTest** | 3 | **Yes** — database sync operations | Needs PostgreSQL versions |

#### Embedded SQL Resources (All SQL Server-Specific):

| Resource | Content | PostgreSQL Equivalent |
|----------|---------|----------------------|
| `CreateDatabaseScript.sql` | `CREATE DATABASE` with filegroups, recovery model | `CREATE DATABASE` (simpler, no filegroups) |
| `CreateTestTablesScript.sql` | `CREATE TABLE` with `IDENTITY`, `DATETIME`, `NVARCHAR` | Needs PostgreSQL DDL (`SERIAL`, `TIMESTAMP`, `VARCHAR`) |
| `LoggingTable.sql` | SqlBuild_Logging DDL with `dbo` schema, `NONCLUSTERED` indexes | Needs PostgreSQL-compatible DDL |
| `CleanLoggingTable.sql` | Uses `sys.objects` to check existence | Use `pg_class` or `information_schema` |
| `CleanTestTable.sql` | T-SQL DELETE statements | Compatible (standard SQL) |
| `InsertPreRunScriptLogEntryScript.sql` | T-SQL INSERT with `GETDATE()` | Use `NOW()` or `CURRENT_TIMESTAMP` |
| `SyncScriptRaw.sql` | T-SQL script for testing | Needs PostgreSQL equivalent |
| `TableLockingScript.sql` | T-SQL locking test | Needs PostgreSQL equivalent (`SELECT ... FOR UPDATE`) |

#### Initialization.cs (Heavy SQL Server Dependency):

The `Initialization` class:
- Creates databases via `CREATE DATABASE` T-SQL (SQL Server-specific syntax with file paths, recovery model)
- Creates test tables with `IDENTITY`, `DATETIME`, `NVARCHAR(MAX)` data types
- Uses `SqlConnection`/`SqlCommand` directly for setup
- Manages SqlBuild_Logging table creation

**For PostgreSQL**: The initialization would need:
- PostgreSQL-compatible `CREATE DATABASE` (simpler — no filegroups, no SIMPLE recovery)
- PostgreSQL data types: `SERIAL` → `IDENTITY`, `TIMESTAMP` → `DATETIME`, `TEXT` → `NVARCHAR(MAX)`
- Use `NpgsqlConnection`/`NpgsqlCommand` (or `DbConnection`/`DbCommand` via factory)
- Different database file path handling (PostgreSQL manages its own storage)

**Recommendation**: **Create a new project** — `SqlSync.SqlBuild.Dependent.PostgreSQL.UnitTest`

Rationale:
1. The embedded SQL resource files are fundamentally different between platforms
2. The `Initialization.cs` class requires a completely different implementation for database/table creation
3. Some test classes (DacPacHelperTest) are entirely SQL Server-only
4. Mixing both platforms in one project would create confusing conditional compilation
5. Test resources (.sql files) need platform-specific versions that would clutter a shared project

However, consider a **shared base class** approach:
```
SqlSync.SqlBuild.Dependent.UnitTest.Common/
    SqlBuildHelperTestBase.cs    — shared test logic (assertions, flow)
    Initialization base           — shared init interface/pattern

SqlSync.SqlBuild.Dependent.UnitTest/           — SQL Server (existing)
    SqlBuildHelperTest.cs          — inherits base, SQL Server resources
    Initialization.cs              — SQL Server setup

SqlSync.SqlBuild.Dependent.PostgreSQL.UnitTest/  — PostgreSQL (new)
    SqlBuildHelperTest.cs          — inherits base, PostgreSQL resources
    Initialization.cs              — PostgreSQL setup
```

---

### 4. SqlBuildManager.Console.Dependent.UnitTest (Significant Changes)

#### Test Classes and PostgreSQL Compatibility:

| Test Class | Tests | SQL Server-Specific? | PostgreSQL Strategy |
|-----------|-------|---------------------|-------------------|
| **ThreadedExecutionTest** | 15 | **Yes** — executes T-SQL builds against SQL Server via CLI args | Needs PostgreSQL versions with different scripts/overrides |
| **LocalBuildTest** | 6 | **Yes** — end-to-end builds against SQL Server | Needs PostgreSQL versions |
| **CommandLineParsingTest** | 3 | **No** — pure CLI parsing | Can remain shared |

#### Key SQL Server Dependencies:
- CLI args include `--server "localhost\SQLEXPRESS"` (SQL Server instance naming)
- `.sbm` package files contain T-SQL scripts
- `.cfg` override files reference SQL Server databases
- `Initialization.cs` builds connection strings with SQL Server syntax

#### Embedded Resources:
- `.sbm` files (`InsertForThreadedTest.sbm`, `SimpleSelect.sbm`, etc.) — contain T-SQL INSERT/SELECT statements
- These `.sbm` packages would need PostgreSQL-compatible SQL script equivalents
- `.cfg` files (dbconfig) — database override configurations that reference SQL Server databases

**Recommendation**: **Create a new project** — `SqlBuildManager.Console.Dependent.PostgreSQL.UnitTest`

Rationale:
1. The `.sbm` package files contain platform-specific SQL and cannot be shared
2. The test infrastructure (Initialization, config files) is SQL Server-specific
3. `CommandLineParsingTest` can remain in the existing project (it's platform-agnostic)
4. PostgreSQL tests need different `.sbm` packages with PostgreSQL-compatible scripts
5. Server naming differs (`localhost\SQLEXPRESS` vs `localhost:5432`)

---

### 5. SqlBuildManager.Console.ExternalTest (Moderate Changes)

This project tests cloud infrastructure deployments (Azure Batch, ACI, Kubernetes, Container Apps).

#### Analysis:
- **DatabaseHelper.cs** — Creates random tables with T-SQL DDL (`CREATE TABLE`, `DROP TABLE`)
- **Test scripts** — `.sbm` packages with T-SQL (SELECT, INSERT, UPDATE, DELETE)
- **Cloud infrastructure** — Azure-specific (Batch, ACI, K8s, Container Apps)
- **SQL connection** — Uses Azure AD Default authentication to Azure SQL

**For PostgreSQL**: These tests would target Azure Database for PostgreSQL or a PostgreSQL container.

**Recommendation**: **Create a new project** — `SqlBuildManager.Console.PostgreSQL.ExternalTest`

Rationale:
1. Cloud infrastructure tests would target Azure Database for PostgreSQL instead of Azure SQL
2. Database helper needs PostgreSQL DDL
3. Test scripts need PostgreSQL-compatible SQL
4. The ACI/K8s/Container Apps deployment infrastructure can be shared (same containers, different DB target)
5. A separate project allows independent CI/CD pipelines for each database platform

---

### 6. SqlSync.Connection.Dependent.UnitTest (Minor Changes)

Currently has 2 test methods:
- `TestDatabaseConnectionTest_StringOverrideSuccess` — Tests connection to SQL Server
- `TestDatabaseConnectionTest1_ConnDataOverrideSuccess` — Tests connection via ConnectionData

These test actual database connectivity. 

**Recommendation**: **Integrate** PostgreSQL connection tests into the existing project.

Rationale:
1. Only 2 existing tests
2. Connection testing is simple (connect, verify, disconnect)
3. Can use `[DataRow]` or `[DynamicData]` to parameterize by platform
4. Add `TestPostgreSqlConnection_Succeeds` tests alongside existing SQL Server tests
5. Use environment variables (`SBM_TEST_PG_SERVER`, `SBM_TEST_PG_USER`, etc.) for PostgreSQL config

---

### 7. SqlSync.ObjectScript.Dependent.UnitTest & SqlSync.DbInformation.Dependent.UnitTest (Future)

Both currently have 0 test methods (stubs only). These would be for:
- **ObjectScript**: SMO-based scripting (SQL Server-only)
- **DbInformation**: Database metadata queries

**Recommendation**: 
- **ObjectScript**: Keep SQL Server-only. PostgreSQL object scripting would be a separate concern using `pg_dump` or catalog queries — create `SqlSync.ObjectScript.PostgreSQL.Dependent.UnitTest` when that feature is built.
- **DbInformation**: Once `IMetadataProvider` is implemented, add PostgreSQL tests. Can integrate into existing project since the test structure would be similar (just different expected results).

---

## Recommendation Summary

### Integrate into Existing Projects (Add PostgreSQL tests alongside SQL Server)

| Project | Reason |
|---------|--------|
| SqlSync.SqlBuild.UnitTest | Pure mocks; just add tests for new PostgreSQL service implementations |
| SqlBuildManager.Console.UnitTest | Just CLI parsing; add `--platform` option tests |
| SqlSync.Connection.UnitTest | Model tests; add `DatabasePlatform`, factory tests |
| SqlSync.Connection.Dependent.UnitTest | Simple connectivity; parameterize by platform |
| SqlBuildManager.Enterprise.UnitTest | String-matching policies; add PostgreSQL script samples |
| SqlBuildManager.ScriptHandling.UnitTest | No database impact |
| SqlSync.ObjectScript.UnitTest | No database impact |
| SqlSync.DbInformation.UnitTest | No database impact |

### Create New Parallel Projects

| New Project | Based On | Reason |
|------------|---------|--------|
| `SqlSync.SqlBuild.Dependent.PostgreSQL.UnitTest` | SqlSync.SqlBuild.Dependent.UnitTest | Different SQL resources, DDL, initialization, DacPac N/A |
| `SqlBuildManager.Console.Dependent.PostgreSQL.UnitTest` | SqlBuildManager.Console.Dependent.UnitTest | Different `.sbm` packages, server naming, connection config |
| `SqlBuildManager.Console.PostgreSQL.ExternalTest` | SqlBuildManager.Console.ExternalTest | Different cloud DB target, different SQL scripts |

### Skip / Defer

| Project | Reason |
|---------|--------|
| SqlSync.ObjectScript.Dependent.UnitTest | SMO is SQL Server-only; PostgreSQL scripting is a separate future feature |

---

## Shared Test Infrastructure

To avoid duplicating test logic between SQL Server and PostgreSQL projects, consider creating shared components:

### Option A: Shared Base Class Library

Create `SqlSync.TestCommon` project:
```
SqlSync.TestCommon/
    ITestInitialization.cs           — Interface for database setup/teardown
    TestConnectionHelper.cs          — Resolves connection from env vars + platform
    SqlBuildHelperTestBase.cs        — Shared assertion patterns
    ThreadedExecutionTestBase.cs     — Shared threaded test patterns
```

Both `SqlSync.SqlBuild.Dependent.UnitTest` and `SqlSync.SqlBuild.Dependent.PostgreSQL.UnitTest` would reference this shared project and provide platform-specific implementations.

### Option B: Conditional Test Execution

Instead of separate projects, use `[TestCategory]` and build filters:
```csharp
[TestMethod]
[TestCategory("SqlServer")]
public async Task RunBuild_SqlServer() { ... }

[TestMethod]
[TestCategory("PostgreSQL")]
public async Task RunBuild_PostgreSQL() { ... }
```

Run with: `dotnet test --filter TestCategory=PostgreSQL`

**Analysis**: Option B reduces project count but creates large, mixed test files. Option A provides cleaner separation, independent CI/CD, and easier debugging. **Option A is recommended** given the significant differences in SQL resources and initialization.

---

## Environment Variables for PostgreSQL Tests

Add new environment variables parallel to existing SQL Server ones:

| Variable | Purpose | Default |
|----------|---------|---------|
| `SBM_TEST_PG_SERVER` | PostgreSQL server hostname | `localhost` |
| `SBM_TEST_PG_PORT` | PostgreSQL server port | `5432` |
| `SBM_TEST_PG_USER` | PostgreSQL username | `postgres` |
| `SBM_TEST_PG_PASSWORD` | PostgreSQL password | (none) |
| `SBM_TEST_PG_DATABASE` | Test database name | `sbm_test` |

---

## ACI Test Runner for PostgreSQL

The existing `run_dependent_tests_in_aci.ps1` script deploys:
1. SQL Server 2022 sidecar container
2. Test runner container

For PostgreSQL, create `run_dependent_tests_in_aci_postgres.ps1` with:
1. **PostgreSQL 16 sidecar** (`postgres:16-alpine`) instead of SQL Server
2. **Test runner** building from `Dockerfile.dependent-tests-postgres` 
3. Environment variables: `SBM_TEST_PG_*` instead of `SBM_TEST_SQL_*`
4. Health check: `pg_isready` instead of TCP port check

The PostgreSQL sidecar configuration:
```yaml
- name: postgres-server
  properties:
    image: postgres:16-alpine
    environmentVariables:
    - name: POSTGRES_PASSWORD
      value: "$pgPassword"
    - name: POSTGRES_DB
      value: "sbm_test"
    resources:
      requests:
        cpu: 2
        memoryInGb: 4
    ports:
    - port: 5432
```

---

## Implementation Priority

1. **First**: Add unit tests for new abstractions (integrate into existing UnitTest projects)
2. **Second**: Create `SqlSync.SqlBuild.Dependent.PostgreSQL.UnitTest` with core build execution tests
3. **Third**: Create `SqlBuildManager.Console.Dependent.PostgreSQL.UnitTest` for CLI integration tests
4. **Fourth**: Create PostgreSQL ACI test runner script and Dockerfile
5. **Fifth**: Create `SqlBuildManager.Console.PostgreSQL.ExternalTest` for cloud integration tests
