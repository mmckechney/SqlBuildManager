# PostgreSQL Support Feature Plan

## Overview

This document outlines the implementation plan for adding PostgreSQL as an alternative database target alongside the existing Microsoft SQL Server support. The goal is to allow runtime selection of the target database platform while preserving all existing SQL Server functionality.

## Current Architecture Summary

The application already has significant service abstraction in `SqlSync.SqlBuild.Services`:
- **21 interfaces** with **Default*** implementations (e.g., `IConnectionsService` → `DefaultConnectionsService`)
- **Manual DI** in `SqlBuildHelper` constructor with null-coalescing defaults
- **`ConnectionData`** and **`ConnectionHelper`** in `SqlSync.Connection` are tightly coupled to `Microsoft.Data.SqlClient`
- **`BuildConnectData`** directly exposes `SqlConnection` and `SqlTransaction` properties
- Transaction management uses ADO.NET `SqlTransaction` methods (`.Save()`, `.Rollback(name)`, `.Commit()`)

The key SQL Server-specific dependencies are:
1. `Microsoft.Data.SqlClient` (SqlConnection, SqlCommand, SqlTransaction, SqlConnectionStringBuilder)
2. `Microsoft.SqlServer.Dac` (DacPac extraction/comparison)
3. `Microsoft.SqlServer.SqlManagementObjects` (SMO for object scripting in SqlSync.ObjectScript)
4. T-SQL syntax throughout (GO batching, NOLOCK hints, sys.* catalog views, INFORMATION_SCHEMA, square bracket identifiers)
5. `SqlBuild_Logging` table DDL uses SQL Server-specific syntax (dbo schema, NONCLUSTERED indexes, INFORMATION_SCHEMA checks)

---

## Phase 1: Core Abstractions

### 1.1 Database Platform Enum

Create a new enum to identify the target database platform at runtime.

**File**: `SqlSync.Connection/DatabasePlatform.cs`
```csharp
public enum DatabasePlatform
{
    SqlServer,
    PostgreSQL
}
```

### 1.2 Add Platform to ConnectionData

Extend `ConnectionData` with a `DatabasePlatform` property:
- Default to `DatabasePlatform.SqlServer` for backward compatibility
- Add to serialization/settings file support

### 1.3 CLI Option

Add `--platform` option to `CommandLineBuilder.cs`:
```
--platform <SqlServer|PostgreSQL>   Target database platform (default: SqlServer)
```
Register in `CommandLineArgsBinder.cs` and flow through `CommandLineArgs`.

### 1.4 Abstract Connection Types

The core blocker is that `BuildConnectData` directly exposes `Microsoft.Data.SqlClient.SqlConnection` and `SqlTransaction`. These must be abstracted.

#### Why `DbConnection`/`DbTransaction` instead of `IDbConnection`/`IDbTransaction`?

Both `SqlConnection` and `NpgsqlConnection` inherit from `System.Data.Common.DbConnection` (the abstract base class). They also implement `System.Data.IDbConnection` (the interface). However, **neither abstraction exposes all the provider-specific features** the code currently uses:

| Feature Used in Code | `IDbConnection` | `DbConnection` | `SqlConnection` | `NpgsqlConnection` |
|----------------------|-----------------|-----------------|------------------|---------------------|
| `.State` | ✓ | ✓ | ✓ | ✓ |
| `.Open()` / `.Close()` | ✓ | ✓ | ✓ | ✓ |
| `.Database` | ✓ | ✓ | ✓ | ✓ |
| `.DataSource` | ✗ | ✓ | ✓ | ✓ |
| `.InfoMessage` event | ✗ | ✗ | ✓ only | ✗ (uses `Notice` event) |
| `.ServerVersion` | ✗ | ✓ | ✓ | ✓ |

