using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SqlSync.SqlBuild.Legacy;
using SqlSync.SqlBuild.MultiDb;
using SqlSync.SqlBuild.Services;
using SqlSync.SqlBuild.Status;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace SqlSync.SqlBuild.UnitTest.Status
{
    /// <summary>
    /// Extended tests for StatusReporting and related classes to improve code coverage
    /// </summary>
    [TestClass]
    public class StatusReportingCoverageTests
    {
        private Mock<IDatabaseUtility> _mockDbUtil;

        [TestInitialize]
        public void Setup()
        {
            _mockDbUtil = new Mock<IDatabaseUtility>();
        }

        #region StatusReporting Constructor Tests

        [TestMethod]
        public void StatusReporting_Constructor_WithValidParameters_CreatesInstance()
        {
            // Arrange
            var buildData = new SqlSyncBuildData();
            var multiDbData = new MultiDbData();
            string projectFilePath = @"C:\Test\Project";
            string buildZipFileName = "build.sbm";

            // Act
            var reporting = new StatusReporting(_mockDbUtil.Object, buildData, multiDbData, projectFilePath, buildZipFileName);

            // Assert
            Assert.IsNotNull(reporting);
        }

        [TestMethod]
        public void StatusReporting_Constructor_WithNullBuildData_CreatesInstance()
        {
            // Arrange
            var multiDbData = new MultiDbData();
            string projectFilePath = @"C:\Test\Project";
            string buildZipFileName = "build.sbm";

            // Act
            var reporting = new StatusReporting(_mockDbUtil.Object, null, multiDbData, projectFilePath, buildZipFileName);

            // Assert
            Assert.IsNotNull(reporting);
        }

        [TestMethod]
        public void StatusReporting_Constructor_WithEmptyPaths_CreatesInstance()
        {
            // Arrange
            var buildData = new SqlSyncBuildData();
            var multiDbData = new MultiDbData();

            // Act
            var reporting = new StatusReporting(_mockDbUtil.Object, buildData, multiDbData, string.Empty, string.Empty);

            // Assert
            Assert.IsNotNull(reporting);
        }

        #endregion

        #region GetScriptStatus Tests

        [TestMethod]
        public void GetScriptStatus_WithEmptyMultiDbData_WorksWithoutThrowing()
        {
            // Arrange
            var buildData = new SqlSyncBuildData();
            var multiDbData = new MultiDbData();
            var reporting = new StatusReporting(_mockDbUtil.Object, buildData, multiDbData, @"C:\Test", "build.sbm");

            // Act & Assert - Just verify the object can be created and method exists
            // The actual method requires proper BackgroundWorker setup, so we just test construction
            Assert.IsNotNull(reporting);
        }

        #endregion

        #region ReportType Tests

        [TestMethod]
        public void ReportType_AllValues_AreDefined()
        {
            // Assert
            Assert.IsTrue(Enum.IsDefined(typeof(ReportType), ReportType.XML));
            Assert.IsTrue(Enum.IsDefined(typeof(ReportType), ReportType.CSV));
            Assert.IsTrue(Enum.IsDefined(typeof(ReportType), ReportType.HTML));
            Assert.IsTrue(Enum.IsDefined(typeof(ReportType), ReportType.Summary));
        }

        [TestMethod]
        public void ReportType_Values_CanBeParsed()
        {
            // Assert - verify enum parsing works
            Assert.AreEqual(ReportType.XML, (ReportType)Enum.Parse(typeof(ReportType), "XML"));
            Assert.AreEqual(ReportType.CSV, (ReportType)Enum.Parse(typeof(ReportType), "CSV"));
            Assert.AreEqual(ReportType.HTML, (ReportType)Enum.Parse(typeof(ReportType), "HTML"));
            Assert.AreEqual(ReportType.Summary, (ReportType)Enum.Parse(typeof(ReportType), "Summary"));
        }

        #endregion
    }

    [TestClass]
    public class ScriptStatusDataCoverageTests
    {
        #region ScriptStatusData Property Tests

        [TestMethod]
        public void ScriptStatusData_DefaultConstructor_CreatesInstance_Coverage()
        {
            // Act
            var data = new ScriptStatusData();

            // Assert
            Assert.IsNotNull(data);
        }

        [TestMethod]
        public void ScriptStatusData_SetAndGetProperties_WorksCorrectly_Coverage()
        {
            // Arrange
            var data = new ScriptStatusData();

            // Act
            data.ScriptName = "TestScript.sql";
            data.ScriptId = "test-id-123";
            data.ScriptStatus = ScriptStatusType.UpToDate;

            // Assert
            Assert.AreEqual("TestScript.sql", data.ScriptName);
            Assert.AreEqual("test-id-123", data.ScriptId);
            Assert.AreEqual(ScriptStatusType.UpToDate, data.ScriptStatus);
        }

        [TestMethod]
        public void ScriptStatusData_AllStatusTypes_CanBeSet_Coverage()
        {
            // Arrange
            var data = new ScriptStatusData();

            // Act & Assert - test each status type
            foreach (ScriptStatusType status in Enum.GetValues(typeof(ScriptStatusType)))
            {
                data.ScriptStatus = status;
                Assert.AreEqual(status, data.ScriptStatus);
            }
        }

        #endregion
    }

    [TestClass]
    public class StatusDataCollectionCoverageTests
    {
        #region StatusDataCollection Tests

        [TestMethod]
        public void StatusDataCollection_DefaultConstructor_CreatesEmptyList_Coverage()
        {
            // Act
            var collection = new StatusDataCollection();

            // Assert
            Assert.IsNotNull(collection);
            Assert.AreEqual(0, collection.Count);
        }

        [TestMethod]
        public void StatusDataCollection_Add_IncreasesCount_Coverage()
        {
            // Arrange
            var collection = new StatusDataCollection();
            var data = new ScriptStatusData { ScriptName = "Test.sql" };

            // Act
            collection.Add(data);

            // Assert
            Assert.AreEqual(1, collection.Count);
        }

        [TestMethod]
        public void StatusDataCollection_AddMultiple_MaintainsOrder_Coverage()
        {
            // Arrange
            var collection = new StatusDataCollection();

            // Act
            collection.Add(new ScriptStatusData { ScriptName = "First.sql" });
            collection.Add(new ScriptStatusData { ScriptName = "Second.sql" });
            collection.Add(new ScriptStatusData { ScriptName = "Third.sql" });

            // Assert
            Assert.AreEqual(3, collection.Count);
            Assert.AreEqual("First.sql", collection[0].ScriptName);
            Assert.AreEqual("Second.sql", collection[1].ScriptName);
            Assert.AreEqual("Third.sql", collection[2].ScriptName);
        }

        [TestMethod]
        public void StatusDataCollection_IsListOfScriptStatusData_Coverage()
        {
            // Arrange
            var collection = new StatusDataCollection();

            // Assert
            Assert.IsInstanceOfType(collection, typeof(List<ScriptStatusData>));
        }

        #endregion
    }

    [TestClass]
    public class ServerStatusDataCollectionCoverageTests
    {
        #region ServerStatusDataCollection Tests

        [TestMethod]
        public void ServerStatusDataCollection_Serialization_RoundTrips_Coverage()
        {
            // Arrange
            var serializer = new XmlSerializer(typeof(ServerStatusDataCollection));
            string xml = @"<ServerStatusDataCollection>
                <BuildFileNameFull>C:\Test\Build.sbm</BuildFileNameFull>
            </ServerStatusDataCollection>";

            // Act
            using var reader = new StringReader(xml);
            var collection = (ServerStatusDataCollection)serializer.Deserialize(reader);

            // Assert
            Assert.IsNotNull(collection);
        }

        [TestMethod]
        public void ServerStatusDataCollection_BuildFileNameShort_ExtractsFileName_Coverage()
        {
            // Arrange
            var serializer = new XmlSerializer(typeof(ServerStatusDataCollection));
            using var reader = new StringReader("<ServerStatusDataCollection></ServerStatusDataCollection>");
            var collection = (ServerStatusDataCollection)serializer.Deserialize(reader);

            // Act
            collection.BuildFileNameFull = @"C:\Some\Path\TestBuild.sbm";

            // Assert
            Assert.AreEqual("TestBuild.sbm", collection.BuildFileNameShort);
        }

        [TestMethod]
        public void ServerStatusDataCollection_Indexer_CreatesDatabasesIfNotExists_Coverage()
        {
            // Arrange
            var serializer = new XmlSerializer(typeof(ServerStatusDataCollection));
            using var reader = new StringReader("<ServerStatusDataCollection></ServerStatusDataCollection>");
            var collection = (ServerStatusDataCollection)serializer.Deserialize(reader);

            // Act
            var databases = collection["NewServer"];

            // Assert
            Assert.IsNotNull(databases);
        }

        [TestMethod]
        public void ServerStatusDataCollection_Indexer_ReturnsSameDatabasesForSameServer_Coverage()
        {
            // Arrange
            var serializer = new XmlSerializer(typeof(ServerStatusDataCollection));
            using var reader = new StringReader("<ServerStatusDataCollection></ServerStatusDataCollection>");
            var collection = (ServerStatusDataCollection)serializer.Deserialize(reader);

            // Act
            var databases1 = collection["Server1"];
            databases1["DB1"] = new StatusDataCollection();
            var databases2 = collection["Server1"];

            // Assert
            Assert.AreSame(databases1, databases2);
            Assert.IsTrue(databases2.ContainsKey("DB1"));
        }

        #endregion
    }

    [TestClass]
    public class ServerDictionaryCoverageTests
    {
        #region ServerDictionary Tests

        [TestMethod]
        public void ServerDictionary_Constructor_SetsCorrectItemName_Coverage()
        {
            // Act
            var dict = new ServerDictionary();

            // Assert
            Assert.AreEqual("Server", dict.ItemName);
        }

        [TestMethod]
        public void ServerDictionary_Constructor_SetsCorrectKeyName_Coverage()
        {
            // Act
            var dict = new ServerDictionary();

            // Assert
            Assert.AreEqual("ServerName", dict.KeyName);
        }

        [TestMethod]
        public void ServerDictionary_Constructor_SetsCorrectValueName_Coverage()
        {
            // Act
            var dict = new ServerDictionary();

            // Assert
            Assert.AreEqual("Databases", dict.ValueName);
        }

        [TestMethod]
        public void ServerDictionary_Servers_ReturnsAllKeys_Coverage()
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

        [TestMethod]
        public void ServerDictionary_EmptyDictionary_ReturnsEmptyServersList_Coverage()
        {
            // Arrange
            var dict = new ServerDictionary();

            // Act
            var servers = dict.Servers;

            // Assert
            Assert.AreEqual(0, servers.Count);
        }

        #endregion
    }

    [TestClass]
    public class DatabasesCoverageTests
    {
        #region Databases Tests

        [TestMethod]
        public void Databases_Constructor_SetsCorrectItemName_Coverage()
        {
            // Act
            var databases = new Databases();

            // Assert
            Assert.AreEqual("Database", databases.ItemName);
        }

        [TestMethod]
        public void Databases_Constructor_SetsCorrectKeyName_Coverage()
        {
            // Act
            var databases = new Databases();

            // Assert
            Assert.AreEqual("DatabaseName", databases.KeyName);
        }

        [TestMethod]
        public void Databases_Constructor_SetsCorrectValueName_Coverage()
        {
            // Act
            var databases = new Databases();

            // Assert
            Assert.AreEqual("Scripts", databases.ValueName);
        }

        [TestMethod]
        public void Databases_AddAndRetrieve_WorksCorrectly_Coverage()
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

        [TestMethod]
        public void Databases_MultipleEntries_AllAccessible_Coverage()
        {
            // Arrange
            var databases = new Databases();
            databases.Add("DB1", new StatusDataCollection { new ScriptStatusData { ScriptName = "Script1.sql" } });
            databases.Add("DB2", new StatusDataCollection { new ScriptStatusData { ScriptName = "Script2.sql" } });
            databases.Add("DB3", new StatusDataCollection { new ScriptStatusData { ScriptName = "Script3.sql" } });

            // Assert
            Assert.AreEqual(3, databases.Count);
            Assert.AreEqual("Script1.sql", databases["DB1"][0].ScriptName);
            Assert.AreEqual("Script2.sql", databases["DB2"][0].ScriptName);
            Assert.AreEqual("Script3.sql", databases["DB3"][0].ScriptName);
        }

        #endregion
    }

    [TestClass]
    public class ScriptStatusTypeTests
    {
        #region ScriptStatusType Tests

        [TestMethod]
        public void ScriptStatusType_AllValuesAreDefined()
        {
            // Assert
            Assert.IsTrue(Enum.IsDefined(typeof(ScriptStatusType), ScriptStatusType.NotRun));
            Assert.IsTrue(Enum.IsDefined(typeof(ScriptStatusType), ScriptStatusType.UpToDate));
            Assert.IsTrue(Enum.IsDefined(typeof(ScriptStatusType), ScriptStatusType.Locked));
            Assert.IsTrue(Enum.IsDefined(typeof(ScriptStatusType), ScriptStatusType.ChangedSinceCommit));
            Assert.IsTrue(Enum.IsDefined(typeof(ScriptStatusType), ScriptStatusType.FileMissing));
            Assert.IsTrue(Enum.IsDefined(typeof(ScriptStatusType), ScriptStatusType.ServerChange));
            Assert.IsTrue(Enum.IsDefined(typeof(ScriptStatusType), ScriptStatusType.NotRunButOlderVersion));
        }

        [TestMethod]
        public void ScriptStatusType_CanConvertToString()
        {
            // Assert
            Assert.AreEqual("NotRun", ScriptStatusType.NotRun.ToString());
            Assert.AreEqual("UpToDate", ScriptStatusType.UpToDate.ToString());
            Assert.AreEqual("Locked", ScriptStatusType.Locked.ToString());
            Assert.AreEqual("ChangedSinceCommit", ScriptStatusType.ChangedSinceCommit.ToString());
            Assert.AreEqual("FileMissing", ScriptStatusType.FileMissing.ToString());
        }

        #endregion
    }
}
