using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace SqlBuildManager.Console.UnitTest
{
    [TestClass]
    public class StorageWorkerTest
    {
        [TestMethod]
        public void GetDownloadAllDestinationPath_AppendsContainerSubfolder()
        {
            var outputPath = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "sbm-storage"));

            var result = Worker.GetDownloadAllDestinationPath(outputPath, "testresults");

            Assert.AreEqual(
                Path.Combine(outputPath.FullName, "testresults"),
                result);
        }

        [TestMethod]
        public void GetDownloadAllDestinationPath_TrimmedContainerName_AppendsContainerSubfolder()
        {
            var outputPath = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "sbm-storage"));

            var result = Worker.GetDownloadAllDestinationPath(outputPath, "  testresults  ");

            Assert.AreEqual(
                Path.Combine(outputPath.FullName, "testresults"),
                result);
        }

        [TestMethod]
        public void GetDownloadAllDestinationPath_EmptyContainer_Throws()
        {
            var outputPath = new DirectoryInfo(Path.GetTempPath());

            Assert.ThrowsExactly<ArgumentException>(
                () => Worker.GetDownloadAllDestinationPath(outputPath, " "));
        }
    }
}