| Feature Used in Code | `IDbTransaction` | `DbTransaction` | `SqlTransaction` | `NpgsqlTransaction` |
|----------------------|------------------|------------------|--------------------|-----------------------|
| `.Commit()` / `.Rollback()` | ✓ | ✓ | ✓ | ✓ |
| `.Connection` | ✓ | ✓ | ✓ | ✓ |
| `.Save(name)` savepoints | ✗ | ✗ | ✓ only | ✗ (uses `Save(name)` too, but via `DbTransaction` override in newer Npgsql) |
| `.Rollback(name)` | ✗ | ✗ | ✓ only | ✗ (uses SQL: `ROLLBACK TO SAVEPOINT name`) |

**Decision**: Use `DbConnection` / `DbTransaction` (abstract base classes) for the `BuildConnectData` properties. Rationale:

1. **`DbConnection` exposes `.DataSource`** which the code uses for server identification — `IDbConnection` does not.
2. **Both `SqlConnection` and `NpgsqlConnection` inherit from `DbConnection`** — the factories create their first-party concrete objects and store them as `DbConnection`. No information is lost; services that need provider-specific features can cast internally.
3. **`DbCommand.CreateCommand()`** returns `DbCommand` (from `DbConnection`), keeping the chain consistent — `IDbConnection.CreateCommand()` returns `IDbCommand` which is a thinner interface.
4. **`DbTransaction` exposes `.DbConnection`** (typed as `DbConnection`), maintaining type consistency — `IDbTransaction.Connection` returns `IDbConnection` which would force casts elsewhere.
5. Modern ADO.NET and all major providers (Npgsql, MySqlConnector, Microsoft.Data.SqlClient) are built on the `System.Data.Common` abstract classes, not the `System.Data` interfaces. The interfaces are essentially legacy from ADO.NET 1.x.

**Provider-specific features** (savepoints, InfoMessage, Notice events) are abstracted into service interfaces (`ITransactionManager`, `IDbConnectionFactory`) where the implementation knows the concrete type it created and can safely cast:

```csharp
// Example: SqlServerTransactionManager knows it created a SqlTransaction
public class SqlServerTransactionManager : ITransactionManager
{
    public void CreateSavePoint(DbTransaction transaction, string name)
    {
        ((SqlTransaction)transaction).Save(name);  // safe cast — we created it
    }
}

// Example: PostgresTransactionManager uses SQL commands for savepoints
public class PostgresTransactionManager : ITransactionManager
{
    public void CreateSavePoint(DbTransaction transaction, string name)
    {
        using var cmd = transaction.Connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = $"SAVEPOINT {name}";
        cmd.ExecuteNonQuery();
    }
}
```

**Changes to `BuildConnectData`**:
```csharp
public class BuildConnectData
{
    public DbConnection Connection { get; set; }      // was SqlConnection
    public DbTransaction Transaction { get; set; }    // was SqlTransaction
    public string DatabaseName { get; set; }
    public string ServerName { get; set; }
    public bool HasLoggingTable { get; set; }
}
```

**Impact**: Every file that casts or uses `SqlConnection`/`SqlTransaction` directly must be updated. Key files:
- `DefaultConnectionsService.cs` — `PrepConnectionAndTransaction()` uses `SqlException` in Polly retry; change to `DbException`
- `SqlBuildRunner.cs` — `new SqlCommand(sql, cData.Connection, cData.Transaction)` → use `IDbConnectionFactory.CreateCommand()`
- `DefaultSqlLoggingService.cs` — `SqlCommand` for logging table operations → use factory
- `DefaultDatabaseUtility.cs` — `SqlCommand` with `NOLOCK` queries → use factory + `IScriptSyntaxProvider`
- `Rebuilder.cs` — Multiple `SqlConnection`/`SqlCommand`/`SqlDataReader` usages → use factory
- `MultiDbHelper.cs` — `SqlConnection`, `SqlCommand`, `SqlDataAdapter` → use factory
- `DatabaseDiffer.cs` — Multiple SQL operations → use factory
- `QueryCollectionRunner.cs` — Ad-hoc query execution → use factory

### 1.5 Connection Factory Interface

Create `IDbConnectionFactory` to abstract connection creation:

