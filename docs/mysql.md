# MySQL Support

SQL Build Manager supports MySQL as a first-class database target alongside Microsoft SQL Server and PostgreSQL.  
Select MySQL at runtime with `--platform MySQL`.

---

## Quick Start

To run a build against a MySQL database, add `--platform MySQL`:

```bash
sbm build ^
    --packagename "mypackage.sbm" ^
    --server "mysql-server.mysql.database.azure.com" ^
    --database "mydb" ^
    --username "myuser" ^
    --password "mypassword" ^
    --platform MySQL
```

For threaded execution across multiple MySQL databases:

```bash
sbm threaded run ^
    --packagename "mypackage.sbm" ^
    --override "mysql-databasetargets.cfg" ^
    --username "myuser" ^
    --password "mypassword" ^
    --platform MySQL
```

---

## How It Works

When `--platform MySQL` is specified:

1. **Connections** use [MySqlConnector](https://mysqlconnector.net/) (`MySqlConnection`).
2. **Transactions** use savepoint semantics compatible with MySQL execution.
3. **SQL syntax** is adapted for MySQL execution and logging queries.
4. **Build logging** uses a MySQL-native `sqlbuild_logging` table (`DATETIME(6)`, `LONGTEXT`, `TINYINT(1)`, `CHAR(36)`).
5. **Runtime wiring** selects MySQL-specific implementations for:
   - `IDbConnectionFactory`
   - `ITransactionManager`
   - `IScriptSyntaxProvider`
   - `ISqlResourceProvider`

---

## Script Authoring Notes

When authoring scripts for MySQL targets, use MySQL-compatible syntax.

| SQL Server | MySQL |
|-----------|-------|
| `GO` batch separator | Not required; terminate statements with `;` |
| `[dbo].[TableName]` | `` `table_name` `` (or unquoted names) |
| `NVARCHAR(MAX)` | `LONGTEXT` |
| `DATETIME` | `DATETIME(6)` |
| `BIT` | `TINYINT(1)` |
| `UNIQUEIDENTIFIER` | `CHAR(36)` (or `BINARY(16)` if desired) |
| `GETDATE()` | `NOW()` / `CURRENT_TIMESTAMP` |
| `IDENTITY(1,1)` | `AUTO_INCREMENT` |
| `TOP(n)` | `LIMIT n` |

> If your fleet includes multiple database engines, maintain separate `.sbm` packages with platform-appropriate SQL.

---

## Supported Execution Models

MySQL is supported across local and remote execution models:

| Execution Model | Command | MySQL Support |
|----------------|---------|---------------|
| Local build | `sbm build` | ✅ Supported |
| Threaded | `sbm threaded run` | ✅ Supported |
| Azure Batch | `sbm batch run` | ✅ Supported |
| Kubernetes | `sbm k8s run` | ✅ Supported |
| Container Apps | `sbm containerapp run` | ✅ Supported |
| ACI | `sbm aci run` | ✅ Supported |

---

## Settings Files

`--platform MySQL` is preserved in saved settings files.

```bash
sbm batch savesettings ^
    --settingsfile "mysql-settings.json" ^
    --settingsfilekey "mykey" ^
    --platform MySQL ^
    --server "mysql-server.mysql.database.azure.com" ^
    --username "myuser" ^
    --password "mypassword"
```

For `azd up` environments, post-provision scripts can generate MySQL settings in `src/TestConfig`, including password-based variants such as:

- `settingsfile-aci-mysql-password.json`
- `settingsfile-batch-linux-mysql-password.json`
- `settingsfile-containerapp-mysql-password.json`
- `settingsfile-k8s-mysql-password.json`

---

## Authentication

### Username/Password

```bash
--authtype Password --username "myuser" --password "mypassword"
```

This is the default MySQL mode for generated external-test settings (`MYSQL_AUTH_MODE=Password`).

### Managed Identity (Azure MySQL)

Managed Identity mode is supported for Azure Database for MySQL when environment permissions are configured for Entra-based MySQL user creation.

```bash
--authtype ManagedIdentity --identityclientid "<managed-identity-client-id>"
```

For `azd up` deployments, enable this path with:

```bash
azd env set MYSQL_AUTH_MODE ManagedIdentity
```

---

## Features Not Yet Available for MySQL

The following remain SQL Server-only:

- **DACPAC operations** (`create fromdacpacs`, `create fromdacpacdiff`, `--platinumdacpac`, `--targetdacpac`, `--forcecustomdacpac`)
- **SMO-based object scripting**
- **Windows integrated authentication** (`--authtype Windows`)
- **SQL Server-specific script policies** (for example, `WithNoLockPolicy`, `QualifiedNamesPolicy`, T-SQL syntax checks)

