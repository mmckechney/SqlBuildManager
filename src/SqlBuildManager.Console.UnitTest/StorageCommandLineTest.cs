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

        [TestMethod]
        public void StorageDownloadPrefix_RequiredArguments_Parses()
        {
            var outputPath = Path.GetTempPath();
            var result = CommandLineBuilder.Parse(
            [
                "storage",
                "download-prefix",
                "--container",
                "testresults",
                "--prefix",
                "2026-07-20-114546/",
                "--outputpath",
                outputPath
            ]);

            Assert.IsEmpty(result.Errors);
            Assert.AreEqual(
                "2026-07-20-114546/",
                result.GetValue(CommandLineBuilder.storageDownloadPrefixOption));
        }

        [TestMethod]
        public void StorageDownloadPrefix_WithoutPrefix_ReturnsParseError()
        {
            var result = CommandLineBuilder.Parse(
            [
                "storage",
                "download-prefix",
                "--container",
                "testresults",
                "--outputpath",
                Path.GetTempPath()
            ]);

            Assert.IsTrue(result.Errors.Any(error => error.Message.Contains("--prefix")));
        }

        [TestMethod]
        public void StorageDownloadAll_RequiredArguments_Parses()
        {
            var outputPath = Path.GetTempPath();
            var result = CommandLineBuilder.Parse(
            [
                "storage",
                "download-all",
                "--container",
                "testresults",
                "--outputpath",
                outputPath
            ]);

            Assert.IsEmpty(result.Errors);
            Assert.AreEqual("testresults", result.GetValue(CommandLineBuilder.storageContainerOption));
            Assert.AreEqual(outputPath, result.GetValue(CommandLineBuilder.storageOutputPathOption)?.FullName);
        }

        [TestMethod]
        public void StorageDownloadAll_WithoutOutputPath_ReturnsParseError()
        {
            var result = CommandLineBuilder.Parse(
            [
                "storage",
                "download-all",
                "--container",
                "testresults"
            ]);

            Assert.IsTrue(result.Errors.Any(error => error.Message.Contains("--outputpath")));
        }
    }
}
