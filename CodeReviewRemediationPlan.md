# Code Review Remediation Plan

## Executive Summary

This review assessed the complete .NET 10 solution across security, maintainability, and performance, including the command-line application, shared libraries, container images, Azure Batch packaging and execution, Azure infrastructure, CI/CD workflows, PowerShell automation, and tests.

The overall engineering risk is **High**. No confirmed Critical or High exploitable security vulnerability was identified, and a NuGet audit reported no known vulnerable direct or transitive packages. However, one Medium security misconfiguration exposes PostgreSQL to all Azure-origin traffic, encryption can silently fall back to plaintext, and supply-chain hardening is incomplete.

The most urgent operational risks are:

1. Azure Batch packages are published in **Debug** configuration.
2. SQL Server and PostgreSQL connection pooling is explicitly disabled across shared connection paths.
3. Several shared hot paths perform avoidable per-script, per-table, or per-log-flush network round trips.
4. Confirmed correctness defects exist in timeout calculation, ACI error handling, and connection disposal.
5. The CLI assembly owns all execution backends and Azure SDK dependencies, while orchestration, logging, and configuration rely heavily on static mutable state.
6. The most infrastructure-dependent and cloud-specific tests exist but are not run automatically.

Because the same assemblies execute locally, in containers, and on Azure Batch nodes, shared-code findings have a fleet-wide multiplier. Remediation should begin with the low-risk correctness, security, and deployment fixes in Phase 0, establish better automated coverage and performance baselines, and then proceed to the larger orchestration and dependency-boundary refactors.

## Review Scope

The review covered:

- `src/SqlBuildManager-console.sln` and all 28 projects.
- 555 tracked C# files across production, unit, dependent, PostgreSQL-dependent, and external-test projects.
- Command parsing, worker orchestration, threaded execution, SQL build execution, object scripting, database metadata, logging, cryptography, storage, queues, events, and Azure resource managers.
- SQL Server and PostgreSQL connection/authentication paths.
- CLI, local threaded, container, ACI, AKS, Container Apps, and Azure Batch execution models.
- All tracked Dockerfiles, GitHub Actions workflows, Bicep/ARM infrastructure, Azure configuration, and PowerShell/batch/shell automation.
- Package references, version-management practices, Dependabot configuration, and known-vulnerability auditing.
- Test organization, isolation, runtime coverage, documentation, and developer tooling.

Review techniques included repository-wide static analysis, targeted source inspection, cross-runtime call-path tracing, configuration and infrastructure review, and:

```text
dotnet list src\SqlBuildManager-console.sln package --vulnerable --include-transitive
```

The package audit completed successfully and reported no known vulnerable NuGet packages as of 2026-07-14.

The review did not include live penetration testing, deployment to Azure, database profiling, load testing, container image scanning, or verification that documentation example keys were never live. Findings that require runtime measurement are explicitly identified as profiling candidates.

## Overall Risk Summary

| Area | Risk | Rationale |
|---|---|---|
| Security | Medium | Strong cryptography, TLS defaults, managed identity, secret redaction, and restrictive Key Vault/Storage controls are present. PostgreSQL network exposure, plaintext fallback, test credentials, and supply-chain gaps remain. |
| Maintainability | High | Execution backends, CLI concerns, Azure SDKs, static state, and error handling are concentrated in large classes with limited unit-test seams. |
| Performance | High | Debug Batch packages, disabled pooling, N+1 database/storage calls, blocking waits, repeated regex compilation, and inefficient packaging affect core or fleet-multiplied paths. |
| Reliability/Operability | High | Timeout, disposal, cleanup, and error-contract defects can cause hangs, false success, leaked cloud resources, or failed multi-object operations. |
| Test/Delivery Confidence | High | Unit tests cover only part of the solution in CI; dependent and external cloud/runtime suites are not automated. |

### Cross-Runtime Risk Multipliers

- `SqlSync.SqlBuild`, `SqlSync.Connection`, `SqlBuildManager.Logging`, and portions of `SqlBuildManager.Console` are reused by local, container, and Azure Batch workloads.
- Per-target or per-script inefficiencies multiply by scripts x databases x workers.
- Static logging and worker state are especially risky under in-process threaded execution.
- Batch package and container build defects propagate to every worker instance using the artifact.
- Changes to shared command serialization, authentication, or configuration can fail only after moving from the orchestrator machine to a remote node with a different file system and identity context.

## Security Findings

### SEC-001 - PostgreSQL permits traffic from all Azure IP addresses

**Severity:** Medium  
**Confidence:** High  
**Affected surfaces:** Azure deployment, PostgreSQL dependent/external tests, any environment reusing this module

`infra/modules/postgresql.bicep:73-76` enables password authentication, while `:107-114` and `:188-195` unconditionally create `AllowAzureServices` rules using `0.0.0.0` to `0.0.0.0`. This special rule permits traffic from Azure resources outside the subscription and tenant. It is substantially broader than the SQL Server module's network and Entra-only posture.

**Remediation**

- Remove both `AllowAzureServices` rules.
- If temporary public access is required, require an explicit opt-in parameter and restrict access to known IPs or subnets.
- Prefer private endpoints/VNet integration.
- Disable password authentication where Entra authentication supports the required workload.

**Acceptance criteria**

