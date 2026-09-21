# Cybersecurity assessment and remediation summary

Assessed source revision: `f955d1082d1920d400c0a6c88816cde5bd8784fb`.

**Status: historical assessment with subsequently authorized, scoped source remediation.** Finding descriptions and line references below describe the assessed revision, not an assertion that every finding remains unchanged. Live deployment/security sign-off is separate.

### Owner decisions and authorized scope

- Preserve intentional mixed encrypted/plaintext settings-file behavior. The fail-closed persistence proposal in R6/SEC-02 is explicitly declined; this is not a completed fix.
- Implement general ACI secure environment transport, verified Azure PostgreSQL TLS, and R3 identity separation while retaining migration-capable database permissions.
- R3 applies to **fresh disposable deployments only**. No migration tooling, automatic revocation of legacy roles, or live Azure changes are authorized by this source task. Incremental Bicep deployment does not remove legacy assignments.
- Preserve native local/local-container authentication. Their PostgreSQL servers have no provisioned TLS certificates; existing local TLS compatibility remains intentional.
- Earlier Azure MySQL work adds managed-identity test authentication, explicit test-database grants, verified token-mode TLS, and secure bootstrap-token transport. It does not close every work package below.

### Implemented source changes

ACI now uses secure environment properties for runtime credentials, connection strings, storage
keys and SAS URLs. PostgreSQL uses certificate/hostname verification for Entra modes and canonical
Azure endpoints, including the Azure-test helper and bootstrap client. Native local TLS behavior
is retained because those servers have no provisioned certificates.

Fresh-deployment infrastructure separates workers, orchestration, Relay, AKS control plane/kubelet,
Batch account storage, private bootstrap and MySQL directory lookup. Runtime settings select the
worker; trusted test-runner Azure operations explicitly select the orchestrator. Worker database
grants retain migration permissions but target generated test databases only. Relay has separate
SQL grants. SQL bootstrap restores each captured administrator; PostgreSQL/MySQL bootstrap use
short-lived deploying-user tokens without granting the bootstrap identity those administrator
roles. Each token is isolated to its intended database subprocess and cleared afterward.

This is source remediation, not a deployed security attestation. Existing environments, effective
inherited Azure roles, live certificate handshakes and service-side authorization remain outside
the completed offline scope. Abrupt host/process termination can prevent `finally` cleanup;
automatic recovery or reconciliation of legacy/crash state is not claimed.

## Scope and threat model

The assessment examined security-sensitive application paths, database connections, package handling, secrets, distributed execution, the Relay proxy, and the effective `azure.yaml` -> `azd up` Bicep/preprovision/postprovision path. It also reviewed container build contexts and CI artifact-security controls.

There are two distinct deployment contexts:

- **Existing test design:** the owner states that test environments are intended to be short-lived, disposable and entirely disconnected from production, especially with respect to credentials. Findings involving those test credentials must not be presented as evidence of production credential exposure or a path into production.
- **Production-readiness target:** production-grade defaults with private data-plane access, including a private-capable Service Bus Premium configuration, and an explicitly selected, isolated development mode. Findings concerning future production use assess that target, not a verified live production deployment.

Live Azure state, actual isolation/teardown, historical image contents and credential reuse were not independently verified. Confidence scores concern source evidence, not proof of exploitation or deployed exposure. No Critical finding is asserted.

## Findings

Four findings are High for production use, three are Medium, and the test-image finding is Low under the stated disposable, production-isolated design.

