# Local container integration tests

Run one database platform locally. Azurite, Event Hubs Emulator, Service Bus
Emulator, and the production SBM runtime image are always included:

```powershell
./scripts/tests/run_local_container_tests.ps1 -Platform sqlserver
```

Threaded tests launch two production runtime containers by default. Configure
any positive number with `-RuntimeContainerCount`:

```powershell
./scripts/tests/run_local_container_tests.ps1 -Platform sqlserver -RuntimeContainerCount 4
```

The **Local Container Integration Tests** GitHub Actions workflow runs one
all-runtime/emulator pass for SQL Server, PostgreSQL, and MySQL. Emulator images
require Docker image pulls and several minutes of startup time. Emulator
behavior supplements rather than replaces Azure integration tests.

The test coordinator and runtime containers receive `SBM_BLOB_ENDPOINT` so
production Blob clients use Azurite rather than the default Azure Storage
hostname. Each run's console output, `TestResults.html`,
per-assembly test reports, and Compose log are saved under a timestamped folder
in `testresults` named for the selected platform.
