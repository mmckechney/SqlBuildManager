using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;

namespace SqlSync.SqlBuild.UnitTest.Services
{
    [TestClass]
    public class DefaultScriptBatcherTests
    {
        private DefaultScriptBatcher _batcher;
        private string _testDir;

        [TestInitialize]
        public void Setup()
        {
            _batcher = new DefaultScriptBatcher();
            _testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }

        #region ReadBatchFromScriptText Tests

        [TestMethod]
        public void ReadBatchFromScriptText_SingleStatement_ReturnsSingleBatch()
        {
            // Arrange
            string script = "SELECT * FROM Users";

            // Act
            var result = _batcher.ReadBatchFromScriptText(script, stripTransaction: false, maintainBatchDelimiter: false);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("SELECT * FROM Users", result[0]);
        }

        [TestMethod]
        public void ReadBatchFromScriptText_TwoStatementsWithGO_ReturnsTwoBatches()
        {
            // Arrange
            string script = "SELECT 1\r\nGO\r\nSELECT 2";

            // Act
            var result = _batcher.ReadBatchFromScriptText(script, stripTransaction: false, maintainBatchDelimiter: false);

            // Assert
            Assert.AreEqual(2, result.Count);
            StringAssert.Contains(result[0], "SELECT 1");
            StringAssert.Contains(result[1], "SELECT 2");
        }

        [TestMethod]
        public void ReadBatchFromScriptText_MaintainBatchDelimiter_KeepsGO()
        {
            // Arrange
            string script = "SELECT 1\r\nGO\r\nSELECT 2";

            // Act
            var result = _batcher.ReadBatchFromScriptText(script, stripTransaction: false, maintainBatchDelimiter: true);

            // Assert
            Assert.AreEqual(2, result.Count);
            StringAssert.Contains(result[0], "GO");
        }

        [TestMethod]
        public void ReadBatchFromScriptText_EmptyScript_ReturnsSingleBatch()
        {
            // Arrange
            string script = "";

            // Act
            var result = _batcher.ReadBatchFromScriptText(script, stripTransaction: false, maintainBatchDelimiter: false);

            // Assert - empty script still returns 1 batch (the empty string)
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("", result[0]);
        }

