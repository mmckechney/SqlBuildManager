using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Interfaces.ScriptHandling.Tags;
using SqlSync.SqlBuild.Models;
using System.Collections.Generic;

namespace SqlSync.SqlBuild.UnitTest.Models
{
    [TestClass]
    public class BulkAddDataTests
    {
        [TestMethod]
        public void DefaultConstructor_SetsDefaults()
        {
            // Arrange & Act
            var data = new BulkAddData();

            // Assert
            Assert.IsNotNull(data.FileList);
            Assert.AreEqual(0, data.FileList.Count);
            Assert.AreEqual(string.Empty, data.PreSetDatabase);
            Assert.IsFalse(data.DeleteOriginalFiles);
            Assert.IsFalse(data.CreateNewEntriesForPreExisting);
            Assert.AreEqual(0.0, data.LastBuildNumber);
        }

        [TestMethod]
        public void FileList_CanBeSetAndGet()
        {
            // Arrange
            var data = new BulkAddData();
            var files = new List<string> { "file1.sql", "file2.sql", "file3.sql" };

            // Act
            data.FileList = files;

            // Assert
            Assert.AreEqual(3, data.FileList.Count);
            Assert.AreEqual("file1.sql", data.FileList[0]);
            Assert.AreEqual("file2.sql", data.FileList[1]);
            Assert.AreEqual("file3.sql", data.FileList[2]);
        }

        [TestMethod]
        public void PreSetDatabase_CanBeSetAndGet()
        {
            // Arrange
            var data = new BulkAddData();

            // Act
            data.PreSetDatabase = "MyDatabase";

            // Assert
            Assert.AreEqual("MyDatabase", data.PreSetDatabase);
        }

        [TestMethod]
        public void DeleteOriginalFiles_CanBeSetAndGet()
        {
            // Arrange
            var data = new BulkAddData();

            // Act
            data.DeleteOriginalFiles = true;

            // Assert
            Assert.IsTrue(data.DeleteOriginalFiles);
        }

        [TestMethod]
        public void CreateNewEntriesForPreExisting_CanBeSetAndGet()
        {
            // Arrange
            var data = new BulkAddData();

            // Act
            data.CreateNewEntriesForPreExisting = true;

            // Assert
            Assert.IsTrue(data.CreateNewEntriesForPreExisting);
        }

        [TestMethod]
        public void LastBuildNumber_CanBeSetAndGet()
        {
            // Arrange
            var data = new BulkAddData();

            // Act
            data.LastBuildNumber = 15.5;

            // Assert
            Assert.AreEqual(15.5, data.LastBuildNumber);
        }

        [TestMethod]
        public void TagInferSource_CanBeSetAndGet()
        {
            // Arrange
            var data = new BulkAddData();

            // Act
            data.TagInferSource = TagInferenceSource.ScriptText;

            // Assert
            Assert.AreEqual(TagInferenceSource.ScriptText, data.TagInferSource);
        }

        [TestMethod]
        public void TagInferSourceRegexFormats_CanBeSetAndGet()
        {
            // Arrange
            var data = new BulkAddData();
            var formats = new List<string> { @"\d+", @"\w+" };

            // Act
            data.TagInferSourceRegexFormats = formats;

            // Assert
            Assert.AreEqual(2, data.TagInferSourceRegexFormats.Count);
            Assert.AreEqual(@"\d+", data.TagInferSourceRegexFormats[0]);
        }

        [TestMethod]
        public void ScriptTag_CanBeSetAndGet()
        {
            // Arrange
            var data = new BulkAddData();

            // Act
            data.ScriptTag = "Migration";

            // Assert
            Assert.AreEqual("Migration", data.ScriptTag);
        }

        [TestMethod]
        public void StripTransactions_CanBeSetAndGet()
        {
            // Arrange
            var data = new BulkAddData();

            // Act
            data.StripTransactions = true;

            // Assert
            Assert.IsTrue(data.StripTransactions);
        }

        [TestMethod]
        public void Description_CanBeSetAndGet()
        {
            // Arrange
            var data = new BulkAddData();

            // Act
            data.Description = "Test description";

            // Assert
            Assert.AreEqual("Test description", data.Description);
        }

        [TestMethod]
        public void RollBackScript_CanBeSetAndGet()
        {
            // Arrange
            var data = new BulkAddData();

            // Act
            data.RollBackScript = true;

            // Assert
            Assert.IsTrue(data.RollBackScript);
        }

        [TestMethod]
        public void RollBackBuild_CanBeSetAndGet()
        {
            // Arrange
            var data = new BulkAddData();

            // Act
            data.RollBackBuild = true;

            // Assert
            Assert.IsTrue(data.RollBackBuild);
        }

        [TestMethod]
        public void DatabaseName_CanBeSetAndGet()
        {
            // Arrange
            var data = new BulkAddData();

            // Act
            data.DatabaseName = "TargetDb";

            // Assert
            Assert.AreEqual("TargetDb", data.DatabaseName);
        }

        [TestMethod]
        public void AllowMultipleRuns_CanBeSetAndGet()
        {
            // Arrange
            var data = new BulkAddData();

            // Act
            data.AllowMultipleRuns = true;

            // Assert
            Assert.IsTrue(data.AllowMultipleRuns);
        }
    }
}