- No deployed PostgreSQL firewall rule uses `0.0.0.0-0.0.0.0`.
- Connectivity succeeds only from approved identities and networks.
- Deployment tests verify the effective firewall and authentication configuration.

### SEC-002 - Encryption silently returns plaintext on failure

**Severity:** Medium due to consequence; Low exploitability  
**Confidence:** High  
**Affected surfaces:** Shared settings serialization, CLI, containers, Batch

`src/SqlSync.SqlBuild/Utilities/Cryptography.cs:37-54` catches every encryption exception and returns the original input. Callers can then serialize a secret to a settings file without knowing encryption failed.

**Remediation**

- Fail closed by throwing a typed exception or returning an explicit failure result.
- Abort settings persistence when encryption fails.
- Log only the operation and exception type; never log the input.

**Acceptance criteria**

- A forced encryption failure cannot return or persist the original plaintext.
- Unit tests cover encryption failure and confirm the settings file is not written.

### SEC-003 - XML schema validation does not explicitly prohibit DTD processing

**Severity:** Low, defense in depth  
**Confidence:** Medium  
**Affected surfaces:** Shared build-package validation

`src/SqlSync.SqlBuild/Validator/SchemaValidator.cs:44-52` creates `XmlTextReader` without explicitly setting `DtdProcessing.Prohibit` and `XmlResolver = null`. Current .NET defaults mitigate external entity resolution, but the security contract is implicit.

**Remediation**

- Use `XmlReader.Create` with explicit secure settings.
- Add DTD and external-entity rejection tests.

**Acceptance criteria**

- DTD-bearing input is rejected and external resources are never resolved.

### SEC-004 - Committed test credentials and key-shaped documentation examples

**Severity:** Low  
**Confidence:** High for test credentials; Medium that documentation values are placeholders  
**Affected surfaces:** Test automation, documentation, repository hygiene

- `scripts/tests/run_dependent_tests_in_aci.ps1:13-15` contains default SQL Server/PostgreSQL passwords.
- `docs/azure_batch.md:127` and `docs/azure_batch_example.md:31-32` contain structurally valid key-shaped examples.

**Remediation**

- Generate test passwords per run or require secure CI injection.
- Replace all key-shaped examples with unmistakable placeholders.
- Rotate any documentation value if its provenance cannot be proven.
- Add secret scanning to pull-request and default-branch workflows.

**Acceptance criteria**

- No password or realistic account/SAS key literal remains tracked.
- Test provisioning fails clearly when a required secret is absent.
- Secret scanning blocks newly introduced credential patterns.

### SEC-005 - Remote container installers are not integrity verified

**Severity:** Low  
**Confidence:** Medium  
**Affected surfaces:** Test container supply chain

`src/Dockerfile.tests:29-36` and `src/Dockerfile.dependent-tests` execute remote Azure CLI installation content and download kubectl without checksum/signature validation.

**Remediation**

- Install from signed package repositories where possible.
- Pin kubectl and verify its SHA-256 checksum before installation.
- Fail the build on checksum mismatch.

### SEC-006 - Container and workflow provenance is incomplete

**Severity:** Low  
**Confidence:** High  
**Affected surfaces:** Runtime and test images, CI/CD

`src/Dockerfile` uses mutable `:10.0` base-image tags. `.github/workflows/container-build.yml` disables provenance and pushes images for every branch. Workflow actions use version tags rather than immutable commit SHAs.

**Remediation**

- Pin production base images by digest with an automated update process.
- Enable OCI/SLSA provenance and produce an SBOM.
- Restrict registry pushes to protected branches/tags.
- Consider commit-SHA pinning for third-party actions.

### SEC-007 - Reproducible dependency and security gates are incomplete

**Severity:** Low  
**Confidence:** High  
**Affected surfaces:** Build and package supply chain

Dependabot and CodeQL are present, and the current NuGet audit is clean. However, the solution has no central package-version file or lockfile strategy, CodeQL does not run on pull requests, and no automated secret or container-image scanning gate was found.

**Remediation**

- Adopt central package management and an agreed lockfile/reproducible-restore policy.
- Run NuGet audit, CodeQL, secret scanning, SBOM generation, and image scanning before release.
- Document exception and update policies for vulnerable dependencies.

### Existing Security Strengths

- Authenticated settings encryption uses AES-256-CBC with encrypt-then-MAC, random salt/IV, PBKDF2-SHA256, HMAC-SHA256, and constant-time MAC comparison.
- SQL TLS certificate trust is secure by default and requires explicit opt-in to trust the server certificate.
- Managed identity and `TokenCredential` are preferred across Azure services.
- Connection strings, account keys, and passwords are centrally redacted before logging.
- Key Vault uses RBAC; Key Vault and Storage default network access to deny; Storage disables shared-key access.
- Zip extraction strips entry paths, preventing zip-slip traversal in the reviewed code.
- Process launches use fixed executables with `UseShellExecute = false`; no shell interpolation was identified.
- No unsafe `BinaryFormatter`, JSON type-name handling, `pull_request_target`, or cleartext production credential was identified.

## Maintainability Findings

### MAINT-001 - CLI and all execution backends are one deployment and dependency unit

**Priority:** High  
**Effort:** XL  
**Affected surfaces:** All runtimes

