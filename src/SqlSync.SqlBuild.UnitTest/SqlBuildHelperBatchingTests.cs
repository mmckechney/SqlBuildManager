using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.Services;

namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class SqlBuildHelperBatchingTests
    {
        [TestMethod]
        public void ReadBatchFromScriptText_SplitsOnGo_IgnoresGoInComments()
        {
            var script = @"SELECT 1;
-- GO in comment
GO
/*
 GO in block comment
*/
SELECT 2;
";
            IScriptBatcher batcher = new DefaultScriptBatcher();
            var batches = batcher.ReadBatchFromScriptText(script, stripTransaction: false, maintainBatchDelimiter: false);
            Assert.AreEqual(2, batches.Count);
            StringAssert.Contains(batches[0], "SELECT 1");
            StringAssert.Contains(batches[1], "SELECT 2");
        }

        [TestMethod]
        public void ReadBatchFromScriptText_MaintainsDelimiter_WhenRequested()
        {
            var script = "SELECT 1;\nGO\nSELECT 2;\n";
            IScriptBatcher batcher = new DefaultScriptBatcher();
            var batches = batcher.ReadBatchFromScriptText(script, stripTransaction: false, maintainBatchDelimiter: true);
            Assert.AreEqual(2, batches.Count);
            StringAssert.Contains(batches[0], "GO");
        }

        [TestMethod, Ignore("Transaction stripping regex behavior under review.")]
        public void ReadBatchFromScriptText_StripsTransactionReferences_WhenRequested()
        {
            var script = @"BEGIN TRANSACTION
SELECT 1;
COMMIT TRANSACTION
";
            IScriptBatcher batcher = new DefaultScriptBatcher();
            var batches = batcher.ReadBatchFromScriptText(script, stripTransaction: true, maintainBatchDelimiter: false);
            Assert.AreEqual(1, batches.Count);
            Assert.IsFalse(batches[0].ToUpperInvariant().Contains("BEGIN TRAN"));
            Assert.IsFalse(batches[0].ToUpperInvariant().Contains("COMMIT TRAN"));
        }
    }
}
