using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.Queue;
using SqlBuildManager.Console.Threaded;
using SqlSync.SqlBuild.MultiDb;
using System.IO;
namespace SqlBuildManager.Console.UnitTest
{
    [TestClass()]
    public class QueueMessageTest
    {
        [TestMethod]
        public void QueueMessage_Creation_Test()
        {
            string tmpFile = string.Empty;
            MultiDbData multiData;

            try
            {
                (tmpFile, multiData) = ConcurrencyTest.GetMultiDbData(ConcurrencyTest.MultiDbType.DoubleTarget);
                var output = Concurrency.ConcurrencyByInt(multiData, 10);
                var qMgr = new QueueManager("", "testing", CommandLine.ConcurrencyType.Count,true);

                var messages = qMgr.CreateMessages(output, "testing", CommandLine.ConcurrencyType.Count);
            }
            finally
            {
                if (File.Exists(tmpFile))
                {
                    File.Delete(tmpFile);
                }
            }
        }

        [TestMethod]
        public void QueueMessage_Creation_FromTag_Test()
        {
            string tmpFile = string.Empty;
            MultiDbData multiData;

            try
            {
                (tmpFile, multiData) = ConcurrencyTest.GetMultiDbData(ConcurrencyTest.MultiDbType.Tag);
                var output = Concurrency.ConcurrencyByType(multiData, 10, CommandLine.ConcurrencyType.Tag);
                var qMgr = new QueueManager("", "testing", CommandLine.ConcurrencyType.Count,true);

                var messages = qMgr.CreateMessages(output, "testing", CommandLine.ConcurrencyType.Count);
                Assert.AreEqual(2367, messages.Count);
            }
            finally
            {
                if (File.Exists(tmpFile))
                {
                    File.Delete(tmpFile);
                }
            }
        }

        [TestMethod]
        public void QueueMessage_Creation_FromTag_MissingTag_Test()
        {
            string tmpFile = string.Empty;
            MultiDbData multiData;

            try
            {
                (tmpFile, multiData) = ConcurrencyTest.GetMultiDbData(ConcurrencyTest.MultiDbType.SingleTarget);
                var output = Concurrency.ConcurrencyByType(multiData, 10, CommandLine.ConcurrencyType.Count);
                var qMgr = new QueueManager("", "testing", CommandLine.ConcurrencyType.Tag, true);

                var messages = qMgr.CreateMessages(output, "testing", CommandLine.ConcurrencyType.Tag);
                Assert.IsNull(messages);
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
