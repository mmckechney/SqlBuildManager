using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild;
using System.Linq;

namespace SqlSync.SqlBuild.UnitTest
{
    /// <summary>
    /// Tests for ScriptBatchCollection and ScriptBatch classes
    /// </summary>
    [TestClass]
    public class ScriptBatchCollectionFinalTests
    {
        #region ScriptBatch Tests

        [TestMethod]
        public void ScriptBatch_Constructor_SetsAllProperties()
        {
            // Arrange
            var scripts = new[] { "SELECT 1", "SELECT 2" };

            // Act
            var batch = new ScriptBatch("test.sql", scripts, "SCRIPT-001");

            // Assert
            Assert.AreEqual("test.sql", batch.ScriptfileName);
            Assert.AreEqual("SCRIPT-001", batch.ScriptId);
            Assert.AreEqual(2, batch.ScriptBatchContents.Length);
        }

        [TestMethod]
        public void ScriptBatch_WithEmptyScripts_StoresEmptyArray()
        {
            // Act
            var batch = new ScriptBatch("test.sql", new string[0], "SCRIPT-001");

            // Assert
            Assert.AreEqual(0, batch.ScriptBatchContents.Length);
        }

        [TestMethod]
        public void ScriptBatch_WithNullScripts_StoresNull()
        {
            // Act
            var batch = new ScriptBatch("test.sql", null, "SCRIPT-001");

            // Assert
            Assert.IsNull(batch.ScriptBatchContents);
        }

        #endregion

        #region ScriptBatchCollection Tests

        [TestMethod]
        public void ScriptBatchCollection_Constructor_CreatesEmptyCollection()
        {
            // Act
            var collection = new ScriptBatchCollection();

            // Assert
            Assert.AreEqual(0, collection.Count);
        }

        [TestMethod]
        public void ScriptBatchCollection_Add_IncreasesCount()
        {
            // Arrange
            var collection = new ScriptBatchCollection();
            var batch = new ScriptBatch("test.sql", new[] { "SELECT 1" }, "SCRIPT-001");

            // Act
            collection.Add(batch);

            // Assert
            Assert.AreEqual(1, collection.Count);
        }

        [TestMethod]
        public void ScriptBatchCollection_AddMultiple_StoresAll()
        {
            // Arrange
            var collection = new ScriptBatchCollection();

            // Act
            collection.Add(new ScriptBatch("test1.sql", new[] { "SELECT 1" }, "SCRIPT-001"));
            collection.Add(new ScriptBatch("test2.sql", new[] { "SELECT 2" }, "SCRIPT-002"));
            collection.Add(new ScriptBatch("test3.sql", new[] { "SELECT 3" }, "SCRIPT-003"));

            // Assert
            Assert.AreEqual(3, collection.Count);
        }

        [TestMethod]
        public void ScriptBatchCollection_GetScriptBatch_WithValidId_ReturnsBatch()
        {
            // Arrange
            var collection = new ScriptBatchCollection
            {
                new ScriptBatch("test.sql", new[] { "SELECT 1" }, "SCRIPT-001")
            };

            // Act
            var batch = collection.GetScriptBatch("SCRIPT-001");

            // Assert
            Assert.IsNotNull(batch);
            Assert.AreEqual("test.sql", batch.ScriptfileName);
        }

        [TestMethod]
        public void ScriptBatchCollection_GetScriptBatch_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var collection = new ScriptBatchCollection
            {
                new ScriptBatch("test.sql", new[] { "SELECT 1" }, "SCRIPT-001")
            };

            // Act
            var batch = collection.GetScriptBatch("NON-EXISTENT");

            // Assert
            Assert.IsNull(batch);
        }

        [TestMethod]
        public void ScriptBatchCollection_IsEnumerable()
        {
            // Arrange
            var collection = new ScriptBatchCollection
            {
                new ScriptBatch("test1.sql", new[] { "SELECT 1" }, "SCRIPT-001"),
                new ScriptBatch("test2.sql", new[] { "SELECT 2" }, "SCRIPT-002")
            };

            // Act
            var count = 0;
            foreach (var batch in collection)
            {
                count++;
            }

            // Assert
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void ScriptBatchCollection_SupportsLinq()
        {
            // Arrange
            var collection = new ScriptBatchCollection
            {
                new ScriptBatch("test1.sql", new[] { "SELECT 1" }, "SCRIPT-001"),
                new ScriptBatch("test2.sql", new[] { "SELECT 2" }, "SCRIPT-002"),
                new ScriptBatch("test3.sql", new[] { "SELECT 3" }, "SCRIPT-003")
            };

            // Act
            var filtered = collection.Where(b => b.ScriptfileName.StartsWith("test1")).ToList();

            // Assert
            Assert.AreEqual(1, filtered.Count);
            Assert.AreEqual("test1.sql", filtered[0].ScriptfileName);
        }

        [TestMethod]
        public void ScriptBatchCollection_WithEmptyCollection_GetScriptBatch_ReturnsNull()
        {
            // Arrange
            var collection = new ScriptBatchCollection();

            // Act
            var batch = collection.GetScriptBatch("any-id");

            // Assert
            Assert.IsNull(batch);
        }

        #endregion

        #region ScriptBatch Content Tests

        [TestMethod]
        public void ScriptBatch_ScriptBatchContents_PreservesOrder()
        {
            // Arrange
            var scripts = new[] { "SELECT 1", "SELECT 2", "SELECT 3" };

            // Act
            var batch = new ScriptBatch("test.sql", scripts, "SCRIPT-001");

            // Assert
            Assert.AreEqual("SELECT 1", batch.ScriptBatchContents[0]);
            Assert.AreEqual("SELECT 2", batch.ScriptBatchContents[1]);
            Assert.AreEqual("SELECT 3", batch.ScriptBatchContents[2]);
        }

        [TestMethod]
        public void ScriptBatch_WithMultiLineScripts_PreservesContent()
        {
            // Arrange
            var script = @"SELECT 
    Column1,
    Column2
FROM
    Table1
WHERE
    Id = 1";
            var scripts = new[] { script };

            // Act
            var batch = new ScriptBatch("test.sql", scripts, "SCRIPT-001");

            // Assert
            Assert.IsTrue(batch.ScriptBatchContents[0].Contains("Column1"));
            Assert.IsTrue(batch.ScriptBatchContents[0].Contains("Table1"));
        }

        #endregion
    }
}
