using BenchmarkDotNet.Attributes;
using SqlBuildManager.ScriptHandling;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SqlBuildManager.SqlBuild.Benchmarks
{
    /// <summary>
    /// Validates that static-cached regexes (PERF-006) are faster than per-call construction.
    /// All benchmarks are pure in-memory; no external services required.
    /// </summary>
    [MemoryDiagnoser]
    public class RegexCachingBenchmarks
    {
        private const string SimpleSelect = "SELECT col1, col2 FROM dbo.MyTable WHERE id = 1";
        private const string MultiJoin =
            "SELECT a.Id, b.Name FROM dbo.Orders a INNER JOIN dbo.Customers b ON a.CustomerId = b.Id WHERE a.Status = 1";
        private const string ScriptWithGo =
            "SELECT 1\r\nGO\r\nSELECT 2\r\nGO\r\nSELECT 3";

        private List<Match>? _commentBlocks;

        [GlobalSetup]
        public void Setup()
        {
            _commentBlocks = ScriptHandlingHelper.GetScriptCommentBlocks(MultiJoin);
        }

        [Benchmark(Baseline = true, Description = "ProcessNoLock-CachedRegex")]
        public string ProcessNoLock_Cached()
            => ScriptOptimization.ProcessNoLockOptimization(MultiJoin, _commentBlocks!);

        [Benchmark(Description = "GetCommentBlocks-CachedRegex")]
        public List<Match> GetCommentBlocks_Cached()
            => ScriptHandlingHelper.GetScriptCommentBlocks(MultiJoin);

        [Benchmark(Description = "GetCommentIndexes-CachedRegex")]
        public List<int> GetCommentIndexes_Cached()
            => ScriptHandlingHelper.GetScriptCommentIndexes(MultiJoin);

        [Benchmark(Description = "ScriptBatcher-ReadBatch")]
        public List<string> ScriptBatcher_ReadBatch()
        {
            var batcher = new Services.DefaultScriptBatcher();
            return batcher.ReadBatchFromScriptText(ScriptWithGo, stripTransaction: false, maintainBatchDelimiter: false);
        }
    }
}