`src/SqlBuildManager.Console/sbm.csproj` directly references the Azure Batch, Container Apps, Container Instances, Network, Service Bus, Event Hubs, Key Vault, Storage, and other SDKs. Backend implementations live in the CLI assembly, including `BatchManager.cs`, `AciManager.cs`, `KubernetesManager.cs`, `ContainerAppManager.cs`, `QueueManager.cs`, and `StorageManager.cs`.

**Remediation**

- Define an `IExecutionBackend`/`IJobOrchestrator` contract in a shared abstractions project.
- Move backend-specific implementations and SDK references into backend projects.
- Keep command parsing and backend selection in the CLI composition root.

**Acceptance criteria**

- CLI command handlers depend on interfaces rather than concrete Azure managers.
- Backend projects can be tested and versioned independently.
- Existing local, Batch, ACI, AKS, and Container Apps smoke tests remain behaviorally equivalent.

### MAINT-002 - Worker orchestration uses static mutable state and detached execution

**Priority:** High  
**Effort:** Large  
**Affected surfaces:** All runtimes

`src/SqlBuildManager.Console/Worker/Worker.cs:31-35` stores exit code, startup arguments, command-line arguments, and logger in static fields. `:55-85` starts a detached `Task.Run`, while shutdown reads the shared exit code.

**Remediation**

- Convert command handlers to instance services.
- Await command invocation directly or use a scoped `TaskCompletionSource<int>`.
- Inject immutable run context rather than assigning static fields.

**Acceptance criteria**

- No mutable static execution state remains in `Worker` partials.
- Two worker instances can execute independently in one process test.
- Exit and cancellation behavior is deterministic.

### MAINT-003 - Oversized manager classes mix unrelated responsibilities

**Priority:** High  
**Effort:** Large to XL  
**Affected surfaces:** Primarily Batch; pattern repeats across cloud backends

`src/SqlBuildManager.Console/Batch/BatchManager.cs:98-523` combines pool lifecycle, job/task creation, command construction, credentials, resource files, monitoring, and cleanup. Similar concentration exists in Queue, Storage, and Kubernetes managers.

**Remediation**

- Split provisioning, command compilation, task submission, monitoring, and cleanup into collaborators.
- Extract pure command/resource mapping logic first so it can be unit tested without Azure.
- Apply the same boundary model to other backends after the Batch extraction proves the pattern.

### MAINT-004 - Error handling can swallow failures or report failure as success

**Priority:** High  
**Effort:** Medium  
**Affected surfaces:** ACI, Batch, Storage, Worker, Threaded, Queue

`src/SqlBuildManager.Console/Aci/AciManager.cs:256-273` returns `true` for non-404 Azure failures and generic exceptions. Bare catches exist in multiple managers, and Queue methods inconsistently return null, empty collections, or throw for comparable failures.

**Remediation**

- Correct the ACI existence check so only a successful read returns true.
- Define error contracts per method family: result type, empty result, or exception.
- Remove bare catches; catch expected exception types and log actionable context.

**Acceptance criteria**

- Unknown ACI failures never produce success.
- No unexplained empty catch remains.
- Unit tests cover 404, authorization, transient, cancellation, and unknown failures.

### MAINT-005 - Shared logging uses unsynchronized global configuration

**Priority:** High  
**Effort:** Medium to Large  
**Affected surfaces:** All runtimes; highest risk in threaded execution

`src/SqlBuildManager.Logging/ApplicationLogger.cs:11-17` holds logger factory, paths, switches, and Serilog logger in mutable static fields. `:41-50` recreates the factory using an unsynchronized check-then-act path, while path mutation and configuration use inconsistent locking.

**Remediation**

- Prefer a DI-managed logger factory scoped to a logical run.
- As an interim fix, synchronize all shared state and dispose replaced logger/factory instances.
- Avoid rebuilding every sink when one path is added.

**Acceptance criteria**

- A concurrent stress test shows no log cross-talk, lost paths, duplicate sinks, or leaked file handles.

### MAINT-006 - Azure SDK clients are constructed internally with few test seams

**Priority:** High  
**Effort:** Large  
**Affected surfaces:** All Azure-backed runtimes

Batch, ACI, Kubernetes, Container Apps, Storage, and Queue managers directly construct Azure clients and generally have no interfaces. Cloud orchestration is therefore covered mainly by external tests requiring live resources.

**Remediation**

- Inject SDK clients or narrow wrapper interfaces/factories.
- Keep wrappers focused on operations the application actually needs.
- Unit test command construction, retry decisions, resource naming, SAS policy, and status transitions.

### MAINT-007 - Batch resource cleanup is not guaranteed on exceptions

**Priority:** High  
**Effort:** Medium  
**Affected surfaces:** Azure Batch

`src/SqlBuildManager.Console/Batch/BatchManager.cs:451-509` performs cleanup inside the success path rather than a guaranteed `finally`/scope. Early failures can leak cost-bearing jobs or pools.

**Remediation**

- Encapsulate job/pool lifetime in an async cleanup scope.
- Make retention-on-failure an explicit option rather than an incidental control-flow outcome.
- Add fault-injection coverage.

### MAINT-008 - Database-platform rules leak into orchestration

**Priority:** Medium  
**Effort:** Medium  
**Affected surfaces:** Local and threaded SQL Server/PostgreSQL paths