**File**: `SqlSync.Connection/IDbConnectionFactory.cs`
```csharp
public interface IDbConnectionFactory
{
    DbConnection CreateConnection(ConnectionData connData);
    string BuildConnectionString(ConnectionData connData);
    DbCommand CreateCommand(string sql, DbConnection connection, DbTransaction transaction = null);
    DbParameter CreateParameter(string name, object value);
}
```

**Implementations**:
- `SqlServerConnectionFactory` — wraps current `ConnectionHelper` logic using `Microsoft.Data.SqlClient`
- `PostgresConnectionFactory` — uses `Npgsql` (NuGet: `Npgsql`)

### 1.6 Update ConnectionHelper

Refactor `ConnectionHelper` to delegate to `IDbConnectionFactory`:
- Keep static methods for backward compatibility but have them resolve the factory from `ConnectionData.DatabasePlatform`
- Move SQL Server-specific `SqlConnectionStringBuilder` logic to `SqlServerConnectionFactory`

---

## Phase 2: Transaction Handling

### 2.1 SQL Server Transaction Model (Current)

The current transaction model uses:
- `BeginTransaction("SqlBuildTrans")` — named transaction
- `Transaction.Save(savePointName)` — savepoints per script
- `Transaction.Rollback(savePointName)` — partial rollback to savepoint
- `Transaction.Rollback()` — full rollback
- `Transaction.Commit()` — commit all changes
- "Zombied transaction" detection via `InvalidOperationException` with "no longer usable"

### 2.2 PostgreSQL Transaction Differences

| Feature | SQL Server | PostgreSQL |
|---------|-----------|------------|
| Named transactions | `BEGIN TRANSACTION [name]` | Not supported (just `BEGIN`) |
| Savepoints | `SAVE TRANSACTION [name]` via ADO.NET | `SAVEPOINT name` via SQL command |
| Rollback to savepoint | `ROLLBACK TRANSACTION [name]` via ADO.NET | `ROLLBACK TO SAVEPOINT name` via SQL command |
| Release savepoint | Not needed | `RELEASE SAVEPOINT name` (recommended) |
| Transaction after error | Transaction becomes "doomed"/zombied | Transaction enters error state; must rollback to savepoint or abort |
| Auto-commit | Each statement commits unless in explicit transaction | Same behavior |
| Nested transactions | Supported (with @@TRANCOUNT) | Not supported; use savepoints instead |

### 2.3 Transaction Abstraction Interface

Create `ITransactionManager`:

**File**: `SqlSync.SqlBuild/Services/Interfaces/ITransactionManager.cs`
```csharp
public interface ITransactionManager
{
    DbTransaction BeginTransaction(DbConnection connection);
    void CreateSavePoint(DbTransaction transaction, string savePointName);
    void RollbackToSavePoint(DbTransaction transaction, string savePointName);
    void Commit(DbTransaction transaction);
    void Rollback(DbTransaction transaction);
    bool IsTransactionZombied(Exception ex);
}
```

**Implementations**:
- `SqlServerTransactionManager` — uses ADO.NET `SqlTransaction.Save()`, `.Rollback(name)`, checks for "no longer usable"
- `PostgresTransactionManager` — executes `SAVEPOINT name` and `ROLLBACK TO SAVEPOINT name` as raw SQL commands, checks for Npgsql-specific error states

### 2.4 Update SqlBuildRunner.cs

Replace direct `Transaction.Save()` and `Transaction.Rollback(name)` calls with `ITransactionManager` methods:
- Inject `ITransactionManager` via constructor
- `TryCreateSavePoint` → delegates to `_transactionManager.CreateSavePoint()`
- Rollback logic → delegates to `_transactionManager.RollbackToSavePoint()`
- Zombied detection → delegates to `_transactionManager.IsTransactionZombied()`

### 2.5 Update DefaultConnectionsService.cs

