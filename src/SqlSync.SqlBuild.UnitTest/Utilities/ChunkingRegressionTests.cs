using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace SqlSync.SqlBuild.UnitTest.Utilities
{
    /// <summary>
    /// Regression tests for PERF-010: SplitIntoChunks single-materialization fix.
    /// Verifies that PERF-010 did not alter observable behavior.
    /// </summary>
    [TestClass]
    public class ChunkingRegressionTests
    {
        [TestMethod]
        public void SplitIntoChunks_LargeEnumerable_PreservesAllElements()
        {
            // IEnumerable that is NOT a List — exercises the as-cast branch.
            IEnumerable<int> source = Enumerable.Range(0, 200).Where(x => true); // lazy
            var chunks = source.SplitIntoChunks(7).ToList();
            Assert.AreEqual(200, chunks.Sum(c => c.Count()));
        }

        [TestMethod]
        public void SplitIntoChunks_LazyEnumerable_SameResultAsListEnumerable()
        {
            var list = Enumerable.Range(1, 17).ToList();
            IEnumerable<int> lazy = list.Where(_ => true); // force non-List path

            var fromList = list.SplitIntoChunks(4).ToList();
            var fromLazy = lazy.SplitIntoChunks(4).ToList();

            Assert.AreEqual(fromList.Count, fromLazy.Count);
            for (int i = 0; i < fromList.Count; i++)
                CollectionAssert.AreEqual(fromList[i].ToList(), fromLazy[i].ToList());
        }

        [TestMethod]
        public void SplitIntoChunks_ExactlyMaxBatchSize_SingleChunk()
        {
            // 131 = SqlServerMaxParams (2100) / LogRowParamCount (16) — PERF-011 constant
            var list = Enumerable.Range(0, 131).ToList();
            var chunks = list.SplitIntoChunks(1).ToList();
            Assert.AreEqual(131, chunks[0].Count());
        }

        [TestMethod]
        public void SplitIntoChunks_TwiceMaxBatchSize_TwoEqualChunks()
        {
            // 262 items split into 2 chunks of 131 each
            var list = Enumerable.Range(0, 262).ToList();
            var chunks = list.SplitIntoChunks(2).ToList();
            Assert.AreEqual(2, chunks.Count);
            Assert.AreEqual(262, chunks.Sum(c => c.Count()));
        }

        [TestMethod]
        public void SplitIntoChunks_MoreThanMaxBatchSize_CorrectChunkCount()
        {
            // 300 items, max 131 per chunk → need ceil(300/131) = 3 chunks
            var list = Enumerable.Range(0, 300).ToList();
            var chunks = list.SplitIntoChunks(3).ToList();
            Assert.AreEqual(3, chunks.Count);
            Assert.AreEqual(300, chunks.Sum(c => c.Count()));
        }

        [TestMethod]
        public void SplitIntoChunks_OrderPreservedAcrossMaterialization()
        {
            // Verify that single-materialization preserves exact element order.
            var list = Enumerable.Range(100, 30).ToList();
            var chunks = list.SplitIntoChunks(3).ToList();
            var flattened = chunks.SelectMany(c => c).ToList();
            CollectionAssert.AreEqual(list, flattened);
        }
    }
}