| # | Severity | File | Lines | Vulnerability | Confidence |
|---|----------|------|-------|---------------|------------|
| 1 | ⚪ LOW | `src\Dockerfile.tests`; `scripts\ContainerRegistry\build_external_test_image.ps1` | 27-34, 68; 68-75 | Disposable test credentials and their encryption key are included in the external-test image | 10/10 |
| 2 | 🟠 HIGH | `src\SqlBuildManager.Console\Aci\AciManager.cs`; `scripts\ContainerRegistry\run_private_postprovision_container.ps1` | 208-227; 143-154 | General ACI runtime secrets use ordinary readable environment properties; the isolated test-bootstrap example has lower impact | 10/10 |
| 3 | 🟠 HIGH | `infra\modules\identity.bicep`; `infra\modules\aks.bicep` | 62-99; 16-26, 45-49 | Shared runtime/control-plane identity has infrastructure and AKS administrative privileges | 10/10 |
| 4 | 🟠 HIGH | `src\SqlBuildManager.Connection\PostgresConnectionFactory.cs`; `MySqlConnectionFactory.cs` in the same directory | 81-99; 84-101 | Database TLS does not verify server identity | 10/10 |
| 5 | 🟠 HIGH | `src\SqlBuildManager.SqlBuild\SqlBuildRunner.cs`; `src\SqlBuildManager.Console\Threaded\ThreadedManager.cs` | 253-265; 605-613 | Package-controlled filenames can read outside the package directory | 9/10 |
| 6 | 🟡 MEDIUM | `src\SqlBuildManager.Console\CommandLine\Cryptography.cs`; `Worker\Worker.Settings.cs` under the console project | 20-29; 17-28, 78-105 | Missing encryption key can result in a successful plaintext settings write | 10/10 |
| 7 | 🟡 MEDIUM | `infra\main.bicep`; `infra\modules\servicebus.bicep` | 70; 38, 73-82, 113-154 | Default Standard Service Bus remains public despite private-endpoint mode | 10/10 |
| 8 | 🟡 MEDIUM | `src\SqlBuildManager.RelayProxy\EventHubRelayMonitor.cs` | 27-60, 63-73, 124-157 | Authenticated callers can create uncapped monitoring sessions | 9/10 |

### R1 - Test-image credential inclusion: Low, P2 hygiene

The Dockerfile does **not** contain literal hardcoded production passwords. The source chain instead copies generated environment settings:

1. `infra\scripts\preprovision.ps1:168-177` generates a random MySQL administrator password when the active azd environment has no existing value. It retains an existing value, rather than rotating per test run.
2. `infra\main.parameters.json` passes that password to the MySQL deployment. The same deployment password configures both MySQL servers.
3. `infra\scripts\postprovision.ps1:154-172` generates settings in `src\TestConfig`, including password settings when MySQL password authentication is selected.
4. `scripts\create_all_settingsfiles_mysql_password.ps1:39-44` reads that environment's administrator credentials. Platform settings generators store an encryption key beside the encrypted settings, for example `scripts\aci\create_aci_settingsfile_mi_only.ps1:108-119`.
5. `build_external_test_image.ps1:68-75` includes TestConfig in its context and removes the copied `.dockerignore`.
6. `src\Dockerfile.tests:34,68` copies TestConfig into the final image.

**Impact in the stated test design:** a person able to pull the image can recover administrator credentials for the disposable test database servers. They still need network access to those private endpoints to use them. Potential impact is test-data access or destruction, interference with test results and consumption of test database resources during the environment's lifetime. The database password alone does not grant Azure subscription administration or managed-identity permissions.

The Low rating relies on the stated short lifetime and absence of production connections or credential sharing. It is not a production incident finding or an emergency production credential-rotation requirement. Once the relevant servers are destroyed and credentials are not reused, retained image copies no longer provide access to those removed servers.

Reassess severity if test environments become persistent, contain sensitive data, reuse credentials, gain production connectivity, or broaden image/network access beyond intended test operators. Embedding production credentials through the same mechanism could merit High, but that is a conditional risk, not the established scenario.

**Recommended hardening:** prefer source-only images with runtime injection of scoped test credentials; enforce isolation, restrict image readers and verify teardown. Retire artifacts appropriately, recognizing that deleting a tag does not erase previously pulled copies. Rotate still-valid credentials if unintended exposure or reuse warrants it, not unrelated production credentials.

