using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.MultiDb;
using SqlSync.SqlBuild.Services;
using SqlSync.SqlBuild.Status;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;

#nullable enable

namespace SqlSync.SqlBuild.UnitTest.Status
{
    /// <summary>
    /// Comprehensive tests for Status namespace classes - Final Round
    /// </summary>
    [TestClass]
    public class StatusHelperFinalTests
    {
        // Note: StatusHelper.DetermineScriptRunStatus requires database connectivity,
        // so we test what we can without database

        [TestMethod]
        public void ScriptStatusType_HasAllExpectedValues_Final()
        {
            // Assert all enum values exist
            Assert.IsTrue(Enum.IsDefined(typeof(ScriptStatusType), ScriptStatusType.Unknown));
            Assert.IsTrue(Enum.IsDefined(typeof(ScriptStatusType), ScriptStatusType.NotRun));
            Assert.IsTrue(Enum.IsDefined(typeof(ScriptStatusType), ScriptStatusType.Locked));
            Assert.IsTrue(Enum.IsDefined(typeof(ScriptStatusType), ScriptStatusType.UpToDate));
            Assert.IsTrue(Enum.IsDefined(typeof(ScriptStatusType), ScriptStatusType.ChangedSinceCommit));
            Assert.IsTrue(Enum.IsDefined(typeof(ScriptStatusType), ScriptStatusType.ServerChange));
            Assert.IsTrue(Enum.IsDefined(typeof(ScriptStatusType), ScriptStatusType.FileMissing));
            Assert.IsTrue(Enum.IsDefined(typeof(ScriptStatusType), ScriptStatusType.NotRunButOlderVersion));
        }
    }

    [TestClass]
    public class StatusDictionaryFinalTests
    {
        [TestMethod]
        public void StatusDictionary_DefaultConstructor_SetsDefaultValues()
        {
            // Act
            var dict = new StatusDictionary<string>();

            // Assert
            Assert.AreEqual("key", dict.KeyName);
            Assert.AreEqual("value", dict.ValueName);
            Assert.AreEqual("item", dict.ItemName);
        }

        [TestMethod]
        public void StatusDictionary_ParameterizedConstructor_SetsCustomValues()
        {
            // Act
            var dict = new StatusDictionary<int>("myItem", "myKey", "myValue");

            // Assert
            Assert.AreEqual("myKey", dict.KeyName);
            Assert.AreEqual("myValue", dict.ValueName);
            Assert.AreEqual("myItem", dict.ItemName);
        }

        [TestMethod]
        public void StatusDictionary_GetSchema_ReturnsNull()
        {
            // Arrange
            var dict = new StatusDictionary<string>();

            // Act
            var schema = dict.GetSchema();

            // Assert
            Assert.IsNull(schema);
        }

        [TestMethod]
        public void StatusDictionary_CanSetKeyName()
        {
            // Arrange
            var dict = new StatusDictionary<string>();

            // Act
            dict.KeyName = "CustomKey";

            // Assert
            Assert.AreEqual("CustomKey", dict.KeyName);
        }

        [TestMethod]
        public void StatusDictionary_CanSetValueName()
        {
            // Arrange
            var dict = new StatusDictionary<string>();

            // Act
            dict.ValueName = "CustomValue";

            // Assert
            Assert.AreEqual("CustomValue", dict.ValueName);
        }

        [TestMethod]
        public void StatusDictionary_CanSetItemName()
        {
            // Arrange
            var dict = new StatusDictionary<string>();

            // Act
            dict.ItemName = "CustomItem";

            // Assert
            Assert.AreEqual("CustomItem", dict.ItemName);
        }

        [TestMethod]
        public void StatusDictionary_WriteXml_WritesCorrectFormat()
        {
            // Arrange
            var dict = new StatusDictionary<string>("item", "key", "value");
            dict.Add("Key1", "Value1");
            dict.Add("Key2", "Value2");

            // Act
            var sb = new System.Text.StringBuilder();
            using (var writer = XmlWriter.Create(sb, new XmlWriterSettings { OmitXmlDeclaration = true }))
            {
                writer.WriteStartElement("root");
                dict.WriteXml(writer);
                writer.WriteEndElement();
            }

            var xml = sb.ToString();

            // Assert
            Assert.IsTrue(xml.Contains("Name=\"Key1\""));
            Assert.IsTrue(xml.Contains("Name=\"Key2\""));
            Assert.IsTrue(xml.Contains("<string>Value1</string>") || xml.Contains("Value1"));
            Assert.IsTrue(xml.Contains("<string>Value2</string>") || xml.Contains("Value2"));
        }

        [TestMethod]
        public void StatusDictionary_AddAndRetrieve_WorksCorrectly()
        {
            // Arrange
            var dict = new StatusDictionary<string>();

            // Act
            dict.Add("testKey", "testValue");
            var retrieved = dict["testKey"];

            // Assert
            Assert.AreEqual("testValue", retrieved);
        }
    }

