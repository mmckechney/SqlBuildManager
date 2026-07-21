using BenchmarkDotNet.Attributes;
using SqlSync.SqlBuild.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace SqlSync.SqlBuild.Benchmarks
{
    /// <summary>
    /// Benchmarks for PERF-010: SplitIntoChunks single-materialization fix and
    /// PERF-011: parameter-chunk constants.
    /// All benchmarks are pure in-memory; no external services required.
    /// </summary>
    [MemoryDiagnoser]
    public class ChunkingBenchmarks
    {
        private List<int>? _smallList;
        private List<int>? _largeList;

        [GlobalSetup]
        public void Setup()
        {
            _smallList = Enumerable.Range(0, 50).ToList();
            _largeList = Enumerable.Range(0, 500).ToList();
        }

        [Benchmark(Baseline = true, Description = "SplitIntoChunks-50items-5chunks")]
        public int SplitSmall_5Chunks()
        {
            var result = _smallList!.SplitIntoChunks(5);
            return result.Count();
        }

        [Benchmark(Description = "SplitIntoChunks-500items-10chunks")]
        public int SplitLarge_10Chunks()
        {
            var result = _largeList!.SplitIntoChunks(10);
            return result.Count();
        }

        [Benchmark(Description = "SplitIntoChunks-500items-131chunks(MaxBatchSize)")]
        public int SplitLarge_MaxBatchChunks()
        {
            // 131 = SqlServerMaxParams / LogRowParamCount — matches PERF-011 constant
            var result = _largeList!.SplitIntoChunks(131);
            return result.Count();
        }
    }
}
