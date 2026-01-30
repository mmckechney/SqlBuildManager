using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class ScriptLogEventArgsTests
    {
        [TestMethod]
        public void Constructor_WithBasicParameters_SetsPropertiesCorrectly()
        {
            // Arrange
            int scriptIndex = 1;
            string sqlScript = "SELECT * FROM Users";
            string database = "TestDB";
            string sourceFile = "script.sql";
            string results = "5 rows affected";

            // Act
            var args = new ScriptLogEventArgs(scriptIndex, sqlScript, database, sourceFile, results);

            // Assert
            Assert.AreEqual(scriptIndex, args.ScriptIndex);
            Assert.AreEqual(sqlScript, args.SqlScript);
            Assert.AreEqual(database, args.Database);
            Assert.AreEqual(sourceFile, args.SourceFile);
            Assert.AreEqual(results, args.Results);
            Assert.IsFalse(args.InsertStartTransaction);
        }

        [TestMethod]
        public void Constructor_WithTransactionFlag_SetsInsertStartTransaction()
        {
            // Arrange
            int scriptIndex = 2;
            string sqlScript = "INSERT INTO Table VALUES (1)";
            string database = "DB1";
            string sourceFile = "insert.sql";
            string results = "1 row inserted";
            bool insertTransactionStart = true;

            // Act
            var args = new ScriptLogEventArgs(scriptIndex, sqlScript, database, sourceFile, results, insertTransactionStart);

            // Assert
            Assert.AreEqual(scriptIndex, args.ScriptIndex);
            Assert.AreEqual(sqlScript, args.SqlScript);
            Assert.AreEqual(database, args.Database);
            Assert.AreEqual(sourceFile, args.SourceFile);
            Assert.AreEqual(results, args.Results);
            Assert.IsTrue(args.InsertStartTransaction);
        }

        [TestMethod]
        public void Constructor_WithNegativeScriptIndex_AcceptsValue()
        {
            // Arrange & Act
            var args = new ScriptLogEventArgs(-10000, "COMMIT", "DB", "commit.sql", "Success");

            // Assert
            Assert.AreEqual(-10000, args.ScriptIndex);
        }

        [TestMethod]
        public void Constructor_WithEmptyStrings_SetsEmptyProperties()
        {
            // Arrange & Act
            var args = new ScriptLogEventArgs(0, string.Empty, string.Empty, string.Empty, string.Empty);

            // Assert
            Assert.AreEqual(0, args.ScriptIndex);
            Assert.AreEqual(string.Empty, args.SqlScript);
            Assert.AreEqual(string.Empty, args.Database);
            Assert.AreEqual(string.Empty, args.SourceFile);
            Assert.AreEqual(string.Empty, args.Results);
        }

        [TestMethod]
        public void Constructor_WithNullStrings_SetsNullProperties()
        {
            // Arrange & Act
            var args = new ScriptLogEventArgs(0, null, null, null, null);

            // Assert
            Assert.IsNull(args.SqlScript);
            Assert.IsNull(args.Database);
            Assert.IsNull(args.SourceFile);
            Assert.IsNull(args.Results);
        }

        [TestMethod]
        public void Constructor_WithMultiLineScript_PreservesNewlines()
        {
            // Arrange
            string multiLineScript = "SELECT *\r\nFROM Users\r\nWHERE Id = 1";

            // Act
            var args = new ScriptLogEventArgs(1, multiLineScript, "DB", "multi.sql", "OK");

            // Assert
            Assert.AreEqual(multiLineScript, args.SqlScript);
            Assert.IsTrue(args.SqlScript.Contains("\r\n"));
        }
    }

    [TestClass]
    public class ScriptRunStatusEventArgsTests
    {
        [TestMethod]
        public void Constructor_SetsStatusAndDuration()
        {
            // Arrange
            string status = "Script Successful";
            var duration = TimeSpan.FromSeconds(5.5);

            // Act
            var args = new ScriptRunStatusEventArgs(status, duration);

            // Assert
            Assert.AreEqual(status, args.Status);
            Assert.AreEqual(duration, args.Duration);
        }

        [TestMethod]
        public void Constructor_WithZeroDuration_AcceptsValue()
        {
            // Act
            var args = new ScriptRunStatusEventArgs("Complete", TimeSpan.Zero);

            // Assert
            Assert.AreEqual(TimeSpan.Zero, args.Duration);
        }

        [TestMethod]
        public void Constructor_WithLongDuration_AcceptsValue()
        {
            // Arrange
            var longDuration = TimeSpan.FromHours(2);

            // Act
            var args = new ScriptRunStatusEventArgs("Long Running", longDuration);

            // Assert
            Assert.AreEqual(longDuration, args.Duration);
        }

        [TestMethod]
        public void Constructor_WithNullStatus_AcceptsNull()
        {
            // Act
            var args = new ScriptRunStatusEventArgs(null, TimeSpan.FromSeconds(1));

            // Assert
            Assert.IsNull(args.Status);
        }
    }

    [TestClass]
    public class BuildScriptEventArgsTests
    {
        [TestMethod]
        public void Constructor_SetsAllPropertiesCorrectly()
        {
            // Arrange
            double buildOrder = 1.5;
            string fileName = "script.sql";
            string database = "TestDB";
            double originalBuildOrder = 1.0;
            string status = "Pending";
            string scriptId = "abc123";
            bool stripTransactionText = true;
            Guid buildScriptId = Guid.NewGuid();

            // Act
            var args = new BuildScriptEventArgs(buildOrder, fileName, database, originalBuildOrder, status, scriptId, stripTransactionText, buildScriptId);

            // Assert
            Assert.AreEqual(buildOrder, args.BuildOrder);
            Assert.AreEqual(fileName, args.FileName);
            Assert.AreEqual(database, args.Database);
            Assert.AreEqual(originalBuildOrder, args.OriginalBuildOrder);
            Assert.AreEqual(status, args.Status);
            Assert.AreEqual(scriptId, args.ScriptId);
            Assert.AreEqual(stripTransactionText, args.StripTransactionText);
            Assert.AreEqual(buildScriptId, args.BuildScriptId);
        }

        [TestMethod]
        public void Constructor_WithDefaultGuid_AcceptsEmptyGuid()
        {
            // Act
            var args = new BuildScriptEventArgs(1, "test.sql", "DB", 1, "OK", "id", false, Guid.Empty);

            // Assert
            Assert.AreEqual(Guid.Empty, args.BuildScriptId);
        }

        [TestMethod]
        public void Constructor_WithNegativeBuildOrder_AcceptsValue()
        {
            // Act
            var args = new BuildScriptEventArgs(-1, "test.sql", "DB", -1, "Status", "id", false, Guid.NewGuid());

            // Assert
            Assert.AreEqual(-1, args.BuildOrder);
            Assert.AreEqual(-1, args.OriginalBuildOrder);
        }
    }

    [TestClass]
    public class GeneralStatusEventArgsTests
    {
        [TestMethod]
        public void Constructor_SetsStatusMessage()
        {
            // Arrange
            string message = "Build in progress...";

            // Act
            var args = new GeneralStatusEventArgs(message);

            // Assert
            Assert.AreEqual(message, args.StatusMessage);
        }

        [TestMethod]
        public void Constructor_WithNullMessage_AcceptsNull()
        {
            // Act
            var args = new GeneralStatusEventArgs(null);

            // Assert
            Assert.IsNull(args.StatusMessage);
        }

        [TestMethod]
        public void Constructor_WithEmptyMessage_AcceptsEmpty()
        {
            // Act
            var args = new GeneralStatusEventArgs(string.Empty);

            // Assert
            Assert.AreEqual(string.Empty, args.StatusMessage);
        }
    }

    [TestClass]
    public class CommitFailureEventArgsTests
    {
        [TestMethod]
        public void Constructor_SetsErrorMessage()
        {
            // Arrange
            string error = "Database connection failed";

            // Act
            var args = new CommitFailureEventArgs(error);

            // Assert
            Assert.AreEqual(error, args.ErrorMessage);
        }

        [TestMethod]
        public void Constructor_WithNullError_AcceptsNull()
        {
            // Act
            var args = new CommitFailureEventArgs(null);

            // Assert
            Assert.IsNull(args.ErrorMessage);
        }

        [TestMethod]
        public void Constructor_WithDetailedError_PreservesMessage()
        {
            // Arrange
            string detailedError = "Error Code: 1234\r\nMessage: Connection timeout\r\nServer: localhost";

            // Act
            var args = new CommitFailureEventArgs(detailedError);

            // Assert
            Assert.AreEqual(detailedError, args.ErrorMessage);
        }
    }

    [TestClass]
    public class ScriptRunProjectFileSavedEventArgsTests
    {
        [TestMethod]
        public void Constructor_WithTrue_SetsSavedTrue()
        {
            // Act
            var args = new ScriptRunProjectFileSavedEventArgs(true);

            // Assert
            Assert.IsTrue(args.Saved);
        }

        [TestMethod]
        public void Constructor_WithFalse_SetsSavedFalse()
        {
            // Act
            var args = new ScriptRunProjectFileSavedEventArgs(false);

            // Assert
            Assert.IsFalse(args.Saved);
        }

        [TestMethod]
        public void Saved_CanBeModified()
        {
            // Arrange
            var args = new ScriptRunProjectFileSavedEventArgs(false);

            // Act
            args.Saved = true;

            // Assert
            Assert.IsTrue(args.Saved);
        }
    }

    [TestClass]
    public class BuildStartedEventArgsTests
    {
        [TestMethod]
        public void Constructor_Default_CreatesInstance()
        {
            // Act
            var args = new BuildStartedEventArgs();

            // Assert
            Assert.IsNotNull(args);
        }

        [TestMethod]
        public void Constructor_InheritsFromEventArgs()
        {
            // Act
            var args = new BuildStartedEventArgs();

            // Assert
            Assert.IsInstanceOfType(args, typeof(EventArgs));
        }
    }
}