`PrepConnectionAndTransaction()` currently catches `SqlException` for Polly retry:
- Change to catch `DbException` (base class) for platform-agnostic retry
- Use `ITransactionManager.BeginTransaction()` instead of `connection.BeginTransaction(name)`

---

## Phase 3: SQL Syntax Differences

### 3.1 Batch Delimiter (GO)

| Feature | SQL Server | PostgreSQL |
|---------|-----------|------------|
| Batch separator | `GO` (client-side, not T-SQL) | No equivalent; statements separated by `;` |
| Behavior | Splits script into separate execution units | Each statement executes individually |

**Impact**: `DefaultScriptBatcher.cs` uses regex `^ *GO *` to split scripts into batches.

**Strategy**: 
- For PostgreSQL, the batcher should NOT split on `GO` — it's not a PostgreSQL keyword
- Instead, use `;` as the statement delimiter (or send entire script as single batch)
- Create `IScriptSyntaxProvider` interface:

```csharp
public interface IScriptSyntaxProvider
{
    string BatchDelimiterPattern { get; }          // "^ *GO *" for SQL Server, null for PostgreSQL
    bool RequiresBatchSplitting { get; }           // true for SQL Server, false for PostgreSQL
    string TransactionBeginStatement { get; }      // "BEGIN TRANSACTION" vs "BEGIN"
    string TransactionCommitStatement { get; }     // Same for both
    string TransactionRollbackStatement { get; }   // Same for both
    string ParameterPrefix { get; }                // "@" for SQL Server, "@" or "$" for PostgreSQL
    string IdentifierQuoteStart { get; }           // "[" for SQL Server, "\"" for PostgreSQL
    string IdentifierQuoteEnd { get; }             // "]" for SQL Server, "\"" for PostgreSQL
    string CatalogSeparator { get; }               // "." for both
    string StringConcatOperator { get; }           // "+" for SQL Server, "||" for PostgreSQL
    string TopNRowsClause(int n);                  // "TOP(n)" vs "LIMIT n"
    string NoLockHint { get; }                     // "WITH (NOLOCK)" for SQL Server, "" for PostgreSQL
    string SystemObjectsQuery { get; }             // sys.objects vs pg_catalog
    string CheckTableExistsQuery(string table);    // INFORMATION_SCHEMA vs pg_tables
}
```

### 3.2 SQL Syntax Translation Table

| SQL Server | PostgreSQL | Location in Code |
|-----------|------------|-----------------|
| `[dbo].[TableName]` | `"public"."tablename"` | LoggingTable.sql, DefaultDatabaseUtility.cs |
| `WITH (NOLOCK)` | (remove — PostgreSQL uses MVCC) | DefaultDatabaseUtility.cs:56,134,163; DefaultSqlLoggingService.cs:168 |
| `sys.objects WHERE name = 'X'` | `pg_class WHERE relname = 'x'` | DefaultSqlLoggingService.cs:168 |
| `INFORMATION_SCHEMA.COLUMNS` | `information_schema.columns` (same, but case-sensitive) | InfoHelper.cs |
| `INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'` | `information_schema.tables WHERE table_type = 'BASE TABLE'` | InfoHelper.cs |
| `IDENTITY(1,1)` | `SERIAL` or `GENERATED ALWAYS AS IDENTITY` | LoggingTable.sql |
| `NVARCHAR(MAX)` | `TEXT` | LoggingTable.sql |
| `NVARCHAR(n)` | `VARCHAR(n)` | LoggingTable.sql |
| `DATETIME` | `TIMESTAMP` | LoggingTable.sql |
| `BIT` | `BOOLEAN` | LoggingTable.sql |
| `UNIQUEIDENTIFIER` | `UUID` | LoggingTable.sql |
| `GETDATE()` | `NOW()` or `CURRENT_TIMESTAMP` | LoggingTable.sql defaults |
| `sp_spaceused` | Custom query against `pg_total_relation_size()` | InfoHelper.cs |
| `sp_helptext` | `pg_get_functiondef()`, `pg_get_viewdef()` | ObjectValidator.cs |
| `sp_depends` | `pg_depend` catalog | ObjectValidator.cs |
| `@@ROWCOUNT` | (use `GET DIAGNOSTICS row_count`)| Not directly in codebase; handled by ADO.NET |
| `TOP(n)` | `LIMIT n` | QueryCollector.cs |

