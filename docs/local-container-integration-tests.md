# Local container integration tests

Run one database platform locally:

```powershell
./scripts/tests/run_local_container_tests.ps1 -Platform sqlserver
```

Include Azurite, the Event Hubs emulator, and the Service Bus emulator:

```powershell
./scripts/tests/run_local_container_tests.ps1 -Platform sqlserver -IncludeEmulators
```

The same options are available from the **Local Container Integration Tests**
GitHub Actions workflow using `workflow_dispatch`; set `include_emulators` to
`true` to run the messaging tests. Emulator images require Docker image pulls
and several minutes of startup time. Emulator behavior is not a substitute for
Azure integration tests, and the emulator tests are inconclusive when the
`SBM_TEST_*` emulator variables are absent, so ordinary `dotnet test` runs do
not require Docker.

When emulators are enabled, the test container also receives
`SBM_BLOB_ENDPOINT` so production Blob clients use Azurite rather than the
default Azure Storage hostname.
