using BenchmarkDotNet.Attributes;
using SqlBuildManager.Enterprise.Policy;
using SqlBuildManager.ScriptHandling;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SqlBuildManager.SqlBuild.Benchmarks
{
    /// <summary>
    /// Benchmarks for PERF-006: policy class regex caching.
    /// Uses only public policy classes.  All inputs are in-memory strings; no external services required.
    /// </summary>
    [MemoryDiagnoser]
    public class PolicyBenchmarks
    {
        private const string ScriptWithNoProc =
            "SELECT Id, Name FROM dbo.Customers WITH (NOLOCK) WHERE Status = 1";

        private const string ScriptWithSelectFromJoin =
            "SELECT a.Id FROM dbo.Orders a INNER JOIN dbo.Customers b ON a.CustomerId = b.Id WHERE a.Status = 1";

        private static readonly List<Match> EmptyCommentBlocks = new List<Match>();

        private CommentHeaderPolicy _commentHeaderPolicy = null!;
        private WithNoLockPolicy _withNoLockPolicy = null!;

        [GlobalSetup]
        public void Setup()
        {
            _commentHeaderPolicy = new CommentHeaderPolicy();
            _withNoLockPolicy = new WithNoLockPolicy();
        }

        [Benchmark(Baseline = true, Description = "CommentHeaderPolicy-NoRoutine")]
        public bool CommentHeaderPolicy_NoRoutine()
        {
            _commentHeaderPolicy.CheckPolicy(ScriptWithNoProc, EmptyCommentBlocks, out _);
            return true;
        }

        [Benchmark(Description = "WithNoLockPolicy-HasNoLock")]
        public bool WithNoLockPolicy_HasNoLock()
        {
            return _withNoLockPolicy.CheckPolicy(ScriptWithNoProc, EmptyCommentBlocks, out _);
        }
    }
}