### 3.3 SqlBuild_Logging Table — PostgreSQL DDL

Create a PostgreSQL-compatible version of the logging table:

**File**: `SqlSync.SqlBuild/SqlLogging/LoggingTable.PostgreSQL.sql`
```sql
CREATE TABLE IF NOT EXISTS sqlbuild_logging (
    buildfilename VARCHAR(300) NOT NULL,
    scriptfilename VARCHAR(300) NOT NULL,
    scriptid UUID NOT NULL,
    scriptfilehash VARCHAR(100) NULL,
    commitdate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    sequence INT NOT NULL DEFAULT 0,
    userid VARCHAR(100) NULL,
    allowscriptblock BOOLEAN NOT NULL DEFAULT TRUE,
    scripttext TEXT NULL,
    tag VARCHAR(200) NULL,
    targetdatabase VARCHAR(200) NULL,
    runwithversion VARCHAR(50) NULL,
    buildprojecthash VARCHAR(100) NULL,
    buildrequestedby VARCHAR(200) NULL,
    scriptrunstart TIMESTAMP NULL,
    scriptrunend TIMESTAMP NULL,
    description VARCHAR(500) NULL
);

CREATE INDEX IF NOT EXISTS ix_sqlbuild_logging ON sqlbuild_logging (buildfilename);
CREATE INDEX IF NOT EXISTS ix_sqlbuild_logging_1 ON sqlbuild_logging (scriptfilename);
CREATE INDEX IF NOT EXISTS ix_sqlbuild_logging_commitcheck ON sqlbuild_logging (scriptid, commitdate DESC) 
    INCLUDE (scriptfilehash, allowscriptblock);
```

### 3.4 LogScript — PostgreSQL INSERT

**File**: `SqlSync.SqlBuild/SqlLogging/LogScript.PostgreSQL.sql`
```sql
INSERT INTO sqlbuild_logging (buildfilename, scriptfilename, scriptid, scriptfilehash,
    commitdate, sequence, userid, allowscriptblock, scripttext, tag,
    targetdatabase, runwithversion, buildprojecthash, buildrequestedby,
    scriptrunstart, scriptrunend, description)
VALUES (@buildfilename, @scriptfilename, @scriptid::uuid, @scriptfilehash,
    @commitdate, @sequence, @userid, true, @scripttext, @tag,
    @targetdatabase, @runwithversion, @buildprojecthash, @buildrequestedby,
    @scriptrunstart, @scriptrunend, @description)
```

Note: Npgsql uses `@paramName` prefix (same as SQL Server), so most parameterized queries can remain unchanged. The `::uuid` cast may be needed for UUID parameters.

### 3.5 SQL Resource Provider

Create a provider to select the correct SQL resource based on platform:

```csharp
public interface ISqlResourceProvider
{
    string LoggingTableDdl { get; }
    string LogScriptInsert { get; }
    string LoggingTableCommitCheckIndex { get; }
    string CheckTableExistsQuery(string tableName);
    string GetScriptRunLogQuery(string scriptId);
    string GetBlockingScriptLogQuery(string scriptId);
}
```

---

## Phase 4: Service Implementations

### 4.1 PostgreSQL NuGet Package

Add `Npgsql` package to:
- `SqlSync.Connection.csproj`
- `SqlSync.SqlBuild.csproj`

### 4.2 PostgresConnectionFactory

```csharp
public class PostgresConnectionFactory : IDbConnectionFactory
{
    public DbConnection CreateConnection(ConnectionData connData)
    {
        var connString = BuildConnectionString(connData);
        return new NpgsqlConnection(connString);
    }

    public string BuildConnectionString(ConnectionData connData)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = connData.SQLServerName,
            Database = connData.DatabaseName,
            Username = connData.UserId,
            Password = connData.Password,
            Timeout = connData.ScriptTimeout,
            Pooling = false
        };
        // Handle SSL, trust cert, etc.
        return builder.ConnectionString;
    }
}
```

