# SqlBuildManager.SqlBuild.Benchmarks

BenchmarkDotNet-based performance baselines for Phase 1 of the SQL Build Manager
remediation roadmap (PERF-017). All benchmarks are pure in-memory — no database or
Azure connectivity required.

## Benchmark suites

| Suite | Findings addressed | What is measured |
|---|---|---|
| `ChunkingBenchmarks` | PERF-010, PERF-011 | `SplitIntoChunks` single-materialization and MaxRowsPerBatch constant |
| `PolicyBenchmarks` | PERF-006 | Policy class regex caching (CommentHeaderPolicy, WithNoLockPolicy) |
| `RegexCachingBenchmarks` | PERF-006 | Static-cached regex vs. per-call construction in script-handling paths |

## Running benchmarks locally

```bash
# Build in Release mode (required by BenchmarkDotNet)
dotnet build src/SqlBuildManager.SqlBuild.Benchmarks/SqlBuildManager.SqlBuild.Benchmarks.csproj --configuration Release

# Run all benchmarks
dotnet run --project src/SqlBuildManager.SqlBuild.Benchmarks/SqlBuildManager.SqlBuild.Benchmarks.csproj \
  --configuration Release --no-build \
  -- --filter '*'

# Run a specific suite
dotnet run --project src/SqlBuildManager.SqlBuild.Benchmarks/SqlBuildManager.SqlBuild.Benchmarks.csproj \
  --configuration Release --no-build \
  -- --filter '*Chunking*'

# Export results (HTML, CSV, JSON)
dotnet run --project src/SqlBuildManager.SqlBuild.Benchmarks/SqlBuildManager.SqlBuild.Benchmarks.csproj \
  --configuration Release --no-build \
  -- --filter '*' --exporters json html csv --artifacts ./BenchmarkResults
```

## CI workflow

The `.github/workflows/benchmarks.yml` workflow runs all benchmarks and uploads
results as GitHub Actions artifacts for comparison. It triggers:

- **Weekly** (Wednesdays 01:00 UTC) — rolling baseline.
- **Manual** (`workflow_dispatch`) — on-demand before/after a hot-path change.
- **Pull requests** labeled `run-benchmarks` — opt-in pre-merge validation.

Artifact naming: `benchmark-results-<run_id>`.

## Interpreting results

BenchmarkDotNet reports:
- **Mean** — average elapsed time per operation.
- **Allocated** — bytes allocated per operation (via `[MemoryDiagnoser]`).

A change is considered a regression when **Mean or Allocated increases by > 10%**
relative to the committed baseline artifact. Phase 2 will automate this comparison;
for Phase 1, review artifacts manually in the Actions tab.

## Adding a new benchmark

1. Create a class annotated with `[MemoryDiagnoser]` and `[SimpleJob]` in this project.
2. Annotate benchmark methods with `[Benchmark]`.
3. Reference only pure, in-memory production code — no live services.
4. Add a row to the table above.

See the [BenchmarkDotNet documentation](https://benchmarkdotnet.org/) for advanced options.
