using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.Queue;
using SqlBuildManager.Console.Threaded;
using SqlSync.SqlBuild.MultiDb;
using System.IO;
namespace SqlBuildManager.Console.UnitTest
{
    [TestClass()]
    internal class QueueMessageTest
    {
        [TestMethod]
        public void QueueMessage_Creation_Test()
        {
            string tmpFile = string.Empty;
            MultiDbData multiData;

            try
            {
                (tmpFile, multiData) = ConcurrencyTest.GetMultiDbData(true);
                var output = Concurrency.ConcurrencyByInt(multiData, 10);
                var qMgr = new QueueManager("", "testing", CommandLine.ConcurrencyType.Count);

                var messages = qMgr.CreateMessages(output, "testing");
            }
            finally
            {
                if (File.Exists(tmpFile))
                {
                    File.Delete(tmpFile);
                }
            }
        }
    }
}