### 4.3 PostgresTransactionManager

Handles PostgreSQL-specific savepoint behavior via SQL commands:
```csharp
public class PostgresTransactionManager : ITransactionManager
{
    public void CreateSavePoint(DbTransaction transaction, string name)
    {
        using var cmd = transaction.Connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = $"SAVEPOINT \"{name}\"";
        cmd.ExecuteNonQuery();
    }

    public void RollbackToSavePoint(DbTransaction transaction, string name)
    {
        using var cmd = transaction.Connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = $"ROLLBACK TO SAVEPOINT \"{name}\"";
        cmd.ExecuteNonQuery();
    }

    public bool IsTransactionZombied(Exception ex)
    {
        // Npgsql throws NpgsqlException or PostgresException
        // Check for "current transaction is aborted" state
        return ex.Message.Contains("current transaction is aborted");
    }
}
```

### 4.4 Platform-Aware DefaultSqlLoggingService

The logging service needs platform-specific SQL. Update to use `ISqlResourceProvider`:
- Table existence check: `sys.objects` → `pg_class` or `information_schema.tables`
- INSERT statement: different resource file per platform
- Parameter handling: PostgreSQL uses same `@param` prefix via Npgsql

### 4.5 Platform-Aware DefaultDatabaseUtility

Queries that use `WITH (NOLOCK)` must strip that hint for PostgreSQL (MVCC handles read consistency natively).

---

## Phase 5: Authentication Differences

### 5.1 Authentication Comparison

| Auth Method | SQL Server | PostgreSQL |
|------------|-----------|------------|
| Username/Password | `SqlPassword` auth method | Standard `Username`/`Password` in connection string |
| Windows/Integrated | `IntegratedSecurity = true` | Not natively supported; use SSPI/GSSAPI on domain-joined |
| Azure AD | `ActiveDirectoryDefault`, `ActiveDirectoryManagedIdentity`, etc. | Azure AD via `azure.identity` plugin for Npgsql (`Npgsql.Authentication.AzureIdentity`) |
| Managed Identity | `ActiveDirectoryManagedIdentity` | Supported via Npgsql Azure Identity plugin |
| Certificate | Not in current codebase | `SslMode=Require` + client cert |

### 5.2 AuthenticationType Updates

Extend the enum or map existing values:
- `Password` → works for both platforms
- `Windows` → SQL Server only; warn/error for PostgreSQL
- `AzureADDefault` / `ManagedIdentity` → supported for both via Npgsql plugin
- Consider adding `PostgreSQLNative` if needed

### 5.3 PostgreSQL Connection String Authentication

```csharp
// Password auth
builder.Username = connData.UserId;
builder.Password = connData.Password;

// Azure AD / Managed Identity (requires Npgsql.Authentication.AzureIdentity)
builder.Username = connData.UserId; // or managed identity client ID
// Token provider callback handles the rest
```

---

## Phase 6: Master Database Equivalent

### 6.1 SQL Server "master" Database

SQL Server has a `master` system database that:
- Lists all databases via `sys.databases`
- Is the default connection target
- Is used by `QueryCollectionRunner.cs` for `masterConnData`

### 6.2 PostgreSQL Equivalent

PostgreSQL uses:
- **`postgres` database** — default administrative database (analogous to `master`)
- **`pg_database` catalog** — lists all databases: `SELECT datname FROM pg_database WHERE datistemplate = false`
- **`\l` or `pg_catalog`** — metadata views

### 6.3 Implementation

- Map "master" references to "postgres" for PostgreSQL platform
- Update `QueryCollectionRunner.cs` to use platform-specific default database
- Add `DefaultAdminDatabase` property to `IScriptSyntaxProvider` ("master" vs "postgres")