PostgreSQL-specific managed-identity user mapping is duplicated in `Worker.Local.cs:131-136`, `ThreadedQuery.cs:44-49`, and `ThreadedRunner.cs:254`, despite factory/provider abstractions already existing.

**Remediation**

- Move identity resolution into the platform connection/authentication provider.
- Require orchestration code to request a ready connection without platform-specific mutation.

### MAINT-009 - Object scripting is a SQL Server-specific god class with recursive retry

**Priority:** High  
**Effort:** Small immediate fix; XL decomposition  
**Affected surfaces:** Object scripting across all execution models

`src/SqlSync.ObjectScript/ObjectScriptHelper.cs:1055-1134` directly configures SMO and recursively calls `ConnectToServer()` on retry without a bounded depth. The class also combines connection, scripting, file I/O, and progress reporting.

**Remediation**

- Immediately replace recursion with bounded, cancellation-aware retry.
- Introduce `IObjectScriptService` and separate connection, scripting, output, and progress concerns.
- Design a provider model before adding more PostgreSQL-specific branching.

### MAINT-010 - Database metadata helper is static, duplicated, and SQL Server-specific

**Priority:** Medium  
**Effort:** Large  
**Affected surfaces:** Shared metadata and scripting workflows

`src/SqlSync.DbInformation/InfoHelper.cs` contains multiple `GetDatabaseTableList` implementations, direct `SqlConnection` construction, legacy `ArrayList`, broad catches, and no provider abstraction.

**Remediation**

- Consolidate duplicate implementations.
- Replace non-generic collections.
- Route platform-specific SQL through an injected metadata provider.

### MAINT-011 - Initialization, polling, retry, and configuration logic has drifted

**Priority:** Medium  
**Effort:** Medium  
**Affected surfaces:** Queue, ACI, AKS, Batch, Storage

- Worker command handlers duplicate and bypass portions of the shared initialization sequence.
- Each backend implements its own polling and retry loop.
- Namespace, queue/topic names, pool names, image coordinates, resource sizing, and SAS durations are hard-coded in managers.

**Remediation**

- Introduce immutable execution options with validated defaults.
- Centralize common initialization.
- Use one cancellation-aware polling/retry policy with backend-specific status adapters.

### MAINT-012 - Command-line options require coordinated changes in multiple files

**Priority:** Medium  
**Effort:** Small to Medium  
**Affected surfaces:** CLI and remote command serialization

Options are defined, bound, and serialized in separate partial-file surfaces. `FileInfo` values require special Batch serialization to avoid carrying orchestrator paths to remote nodes.

**Remediation**

- Make Batch-safe file-name serialization the default for all file arguments.
- Add reflection/data-driven tests covering every `CommandLineArgs` property.
- Consider generating binders/serialization metadata from one option definition.

### MAINT-013 - CI does not execute existing dependent or external test tiers

**Priority:** High  
**Effort:** Medium  
**Affected surfaces:** Delivery confidence for all runtimes

`.github/workflows/dotnetcore-build.yml:35-52` runs six unit-test projects. SQL Server/PostgreSQL dependent tests and external ACI/Batch/AKS/Container Apps tests are not wired into any workflow, although dedicated Dockerfiles and scripts already exist.

**Remediation**

- Run hermetic unit tests on every pull request.
- Run dependent SQL Server/PostgreSQL suites on a scheduled or labeled workflow.
- Run cost-bearing external suites nightly, pre-release, or on demand with automatic cleanup.

### MAINT-014 - Unit tests are not consistently isolated

**Priority:** Medium  
**Effort:** Medium  
**Affected surfaces:** Test reliability

Nominal unit tests use hard-coded paths, real file I/O, timing sleeps, and little mocking. This increases platform sensitivity and flakiness.

**Remediation**

- Use per-test temporary directories and deterministic synchronization.
- Inject file system, clock, and cloud-client boundaries where behavior warrants it.
- Remove unexplained ignored tests or link them to tracked issues.

### MAINT-015 - Build and deployment automation is fragile and machine-specific

**Priority:** High  
**Effort:** Medium  
**Affected surfaces:** Developer, CI, container, and Azure deployment workflows

- `Build_Images.bat:6-7` and `container_prompts.bat` hard-code executable and repository paths.
- Most scripts lack `Set-StrictMode`, `$ErrorActionPreference = 'Stop'`, and external-command exit-code checks.
- Repository-root discovery is duplicated across scripts.

**Remediation**

- Resolve paths from `%~dp0`, `$PSScriptRoot`, or a shared module.
- Enable strict error handling and validate `$LASTEXITCODE`.
- Treat failed `az`, `dotnet`, `docker`, and `kubectl` commands as terminal failures.

### MAINT-016 - Package versions are duplicated across project files

**Priority:** Medium  
**Effort:** Medium  
**Affected surfaces:** Build and dependency management

`src/Directory.Build.props` only enables nullable reference types, and no `Directory.Packages.props` exists. Common package versions are repeated across projects.

**Remediation**

- Adopt NuGet central package management.
- Define the restore-lock strategy and audit step centrally.
- Keep Dependabot grouped by compatible dependency families.

### MAINT-017 - Critical workflow documentation and exit contracts are incomplete

