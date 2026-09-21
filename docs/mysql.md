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

For `azd up` environments, post-provision generates MySQL managed-identity settings in `src/TestConfig` for the selected compute platforms:

- `settingsfile-aci-mysql-mi-only.json`
- `settingsfile-batch-linux-mysql-mi-only.json`
- `settingsfile-batch-linux-queue-mysql-mi-only.json`
- `settingsfile-containerapp-mysql-mi-only.json`
- `settingsfile-k8s-mysql-mi-only.json`

---

## Authentication

### Username/Password

```bash
--authtype Password --username "myuser" --password "mypassword"
```

Local and local-container tests continue to use native username/password authentication. They do not require Entra ID, Azure tokens, or Azure connectivity. Explicit legacy Azure deployments can retain `MYSQL_AUTH_MODE=Password`, but the Azure MySQL test suites reject password settings.

### Managed Identity (Azure MySQL)

Managed identity is the default Azure MySQL test mode (`MYSQL_AUTH_MODE=ManagedIdentity`). Azure Database for MySQL Flexible Server supports [Entra authentication](https://learn.microsoft.com/en-us/azure/mysql/security/security-entra-authentication); database authorization must be provisioned separately.

```bash
--platform MySQL --authtype ManagedIdentity --clientid "<managed-identity-client-id>" --identityname "<managed-identity-name>"
```

The deploying Entra user is the MySQL Entra administrator. The server uses a dedicated `id-{env}-mysql-directory` identity for directory lookups, separate from both the private bootstrap identity and the `id-{env}-worker` identity used by database tests. Do not attach this directory identity to containers or grant its Graph permissions to workers. The operator needs Privileged Role Administrator (or Global Administrator) to grant the server identity `User.Read.All`, `GroupMember.Read.All`, and `Application.Read.All`, as described in [Microsoft's setup guide](https://learn.microsoft.com/en-us/azure/mysql/security/security-how-to-entra).

The launcher acquires a short-lived administrator token just before starting private bootstrap ACI and passes it as a **secure environment variable**, not an image layer or ordinary ARM environment value. Inside ACI, MySQL initialization runs first; `mysql` receives the token through temporary `MYSQL_PWD`, not a password argument. The parent clears the token before running other database initialization scripts. Bootstrap creates an Entra user for the runtime identity and grants privileges only on `sbm_mysql_testN` databases. It refuses to replace an existing native user or a principal mapped to another identity. Native server-administrator provisioning credentials remain available; they are not used by Azure tests.

ACI, Batch and Container Apps use their assigned managed identity. AKS uses its federated workload-identity token. Token-authenticated MySQL connections verify the server certificate and hostname.

### Migrating an existing Azure test environment

**Identity-separation scope:** the current R3 templates and runners target fresh disposable environments. The earlier authentication-only procedure below is not an RBAC migration: incremental Bicep does not remove old Contributor/AKS administrator assignments or stale identity attachments. Use a fresh environment and regenerate settings/images for the new worker/orchestrator identities. No automatic legacy-role revocation or migration tooling is provided.

Existing saved `Password` selections are not silently changed. Select the intended environment and set:

```bash
azd env set MYSQL_AUTH_MODE ManagedIdentity
```

Move stale Azure MySQL password settings (`settingsfile-*-mysql-password.json`) and `mysql-pw.txt` out of `src/TestConfig` before rerunning `azd up`; the target generator refuses to continue while these artifacts would still be copied into the test image. Do not change local/local-container credential files. Sign in to Azure CLI as the deploying Entra user, rerun `azd up` to update the administrator/grants and regenerate settings, and rebuild both runtime and Azure test images. Use `scripts\tests\run_all_mysql_external_tests_in_aci.ps1 -envName <env> -buildImage` when ready to run Azure tests.

Headless service-principal-only bootstrap is not supported by this workflow; it fails rather than falling back to a native password. After a token-expiry or directory-propagation failure, rerun postprovision as the same deploying user to obtain a fresh token.

---

## Features Not Yet Available for MySQL

The following remain SQL Server-only:

- **DACPAC operations** (`create fromdacpacs`, `create fromdacpacdiff`, `--platinumdacpac`, `--targetdacpac`, `--forcecustomdacpac`)
- **SMO-based object scripting**
- **Windows integrated authentication** (`--authtype Windows`)
- **SQL Server-specific script policies** (for example, `WithNoLockPolicy`, `QualifiedNamesPolicy`, T-SQL syntax checks)
