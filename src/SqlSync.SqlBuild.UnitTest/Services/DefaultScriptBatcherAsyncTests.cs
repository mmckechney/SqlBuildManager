using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Services;

namespace SqlSync.SqlBuild.UnitTest.Services
{
    [TestClass]
    public class DefaultScriptBatcherAsyncTests
    {
        [TestMethod]
        public async Task ReadBatchFromScriptFileAsync_BatchesGoDelimiter()
        {
            var tmp = Path.GetTempFileName();
            try
            {
                var content = "SELECT 1;\nGO\nSELECT 2;";
                await File.WriteAllTextAsync(tmp, content);
                var batcher = new DefaultScriptBatcher();

                var batches = await batcher.ReadBatchFromScriptFileAsync(tmp, stripTransaction: false, maintainBatchDelimiter: false);

                Assert.AreEqual(2, batches.Length);
                StringAssert.Contains(batches[0], "SELECT 1");
                StringAssert.Contains(batches[1], "SELECT 2");
            }
            finally
            {
                File.Delete(tmp);
            }
        }
    }
}
