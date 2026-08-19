# Local container integration tests

The local runner uses Docker Compose, the test coordinator image, and the same
production SBM runtime image used by Azure container compute. It works with
Docker Desktop on Windows and Docker on Ubuntu runners. Each run starts the
selected database plus Azurite, Event Hubs Emulator, and Service Bus Emulator:

```powershell
./scripts/tests/run_local_container_tests.ps1 -Platform sqlserver
./scripts/tests/run_local_container_tests.ps1 -Platform postgresql
./scripts/tests/run_local_container_tests.ps1 -Platform mysql
```

Every SBM behavioral command executes in production runtime containers. Two
runtime containers are used by default; pass `-RuntimeContainerCount` with any
integer greater than zero to change the worker count:

```powershell
./scripts/tests/run_local_container_tests.ps1 -Platform postgresql -RuntimeContainerCount 4
```

Each run writes to a timestamped platform folder under
`testresults` (for example, `postgresql-20260807-153500`).
The folder contains the test container console output, `TestResults.html`,
per-assembly TRX/HTML results, and the Docker Compose log. Containers, networks,
and volumes are removed when the script exits. The test coordinator creates
inputs and validates database and emulator effects; it does not execute SBM
commands in-process.

The GitHub Actions workflow
`.github/workflows/local-container-integration-tests.yml` is manually
dispatchable and uploads results and logs as artifacts. It requires no Azure
credentials.