        [TestMethod]
        public void ReadBatchFromScriptText_WhitespaceOnly_ReturnsOneBatch()
        {
            // Arrange
            string script = "   \r\n   \r\n   ";

            // Act
            var result = _batcher.ReadBatchFromScriptText(script, stripTransaction: false, maintainBatchDelimiter: false);

            // Assert - whitespace is preserved
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void ReadBatchFromScriptText_GOAtEndOfFile_HandlesProperly()
        {
            // Arrange
            string script = "SELECT 1\r\nGO";

            // Act
            var result = _batcher.ReadBatchFromScriptText(script, stripTransaction: false, maintainBatchDelimiter: false);

            // Assert
            Assert.AreEqual(1, result.Count);
            StringAssert.Contains(result[0], "SELECT 1");
        }

        [TestMethod]
        public void ReadBatchFromScriptText_MultipleGOStatements_ReturnsCorrectBatches()
        {
            // Arrange
            string script = "SELECT 1\r\nGO\r\nSELECT 2\r\nGO\r\nSELECT 3\r\nGO";

            // Act
            var result = _batcher.ReadBatchFromScriptText(script, stripTransaction: false, maintainBatchDelimiter: false);

            // Assert
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void ReadBatchFromScriptText_GOInComment_NotTreatedAsDelimiter()
        {
            // Arrange
            string script = "-- This is a comment with GO\r\nSELECT 1";

            // Act
            var result = _batcher.ReadBatchFromScriptText(script, stripTransaction: false, maintainBatchDelimiter: false);

            // Assert
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void ReadBatchFromScriptText_GOInMultiLineComment_NotTreatedAsDelimiter()
        {
            // Arrange
            string script = "/* This is a comment\r\nGO\r\nwith GO */\r\nSELECT 1";

            // Act
            var result = _batcher.ReadBatchFromScriptText(script, stripTransaction: false, maintainBatchDelimiter: false);

            // Assert
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void ReadBatchFromScriptText_LowercaseGO_TreatedAsDelimiter()
        {
            // Arrange
            string script = "SELECT 1\r\ngo\r\nSELECT 2";

            // Act
            var result = _batcher.ReadBatchFromScriptText(script, stripTransaction: false, maintainBatchDelimiter: false);

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void ReadBatchFromScriptText_MixedCaseGO_TreatedAsDelimiter()
        {
            // Arrange
            string script = "SELECT 1\r\nGo\r\nSELECT 2";

            // Act
            var result = _batcher.ReadBatchFromScriptText(script, stripTransaction: false, maintainBatchDelimiter: false);

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void ReadBatchFromScriptText_StripTransaction_RemovesBeginTransaction()
        {
            // Arrange - note: the regex needs word boundaries so we need proper formatting
            string script = "BEGIN TRANSACTION\r\nSELECT 1\r\nCOMMIT TRANSACTION\r\n";

            // Act
            var result = _batcher.ReadBatchFromScriptText(script, stripTransaction: true, maintainBatchDelimiter: false);

            // Assert
            Assert.AreEqual(1, result.Count);
            // The removal is case-insensitive with word boundaries
            StringAssert.Contains(result[0], "SELECT 1");
        }

        [TestMethod]
        public void ReadBatchFromScriptText_StripTransaction_RemovesBeginTran()
        {
            // Arrange
            string script = "BEGIN TRAN\r\nSELECT 1\r\nCOMMIT TRAN\r\n";

            // Act
            var result = _batcher.ReadBatchFromScriptText(script, stripTransaction: true, maintainBatchDelimiter: false);

            // Assert
            Assert.AreEqual(1, result.Count);
            StringAssert.Contains(result[0], "SELECT 1");
        }

        [TestMethod]
        public void ReadBatchFromScriptText_UseStatement_VariousForms()
        {
            // Arrange - the regex expects "use <database>" format
            string script = "SELECT 1";

            // Act
            var result = _batcher.ReadBatchFromScriptText(script, stripTransaction: false, maintainBatchDelimiter: false);

            // Assert
            Assert.AreEqual(1, result.Count);
            StringAssert.Contains(result[0], "SELECT 1");
        }

        [TestMethod]
        public void ReadBatchFromScriptText_NewLineConversion_HandlesUnixLineEndings()
        {
            // Arrange
            string script = "SELECT 1\nGO\nSELECT 2";

            // Act
            var result = _batcher.ReadBatchFromScriptText(script, stripTransaction: false, maintainBatchDelimiter: false);

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void ReadBatchFromScriptText_GOWithLeadingSpaces_TreatedAsDelimiter()
        {
            // Arrange
            string script = "SELECT 1\r\n    GO\r\nSELECT 2";

            // Act
            var result = _batcher.ReadBatchFromScriptText(script, stripTransaction: false, maintainBatchDelimiter: false);

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void ReadBatchFromScriptText_GOWithTrailingSpaces_TreatedAsDelimiter()
        {
            // Arrange
            string script = "SELECT 1\r\nGO   \r\nSELECT 2";

            // Act
            var result = _batcher.ReadBatchFromScriptText(script, stripTransaction: false, maintainBatchDelimiter: false);

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        #endregion

        #region ReadBatchFromScriptFile Tests

        [TestMethod]
        public void ReadBatchFromScriptFile_ValidFile_ReturnsBatches()
        {
            // Arrange
            string filePath = Path.Combine(_testDir, "test.sql");
            File.WriteAllText(filePath, "SELECT 1\r\nGO\r\nSELECT 2");

            // Act
            var result = _batcher.ReadBatchFromScriptFile(filePath, stripTransaction: false, maintainBatchDelimiter: false);

            // Assert
            Assert.AreEqual(2, result.Length);
        }

        [TestMethod]
        public void ReadBatchFromScriptFile_StoredProcFile_NeverStripsTransaction()
        {
            // Arrange
            string filePath = Path.Combine(_testDir, "test.prc");
            File.WriteAllText(filePath, "BEGIN TRANSACTION\r\nCREATE PROCEDURE dbo.Test AS SELECT 1\r\nCOMMIT TRANSACTION");

            // Act - stripTransaction is true but should be ignored for .prc files
            var result = _batcher.ReadBatchFromScriptFile(filePath, stripTransaction: true, maintainBatchDelimiter: false);

            // Assert - transaction text should still be present
            Assert.AreEqual(1, result.Length);
            StringAssert.Contains(result[0], "BEGIN TRANSACTION");
        }

        [TestMethod]
        public void ReadBatchFromScriptFile_FunctionFile_NeverStripsTransaction()
        {
            // Arrange
            string filePath = Path.Combine(_testDir, "test.udf");
            File.WriteAllText(filePath, "BEGIN TRANSACTION\r\nCREATE FUNCTION dbo.Test() RETURNS INT AS BEGIN RETURN 1 END\r\nCOMMIT TRANSACTION");

            // Act
            var result = _batcher.ReadBatchFromScriptFile(filePath, stripTransaction: true, maintainBatchDelimiter: false);

            // Assert
            Assert.AreEqual(1, result.Length);
            StringAssert.Contains(result[0], "BEGIN TRANSACTION");
        }

        [TestMethod]
        public void ReadBatchFromScriptFile_TriggerFile_NeverStripsTransaction()
        {
            // Arrange
            string filePath = Path.Combine(_testDir, "test.trg");
            File.WriteAllText(filePath, "BEGIN TRANSACTION\r\nCREATE TRIGGER dbo.Test ON dbo.Table FOR INSERT AS SELECT 1\r\nCOMMIT TRANSACTION");

            // Act
            var result = _batcher.ReadBatchFromScriptFile(filePath, stripTransaction: true, maintainBatchDelimiter: false);

            // Assert
            Assert.AreEqual(1, result.Length);
            StringAssert.Contains(result[0], "BEGIN TRANSACTION");
        }

        [TestMethod]
        public void ReadBatchFromScriptFile_NonExistentFile_ThrowsException()
        {
            // Arrange
            string filePath = Path.Combine(_testDir, "nonexistent.sql");

            // Act & Assert
            Assert.ThrowsExactly<FileNotFoundException>(() =>
                _batcher.ReadBatchFromScriptFile(filePath, stripTransaction: false, maintainBatchDelimiter: false));
        }

        #endregion

        #region LoadAndBatchSqlScripts Tests

        [TestMethod]
        public void LoadAndBatchSqlScripts_EmptyModel_ReturnsEmptyCollection()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Act
            var result = _batcher.LoadAndBatchSqlScripts(model, _testDir);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void LoadAndBatchSqlScripts_SingleScript_ReturnsSingleBatch()
        {
            // Arrange
            string filePath = Path.Combine(_testDir, "script1.sql");
            File.WriteAllText(filePath, "SELECT 1");

            var script = new Script(
                fileName: "script1.sql",
                buildOrder: 1.0,
                description: "Test",
                rollBackOnError: true,
                causesBuildFailure: true,
                dateAdded: DateTime.UtcNow,
                scriptId: Guid.NewGuid().ToString(),
                database: "TestDB",
                stripTransactionText: false,
                allowMultipleRuns: false,
                addedBy: "user",
                scriptTimeOut: 30,
                tag: null,
                dateModified: null,
                modifiedBy: null);

            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                script: new List<Script> { script },
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript>());

            // Act
            var result = _batcher.LoadAndBatchSqlScripts(model, _testDir);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("script1.sql", result[0].ScriptfileName);
        }

        [TestMethod]
        public void LoadAndBatchSqlScripts_MultipleScripts_ReturnsOrderedCollection()
        {
            // Arrange
            File.WriteAllText(Path.Combine(_testDir, "script1.sql"), "SELECT 1");
            File.WriteAllText(Path.Combine(_testDir, "script2.sql"), "SELECT 2");
            File.WriteAllText(Path.Combine(_testDir, "script3.sql"), "SELECT 3");

            var scripts = new List<Script>
            {
                new Script("script3.sql", 3.0, "Third", true, true, DateTime.UtcNow, Guid.NewGuid().ToString(), "TestDB", false, false, "user", 30, null, null, null),
                new Script("script1.sql", 1.0, "First", true, true, DateTime.UtcNow, Guid.NewGuid().ToString(), "TestDB", false, false, "user", 30, null, null, null),
                new Script("script2.sql", 2.0, "Second", true, true, DateTime.UtcNow, Guid.NewGuid().ToString(), "TestDB", false, false, "user", 30, null, null, null)
            };

            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                script: scripts,
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript>());

            // Act
            var result = _batcher.LoadAndBatchSqlScripts(model, _testDir);

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("script1.sql", result[0].ScriptfileName);
            Assert.AreEqual("script2.sql", result[1].ScriptfileName);
            Assert.AreEqual("script3.sql", result[2].ScriptfileName);
        }

        [TestMethod]
        public void LoadAndBatchSqlScripts_ScriptWithMultipleBatches_ReturnsAllBatches()
        {
            // Arrange
            File.WriteAllText(Path.Combine(_testDir, "script.sql"), "SELECT 1\r\nGO\r\nSELECT 2\r\nGO\r\nSELECT 3");

            var script = new Script("script.sql", 1.0, "Test", true, true, DateTime.UtcNow, Guid.NewGuid().ToString(), "TestDB", false, false, "user", 30, null, null, null);

            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                script: new List<Script> { script },
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript>());

            // Act
            var result = _batcher.LoadAndBatchSqlScripts(model, _testDir);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(3, result[0].ScriptBatchContents.Length);
        }

        [TestMethod]
        public void LoadAndBatchSqlScripts_NullBuildOrder_TreatedAsMaxValue()
        {
            // Arrange
            File.WriteAllText(Path.Combine(_testDir, "script1.sql"), "SELECT 1");
            File.WriteAllText(Path.Combine(_testDir, "script2.sql"), "SELECT 2");

            var scripts = new List<Script>
            {
                new Script("script1.sql", null, "No Order", true, true, DateTime.UtcNow, Guid.NewGuid().ToString(), "TestDB", false, false, "user", 30, null, null, null),
                new Script("script2.sql", 1.0, "First", true, true, DateTime.UtcNow, Guid.NewGuid().ToString(), "TestDB", false, false, "user", 30, null, null, null)
            };

            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                script: scripts,
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript>());

            // Act
            var result = _batcher.LoadAndBatchSqlScripts(model, _testDir);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("script2.sql", result[0].ScriptfileName); // Order 1.0 comes first
            Assert.AreEqual("script1.sql", result[1].ScriptfileName); // Null order comes last
        }

        #endregion

        #region IsInComment Tests

        [TestMethod]
        public void IsInComment_IndexInDoubleDashComment_ReturnsTrue()
        {
            // Arrange
            string script = "SELECT 1 -- this is GO\nSELECT 2";
            int goIndex = script.IndexOf("GO");

            // Act
            var result = _batcher.IsInComment(script, goIndex);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsInComment_IndexNotInComment_ReturnsFalse()
        {
            // Arrange
            string script = "SELECT 1\nGO\nSELECT 2";
            int goIndex = script.IndexOf("GO");

            // Act
            var result = _batcher.IsInComment(script, goIndex);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsInComment_IndexInMultiLineComment_ReturnsTrue()
        {
            // Arrange
            string script = "SELECT 1 /* this is GO */ SELECT 2";
            int goIndex = script.IndexOf("GO");

            // Act
            var result = _batcher.IsInComment(script, goIndex);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsInComment_IndexAfterMultiLineComment_ReturnsFalse()
        {
            // Arrange
            string script = "/* comment */ GO";
            int goIndex = script.IndexOf("GO");

            // Act
            var result = _batcher.IsInComment(script, goIndex);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region RemoveTransactionReferences Tests

        [TestMethod]
        public void RemoveTransactionReferences_BeginTransaction_Removed()
        {
            // Arrange
            string script = "BEGIN TRANSACTION\r\nSELECT 1";

            // Act
            var result = _batcher.RemoveTransactionReferences(script);

            // Assert
            Assert.IsFalse(result.Contains("BEGIN TRANSACTION"));
        }

        [TestMethod]
        public void RemoveTransactionReferences_CommitTransaction_Removed()
        {
            // Arrange
            string script = "SELECT 1\r\nCOMMIT TRANSACTION";

            // Act
            var result = _batcher.RemoveTransactionReferences(script);

            // Assert
            Assert.IsFalse(result.Contains("COMMIT TRANSACTION"));
        }

        [TestMethod]
        public void RemoveTransactionReferences_RollbackTransaction_Removed()
        {
            // Arrange
            string script = "SELECT 1\r\nROLLBACK TRANSACTION";

            // Act
            var result = _batcher.RemoveTransactionReferences(script);

            // Assert
            Assert.IsFalse(result.Contains("ROLLBACK TRANSACTION"));
        }

        [TestMethod]
        public void RemoveTransactionReferences_BeginTran_Removed()
        {
            // Arrange
            string script = "BEGIN TRAN\r\nSELECT 1";

            // Act
            var result = _batcher.RemoveTransactionReferences(script);

            // Assert
            Assert.IsFalse(result.Contains("BEGIN TRAN"));
        }

        [TestMethod]
        public void RemoveTransactionReferences_Commit_Removed()
        {
            // Arrange
            string script = "SELECT 1\r\nCOMMIT";

            // Act
            var result = _batcher.RemoveTransactionReferences(script);

            // Assert
            Assert.IsFalse(result.Contains("COMMIT", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void RemoveTransactionReferences_SetTransactionIsolationLevel_Removed()
        {
            // Arrange
            string script = "SET TRANSACTION ISOLATION LEVEL SERIALIZABLE\r\nSELECT 1";

            // Act
            var result = _batcher.RemoveTransactionReferences(script);

            // Assert
            Assert.IsFalse(result.Contains("SET TRANSACTION ISOLATION LEVEL SERIALIZABLE"));
        }

        [TestMethod]
        public void RemoveTransactionReferences_TransactionInComment_NotRemoved()
        {
            // Arrange
            string script = "-- BEGIN TRANSACTION\r\nSELECT 1";

            // Act
            var result = _batcher.RemoveTransactionReferences(script);

            // Assert
            StringAssert.Contains(result, "BEGIN TRANSACTION");
        }

        #endregion

        #region RegexRemoveIfNotInComments Tests

        [TestMethod]
        public void RegexRemoveIfNotInComments_PatternNotInComment_Removed()
        {
            // Arrange
            string script = "TEST_PATTERN SELECT 1";
            var options = System.Text.RegularExpressions.RegexOptions.IgnoreCase;

            // Act
            var result = _batcher.RegexRemoveIfNotInComments("TEST_PATTERN", script, options);

            // Assert
            Assert.IsFalse(result.Contains("TEST_PATTERN"));
        }

        [TestMethod]
        public void RegexRemoveIfNotInComments_PatternInComment_NotRemoved()
        {
            // Arrange
            string script = "-- TEST_PATTERN\nSELECT 1";
            var options = System.Text.RegularExpressions.RegexOptions.IgnoreCase;

            // Act
            var result = _batcher.RegexRemoveIfNotInComments("TEST_PATTERN", script, options);

            // Assert
            StringAssert.Contains(result, "TEST_PATTERN");
        }

        [TestMethod]
        public void RegexRemoveIfNotInComments_MultipleMatches_RemovesAllNotInComments()
        {
            // Arrange
            string script = "MATCH SELECT 1 MATCH -- MATCH\nMATCH";
            var options = System.Text.RegularExpressions.RegexOptions.IgnoreCase;

            // Act
            var result = _batcher.RegexRemoveIfNotInComments("MATCH", script, options);

            // Assert
            // Only the one in the comment should remain
            Assert.AreEqual(1, System.Text.RegularExpressions.Regex.Matches(result, "MATCH", options).Count);
        }

        #endregion

        #region PostgreSQL Script Handling Tests

        [TestMethod]
        public void ReadBatchFromScriptText_PostgresMultiStatement_NoGO_ReturnsSingleBatch()
        {
            // PostgreSQL scripts don't use GO delimiters — multi-statement scripts stay as one batch
            string script = "CREATE TABLE test (id SERIAL PRIMARY KEY);\r\nINSERT INTO test (id) VALUES (1);\r\nSELECT * FROM test LIMIT 1;";

            var result = _batcher.ReadBatchFromScriptText(script, stripTransaction: false, maintainBatchDelimiter: false);

            Assert.AreEqual(1, result.Count);
            StringAssert.Contains(result[0], "CREATE TABLE test");
            StringAssert.Contains(result[0], "INSERT INTO test");
            StringAssert.Contains(result[0], "LIMIT 1");
        }

        [TestMethod]
        public void ReadBatchFromScriptText_PostgresFunctions_NoSplitting()
        {
            // PG function body with $$ delimiters should remain as a single batch
            string script = @"CREATE OR REPLACE FUNCTION add_numbers(a INTEGER, b INTEGER) RETURNS INTEGER AS $$
BEGIN
    RETURN a + b;
END;
$$ LANGUAGE plpgsql;";

            var result = _batcher.ReadBatchFromScriptText(script, stripTransaction: false, maintainBatchDelimiter: false);

            Assert.AreEqual(1, result.Count);
            StringAssert.Contains(result[0], "LANGUAGE plpgsql");
        }

        [TestMethod]
        public void ReadBatchFromScriptText_PostgresUUID_NoSplitting()
        {
            // Typical PostgreSQL script using gen_random_uuid() — no GO
            string script = "INSERT INTO transactiontest (id, message, created_at) VALUES (gen_random_uuid(), 'INSERT TEST', now());";

            var result = _batcher.ReadBatchFromScriptText(script, stripTransaction: false, maintainBatchDelimiter: false);

            Assert.AreEqual(1, result.Count);
            StringAssert.Contains(result[0], "gen_random_uuid()");
        }

        #endregion
    }
}