    [TestClass]
    public class ScriptStatusDataFinalTests
    {
        [TestMethod]
        public void ScriptStatusData_DefaultConstructor_SetsDefaults()
        {
            // Act
            var data = new ScriptStatusData();

            // Assert
            Assert.AreEqual(string.Empty, data.ScriptName);
            Assert.AreEqual(string.Empty, data.ScriptId);
            Assert.AreEqual(string.Empty, data.ServerName);
            Assert.AreEqual(string.Empty, data.DatabaseName);
            Assert.AreEqual(ScriptStatusType.Unknown, data.ScriptStatus);
        }

        [TestMethod]
        public void ScriptStatusData_Properties_CanBeSet()
        {
            // Arrange
            var data = new ScriptStatusData();
            var commitDate = new DateTime(2023, 6, 15);
            var changeDate = new DateTime(2023, 6, 16);

            // Act
            data.ScriptName = "TestScript.sql";
            data.ScriptId = "test-id-123";
            data.ServerName = "TestServer";
            data.DatabaseName = "TestDb";
            data.ScriptStatus = ScriptStatusType.UpToDate;
            data.LastCommitDate = commitDate;
            data.ServerChangeDate = changeDate;

            // Assert
            Assert.AreEqual("TestScript.sql", data.ScriptName);
            Assert.AreEqual("test-id-123", data.ScriptId);
            Assert.AreEqual("TestServer", data.ServerName);
            Assert.AreEqual("TestDb", data.DatabaseName);
            Assert.AreEqual(ScriptStatusType.UpToDate, data.ScriptStatus);
            Assert.AreEqual(commitDate, data.LastCommitDate);
            Assert.AreEqual(changeDate, data.ServerChangeDate);
        }

        [TestMethod]
        public void ScriptStatusData_Fill_WithValidDataRow_ReturnsTrue()
        {
            // Arrange
            var data = new ScriptStatusData();
            var table = new DataTable();
            table.Columns.Add("FileName", typeof(string));
            table.Columns.Add("ScriptId", typeof(string));
            table.Columns.Add("Database", typeof(string));
            var row = table.NewRow();
            row["FileName"] = "script.sql";
            row["ScriptId"] = "id-123";
            row["Database"] = "MyDatabase";
            table.Rows.Add(row);

            // Act
            var result = data.Fill(row);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual("script.sql", data.ScriptName);
            Assert.AreEqual("id-123", data.ScriptId);
            Assert.AreEqual("MyDatabase", data.DatabaseName);
        }

        [TestMethod]
        public void ScriptStatusData_Fill_WithNullValues_ReturnsTrue()
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
            var result = data.Fill(row);