**Priority:** Low to Medium  
**Effort:** Small to Medium  
**Affected surfaces:** Contributors and automation consumers

The repository lacks a contributor/local-dependent-test setup guide. Numeric exit codes are not centrally named or documented, and command documentation is manually synchronized.

**Remediation**

- Add a local development/test-tier guide linked from the README.
- Replace magic exit codes with a documented enum/constants.
- Add a lightweight command-help/documentation drift check.

### Existing Maintainability Strengths

- Runtime-specific folders and Worker partials make backend code discoverable.
- Database connection, transaction, syntax, and resource-provider interfaces provide a useful foundation.
- Test projects are intentionally separated by dependency tier.
- Bicep infrastructure is modular and parameterized.
- Central logging and explicit Batch path-serialization handling show awareness of cross-runtime concerns.

## Performance Findings

### PERF-001 - Azure Batch packages are published in Debug configuration

**Impact:** High  
**Confidence:** High  
**Affected surfaces:** Every Azure Batch task

`scripts/Batch/build_and_upload_batch.ps1:64-73` publishes and packages `bin\Debug` output.

**Remediation:** Publish Release artifacts and fail packaging if the expected Release output is absent.

**Measurement/acceptance:** Compare a fixed representative workload before and after; production packages must identify and contain Release output.

**Implementation update (2026-07-15):** Azure Batch application packages were replaced by the
Release-built `sqlbuildmanager:latest-vNext` Linux container image in ACR. Batch pools now use
managed identity for ACR pulls and run `/app/sbm` in Linux container tasks. Azure accepted the
AlmaLinux 8 Gen1 container pool with the default `STANDARD_D1_V2` VM size.

### PERF-002 - Connection pooling is disabled for both database platforms

**Impact:** High  
**Confidence:** High  
**Affected surfaces:** Shared SQL Server/PostgreSQL access across all runtimes

`src/SqlSync.Connection/SqlServerConnectionFactory.cs:43-52`, `ConnectionHelper.cs:115`, and `PostgresConnectionFactory.cs:64` set pooling to false.

**Remediation**

- Enable pooling by default with configurable limits.
- Keep a diagnostic opt-out.
- Ensure credentials/tokens and connection-string construction do not unnecessarily fragment pools.

**Measurement/acceptance:** Capture physical login count and open latency for representative metadata and 200/1000-script builds; physical logins should approach active pool size rather than operation count.

### PERF-003 - Script status checks create redundant per-script database work

**Impact:** High  
**Confidence:** High  
**Affected surfaces:** Core build engine, fleet-multiplied

`src/SqlSync.SqlBuild/Status/StatusHelper.cs:34-45` can call `HasBlockingSqlLog` twice with identical inputs. The utility opens a new connection and performs a synchronous query per call.

**Remediation**

- Reuse the first call's output.
- Replace per-script checks with one set-based status query per target database.
- Reuse the active build connection where transaction semantics allow.

### PERF-004 - Blob containers are synchronously checked/created on hot paths

**Impact:** High  
**Confidence:** High  
**Affected surfaces:** Orchestrator and every remote worker

`src/SqlBuildManager.Console/CloudStorage/StorageManager.cs:617-635` constructs a client and calls synchronous `CreateIfNotExists()` whenever a container client is requested, including repeated log flushes.

**Remediation**

- Cache clients by account/container.
- Create the container asynchronously once during initialization.
- Use bounded asynchronous file upload.

**Measurement/acceptance:** At most one create/check transaction per worker and container per run.

### PERF-005 - Blocking waits and sleeps occur in asynchronous workflows

**Impact:** Medium to High  
**Confidence:** High  
**Affected surfaces:** Worker, Queue, Storage, ACI, Batch, AKS, Container Apps

Examples include `.Wait()`, `.Result`, synchronous SDK calls, and `Thread.Sleep` polling in `QueueManager.cs:46`, `ThreadedQuery.cs:199-203`, `ArmHelper.cs:136-140`, and multiple manager/worker loops.

**Remediation**

- Propagate async through constructors/callers using factories where needed.
- Replace sleeps with cancellation-aware `Task.Delay`.
- Replace synchronous Azure SDK operations with async equivalents.

### PERF-006 - Regex compilation is repeatedly paid in hot loops

**Impact:** High  
**Confidence:** High  
**Affected surfaces:** Policy checks, script optimization, Batch package preparation

Policy classes and `src/SqlBuildManager.ScriptHandling/ScriptOptimization.cs:46-65` construct many `RegexOptions.Compiled` instances per invocation. Compiled-regex startup cost is therefore paid repeatedly instead of amortized.

**Remediation:** Use `[GeneratedRegex]` or cached `static readonly Regex` instances and precompute script comment spans once.

**Measurement/acceptance:** Benchmark a 200-500 script corpus; require equivalent findings with materially lower allocations and elapsed time.

### PERF-007 - Event monitor timeout uses the seconds component

**Impact:** Medium correctness/reliability  
**Confidence:** High  
**Affected surfaces:** CLI Event Hub monitor

`src/SqlBuildManager.Console/Worker/Worker.cs:557` uses `Elapsed.Seconds`, which wraps every minute, rather than total elapsed seconds.

**Remediation:** Use `Elapsed.TotalSeconds` or compare elapsed `TimeSpan` values.

