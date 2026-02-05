using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.CodeTable;

namespace SqlSync.SqlBuild.UnitTest.CodeTable
{
    [TestClass]
    public class ScriptUpdatesTests
    {
        #region Constructor and Property Tests

        [TestMethod]
        public void Constructor_Default_CreatesInstance()
        {
            // Act
            var scriptUpdates = new ScriptUpdates();

            // Assert
            Assert.IsNotNull(scriptUpdates);
        }

        [TestMethod]
        public void Query_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var scriptUpdates = new ScriptUpdates();
            var expected = "SELECT * FROM Table1";

            // Act
            scriptUpdates.Query = expected;

            // Assert
            Assert.AreEqual(expected, scriptUpdates.Query);
        }

        [TestMethod]
        public void ShortFileName_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var scriptUpdates = new ScriptUpdates();
            var expected = "Script1.sql";

            // Act
            scriptUpdates.ShortFileName = expected;

            // Assert
            Assert.AreEqual(expected, scriptUpdates.ShortFileName);
        }

        [TestMethod]
        public void SourceTable_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var scriptUpdates = new ScriptUpdates();
            var expected = "dbo.Customers";

            // Act
            scriptUpdates.SourceTable = expected;

            // Assert
            Assert.AreEqual(expected, scriptUpdates.SourceTable);
        }

        [TestMethod]
        public void SourceDatabase_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var scriptUpdates = new ScriptUpdates();
            var expected = "ProductionDB";

            // Act
            scriptUpdates.SourceDatabase = expected;

            // Assert
            Assert.AreEqual(expected, scriptUpdates.SourceDatabase);
        }

        [TestMethod]
        public void SourceServer_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var scriptUpdates = new ScriptUpdates();
            var expected = "ProdServer\\Instance";

            // Act
            scriptUpdates.SourceServer = expected;

            // Assert
            Assert.AreEqual(expected, scriptUpdates.SourceServer);
        }

        [TestMethod]
        public void KeyCheckColumns_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var scriptUpdates = new ScriptUpdates();
            var expected = "Id,Name,Date";

            // Act
            scriptUpdates.KeyCheckColumns = expected;

            // Assert
            Assert.AreEqual(expected, scriptUpdates.KeyCheckColumns);
        }

        #endregion

        #region Multiple Property Tests

        [TestMethod]
        public void AllProperties_SetAndGet_PreservesValues()
        {
            // Arrange
            var scriptUpdates = new ScriptUpdates();

            // Act
            scriptUpdates.Query = "SELECT * FROM Test";
            scriptUpdates.ShortFileName = "test.sql";
            scriptUpdates.SourceTable = "dbo.Test";
            scriptUpdates.SourceDatabase = "TestDB";
            scriptUpdates.SourceServer = "TestServer";
            scriptUpdates.KeyCheckColumns = "Id,Code";

            // Assert
            Assert.AreEqual("SELECT * FROM Test", scriptUpdates.Query);
            Assert.AreEqual("test.sql", scriptUpdates.ShortFileName);
            Assert.AreEqual("dbo.Test", scriptUpdates.SourceTable);
            Assert.AreEqual("TestDB", scriptUpdates.SourceDatabase);
            Assert.AreEqual("TestServer", scriptUpdates.SourceServer);
            Assert.AreEqual("Id,Code", scriptUpdates.KeyCheckColumns);
        }

        [TestMethod]
        public void Properties_WithSpecialCharacters_PreservesValues()
        {
            // Arrange
            var scriptUpdates = new ScriptUpdates();

            // Act
            scriptUpdates.Query = "SELECT * FROM [dbo].[Table With Spaces]";
            scriptUpdates.ShortFileName = "Script-With_Special!@#.sql";
            scriptUpdates.SourceServer = "Server\\Instance";

            // Assert
            Assert.AreEqual("SELECT * FROM [dbo].[Table With Spaces]", scriptUpdates.Query);
            Assert.AreEqual("Script-With_Special!@#.sql", scriptUpdates.ShortFileName);
            Assert.AreEqual("Server\\Instance", scriptUpdates.SourceServer);
        }

        [TestMethod]
        public void Properties_WithNullValues_AcceptsNull()
        {
            // Arrange
            var scriptUpdates = new ScriptUpdates();
            scriptUpdates.Query = "Initial";

            // Act
            scriptUpdates.Query = null;
            scriptUpdates.ShortFileName = null;
            scriptUpdates.SourceTable = null;
            scriptUpdates.SourceDatabase = null;
            scriptUpdates.SourceServer = null;
            scriptUpdates.KeyCheckColumns = null;

            // Assert
            Assert.IsNull(scriptUpdates.Query);
            Assert.IsNull(scriptUpdates.ShortFileName);
            Assert.IsNull(scriptUpdates.SourceTable);
            Assert.IsNull(scriptUpdates.SourceDatabase);
            Assert.IsNull(scriptUpdates.SourceServer);
            Assert.IsNull(scriptUpdates.KeyCheckColumns);
        }

        [TestMethod]
        public void Properties_WithEmptyStrings_AcceptsEmpty()
        {
            // Arrange
            var scriptUpdates = new ScriptUpdates();

            // Act
            scriptUpdates.Query = string.Empty;
            scriptUpdates.ShortFileName = string.Empty;
            scriptUpdates.SourceTable = string.Empty;
            scriptUpdates.SourceDatabase = string.Empty;
            scriptUpdates.SourceServer = string.Empty;
            scriptUpdates.KeyCheckColumns = string.Empty;

            // Assert
            Assert.AreEqual(string.Empty, scriptUpdates.Query);
            Assert.AreEqual(string.Empty, scriptUpdates.ShortFileName);
            Assert.AreEqual(string.Empty, scriptUpdates.SourceTable);
            Assert.AreEqual(string.Empty, scriptUpdates.SourceDatabase);
            Assert.AreEqual(string.Empty, scriptUpdates.SourceServer);
            Assert.AreEqual(string.Empty, scriptUpdates.KeyCheckColumns);
        }

        [TestMethod]
        public void Query_WithMultiLineQuery_PreservesLineBreaks()
        {
            // Arrange
            var scriptUpdates = new ScriptUpdates();
            var multiLineQuery = "SELECT *\r\nFROM Table1\r\nWHERE Id = 1";

            // Act
            scriptUpdates.Query = multiLineQuery;

            // Assert
            Assert.AreEqual(multiLineQuery, scriptUpdates.Query);
            Assert.IsTrue(scriptUpdates.Query.Contains("\r\n"));
        }

        #endregion
    }
}