            // Assert
            Assert.IsTrue(result);
        }
    }

    [TestClass]
    public class StatusDataCollectionFinalTests
    {
        [TestMethod]
        public void StatusDataCollection_Constructor_CreatesEmptyList()
        {
            // Act
            var collection = new StatusDataCollection();

            // Assert
            Assert.AreEqual(0, collection.Count);
        }

        [TestMethod]
        public void StatusDataCollection_AddMultipleItems_IncreasesCount()
        {
            // Arrange
            var collection = new StatusDataCollection();

            // Act
            collection.Add(new ScriptStatusData { ScriptName = "Script1.sql" });
            collection.Add(new ScriptStatusData { ScriptName = "Script2.sql" });
            collection.Add(new ScriptStatusData { ScriptName = "Script3.sql" });

            // Assert
            Assert.AreEqual(3, collection.Count);
        }

        [TestMethod]
        public void StatusDataCollection_IndexAccess_ReturnsCorrectItem()
        {
            // Arrange
            var collection = new StatusDataCollection();
            collection.Add(new ScriptStatusData { ScriptName = "First.sql" });
            collection.Add(new ScriptStatusData { ScriptName = "Second.sql" });

            // Act
            var first = collection[0];
            var second = collection[1];

            // Assert
            Assert.AreEqual("First.sql", first.ScriptName);
            Assert.AreEqual("Second.sql", second.ScriptName);
        }
    }

    [TestClass]
    public class ServerDictionaryFinalTests
    {
        [TestMethod]
        public void ServerDictionary_Constructor_SetsCorrectNames()
        {
            // Act
            var dict = new ServerDictionary();

            // Assert
            Assert.AreEqual("Server", dict.ItemName);
            Assert.AreEqual("ServerName", dict.KeyName);
            Assert.AreEqual("Databases", dict.ValueName);
        }

        [TestMethod]
        public void ServerDictionary_Servers_ReturnsAllKeys()
        {
            // Arrange
            var dict = new ServerDictionary();
            dict.Add("Server1", new Databases());
            dict.Add("Server2", new Databases());
            dict.Add("Server3", new Databases());

            // Act
            var servers = dict.Servers;

            // Assert
            Assert.AreEqual(3, servers.Count);
            Assert.IsTrue(servers.Contains("Server1"));
            Assert.IsTrue(servers.Contains("Server2"));
            Assert.IsTrue(servers.Contains("Server3"));
        }
    }

    [TestClass]
    public class DatabasesFinalTests
    {
        [TestMethod]
        public void Databases_Constructor_SetsCorrectNames()
        {
            // Act
            var databases = new Databases();

            // Assert
            Assert.AreEqual("Database", databases.ItemName);
            Assert.AreEqual("DatabaseName", databases.KeyName);
            Assert.AreEqual("Scripts", databases.ValueName);
        }

        [TestMethod]
        public void Databases_AddAndRetrieve_WorksCorrectly()
        {
            // Arrange
            var databases = new Databases();
            var scripts = new StatusDataCollection
            {
                new ScriptStatusData { ScriptName = "Script1.sql" },
                new ScriptStatusData { ScriptName = "Script2.sql" }
            };

            // Act
            databases.Add("TestDB", scripts);

            // Assert
            Assert.IsTrue(databases.ContainsKey("TestDB"));
            Assert.AreEqual(2, databases["TestDB"].Count);
        }
    }

    [TestClass]
    public class ServerStatusDataCollectionFinalTests
    {
        [TestMethod]
        public void ServerStatusDataCollection_SetBuildFileNameFull_UpdatesBuildFileNameShort()
        {
            // Arrange
            var serializer = new XmlSerializer(typeof(ServerStatusDataCollection));
            ServerStatusDataCollection collection;
            using (var reader = new StringReader("<ServerStatusDataCollection></ServerStatusDataCollection>"))
            {
                collection = (ServerStatusDataCollection)serializer.Deserialize(reader)!;
            }

            // Act
            collection.BuildFileNameFull = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "MyBuild.sbm");

            // Assert
            Assert.AreEqual("MyBuild.sbm", collection.BuildFileNameShort);
        }

        [TestMethod]
        public void ServerStatusDataCollection_IndexerCreatesServerIfNotExists()
        {
            // Arrange
            var serializer = new XmlSerializer(typeof(ServerStatusDataCollection));
            ServerStatusDataCollection collection;
            using (var reader = new StringReader("<ServerStatusDataCollection></ServerStatusDataCollection>"))
            {
                collection = (ServerStatusDataCollection)serializer.Deserialize(reader)!;
            }

            // Act - accessing non-existent key should create it
            var databases = collection["NewServer"];

            // Assert
            Assert.IsNotNull(databases);
        }

        [TestMethod]
        public void ServerStatusDataCollection_MultipleServers_StoresAllCorrectly()
        {
            // Arrange
            var serializer = new XmlSerializer(typeof(ServerStatusDataCollection));
            ServerStatusDataCollection collection;
            using (var reader = new StringReader("<ServerStatusDataCollection></ServerStatusDataCollection>"))
            {
                collection = (ServerStatusDataCollection)serializer.Deserialize(reader)!;
            }

            // Act
            collection["Server1"]["Db1"] = new StatusDataCollection { new ScriptStatusData { ScriptName = "S1.sql" } };
            collection["Server1"]["Db2"] = new StatusDataCollection { new ScriptStatusData { ScriptName = "S2.sql" } };
            collection["Server2"]["Db1"] = new StatusDataCollection { new ScriptStatusData { ScriptName = "S3.sql" } };

            // Assert
            Assert.AreEqual(1, collection["Server1"]["Db1"].Count);
            Assert.AreEqual(1, collection["Server1"]["Db2"].Count);
            Assert.AreEqual(1, collection["Server2"]["Db1"].Count);
        }
    }

    [TestClass]
    public class StatusReportingExtendedTests
    {
        private static SqlSyncBuildDataModel CreateEmptyModel() => new SqlSyncBuildDataModel(
            new List<SqlSyncBuildProject>(),
            new List<Script>(),
            new List<Build>(),
            new List<ScriptRun>(),
            new List<CommittedScript>());

        [TestMethod]
        public void StatusReporting_Constructor_WithValidParameters_CreatesInstance()
        {
            // Arrange
            var dbUtilMock = new Mock<IDatabaseUtility>();
            var buildDataModel = CreateEmptyModel();
            var multiDbData = new MultiDbData();

            // Act
            var reporting = new StatusReporting(dbUtilMock.Object, buildDataModel, multiDbData, Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "Test"), "build.sbm");

            // Assert
            Assert.IsNotNull(reporting);
        }
    }

    [TestClass]
    public class ReportTypeFinalTests
    {
        [TestMethod]
        public void ReportType_AllValuesAreDefined()
        {
            // Assert
            var values = Enum.GetValues(typeof(ReportType));
            Assert.AreEqual(4, values.Length); // XML, CSV, HTML, Summary
        }

        [TestMethod]
        public void ReportType_CanCastToInt()
        {
            // Act & Assert
            Assert.IsTrue((int)ReportType.XML >= 0);
            Assert.IsTrue((int)ReportType.CSV >= 0);
            Assert.IsTrue((int)ReportType.HTML >= 0);
            Assert.IsTrue((int)ReportType.Summary >= 0);
        }
    }
}