**Acceptance criteria:** Tests cover values below, equal to, and above 60 seconds and prove deterministic termination.

### PERF-008 - SQL result accumulation and logging can grow quadratically

**Impact:** Medium  
**Confidence:** High  
**Affected surfaces:** Core executor across all runtimes

`src/SqlSync.SqlBuild/SqlBuildRunner.cs:166-170` repeatedly concatenates result strings and republishes the full accumulated result for every batch.

**Remediation**

- Accumulate with `StringBuilder`.
- Publish only the current batch delta while preserving the final result.

**Measurement/acceptance:** Heap allocation and emitted log bytes grow linearly for a high-batch, high-output script.

### PERF-009 - Script batching repeatedly rescans and reallocates whole scripts

**Impact:** Medium  
**Confidence:** High  
**Affected surfaces:** Shared build preparation

`src/SqlSync.SqlBuild/Services/DefaultScriptBatcher.cs` rebuilds regexes, repeatedly checks comments by rescanning scripts, performs duplicate matches, and replaces full strings per `USE` statement.

**Remediation:** Build a comment-span map once, cache regexes, and perform single-pass transformations.

### PERF-010 - Target chunking repeatedly materializes the input

**Impact:** Medium  
**Confidence:** High  
**Affected surfaces:** Large threaded/distributed target fleets

`src/SqlSync.SqlBuild/Utilities/CustomExtensionMethods.cs:16-65` calls `list.Count()` and `list.ToList()` repeatedly inside chunk construction.

**Remediation:** Materialize once and use indexed slices or `Enumerable.Chunk`.

### PERF-011 - Commit logging exceeds SQL parameter limits and falls back to row-at-a-time inserts

**Impact:** Medium  
**Confidence:** Medium  
**Affected surfaces:** Large builds

`src/SqlSync.SqlBuild/Services/DefaultSqlLoggingService.cs:325-437` uses approximately 17 parameters per row. A large batch can exceed SQL Server's 2100-parameter limit and fall back to one insert per script.

**Remediation:** Chunk below the limit or use `SqlBulkCopy`/platform-equivalent bulk loading.

### PERF-012 - PostgreSQL managed-identity tokens and credentials are recreated per connection

**Impact:** High  
**Confidence:** High  
**Affected surfaces:** PostgreSQL managed-identity workloads

`src/SqlSync.Connection/PostgresConnectionFactory.cs:78-139` constructs credentials and synchronously obtains a token per connection, unlike the cached SQL Server authentication provider.

**Remediation:** Cache `TokenCredential`, asynchronously refresh tokens shortly before expiry, and avoid embedding volatile tokens in a way that fragments pools.

### PERF-013 - Metadata row counts use one round trip per table

**Impact:** High  
**Confidence:** High  
**Affected surfaces:** SQL Server database information workflows

`src/SqlSync.DbInformation/InfoHelper.cs:190-226` executes `sp_spaceused` sequentially for every table.

**Remediation:** Replace with one set-based query using catalog/partition metadata.

### PERF-014 - Object validation performs many round trips and disposes a reused connection inside the loop

**Impact:** High reliability and performance  
**Confidence:** High  
**Affected surfaces:** SQL Server object validation

`src/SqlSync.ObjectScript/ObjectValidator.cs:46-112` performs multiple commands per object. `:100` disposes the shared connection inside the loop, making the next iteration susceptible to `ObjectDisposedException`.

**Remediation**

- Keep one properly scoped connection for the operation.
- Batch compatible SET/parse commands.
- Reduce per-object metadata round trips.

**Acceptance criteria:** Multi-object validation completes without disposal failure and uses a bounded number of opens/round trips.

### PERF-015 - Container builds defeat caching and ship redundant runtime content

**Impact:** Medium to High build/deployment efficiency  
**Confidence:** High  
**Affected surfaces:** Runtime images and CI

`src/Dockerfile:3-19` copies all source before restore/publish, clears NuGet caches, and performs self-contained publish onto a full `dotnet/runtime` image.

**Remediation**

- Copy solution/project manifests and restore in a cacheable layer.
- Copy source and publish with `--no-restore`.
- Use `runtime-deps` for self-contained output or publish framework-dependent to the runtime image.
- Enable GitHub Actions BuildKit cache.

**Measurement/acceptance:** Track clean/incremental build duration and image size; source-only changes should reuse restore layers.

### PERF-016 - External ACI test suites and provisioning loops are unnecessarily serial

**Impact:** Medium to High delivery latency  
**Confidence:** High  
**Affected surfaces:** External tests and provisioning

`scripts/tests/run_all_sqlserver_external_tests_in_aci.ps1:116-144` waits for each independent suite before starting the next. Database grant scripts invoke CLI commands serially per database and statement.

**Remediation:** Run independent suites with bounded concurrency, combine compatible grant statements, and retain deterministic result collection and cleanup.

### PERF-017 - No performance baseline or regression suite exists

**Impact:** Medium strategic risk  
**Confidence:** High  
**Affected surfaces:** Core engine and distributed runtimes

No BenchmarkDotNet project, representative load test, connection/round-trip budget, storage-transaction budget, worker throughput baseline, or container cold-start tracking was identified.

**Remediation**

