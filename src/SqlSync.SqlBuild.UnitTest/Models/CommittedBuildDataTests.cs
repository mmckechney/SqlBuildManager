using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Models;
using System;

namespace SqlSync.SqlBuild.UnitTest.Models
{
    [TestClass]
    public class CommittedBuildDataTests
    {
        [TestMethod]
        public void DefaultConstructor_InitializesObject()
        {
            // Arrange & Act
            var data = new CommittedBuildData();

            // Assert
            Assert.IsNotNull(data);
            Assert.AreEqual(string.Empty, data.BuildFileName);
            Assert.AreEqual(0, data.ScriptCount);
            Assert.AreEqual(default(DateTime), data.CommitDate);
        }

        [TestMethod]
        public void BuildFileName_CanBeSetAndGet()
        {
            // Arrange
            var data = new CommittedBuildData();

            // Act
            data.BuildFileName = "build.sbm";

            // Assert
            Assert.AreEqual("build.sbm", data.BuildFileName);
        }

        [TestMethod]
        public void ScriptCount_CanBeSetAndGet()
        {
            // Arrange
            var data = new CommittedBuildData();

            // Act
            data.ScriptCount = 42;

            // Assert
            Assert.AreEqual(42, data.ScriptCount);
        }

        [TestMethod]
        public void CommitDate_CanBeSetAndGet()
        {
            // Arrange
            var data = new CommittedBuildData();
            var expectedDate = new DateTime(2024, 1, 15, 10, 30, 0);

            // Act
            data.CommitDate = expectedDate;

            // Assert
            Assert.AreEqual(expectedDate, data.CommitDate);
        }

        [TestMethod]
        public void Database_CanBeSetAndGet()
        {
            // Arrange
            var data = new CommittedBuildData();

            // Act
            data.Database = "TestDatabase";

            // Assert
            Assert.AreEqual("TestDatabase", data.Database);
        }

        [TestMethod]
        public void AllProperties_CanBeSetTogether()
        {
            // Arrange
            var data = new CommittedBuildData();
            var commitDate = DateTime.UtcNow;

            // Act
            data.BuildFileName = "myBuild.sbm";
            data.ScriptCount = 10;
            data.CommitDate = commitDate;
            data.Database = "ProductionDb";

            // Assert
            Assert.AreEqual("myBuild.sbm", data.BuildFileName);
            Assert.AreEqual(10, data.ScriptCount);
            Assert.AreEqual(commitDate, data.CommitDate);
            Assert.AreEqual("ProductionDb", data.Database);
        }
    }
}
