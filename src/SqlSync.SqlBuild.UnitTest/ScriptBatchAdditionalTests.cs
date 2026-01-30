using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace SqlSync.SqlBuild.UnitTest
{
    /// <summary>
    /// Additional tests for ScriptBatch and ScriptBatchCollection
    /// </summary>
    [TestClass]
    public class ScriptBatchAdditionalTests
    {
        #region ScriptBatch Constructor Tests

        [TestMethod]
        public void ScriptBatch_Constructor_SetsAllProperties()
        {
            var fileName = "test.sql";
            var contents = new[] { "SELECT 1;", "SELECT 2;" };
            var scriptId = "script-123";

            var batch = new ScriptBatch(fileName, contents, scriptId);

            Assert.AreEqual(fileName, batch.ScriptfileName);
            CollectionAssert.AreEqual(contents, batch.ScriptBatchContents);
            Assert.AreEqual(scriptId, batch.ScriptId);
        }

        [TestMethod]
        public void ScriptBatch_Constructor_WithEmptyContents_Works()
        {
            var batch = new ScriptBatch("test.sql", new string[0], "id");

            Assert.AreEqual(0, batch.ScriptBatchContents.Length);
        }

        [TestMethod]
        public void ScriptBatch_Constructor_WithNullContents_AllowsIt()
        {
            var batch = new ScriptBatch("test.sql", null, "id");

            Assert.IsNull(batch.ScriptBatchContents);
        }

        #endregion

        #region ScriptBatch Property Setters Tests

        [TestMethod]
        public void ScriptBatch_ScriptfileName_CanBeChanged()
        {
            var batch = new ScriptBatch("original.sql", new[] { "SELECT 1" }, "id");

            batch.ScriptfileName = "changed.sql";

            Assert.AreEqual("changed.sql", batch.ScriptfileName);
        }

        [TestMethod]
        public void ScriptBatch_ScriptBatchContents_CanBeChanged()
        {
            var batch = new ScriptBatch("test.sql", new[] { "SELECT 1" }, "id");
            var newContents = new[] { "SELECT A;", "SELECT B;", "SELECT C;" };

            batch.ScriptBatchContents = newContents;

            Assert.AreEqual(3, batch.ScriptBatchContents.Length);
            CollectionAssert.AreEqual(newContents, batch.ScriptBatchContents);
        }

        [TestMethod]
        public void ScriptBatch_ScriptId_CanBeChanged()
        {
            var batch = new ScriptBatch("test.sql", new[] { "SELECT 1" }, "original-id");

            batch.ScriptId = "new-id";

            Assert.AreEqual("new-id", batch.ScriptId);
        }

        #endregion

        #region ScriptBatchCollection GetScriptBatch Tests

        [TestMethod]
        public void GetScriptBatch_FindsFirstMatch()
        {
            var batch1 = new ScriptBatch("file1.sql", new[] { "SELECT 1" }, "id-1");
            var batch2 = new ScriptBatch("file2.sql", new[] { "SELECT 2" }, "id-2");
            var batch3 = new ScriptBatch("file3.sql", new[] { "SELECT 3" }, "id-3");
            var collection = new ScriptBatchCollection { batch1, batch2, batch3 };

            var result = collection.GetScriptBatch("id-2");

            Assert.AreEqual(batch2, result);
            Assert.AreEqual("file2.sql", result.ScriptfileName);
        }

        [TestMethod]
        public void GetScriptBatch_WithMultipleSameId_ReturnsFirst()
        {
            var batch1 = new ScriptBatch("first.sql", new[] { "FIRST" }, "same-id");
            var batch2 = new ScriptBatch("second.sql", new[] { "SECOND" }, "same-id");
            var collection = new ScriptBatchCollection { batch1, batch2 };

            var result = collection.GetScriptBatch("same-id");

            Assert.AreEqual("first.sql", result.ScriptfileName);
        }

        [TestMethod]
        public void GetScriptBatch_WithEmptyCollection_ReturnsNull()
        {
            var collection = new ScriptBatchCollection();

            var result = collection.GetScriptBatch("any-id");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetScriptBatch_IsCaseSensitive()
        {
            var batch = new ScriptBatch("test.sql", new[] { "SELECT 1" }, "MyId");
            var collection = new ScriptBatchCollection { batch };

            var result = collection.GetScriptBatch("myid");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetScriptBatch_WithNullId_ReturnsNull()
        {
            var batch = new ScriptBatch("test.sql", new[] { "SELECT 1" }, "id-1");
            var collection = new ScriptBatchCollection { batch };

            var result = collection.GetScriptBatch(null);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetScriptBatch_WithEmptyStringId_ReturnsNull()
        {
            var batch = new ScriptBatch("test.sql", new[] { "SELECT 1" }, "id-1");
            var collection = new ScriptBatchCollection { batch };

            var result = collection.GetScriptBatch(string.Empty);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetScriptBatch_CanFindBatchWithEmptyId()
        {
            var batch = new ScriptBatch("test.sql", new[] { "SELECT 1" }, string.Empty);
            var collection = new ScriptBatchCollection { batch };

            var result = collection.GetScriptBatch(string.Empty);

            Assert.AreEqual(batch, result);
        }

        #endregion

        #region ScriptBatchCollection List Operations

        [TestMethod]
        public void ScriptBatchCollection_InheritsFromList()
        {
            var collection = new ScriptBatchCollection();

            Assert.IsInstanceOfType(collection, typeof(System.Collections.Generic.List<ScriptBatch>));
        }

        [TestMethod]
        public void ScriptBatchCollection_SupportsLinqOperations()
        {
            var collection = new ScriptBatchCollection
            {
                new ScriptBatch("a.sql", new[] { "A" }, "1"),
                new ScriptBatch("b.sql", new[] { "B" }, "2"),
                new ScriptBatch("c.sql", new[] { "C" }, "3")
            };

            var fileNames = collection.Select(b => b.ScriptfileName).ToList();

            Assert.AreEqual(3, fileNames.Count);
            Assert.AreEqual("a.sql", fileNames[0]);
            Assert.AreEqual("b.sql", fileNames[1]);
            Assert.AreEqual("c.sql", fileNames[2]);
        }

        [TestMethod]
        public void ScriptBatchCollection_SupportsRemove()
        {
            var batch = new ScriptBatch("test.sql", new[] { "SELECT 1" }, "id");
            var collection = new ScriptBatchCollection { batch };

            collection.Remove(batch);

            Assert.AreEqual(0, collection.Count);
        }

        [TestMethod]
        public void ScriptBatchCollection_SupportsClear()
        {
            var collection = new ScriptBatchCollection
            {
                new ScriptBatch("a.sql", new[] { "A" }, "1"),
                new ScriptBatch("b.sql", new[] { "B" }, "2")
            };

            collection.Clear();

            Assert.AreEqual(0, collection.Count);
        }

        [TestMethod]
        public void ScriptBatchCollection_SupportsInsertAt()
        {
            var collection = new ScriptBatchCollection
            {
                new ScriptBatch("a.sql", new[] { "A" }, "1"),
                new ScriptBatch("c.sql", new[] { "C" }, "3")
            };
            var batchB = new ScriptBatch("b.sql", new[] { "B" }, "2");

            collection.Insert(1, batchB);

            Assert.AreEqual(3, collection.Count);
            Assert.AreEqual("b.sql", collection[1].ScriptfileName);
        }

        [TestMethod]
        public void ScriptBatchCollection_SupportsContains()
        {
            var batch = new ScriptBatch("test.sql", new[] { "SELECT 1" }, "id");
            var collection = new ScriptBatchCollection { batch };

            Assert.IsTrue(collection.Contains(batch));
        }

        [TestMethod]
        public void ScriptBatchCollection_SupportsIndexOf()
        {
            var batch1 = new ScriptBatch("a.sql", new[] { "A" }, "1");
            var batch2 = new ScriptBatch("b.sql", new[] { "B" }, "2");
            var collection = new ScriptBatchCollection { batch1, batch2 };

            Assert.AreEqual(0, collection.IndexOf(batch1));
            Assert.AreEqual(1, collection.IndexOf(batch2));
        }

        #endregion

        #region ScriptBatch with Various Content Tests

        [TestMethod]
        public void ScriptBatch_WithMultipleBatches_HandlesCorrectly()
        {
            var contents = new[]
            {
                "CREATE TABLE Test (Id INT);",
                "INSERT INTO Test VALUES (1);",
                "INSERT INTO Test VALUES (2);",
                "SELECT * FROM Test;"
            };
            var batch = new ScriptBatch("multi.sql", contents, "multi-id");

            Assert.AreEqual(4, batch.ScriptBatchContents.Length);
            Assert.AreEqual("CREATE TABLE Test (Id INT);", batch.ScriptBatchContents[0]);
            Assert.AreEqual("SELECT * FROM Test;", batch.ScriptBatchContents[3]);
        }

        [TestMethod]
        public void ScriptBatch_WithSpecialCharactersInContent_HandlesCorrectly()
        {
            var contents = new[]
            {
                "SELECT 'Hello, World!' AS Message;",
                "SELECT N'Unicode: ñ á é í ó ú' AS Text;",
                "SELECT 'Tab:\tNewline:\nCarriage:\r' AS Escaped;"
            };
            var batch = new ScriptBatch("special.sql", contents, "special-id");

            Assert.IsTrue(batch.ScriptBatchContents[1].Contains("ñ"));
            Assert.IsTrue(batch.ScriptBatchContents[2].Contains("\t"));
        }

        [TestMethod]
        public void ScriptBatch_WithLargeContent_HandlesCorrectly()
        {
            var largeScript = new string('X', 100000); // 100KB of content
            var contents = new[] { largeScript };
            var batch = new ScriptBatch("large.sql", contents, "large-id");

            Assert.AreEqual(100000, batch.ScriptBatchContents[0].Length);
        }

        #endregion
    }
}
