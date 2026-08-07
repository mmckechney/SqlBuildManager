# Local container integration tests

The local runner uses Docker Compose and the existing dependent-test image. It
works with Docker Desktop on Windows and Docker on Ubuntu runners, and starts
only the selected database platform:

```powershell
./scripts/tests/run_local_container_tests.ps1 -Platform sqlserver
./scripts/tests/run_local_container_tests.ps1 -Platform postgresql
./scripts/tests/run_local_container_tests.ps1 -Platform mysql
```

Results and the compose log are written to `testresults`. Containers, networks,
and volumes are removed when the script exits. Add `-IncludeEmulators` to start
the optional Azurite, Event Hubs emulator, and Service Bus emulator services.
The messaging smoke tests are compiled into each platform runner, so the SBM
runtime can exercise the same emulator endpoints with SQL Server, PostgreSQL,
or MySQL. `SBM_BLOB_ENDPOINT` can point blob operations at Azurite without
changing the normal Azure endpoint behavior.

The GitHub Actions workflow
`.github/workflows/local-container-integration-tests.yml` is manually
dispatchable and uploads results and logs as artifacts. It requires no Azure
credentials.
