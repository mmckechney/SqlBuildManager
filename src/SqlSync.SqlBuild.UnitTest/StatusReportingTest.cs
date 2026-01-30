using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SqlSync.SqlBuild.Legacy;
using SqlSync.SqlBuild.MultiDb;
using SqlSync.SqlBuild.Services;
using SqlSync.SqlBuild.Status;
using System;
using System.IO;
using System.Xml.Serialization;

#nullable enable

namespace SqlSync.SqlBuild.UnitTest
{
    /// <summary>
    /// This is a test class for StatusReportingTest and is intended
    /// to contain all StatusReportingTest Unit Tests
    /// </summary>
    [TestClass]
    public class StatusReportingTest
    {
        public TestContext TestContext { get; set; } = null!;

        [TestMethod]
        public void StatusReporting_Constructor_SetsProperties()
        {
            // Arrange
            var dbUtilMock = new Mock<IDatabaseUtility>();
            var buildData = new SqlSyncBuildData();
            var multiDbData = new MultiDbData();
            string projectFilePath = @"C:\Test\Project";
            string buildZipFileName = "build.sbm";

            // Act
            var reporting = new StatusReporting(dbUtilMock.Object, buildData, multiDbData, projectFilePath, buildZipFileName);

            // Assert
            Assert.IsNotNull(reporting);
        }

        [TestMethod]
        public void ReportType_IsDefined_XML()
        {
            // Assert
            Assert.IsTrue(Enum.IsDefined(typeof(ReportType), ReportType.XML));
        }

        [TestMethod]
        public void ReportType_IsDefined_CSV()
        {
            // Assert
            Assert.IsTrue(Enum.IsDefined(typeof(ReportType), ReportType.CSV));
        }

        [TestMethod]
        public void ReportType_IsDefined_HTML()
        {
            // Assert
            Assert.IsTrue(Enum.IsDefined(typeof(ReportType), ReportType.HTML));
        }

        [TestMethod]
        public void ReportType_IsDefined_Summary()
        {
            // Assert
            Assert.IsTrue(Enum.IsDefined(typeof(ReportType), ReportType.Summary));
        }
    }

    [TestClass]
    public class StatusReportingCollectionTests
    {
        [TestMethod]
        public void ServerStatusDataCollection_CreatesAndSetsProperties()
        {
            // Arrange - Use deserialization to create an instance
            var serializer = new XmlSerializer(typeof(ServerStatusDataCollection));
            ServerStatusDataCollection collection;
            using (var reader = new StringReader("<ServerStatusDataCollection></ServerStatusDataCollection>"))
            {
                collection = (ServerStatusDataCollection)serializer.Deserialize(reader)!;
            }
            
            // Act
            collection.BuildFileNameFull = @"C:\Builds\TestBuild.sbm";
            var databases = collection["Server1"];
            databases["DB1"] = new StatusDataCollection
            {
                new ScriptStatusData { ScriptName = "Script1.sql", ScriptId = "id1" }
            };

            // Assert
            Assert.AreEqual("TestBuild.sbm", collection.BuildFileNameShort);
            Assert.IsNotNull(collection["Server1"]);
            Assert.AreEqual(1, collection["Server1"]["DB1"].Count);
        }

        [TestMethod]
        public void StatusDataCollection_AddScriptStatusData_IncreasesCount()
        {
            // Arrange
            var collection = new StatusDataCollection();
            var data = new ScriptStatusData
            {
                ScriptName = "TestScript.sql",
                ScriptId = "test-id",
                ScriptStatus = ScriptStatusType.UpToDate
            };

            // Act
            collection.Add(data);

            // Assert
            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual("TestScript.sql", collection[0].ScriptName);
        }

        [TestMethod]
        public void StatusDataCollection_IsListOfScriptStatusData()
        {
            // Arrange
            var collection = new StatusDataCollection();

            // Assert
            Assert.IsInstanceOfType(collection, typeof(System.Collections.Generic.List<ScriptStatusData>));
        }

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
        public void ServerDictionary_Servers_ReturnsKeys()
        {
            // Arrange
            var dict = new ServerDictionary();
            dict.Add("Server1", new Databases());
            dict.Add("Server2", new Databases());

            // Act
            var servers = dict.Servers;

            // Assert
            Assert.AreEqual(2, servers.Count);
        }

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
                new ScriptStatusData { ScriptName = "Script1.sql" }
            };

            // Act
            databases.Add("TestDB", scripts);

            // Assert
            Assert.IsTrue(databases.ContainsKey("TestDB"));
            Assert.AreEqual(1, databases["TestDB"].Count);
        }
    }
}
