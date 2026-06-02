using Microsoft.Azure.Batch;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.Batch;
using SqlBuildManager.Console.CommandLine;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.UnitTest
{
    [TestClass]
    public class DistributedCommandSafetyTest
    {
        [TestMethod]
        public void BatchManager_CompileCommandLines_DoesNotDumpEnvironment()
        {
            var cmdLine = new CommandLineArgs();
            var manager = new BatchManager(cmdLine);

            var windows = manager.CompileCommandLines(cmdLine, new List<ResourceFile>(), "sas", 1, "job", OsType.Windows, "sbm", BatchManager.BatchType.Run)[0];
            var linux = manager.CompileCommandLines(cmdLine, new List<ResourceFile>(), "sas", 1, "job", OsType.Linux, "sbm", BatchManager.BatchType.Run)[0];

            Assert.IsFalse(windows.Contains("cmd /c set"));
            Assert.IsFalse(linux.Contains("printenv"));
        }

        [TestMethod]
        public async Task AciDeploy_MissingSubscriptionId_ReturnsFailure()
        {
            var cmdLine = new CommandLineArgs();
            cmdLine.IdentityArgs.SubscriptionId = string.Empty;

            var result = await Worker.AciDeploy(cmdLine, monitor: false, unittest: true);

            Assert.AreEqual(1, result);
        }
    }
}