- Benchmark policy checks, script batching, target chunking, commit logging, and status lookup.
- Add representative end-to-end threaded and Batch workload telemetry.
- Track wall time, allocations, physical database logins, round trips, storage calls, queue rate, and worker startup.

### Existing Performance Strengths

- Core runner paths propagate cancellation and use async database execution.
- Local parallelism and queue receive behavior are bounded.
- Several Azure operations already use Polly exponential backoff.
- SMO default initialization fields are configured to avoid property-fetch round trips.
- Event Hub producers and batches are reused.
- Batch runtime container images are prefetched from ACR onto pool nodes with managed identity.
- AKS autoscaling/workload identity and explicit ACI resource requests are configured.

## Consolidated Remediation Roadmap

### Phase 0 - Immediate Corrections (0-2 weeks)

| Order | Work item | Findings | Primary outcome |
|---:|---|---|---|
| 1 | Remove PostgreSQL `0.0.0.0` firewall rules and define the Entra/private-network target posture. | SEC-001 | Reduce externally reachable attack surface. |
| 2 | Make encryption fail closed. | SEC-002 | Prevent plaintext secret persistence. |
| 3 | Publish Azure Batch packages in Release configuration. | PERF-001 | Restore expected worker throughput. |
| 4 | Fix ACI unknown-error handling, Event Hub timeout calculation, and ObjectValidator connection scope. | MAINT-004, PERF-007, PERF-014 | Remove confirmed correctness defects. |
| 5 | Remove committed test passwords and realistic documentation keys. | SEC-004 | Improve credential hygiene. |
| 6 | Decide and test the connection-pooling policy, then enable pooling with safe limits. | PERF-002, PERF-012 | Eliminate repeated physical login cost. |
| 7 | Guarantee Batch job/pool cleanup on failure. | MAINT-007 | Prevent leaked resources and cost. |

### Phase 1 - Safety Net and High-Return Hot Paths (2-6 weeks)

> **Execution status (2026-07-15): Completed locally.**
> Phase 1 implementation and local validation are complete. Azure-dependent and cost-bearing
> suites are automated but require the documented repository OIDC/resource configuration before
> their first hosted run.

1. Automate dependent SQL Server/PostgreSQL tests and scheduled external runtime tests.
2. Add targeted regression tests for all Phase 0 defects.
3. Establish initial benchmarks and telemetry before changing major hot paths.
4. Eliminate duplicate/per-script status queries and per-table row-count queries.
5. Cache blob clients and asynchronously initialize containers.
6. Cache/generated-compile policy and script-optimization regexes.
7. Fix SQL result accumulation, script batching, target chunking, and commit-log chunking.
8. Replace blocking waits/sleeps in asynchronous runtime paths.
9. Harden XML parsing and container installer integrity.
10. Add strict PowerShell error handling and portable path resolution.

#### Phase 1 Implementation Evidence

| Item | Status | Evidence |
|---:|---|---|
| 1 | Complete | Added scheduled/manual/labeled dependent-test workflow and scheduled/manual/tagged external SQL Server/PostgreSQL workflows with OIDC, generated credentials, cleanup, and artifact collection. Added the missing `SqlSync.DbInformation.UnitTest` project to pull-request CI. |
| 2 | Complete for code/runtime defects | Added or confirmed regression coverage and fixes for fail-closed encryption, ACI unknown errors, Event Hub elapsed time, ObjectValidator connection scope, Release Batch packaging, connection pooling, and Batch cleanup. Cloud-dependent behavior is exercised by the scheduled dependent/external tiers. SEC-001's PostgreSQL network-posture change remains a Phase 0 infrastructure prerequisite and was not broadened into this Phase 1 execution. |
| 3 | Complete | Added BenchmarkDotNet baselines for policy checks, regex caching, and target chunking plus a scheduled/manual/labeled workflow and result artifacts. |
| 4 | Complete | Added set-based, parameterized status prefetch grouped by target database and replaced per-table `sp_spaceused` calls with one catalog/partition query. |
| 5 | Complete | Added concurrency-safe cached blob-container initialization, fault eviction for retry, async creation/upload paths, and cache eviction on deletion. |
| 6 | Complete | Cached compiled policy, script-handling, optimization, and batching regexes. |
| 7 | Complete | Made SQL output/logging linear, precomputed comment spans for single-pass script transformations, materialized target input once, and chunked commit logging below SQL Server's parameter limit. |
| 8 | Complete | Replaced blocking waits/sleeps in targeted asynchronous Queue, Storage, Threaded, ARM, ACI, Batch, Container Apps, and Worker paths. Remaining synchronous kubectl polling stays with its synchronous process API pending Phase 3 decomposition. |
| 9 | Complete | Explicitly prohibited DTDs and external XML resolution; installed Azure CLI from Microsoft's signed repository; pinned kubectl v1.33.0 and verified architecture-specific SHA-256 hashes. |
| 10 | Complete | Added strict/error-stop behavior, portable repository/script path resolution, and native command checks to active scripts used by the new workflows and Batch packaging. |

#### Phase 1 Validation Evidence

- Release `net10.0` console build: succeeded with 0 errors.
- Seven isolated unit-test projects: 2,463 passed, 7 skipped, 0 failed.
- Benchmark project: Release build succeeded; dry smoke run produced a measured result.
- Eight changed PowerShell scripts: parser validation succeeded.
- All GitHub Actions workflow YAML files: parser validation succeeded.
- `git diff --check`: succeeded.
- kubectl v1.33.0 amd64/arm64 checksums match the values published by `dl.k8s.io`.