---

## Phase 7: Features NOT Applicable to PostgreSQL

### 7.1 DacPac (Data-Tier Application Package)

`Microsoft.SqlServer.Dac` is SQL Server-only. For PostgreSQL:
- **Skip DacPac features** when platform is PostgreSQL
- The `IDacPacFallbackHandler` interface should return "not applicable" for PostgreSQL
- `DacPacHelper.ExtractDacPac()` and `ScriptDacPacDeltas()` should throw `NotSupportedException` or return gracefully
- Consider alternatives: `pg_dump` for schema extraction, `migra` for diff generation (future enhancement)

### 7.2 SMO (SQL Server Management Objects)

`SqlSync.ObjectScript` uses SMO extensively for object scripting:
- **Not applicable to PostgreSQL**
- For PostgreSQL object scripting (future): use `pg_dump --schema-only` or query `pg_catalog` directly
- Consider marking `SqlSync.ObjectScript` features as SQL Server-only initially

### 7.3 SQL Server-Specific Script Policies

Some `IScriptPolicy` implementations in `SqlBuildManager.Enterprise` are SQL Server-specific:
- `WithNoLockPolicy` — `WITH (NOLOCK)` is SQL Server syntax; irrelevant for PostgreSQL
- `QualifiedNamesPolicy` — bracket quoting `[schema].[object]` vs double-quote `"schema"."object"`
- `ScriptSyntaxCheckPolicy` — T-SQL syntax validation

**Strategy**: Add `ApplicablePlatforms` property to `IScriptPolicy`:
```csharp
DatabasePlatform[] ApplicablePlatforms { get; }  // null = all platforms
```

---

## Phase 8: Project Structure

### 8.1 New NuGet Dependencies

| Project | Package | Purpose |
|---------|---------|---------|
| SqlSync.Connection | `Npgsql` (9.x) | PostgreSQL ADO.NET provider |
| SqlSync.Connection | `Npgsql.Authentication.AzureIdentity` | Azure AD auth for PostgreSQL |

### 8.2 New Files

| File | Project | Purpose |
|------|---------|---------|
| `DatabasePlatform.cs` | SqlSync.Connection | Platform enum |
| `IDbConnectionFactory.cs` | SqlSync.Connection | Connection abstraction |
| `SqlServerConnectionFactory.cs` | SqlSync.Connection | SQL Server implementation |
| `PostgresConnectionFactory.cs` | SqlSync.Connection | PostgreSQL implementation |
| `ITransactionManager.cs` | SqlSync.SqlBuild | Transaction abstraction |
| `SqlServerTransactionManager.cs` | SqlSync.SqlBuild | SQL Server transactions |
| `PostgresTransactionManager.cs` | SqlSync.SqlBuild | PostgreSQL transactions |
| `IScriptSyntaxProvider.cs` | SqlSync.SqlBuild | SQL syntax differences |
| `SqlServerSyntaxProvider.cs` | SqlSync.SqlBuild | SQL Server syntax |
| `PostgresSyntaxProvider.cs` | SqlSync.SqlBuild | PostgreSQL syntax |
| `ISqlResourceProvider.cs` | SqlSync.SqlBuild | Platform-specific SQL resources |
| `SqlServerResourceProvider.cs` | SqlSync.SqlBuild | SQL Server SQL resources |
| `PostgresResourceProvider.cs` | SqlSync.SqlBuild | PostgreSQL SQL resources |
| `LoggingTable.PostgreSQL.sql` | SqlSync.SqlBuild | PostgreSQL logging table DDL |
| `LogScript.PostgreSQL.sql` | SqlSync.SqlBuild | PostgreSQL insert statement |
| `LoggingTableCommitCheckIndex.PostgreSQL.sql` | SqlSync.SqlBuild | PostgreSQL index DDL |

### 8.3 Modified Files