The dependent-test build also uploads an insufficiently filtered context, but equivalent final-image inclusion was not established there. Repeat Relay builds need context protection too. The production runtime build wrapper explicitly excludes TestConfig and should retain that protection.

### R2 - Ordinary ACI environment values

`EnvironmentVariableHelper.cs:43-60` can supply connection strings, SAS, storage keys and database credentials. `AciManager.cs:208-227` serializes every value through ordinary `Value`, not `SecureValue`. Resource readers can consequently obtain secrets without execution inside the container.

The bootstrap script additionally passes an unused MySQL administrator password with `--environment-variables` and leaves the completed container-group configuration in place. **That test-bootstrap example is Low under R1's isolation assumptions.** The High rating concerns the general application's ability to expose production runtime secrets when used in production.

Remove the unused bootstrap password and use target-appropriate secure transport or runtime secret retrieval. Review Batch separately because its environment settings have different read permissions. Preserve existing Container Apps secret references and Kubernetes Secret handling. Secure values do not protect against a compromised process inside the container.

### R3 - Shared excessive privileges

The common identity has resource-group Contributor and AKS administrative roles and is reused for worker federation/control-plane duties. Relay also receives the broad runtime identity. Database grant scripts grant broad rights, and generated MySQL password settings use the server administrator.

For production use, a compromised worker or Relay can therefore have infrastructure-management and cross-workload data privileges beyond its migration task. Contributor is not Owner and does not itself grant arbitrary RBAC role-assignment authority. This finding does not establish connectivity from the existing test environment into production.

Separate deployment/orchestration, AKS control plane/kubelet, workers, Relay and bootstrap identities; scope data roles and database targets explicitly. Preserve necessary migration DDL on approved databases without granting cloud administration.

PostgreSQL retains bootstrap administrator assignments. MySQL MI mode reuses a bootstrap identity for persistent server identity/administrator duties and can require Graph directory-read permissions. Separate persistent server authentication from temporary initialization. SQL administrator restoration must restore captured prior state, not merely the configured deployer; cleanup failures must be explicit.

### R4 - Unverified database TLS

PostgreSQL uses Prefer for password authentication and Require for token modes; MySQL uses Preferred and Required. These modes do not verify the expected server identity. Password preferences also permit plaintext fallback where a server allows it. Azure's server TLS enforcement and private endpoints do not replace client-side certificate verification.

The psql grant hook does not explicitly require certificate-verified SSL; the MySQL hook uses `--ssl-mode=REQUIRED` with the cleartext authentication plugin.

Enforce VerifyFull-equivalent policy with trusted roots and hostname verification across factories, hooks, settings and workers. Require explicit development mode for insecure local exceptions and never retry with weaker TLS after verification fails.

