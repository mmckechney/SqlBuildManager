# Package (aka Build Package)

Contents:

- [Package (aka Build Package)](#package-aka-build-package)
  - [Package and meta-data](#package-and-meta-data)
  - [Package file boundaries](#package-file-boundaries)
  - [Creating a Package](#creating-a-package)

---

## Package and meta-data
At the core of the process is the "SQL Build Manager Package" file (.sbm extension).  Under the hood, this file is a Zip file that contains the scripts that constitute your "build" along with a configuration file  (SqlSyncBuildProject.xml) that contains meta data on the scripts and execution parameters:

- `FileName`: Self explanatory, the name of the script file
- `BuildOrder`: The relative order that the scripts in the package will be run
- `Description`: Optional description about the script
- `RollBackOnError`: Option on whether or not to roll back the transaction if there is an error running this script (default: `true`)
- `CausesBuildFailure`: Option on whether or not to roll back the entire build if there is a failure with this script (default `true`)
- `DateAdded`: Date and time that the script was added to the package
- `ScriptId`: System generated GUID identifier for the script
- `Database`: Target database to run the scripts against. (This can be overridden in the case of multiple DB targets)
- `StripTransactionText`: Script handling to remove any inline transaction statements (default is `true` because you want SQL Build Manager to handle transactions)
- `AllowMultipleRuns`: Whether or not this script can be run on the same database multiple times (default is `true` and you should always write scripts so this is viable)
- `AddedBy`: User ID of the user that added the script to the package
- `ScriptTimeOut`: Timeout setting for the execution of this script
- `DateModified`: If the script has been modified after being added, this will be populated (otherwise `DateTime.Min`)
- `ModifiedBy`: If the script has been modified after being added, this will be populated with the user's ID
- `Tag`: Optional tag for the script

If you are using a DACPAC deployment, this all gets generated for you based on your command line parameters and defaults

Example `SqlSyncBuildProject.xml` file. You can build this by hand to create your own `.sbm` file or leverage the options below (recommended).

``` xml
<?xml version="1.0" standalone="yes"?>
<SqlSyncBuildData xmlns="http://schemas.mckechney.com/SqlSyncBuildProject.xsd">
  <SqlSyncBuildProject ProjectName="" ScriptTagRequired="false">
    <Scripts>
      <Script FileName="select.sql" BuildOrder="1" Description="Testing select script" RollBackOnError="true" CausesBuildFailure="true" DateAdded="2019-04-11T19:45:05.081043-04:00" ScriptId="14f775d2-d026-426b-bece-7faa323e0e14" Database="Client" StripTransactionText="true" AllowMultipleRuns="true" AddedBy="mimcke" ScriptTimeOut="500" DateModified="0001-01-01T00:00:00-05:00" ModifiedBy="" Tag="default" />
    </Scripts>
    <Builds />
  </SqlSyncBuildProject>
</SqlSyncBuildData>
```

----

## Package file boundaries

Package metadata is not permission to access arbitrary files on a worker. Before extracting an
SBM, SQL Build Manager checks the archive's filenames and requires every script reference to
identify a file in that archive. A leftover script or XML file in the destination cannot supply
a missing archive member or manifest.

Archive directories are still flattened during extraction. For example, `scripts/select.sql`
is extracted as `select.sql`, and its metadata must reference `select.sql`. Duplicate extracted
names, including case-only differences, are rejected on all platforms. References must match
the archive filename's case so packages behave consistently on Windows and Linux.

Package-controlled paths cannot be absolute, drive-relative, UNC, or contain parent traversal.
Windows alternate data streams, device names, trailing dots/spaces and other nonportable filename
forms are also rejected. Symbolic links/reparse points in package paths (including the working
directory's ancestors), and archive link/special-file entries, are not supported. Use ordinary
directories/files for package workspaces.

Containment checks also cover script execution and batch loading, hashes, packaging,
import/export and script deletion. Explicit source and output paths chosen by the operator remain
separate from package-relative filenames. With overwrite disabled, existing extracted files must
match the archive contents; use a fresh directory or explicitly enable overwrite to replace them.
Invalid metadata fails rather than silently skipping empty filenames. Failed extraction does not
recursively delete a caller-supplied directory; automatic cleanup is limited to a temporary directory
allocated by that extraction operation. An I/O failure can leave partial files in a caller-supplied
directory; retry in a fresh workspace or with overwrite enabled.

**No resource budgets are imposed by this hardening.** There are no new package-size, entry-count,
per-file size, expanded-byte, compression-ratio or parsing-work limits. Package publishers are trusted
for resource consumption as well as SQL contents; ordinary runtime/filesystem limits still apply.
These controls are not publisher authentication or a SQL sandbox. Protect working directories from
concurrent modification by untrusted local processes; path checks do not provide an OS-level
sandbox against filesystem races or hard-link attacks.

### Shared scripts in threaded execution

Integration-test fixtures follow the same package contract: each fixture owns a temporary
project directory, script metadata stores relative member names, and batching/hashing uses
that directory even for pre-batched runs. Fixture cleanup is scoped to its own directory.
Embedded test packages use relative archive entries; `EmbeddedPackageAssetTests` validates
the integration and Azure resource packages for safe names and complete script membership.
These test-fixture rules do not change runtime extraction or Azure worker processing.

Threaded execution (including the workers used by Batch, ACI, Container Apps and Kubernetes)
validates and extracts the input package once per worker execution, then shares the batched
scripts across database targets. Each target has independent project/history metadata and logs.
Target finalization does **not** copy the SQL files, produce a full archive per target, or rewrite
the input package. Token replacement uses a target-local statement without changing the cache.
Target XML is an execution record, not a standalone package with its own scripts.

Package creation/repackaging still requires every referenced script to exist in the validated
package directory. Per-target DACPAC generation remains separate because its generated scripts
can legitimately differ between databases. No containment checks or resource-budget policies
are relaxed to support shared execution.

A pre-commit persistence failure prevents transactional commit and runs rollback/connection
cleanup. A failure after commit is reported as an output/finalization failure without pretending
that committed changes were rolled back. Nontransactional statements may already be applied.
Do not automatically replay either kind of potentially committed work.

Finalization disposes and clears transaction references even when commit, rollback or disposal
fails. Cleanup recognizes the provider's known completed-transaction exception instead of
reporting a second rollback failure. Other cleanup errors are logged and still fail an otherwise
successful run; when a build has already failed, its original exception or failure status remains
primary rather than being replaced by a cleanup aggregate.

### Local containment verification

Run the unit regressions and the database-free CLI integration harness from the repository root:

```powershell
dotnet test .\src\SqlBuildManager.SqlBuild.UnitTest\SqlBuildManager.SqlBuild.UnitTest.csproj --filter FullyQualifiedName~PackageContainmentTests
dotnet test .\src\SqlBuildManager.Console.UnitTest\SqlBuildManager.Console.UnitTest.csproj --filter "FullyQualifiedName~ThreadedPackageValidationTest|FullyQualifiedName~UnpackPackageTest"
dotnet test .\src\SqlBuildManager.Console.UnitTest\SqlBuildManager.Console.UnitTest.csproj --filter FullyQualifiedName~SharedPackagePersistenceTest
dotnet test .\src\SqlBuildManager.SqlBuild.UnitTest\SqlBuildManager.SqlBuild.UnitTest.csproj --filter FullyQualifiedName~SqlBuildRunnerPersistenceTests
dotnet build .\src\SqlBuildManager.Console\sbm.csproj --configuration Release -f net10.0
pwsh .\scripts\tests\Test-PackageContainment.ps1 -SbmPath .\src\SqlBuildManager.Console\bin\Release\net10.0\sbm.dll
```

The integration harness starts real CLI processes and checks successful creation, listing,
hashing, unpacking and repackaging, plus rejection of traversal, missing members, duplicate names,
archive links and linked destinations. It verifies nonzero failure exit codes, absence of
outside-file disclosure and preservation of caller-owned files. Fixtures are isolated in a
temporary workspace and removed afterward. No database, Azure credentials or Docker are needed.
The harness's process timeout prevents hung tests; it does not impose a product/package limit.

Each native Console integration-test project also links `SharedThreadedPackageTest`. With that
platform's existing local test databases provisioned, run it with
`--filter FullyQualifiedName~SharedThreadedPackageTest`. It checks successful and failing
transactional/nontransactional runs, single/double-database targets, final persisted statuses,
independent histories, one extracted script set and an unchanged input archive.

## Creating a Package


There are several ways to create a build package from the command line.  Which you choose depends on your starting point:

[Command line reference](commandline.md)

1. From various sources using `sbm create`. There are four sub-commands to help create an SBM package:

   - `fromscripts` - Creates an SBM package or SBX project file from a list of scripts (type is determined by file extension: .sbm or .sbx)
   - `fromdiff` - Creates an SBM package from a calculated diff between two databases (you provide the server and database names, and it connects them them and generates the diff scripts)
   - `fromdacpacs` - Creates an SBM package from differences between two DACPAC files (use DACPACs you have created elsewhere or use `sbm dacpac` to create them)
   - `fromdacpacdiff`- This method leverages a DACPAC that was created against your "Platinum Database" (why platinum? because it's even more precious than gold!). The Platinum database should have the schema that you want all of your other databases to look like.

   Learn more about DACPACs and [data-tier applications](https://docs.microsoft.com/en-us/sql/relational-databases/data-tier-applications/data-tier-applications?view=sql-server-2017)  method.

2. From an SBX file. What is this? An SBX file is an XML file in the format of the `SqlSyncBuildProject.xml` file (see above) that has an `.sbx` extension. When you use the `sbm package` command, it will read the `.sbx` file and create the `.sbm` file with the referenced scripts.
3. An SBM package file can be created indirectly as well, using the `sbm threaded run` and `sbm batch run` commands along with the `--platinumdbsource="<database name>"` and `--platinumserversource="<server name>"` the app will generate a DACPAC from the source database which will then be used to generate an SBM at run time to build directly on your target(s).
4. You can also add new scripts to an existing SBM package or SBX project file using `sbm add`