| File | Changes |
|------|---------|
| `ConnectionData.cs` | Add `DatabasePlatform` property |
| `ConnectionHelper.cs` | Delegate to `IDbConnectionFactory` |
| `BuildConnectData.cs` | Change `SqlConnection`→`DbConnection`, `SqlTransaction`→`DbTransaction` |
| `DefaultConnectionsService.cs` | Use `DbException`, `ITransactionManager`, `IDbConnectionFactory` |
| `SqlBuildRunner.cs` | Use `ITransactionManager`, `DbCommand` instead of `SqlCommand` |
| `DefaultSqlLoggingService.cs` | Use `ISqlResourceProvider`, `DbCommand` |
| `DefaultDatabaseUtility.cs` | Use `ISqlResourceProvider`, remove `NOLOCK` for PostgreSQL |
| `DefaultScriptBatcher.cs` | Use `IScriptSyntaxProvider` for batch delimiter |
| `CommandLineBuilder.cs` | Add `--platform` option |
| `CommandLineArgsBinder.cs` | Bind `--platform` option |
| `CommandLineArgs.cs` | Add `DatabasePlatform` property |
| `SqlBuildHelper.cs` | Wire new services, resolve factories based on platform |

---

## Phase 9: Implementation Order

### Step 1 — Foundation (No Breaking Changes)
1. Create `DatabasePlatform` enum
2. Add `DatabasePlatform` property to `ConnectionData` (default: SqlServer)
3. Create `IDbConnectionFactory`, `ITransactionManager`, `IScriptSyntaxProvider`, `ISqlResourceProvider` interfaces
4. Create SQL Server implementations that wrap existing code
5. Add `--platform` CLI option

### Step 2 — Refactor Core to Use Abstractions
1. Change `BuildConnectData` to use `DbConnection`/`DbTransaction`
2. Update `DefaultConnectionsService` to use `IDbConnectionFactory` and `ITransactionManager`
3. Update `SqlBuildRunner` to use `ITransactionManager` and `DbCommand`
4. Update `DefaultSqlLoggingService` to use `ISqlResourceProvider`
5. Update `DefaultDatabaseUtility` to use `ISqlResourceProvider`
6. **Run all existing unit tests** to verify no regressions

### Step 3 — PostgreSQL Implementations
1. Add `Npgsql` NuGet package
2. Implement `PostgresConnectionFactory`
3. Implement `PostgresTransactionManager`
4. Implement `PostgresSyntaxProvider`
5. Implement `PostgresResourceProvider` with PostgreSQL-specific SQL
6. Create PostgreSQL logging table DDL

### Step 4 — Integration & Testing
1. Create PostgreSQL integration tests (see `postgres-feature-tests.md`)
2. Test with local PostgreSQL instance
3. Test in ACI with PostgreSQL sidecar
4. Verify all SQL Server tests still pass

### Step 5 — Extended Features
1. Update script policies for platform awareness
2. Add PostgreSQL metadata queries to `SqlSync.DbInformation` (via new `IMetadataProvider`)
3. Consider PostgreSQL object scripting (via `pg_dump` or catalog queries)
4. Update documentation and settings file format

---

## Risks and Considerations

1. **Breaking change to BuildConnectData**: Changing from `SqlConnection`→`DbConnection` affects all consumers. Must be done carefully with full test coverage.
2. **GO batch delimiter**: PostgreSQL scripts should not contain `GO`. Users must be aware that SQL scripts are platform-specific.
3. **DacPac unavailability**: PostgreSQL users lose schema comparison/extraction features. Document this clearly.
4. **Transaction behavior differences**: PostgreSQL aborts the entire transaction on any error (unless using savepoints). The current retry logic must be validated.
5. **Case sensitivity**: PostgreSQL identifiers are case-sensitive when quoted. The logging table and queries must use consistent casing.
6. **Parameter handling**: Npgsql supports `@param` syntax but UUID parameters may need explicit `::uuid` casting.
7. **Existing script compatibility**: User SQL scripts written for SQL Server will NOT work on PostgreSQL. The application should validate/warn about this.
