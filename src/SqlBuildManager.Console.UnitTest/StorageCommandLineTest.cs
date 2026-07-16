using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using System.IO;
using System.Linq;

namespace SqlBuildManager.Console.UnitTest
{
    [TestClass]
    public class StorageCommandLineTest
    {
        [TestMethod]
        public void StorageList_RequiredArguments_Parses()
        {
            var result = CommandLineBuilder.Parse(
            [
                "storage",
                "list",
                "--container",
                "batch-output",
                "--prefix",
                "worker-1/"
            ]);

            Assert.IsEmpty(result.Errors);
            Assert.AreEqual("batch-output", result.GetValue(CommandLineBuilder.storageContainerOption));
            Assert.AreEqual("worker-1/", result.GetValue(CommandLineBuilder.storagePrefixOption));
        }

        [TestMethod]
        public void StorageDownload_MultipleBlobs_ParsesAllNames()
        {
            var outputPath = Path.GetTempPath();
            var result = CommandLineBuilder.Parse(
            [
                "storage",
                "download",
                "--container",
                "batch-output",
                "--blob",
                "worker-1/commits.log",
                "worker-2/commits.log",
                "--outputpath",
                outputPath
            ]);

            Assert.IsEmpty(result.Errors);
            CollectionAssert.AreEqual(
                new[] { "worker-1/commits.log", "worker-2/commits.log" },
                result.GetValue(CommandLineBuilder.storageBlobOption));
        }

        [TestMethod]
        public void StorageDownload_WithoutBlob_ReturnsParseError()
        {
            var result = CommandLineBuilder.Parse(
            [
                "storage",
                "download",
                "--container",
                "batch-output",
                "--outputpath",
                Path.GetTempPath()
            ]);

            Assert.IsTrue(result.Errors.Any(error => error.Message.Contains("--blob")));
        }
    }
}