References: [Npgsql SSL security](https://www.npgsql.org/doc/security.html), [MySqlConnector SSL options](https://mysqlconnector.net/connection-options/).

### R5 - Package metadata escapes the package directory

Package XML controls `Script.FileName`. Schema validation in `SqlBuildFileHelper.cs:38-42` unconditionally succeeds, and threaded validation only checks whether `Path.Combine(projectDirectory, fileName)` exists. The batcher and runner subsequently read those paths without containment enforcement.

A malicious package publisher can reference a known file readable by the worker outside the package directory. Contents can reach database commands or "Offending Script" failure details. No live secret was read and no complete exploit package was executed.

This is metadata-path traversal, not a demonstrated ZIP full-path traversal: ZIP extraction uses basenames. Related unchecked import/export helpers also need protection, but current CLI reachability of their destructive operations was not established.

Apply centralized canonical containment and manifest-membership validation to reads, hashing, caching, import/export and writes. Reject rooted/UNC/drive/parent escapes, handle Windows/Linux semantics, enforce a symlink/reparse-point policy and reject duplicate flattened filenames. Real schema validation alone is insufficient.

### R6 - Settings persistence fails open

Encryption returns unchanged arguments after logging a missing-key error. The settings writer ignores initialization success, serializes the returned arguments and can write plaintext successfully without explicit `--cleartext`.

**Owner disposition:** mixed encrypted/plaintext settings are intentional. The proposed fail-closed settings persistence change is not to be implemented. Preserve this behavior; secure ACI transport is a separate, authorized remediation. The modern authenticated encryption format itself is not the demonstrated defect.

### R7 - Private-endpoint flag does not guarantee private Service Bus

The effective azd parameters enable private endpoints but retain Standard Service Bus. Private endpoints and relevant network rules are conditional on Premium; public access remains enabled.

Authentication is still required. This is a production network-isolation gap, not an anonymous access finding. Storage/Event Hub retain public endpoints with selected IP/subnet restrictions; they are not equivalent to an unrestricted public endpoint. SQL Server/PostgreSQL/MySQL modules disable public access.

Validate private-capable SKUs and disabled public data-plane access in a production profile. Fail invalid combinations rather than silently skipping private endpoints. Move required clients onto private connectivity; do not silently exempt the public Relay from the agreed production boundary.

### R8 - Authenticated Relay resource exhaustion

Each accepted monitoring session creates a consumer and partition readers with a separate 5,000-item queue. Session count and aggregate buffered bytes are not capped; active polling refreshes the idle timeout. Requests lack an application-wide concurrency gate, and some operations use no request cancellation or collect unbounded listings.

The attack requires an authorized Azure Relay Sender, not an anonymous caller. Add atomic global limits, trusted-identity quotas where feasible, byte budgets, cancellation/deadlines, pagination and predictable overload responses. Preserve existing Dacpac job caps and serialized extraction.

## Additional production hardening

| Area | Gap and recommendation |
|------|------------------------|
| Authentication policy | Explicitly disable local/SAS authentication where production MI support permits. Storage already disables shared keys; explicitly declare blob-public-access and minimum TLS policy. The Key Vault module is not currently invoked by the main deployment; do not assume it supplies an active secret store. |
| Kubernetes administration | Replace routine postprovision `--admin` credential acquisition with Entra RBAC; disable local accounts and declare a private/restricted API posture with controlled break-glass access. |
| Containers and segmentation | Add non-root execution, supported restrictive security contexts, minimal writable volumes and tested ingress/egress policies. Enabled network-policy support alone does not create policies. |
| Artifact integrity | The release workflow scans one image and rebuilds for publication, so scanned and published digests are not guaranteed identical. Build once, scan/promote the same digest and verify provenance/SBOM/signatures. Cover Relay, bootstrap, test images and azd/ACR builds; include the standalone Relay project in CodeQL coverage. |
| Logs and local secrets | Redact complete secret values rather than retaining password characters. Restrict SQL diagnostic text, log destinations and retention; protect local azd/settings files and exclude them from support bundles. |
| Detection and recovery | Add service-specific audit settings, security alerts, controlled retention, incident/rotation procedures and authorized backup/restore drills. Existing Log Analytics/AKS monitoring is not a complete security evidence trail. |
| Package budgets and provenance | Bound expanded bytes, entries, individual files and parsing work. Package hashes are not publisher authorization. SQL packages remain privileged deployment instructions even after path validation. |

## Prioritized remediation plan

This is the original prioritized backlog, subject to the owner decisions above. Selected source remediations do not constitute completion of the entire backlog or live production sign-off. Production priorities must not be interpreted as an emergency caused by isolated, already-destroyed test credentials.

| Work package | Priority and dependencies | Acceptance criteria |
|--------------|---------------------------|---------------------|
| SEC-01: Source-only build contexts and images | P2 test hygiene; required before reusing the pattern with production secrets | Synthetic settings/key sentinels absent from outgoing contexts and image layers; runtime test configuration works; isolation and teardown verified |
| SEC-02: Secure runtime transport | P0 before supplying production runtime secrets; P2 test-bootstrap cleanup | No sentinel secrets in ordinary ARM environment properties; preserve intentionally mixed settings encryption per owner decision |
| SEC-03: Exposure scope and conditional rotation | Risk-based; final credential cutover follows SEC-01/02 | Determine validity/reuse; close destroyed isolated environments with evidence; rotate only affected still-valid credentials where warranted |
| SEC-04: Verified database TLS | P1; independent | Password/token modes reject wrong-host, untrusted and plaintext endpoints; approved private-DNS connections work on every target |
| SEC-05: Separate identities and migration privileges | P1; design independently | Workers cannot administer infrastructure/AKS, access unrelated databases or assume bootstrap duties; supported jobs still work |
| SEC-06: Temporary bootstrap privilege | P1; follows SEC-02/05 | Success/failure/cancellation/repeat deployment restores exact prior state and removes temporary privileges |
| SEC-07: Package containment and budgets | P1; independent | Windows/Linux malicious paths cannot read, transmit, log, overwrite or delete outside-root sentinels; valid packages still work |
| SEC-08: Private production/development profiles | P2 production baseline; follows SEC-01/04/05/06 | Invalid SKUs/configurations fail early; production public data paths are blocked; private clients work; hooks preserve explicit profile selection |
| SEC-09: Relay quotas and authorization boundaries | P2; integrate with SEC-05 | Concurrent synthetic load stays within configured session/byte/reader limits and recovers predictably |
| SEC-10: Container and network hardening | P2; follows SEC-01/05, integrates SEC-08 | Workloads function without root; forbidden capabilities/filesystem/network actions fail with platform-specific controls |
| SEC-11: Artifact promotion and complete security gates | P2; follows SEC-01, integrates SEC-04/07/10 | Scanned/published/deployed digests match; attestations verify; all projects/images covered; dependency audit has a real current result |
| SEC-12: Detection, runbooks and staged sign-off | P3; follows applicable SEC-03 through SEC-11 | Authorized network/RBAC negative tests, exposure disposition, restore drill and documented exception owners support production sign-off |

Live artifact inspection, credential rotation, permission revocation, image deletion and cloud deployments require separate authorization. No production rotation is justified merely by finding credentials for destroyed, non-reused disposable test servers. If isolation, credential lifetime or data sensitivity changes, reassess the priority.

Preserve supported database platforms and execution targets. Roll out application fixes with focused tests, then use an isolated production-like environment for authorized network/RBAC checks. Rollback must not restore exposed credentials, insecure TLS, public production data paths or broad worker roles.

## Existing controls and assessment limitations

Preserve the existing managed-identity/workload-identity paths, database private endpoints, Storage shared-key prohibition, Relay client authorization/resource allowlists, Container Apps/Kubernetes secret mechanisms, modern authenticated settings encryption, AKS Entra/OIDC controls, and CI CodeQL/Trivy/Dependabot/locked-restore/NuGet audit controls.

The source inventory contained 621 tracked C# files, 20 Bicep files, 78 PowerShell files, 38 project files and 12 YAML files; this is not a claim of exhaustive manual review of every file. All 78 PowerShell files parsed without syntax errors. A synthetic .NET path check confirmed that rooted and parent-relative paths can escape a package root. Thirty-eight lock files contained 157 distinct package/version pairs.

**Dependency advisory matching was blocked by a NuGet TLS handshake failure. There is no clean dependency vulnerability verdict.** Certificate verification was not bypassed.

No live Azure/RBAC/Graph audit, deployed-image inspection, full exploit test, Bicep compilation, full application build/test run, binary/container scan or dedicated history-wide secret scan was completed. Source findings do not prove current deployed exposure or compromise.

No exploitable unauthenticated Relay access, active shell-injection path or XXE chain was established. Inactive modules and excluded code were distinguished from the effective deployment path. Existing test isolation is an owner-stated design assumption, not a live attestation.

The original assessment remains historical evidence. Subsequent source changes and owner decisions are recorded separately; neither this document nor offline validation authorizes live changes to Azure resources, credentials or access.
