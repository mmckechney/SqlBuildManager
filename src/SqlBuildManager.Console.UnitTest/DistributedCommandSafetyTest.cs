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

            var linux = manager.CompileCommandLines(cmdLine, new List<ResourceFile>(), "sas", 1, "job", OsType.Linux, BatchManager.BatchType.Run)[0];

            Assert.IsFalse(linux.Contains("printenv"));
            StringAssert.StartsWith(linux, "/bin/sh -c '/app/sbm ");
            Assert.IsFalse(linux.Contains("AZ_BATCH_APP_PACKAGE_"));
        }

        [TestMethod]
        public void BatchManager_GetBatchContainerImage_NormalizesRegistryServer()
        {
            var cmdLine = new CommandLineArgs();
            cmdLine.ContainerRegistryArgs.RegistryServer = "https://example.azurecr.io/";
            cmdLine.ContainerRegistryArgs.ImageName = "sqlbuildmanager";
            cmdLine.ContainerRegistryArgs.ImageTag = "latest-vNext";

            var image = BatchManager.GetBatchContainerImage(cmdLine);

            Assert.AreEqual("example.azurecr.io/sqlbuildmanager:latest-vNext", image);
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
