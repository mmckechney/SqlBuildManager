# Dependency and supply-chain policy

## Central package management

NuGet versions are declared once in `src/Directory.Packages.props`. Project files contain
`PackageReference` entries without version attributes. When adding a package:

1. Add its version to `Directory.Packages.props`.
2. Add the unversioned `PackageReference` to the consuming project.
3. Refresh and commit every changed `packages.lock.json`.

The repository intentionally aligns shared test tooling and Microsoft.Extensions packages to one
version. Use `VersionOverride` only for a documented compatibility exception.

## Reproducible restore

Every project has a committed `packages.lock.json`. Normal local restores may update a lock file;
CI and container builds use locked mode and fail when a project file, central version, and lock file
do not agree.

Refresh locks after an intentional dependency change:

```powershell
dotnet restore .\src\SqlBuildManager-console.sln --force-evaluate
dotnet restore .\src\SqlSync.DbInformation.UnitTest\SqlSync.DbInformation.UnitTest.csproj --force-evaluate
dotnet restore .\src\SqlSync.SqlBuild.Benchmarks\SqlSync.SqlBuild.Benchmarks.csproj --force-evaluate
```

Verify the committed graph:

```powershell
dotnet restore .\src\SqlBuildManager-console.sln --locked-mode
dotnet restore .\src\SqlSync.DbInformation.UnitTest\SqlSync.DbInformation.UnitTest.csproj --locked-mode
dotnet restore .\src\SqlSync.SqlBuild.Benchmarks\SqlSync.SqlBuild.Benchmarks.csproj --locked-mode
```

`Directory.Build.props` enables NuGet auditing for direct and transitive packages. CI treats high
and critical advisories (`NU1903` and `NU1904`) as errors.

## Automated controls

Pull requests run:

- CodeQL analysis.
- Verified-secret scanning with TruffleHog.
- Locked NuGet restore and vulnerability auditing.
- A local container build followed by Trivy high/critical vulnerability scanning.

Images are pushed only after the vulnerability scan succeeds. Published images include BuildKit
SBOM and provenance attestations, plus a GitHub build-provenance attestation. Dependabot tracks
NuGet, Docker base images, and GitHub Actions.

GitHub secret scanning and push protection are repository settings, not workflow files. Enable both
under **Settings > Code security and analysis** in addition to the pull-request scanner.

## Container distribution

The production CLI and RelayProxy use framework-dependent .NET 10 output on the matching
`mcr.microsoft.com/dotnet/runtime:10.0` image. Dockerfiles copy central package metadata, project
manifests, and lock files before source so source-only changes reuse the restore layer. Test images
retain the SDK runtime because they execute `dotnet vstest`.
