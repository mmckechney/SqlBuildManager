using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Status;
using System;
using System.Data;

namespace SqlSync.SqlBuild.UnitTest.Status
{
    [TestClass]
    public class ScriptStatusDataTests
    {
        [TestMethod]
        public void Constructor_Default_InitializesWithDefaultValues()
        {
            // Act
            var data = new ScriptStatusData();

            // Assert
            Assert.AreEqual(string.Empty, data.ScriptName);
            Assert.AreEqual(string.Empty, data.ScriptId);
            Assert.AreEqual(ScriptStatusType.Unknown, data.ScriptStatus);
            Assert.AreEqual(string.Empty, data.ServerName);
            Assert.AreEqual(string.Empty, data.DatabaseName);
            Assert.AreEqual(default(DateTime), data.LastCommitDate);
            Assert.AreEqual(default(DateTime), data.ServerChangeDate);
        }

        [TestMethod]
        public void ScriptName_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var data = new ScriptStatusData();
            string expected = "TestScript.sql";

            // Act
            data.ScriptName = expected;

            // Assert
            Assert.AreEqual(expected, data.ScriptName);
        }

        [TestMethod]
        public void ScriptId_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var data = new ScriptStatusData();
            string expected = Guid.NewGuid().ToString();

            // Act
            data.ScriptId = expected;

            // Assert
            Assert.AreEqual(expected, data.ScriptId);
        }

        [TestMethod]
        public void ScriptStatus_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var data = new ScriptStatusData();

            // Act & Assert for various status types
            data.ScriptStatus = ScriptStatusType.UpToDate;
            Assert.AreEqual(ScriptStatusType.UpToDate, data.ScriptStatus);

            data.ScriptStatus = ScriptStatusType.NotRun;
            Assert.AreEqual(ScriptStatusType.NotRun, data.ScriptStatus);

            data.ScriptStatus = ScriptStatusType.ChangedSinceCommit;
            Assert.AreEqual(ScriptStatusType.ChangedSinceCommit, data.ScriptStatus);
        }

        [TestMethod]
        public void ServerName_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var data = new ScriptStatusData();
            string expected = "ProductionServer";

            // Act
            data.ServerName = expected;

            // Assert
            Assert.AreEqual(expected, data.ServerName);
        }

        [TestMethod]
        public void DatabaseName_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var data = new ScriptStatusData();
            string expected = "MainDatabase";

            // Act
            data.DatabaseName = expected;

            // Assert
            Assert.AreEqual(expected, data.DatabaseName);
        }

        [TestMethod]
        public void LastCommitDate_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var data = new ScriptStatusData();
            DateTime expected = new DateTime(2024, 1, 15, 10, 30, 0);

            // Act
            data.LastCommitDate = expected;

            // Assert
            Assert.AreEqual(expected, data.LastCommitDate);
        }

        [TestMethod]
        public void ServerChangeDate_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var data = new ScriptStatusData();
            DateTime expected = new DateTime(2024, 2, 20, 14, 45, 30);

            // Act
            data.ServerChangeDate = expected;

            // Assert
            Assert.AreEqual(expected, data.ServerChangeDate);
        }

        [TestMethod]
        public void Fill_WithValidDataRow_PopulatesProperties()
        {
            // Arrange
            var data = new ScriptStatusData();
            var table = new DataTable();
            table.Columns.Add("FileName", typeof(string));
            table.Columns.Add("ScriptId", typeof(string));
            table.Columns.Add("Database", typeof(string));

            var row = table.NewRow();
            row["FileName"] = "TestScript.sql";
            row["ScriptId"] = "abc123";
            row["Database"] = "TestDB";
            table.Rows.Add(row);

            // Act
            bool result = data.Fill(row);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual("TestScript.sql", data.ScriptName);
            Assert.AreEqual("abc123", data.ScriptId);
            Assert.AreEqual("TestDB", data.DatabaseName);
        }

        [TestMethod]
        public void Fill_WithNullValues_HandlesGracefully()
        {
            // Arrange
            var data = new ScriptStatusData();
            var table = new DataTable();
            table.Columns.Add("FileName", typeof(string));
            table.Columns.Add("ScriptId", typeof(string));
            table.Columns.Add("Database", typeof(string));

            var row = table.NewRow();
            row["FileName"] = DBNull.Value;
            row["ScriptId"] = DBNull.Value;
            row["Database"] = DBNull.Value;
            table.Rows.Add(row);

            // Act
            bool result = data.Fill(row);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(string.Empty, data.ScriptName);
            Assert.AreEqual(string.Empty, data.ScriptId);
            Assert.AreEqual(string.Empty, data.DatabaseName);
        }

        [TestMethod]
        public void Fill_WithPartialData_PopulatesOnlyAvailableFields()
        {
            // Arrange
            var data = new ScriptStatusData();
            var table = new DataTable();
            table.Columns.Add("FileName", typeof(string));
            table.Columns.Add("ScriptId", typeof(string));
            table.Columns.Add("Database", typeof(string));

            var row = table.NewRow();
            row["FileName"] = "OnlyFileName.sql";
            row["ScriptId"] = DBNull.Value;
            row["Database"] = "OnlyDB";
            table.Rows.Add(row);

            // Act
            bool result = data.Fill(row);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual("OnlyFileName.sql", data.ScriptName);
            Assert.AreEqual(string.Empty, data.ScriptId);
            Assert.AreEqual("OnlyDB", data.DatabaseName);
        }

        [TestMethod]
        public void AllScriptStatusTypes_CanBeAssigned()
        {
            // Arrange
            var data = new ScriptStatusData();
            var statusTypes = new[]
            {
                ScriptStatusType.NotRun,
                ScriptStatusType.Locked,
                ScriptStatusType.UpToDate,
                ScriptStatusType.ChangedSinceCommit,
                ScriptStatusType.ServerChange,
                ScriptStatusType.NotRunButOlderVersion,
                ScriptStatusType.FileMissing,
                ScriptStatusType.Unknown
            };

            // Act & Assert
            foreach (var status in statusTypes)
            {
                data.ScriptStatus = status;
                Assert.AreEqual(status, data.ScriptStatus, $"Failed for status: {status}");
            }
        }

        [TestMethod]
        public void Properties_SetWithSpecialCharacters_HandlesCorrectly()
        {
            // Arrange
            var data = new ScriptStatusData();

            // Act
            data.ScriptName = "Script With Spaces & Special!@#.sql";
            data.ServerName = "Server\\Instance";
            data.DatabaseName = "DB-Name_Test";

            // Assert
            Assert.AreEqual("Script With Spaces & Special!@#.sql", data.ScriptName);
            Assert.AreEqual("Server\\Instance", data.ServerName);
            Assert.AreEqual("DB-Name_Test", data.DatabaseName);
        }
    }
}