### Phase 2 - Build, Supply Chain, and Configuration (1-2 sprints)

1. Adopt `Directory.Packages.props` and a reproducible restore policy.
2. Add pull-request CodeQL, secret scanning, NuGet audit, SBOM, provenance, and image scanning.
3. Refactor Dockerfiles for cached restore and a single runtime distribution model.
4. Centralize validated execution options for names, timeouts, resource sizes, SAS duration, retry, and polling.
5. Normalize documented exit codes and operational error contracts.

### Phase 3 - Architectural Refactoring (1-2 quarters)

1. Introduce execution-backend interfaces and move Azure SDK dependencies out of the CLI.
2. Convert `Worker` and command handlers from static state to injected instance services.
3. Introduce injectable Azure client boundaries and decompose Batch, Queue, Storage, and Kubernetes managers.
4. Replace global mutable logging configuration with run-scoped DI-managed logging.
5. Move platform-specific authentication and SQL behavior behind providers.
6. Decompose `ObjectScriptHelper` and `InfoHelper`; remove recursive retry and duplicated SQL Server-only logic.

### Phase 4 - Long-Term Quality Controls

- Require benchmark comparison for core hot-path changes.
- Track performance and cost budgets for representative local, container, and Batch workloads.
- Keep dependency, container digest, SBOM, and provenance automation current.
- Maintain a documented local-development and test-tier workflow.
- Review remaining `null!`, bare catches, ignored tests, obsolete APIs, duplicated scripts, and command-documentation drift.

## Prioritization and Ownership

| Workstream | Suggested owner | Depends on | Completion signal |
|---|---|---|---|
| PostgreSQL network/auth hardening | Cloud infrastructure + security | Connectivity requirements | Effective-rule test and successful approved-network deployment |
| Cryptography failure behavior | Core library | None | Failure-path unit tests and no plaintext persistence |
| Batch Release packaging | Build/release | None | Release artifact manifest and workload comparison |
| Pooling/token/round-trip changes | Data access | Baseline telemetry | Reduced physical logins and unchanged functional tests |
| Runtime correctness defects | Runtime/backend owners | None | Targeted regression tests |
| Test-tier automation | DevOps + test | Disposable test environment | Scheduled/labeled suites report and clean up |
| Hot-path optimization | Core runtime | Benchmarks | Measured improvement without output changes |
| Backend architecture | Application architecture | Phase 1 safety net | CLI depends on abstractions; backends independently testable |
| Supply-chain controls | DevOps + security | Registry/workflow support | Audit, SBOM, provenance, and scan gates pass |

## Validation Strategy

Every remediation pull request should:

1. Add or update a test that fails before the fix when the finding is behaviorally testable.
2. Run the smallest affected unit/dependent suite and the solution build.
3. Run both SQL Server and PostgreSQL dependent tests for shared connection/build changes.
4. Run the applicable external backend smoke test for orchestration changes.
5. Compare benchmark or operational counters for performance findings.
6. Verify logs contain actionable context without credentials, keys, tokens, or full sensitive connection strings.
7. Verify cancellation, timeout, transient failure, and cleanup behavior.
8. Document intentional behavior/configuration changes and rollback steps.

Recommended release gates:

- No Critical/High known dependency or container-image vulnerability without an approved exception.
- No detected tracked secret.
- Successful Release build and unit tests.
- Successful scheduled dependent tests before release.
- Successful targeted external tests for changed backends.
- SBOM and provenance attached to released images/packages.
- No regression beyond an agreed threshold in representative core benchmarks.

## Suggested Tracking Metrics

| Metric | Baseline required | Target direction |
|---|---:|---|
| Physical DB logins per representative run | Yes | Down |
| Status/metadata DB round trips | Yes | Down to set-based/bounded counts |
| Blob create/check operations per worker/run | Yes | At most one per container |
| Policy-check and script-batching elapsed time | Yes | Down |
| Allocated bytes for policy/batching/execution | Yes | Down |
| Batch task duration using identical workload | Yes | Down after Release packaging |
| Container image size and incremental build time | Yes | Down |
| External-test workflow wall time | Yes | Down through bounded parallelism |
| Cloud resource leaks after injected failures | Yes | Zero |
| Dependent/external test pass rate and flake rate | Yes | Higher pass rate, lower flake rate |
| High/Critical dependency/image findings | No | Zero or time-bound approved exceptions |

## Definition of Done

The remediation program is complete when:

- Phase 0 defects are fixed with regression tests.
- Shared database paths use an explicitly tested pooling/token policy.
- Core N+1 and blocking hot paths meet documented budgets.
- Dependent and external test tiers run automatically at an appropriate cadence.
- Released containers and Batch packages are reproducible, scanned, provenance-enabled Release artifacts.
- CLI orchestration depends on backend abstractions rather than directly owning every implementation.
- Shared logging and worker execution no longer depend on unsynchronized global mutable state.
- Cloud cleanup, timeout, cancellation, and error contracts are documented and tested.
- Security and performance gates are enforced in CI/CD rather than relying on manual review.
