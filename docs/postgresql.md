# PostgreSQL Support

SQL Build Manager now supports PostgreSQL as an alternative database target alongside Microsoft SQL Server. You can select the target database platform at runtime using the `--platform` option.

---

## Quick Start

To run a build against a PostgreSQL database, add `--platform PostgreSQL` to your command:

```bash
sbm build ^
    --packagename "mypackage.sbm" ^
    --server "postgres-server" ^
    --database "mydb" ^
    --username "pguser" ^
    --password "pgpassword" ^
    --platform PostgreSQL
```

For threaded execution across multiple PostgreSQL databases:

```bash
sbm threaded run ^
    --packagename "mypackage.sbm" ^
    --override "myoverrides.cfg" ^
    --username "pguser" ^
    --password "pgpassword" ^
    --platform PostgreSQL
```

The `--platform` option accepts either `SqlServer` (default) or `PostgreSQL`.

---

## How It Works

When `--platform PostgreSQL` is specified:

1. **Connections** use [Npgsql](https://www.npgsql.org/) (`NpgsqlConnection`) instead of `Microsoft.Data.SqlClient`
2. **Transactions** use PostgreSQL savepoints (`SAVEPOINT` / `ROLLBACK TO SAVEPOINT`) instead of ADO.NET `SqlTransaction.Save()`
3. **SQL syntax** is adapted automatically:
   - No `GO` batch splitting — scripts execute as a single unit
   - PostgreSQL identifier quoting (`"schema"."table"` instead of `[schema].[table]`)
   - PostgreSQL-compatible logging table DDL (`TIMESTAMP`, `TEXT`, `BOOLEAN`, `UUID` types)
   - `LIMIT` instead of `TOP(n)`, `||` instead of `+` for string concatenation
4. **Build logging** creates a `sqlbuild_logging` table with PostgreSQL-native types and indexes
5. **Authentication** uses standard PostgreSQL username/password via the `--username` and `--password` options

---

## Script Authoring

When writing SQL scripts for PostgreSQL targets, keep these syntax differences in mind:

| SQL Server | PostgreSQL |
|-----------|------------|
| `GO` (batch separator) | Not needed; use `;` between statements |
| `[dbo].[TableName]` | `"public"."tablename"` |
| `WITH (NOLOCK)` | Remove — PostgreSQL uses MVCC |
| `NVARCHAR(n)` | `VARCHAR(n)` |
| `NVARCHAR(MAX)` | `TEXT` |
| `DATETIME` | `TIMESTAMP` |
| `BIT` | `BOOLEAN` |
| `UNIQUEIDENTIFIER` | `UUID` |
| `GETDATE()` | `NOW()` or `CURRENT_TIMESTAMP` |
| `IDENTITY(1,1)` | `SERIAL` or `GENERATED ALWAYS AS IDENTITY` |
| `TOP(n)` | `LIMIT n` |
| `+` (string concat) | `\|\|` |

> **Tip**: If your database fleet includes both SQL Server and PostgreSQL targets, you will need to maintain separate script packages (`.sbm` files) with platform-appropriate SQL syntax for each target.

---

## Supported Execution Models

All execution models work with PostgreSQL:

| Execution Model | Command | PostgreSQL Support |
|----------------|---------|-------------------|
| Local build | `sbm build` | ✅ Supported |
| Threaded | `sbm threaded run` | ✅ Supported |
| Azure Batch | `sbm batch run` | ✅ Supported |
| Kubernetes | `sbm k8s run` | ✅ Supported |
| Container Apps | `sbm containerapp run` | ✅ Supported |
| ACI | `sbm aci run` | ✅ Supported |

---

## Settings File

The `--platform` setting can be saved to a settings file like any other option:

```bash
sbm threaded savesettings ^
    --settingsfile "pg-settings.json" ^
    --settingsfilekey "mykey" ^
    --platform PostgreSQL ^
    --server "postgres-server" ^
    --username "pguser" ^
    --password "pgpassword"
```

Then use it in subsequent builds:

```bash
sbm threaded run ^
    --settingsfile "pg-settings.json" ^
    --settingsfilekey "mykey" ^
    --packagename "mypackage.sbm" ^
    --override "myoverrides.cfg"
```

---

## Features Not Yet Available for PostgreSQL

The following features are currently SQL Server-only and are not available when using `--platform PostgreSQL`:

- **DACPAC operations** — Commands that use `.dacpac` files (`create fromdacpacs`, `create fromdacpacdiff`, `--platinumdacpac`, `--targetdacpac`, `--forcecustomdacpac`) rely on `Microsoft.SqlServer.Dac` which is SQL Server-specific
- **Object scripting** — Database object scripting via SQL Server Management Objects (SMO) is not available for PostgreSQL
- **Windows/Integrated authentication** — The `--authtype Windows` option uses SQL Server SSPI; for PostgreSQL, use `--authtype Password` with `--username` and `--password`
- **Script policies** — Some built-in script policies are SQL Server-specific:
  - `WithNoLockPolicy` — validates `WITH (NOLOCK)` usage (not applicable to PostgreSQL)
  - `QualifiedNamesPolicy` — validates `[schema].[object]` bracket quoting (PostgreSQL uses double quotes)
  - `ScriptSyntaxCheckPolicy` — validates T-SQL syntax
- **Query across databases** — `sbm threaded query` and `sbm batch query` have not been tested with PostgreSQL

---

## Authentication

PostgreSQL connections use standard username/password authentication:

```bash
--authtype Password --username "pguser" --password "pgpassword"
```

Connection strings are built automatically using the `--server`, `--database`, `--username`, and `--password` options. Npgsql handles SSL/TLS negotiation with the PostgreSQL server according to server configuration.

> **Note**: Managed Identity authentication (`--authtype ManagedIdentity`) is not currently supported for PostgreSQL. Use username/password authentication.
