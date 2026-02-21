using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.Connection;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.IO;

namespace SqlSync.SqlBuild.UnitTest
{


    /// <summary>
    ///This is a test class for MultiDbHelperTest and is intended
    ///to contain all MultiDbHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MultiDbHelperTest
    {

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        #region .: ImportMultiDbTextConfig(string[] fileContents) Tests :.
        /// <summary>
        ///A test for ImportMultiDbTextConfig
        ///</summary>
        [TestMethod()]
        public void ImportMultiDbTextConfigTest_GoodConfuguration()
        {
            string[] fileContents = new string[] { "SERVER:default,target;default2,target2", "SERVER2:default,target;default2,target2" };
            MultiDbData actual;
            actual = MultiDbHelper.ImportMultiDbTextConfig(fileContents);
            Assert.AreEqual("SERVER", actual[0].ServerName);
            Assert.AreEqual("SERVER2", actual[1].ServerName);

            DbOverrides seq = actual[0].Overrides;
            Assert.AreEqual("default", seq[0].DefaultDbTarget);
            Assert.AreEqual("target2", seq[1].OverrideDbTarget);
            seq = actual[1].Overrides;
            Assert.AreEqual("default2", seq[1].DefaultDbTarget);
            Assert.AreEqual("target", seq[0].OverrideDbTarget);
        }

        /// <summary>
        ///A test for ImportMultiDbTextConfig
        ///</summary>
        [TestMethod()]
        public void ImportMultiDbTextConfigTest_MultipleOfSameServer()
        {
            string[] fileContents = new string[] { "SERVER:default,target;default1,target1", "SERVER:default,target;default2,target2", "SERVER1:default,target;default3,target3" };
            MultiDbData actual;
            actual = MultiDbHelper.ImportMultiDbTextConfig(fileContents);
            Assert.AreEqual("SERVER", actual[0].ServerName); //Make sure the items with the same server only get on server entry.
            Assert.AreEqual("SERVER", actual[1].ServerName);
            Assert.AreEqual("SERVER1", actual[2].ServerName);
            Assert.AreEqual(2, actual[0].Overrides.Count);
            Assert.AreEqual(2, actual[1].Overrides.Count);
            Assert.AreEqual(2, actual[2].Overrides.Count);
        }
        /// <summary>
        ///A test for ImportMultiDbTextConfig
        ///</summary>
        [TestMethod()]
        public void ImportMultiDbTextConfigTest_GoodConfuguration_HandleEmptyLine()
        {
            string[] fileContents = new string[] { "SERVER:default,target;default2,target2", "", "SERVER2:default,target;default2,target2" };
            MultiDbData actual;
            actual = MultiDbHelper.ImportMultiDbTextConfig(fileContents);
            Assert.AreEqual("SERVER", actual[0].ServerName);
            Assert.AreEqual("SERVER2", actual[1].ServerName);

            DbOverrides seq = actual[0].Overrides;
            Assert.AreEqual("default", seq[0].DefaultDbTarget);
            Assert.AreEqual("target2", seq[1].OverrideDbTarget);
            seq = actual[1].Overrides;
            Assert.AreEqual("default2", seq[1].DefaultDbTarget);
            Assert.AreEqual("target", seq[0].OverrideDbTarget);
        }
        /// <summary>
        ///A test for ImportMultiDbTextConfig
        ///</summary>
        [TestMethod()]
        public void ImportMultiDbTextConfigTest_BadConfuguration_MissingColon()
        {
            string[] fileContents = new string[] { "SERVER:default,target;default2,target2", "SERVER2 default,target;default2,target2" };
            Assert.ThrowsExactly<MultiDbConfigurationException>(() => MultiDbHelper.ImportMultiDbTextConfig(fileContents));
        }

        #endregion

        #region .: ImportMultiDbTextConfig(string fileName) Tests :.
        /// <summary>
        ///A test for ImportMultiDbTextConfig
        ///</summary>
        [TestMethod()]
        public void ImportMultiDbTextConfigTest_EmptyFileName()
        {
            string fileName = string.Empty;
            MultiDbData expected = null!;
            MultiDbData actual;
            actual = MultiDbHelper.ImportMultiDbTextConfig(fileName);
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        ///A test for ImportMultiDbTextConfig
        ///</summary>
        [TestMethod()]
        public void ImportMultiDbTextConfigTest_ReadFile()
        {
            string fileName = Path.GetTempFileName();
            try
            {
                string[] fileContents = new string[] { "SERVER:default,target;default2,target2", "SERVER2:default,target;default2,target2" };
                File.WriteAllLines(fileName, fileContents);
                MultiDbData actual;
                actual = MultiDbHelper.ImportMultiDbTextConfig(fileName);

                Assert.AreEqual("SERVER", actual[0].ServerName);
                Assert.AreEqual("SERVER2", actual[1].ServerName);

                DbOverrides seq = actual[0].Overrides;
                Assert.AreEqual("default", seq[0].DefaultDbTarget);
                Assert.AreEqual("target2", seq[1].OverrideDbTarget);
                seq = actual[1].Overrides;
                Assert.AreEqual("default2", seq[1].DefaultDbTarget);
                Assert.AreEqual("target", seq[0].OverrideDbTarget);
            }
            finally
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
        }
        #endregion

        #region .: DeserializeMultiDbConfiguration :.
        /// <summary>
        ///A test for DeserializeMultiDbConfiguration
        ///</summary>
        [TestMethod()]
        public void DeserializeMultiDbConfigurationTest_EmptyFileName()
        {
            string fileName = string.Empty;
            MultiDbData expected = null!;
            MultiDbData actual;
            actual = MultiDbHelper.DeserializeMultiDbConfiguration(fileName);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for DeserializeMultiDbConfiguration
        ///</summary>
        [TestMethod()]
        public void DeserializeMultiDbConfigurationTest_WithQueryRowDataItems()
        {
            string fileName = Path.GetTempFileName();
            try
            {
                string fileContents = Properties.Resources.MultiDb_WithQueryRowData;
                File.WriteAllText(fileName, fileContents);
                MultiDbData actual;
                actual = MultiDbHelper.DeserializeMultiDbConfiguration(fileName);
                actual.IsTransactional = false;

                Assert.AreEqual(@"Server1\Instance_1", actual[0].ServerName);

                Assert.AreEqual(@"Server2\Instance_1", actual[1].ServerName);
                DbOverrides seq = actual[1].Overrides;
                Assert.AreEqual("Default", seq[0].DefaultDbTarget);
                Assert.AreEqual("Db_0002", seq[0].OverrideDbTarget);

                List<QueryRowItem> queryItems = seq[0].QueryRowData;
                Assert.AreEqual("MyCompany2", queryItems[0].Value);
                Assert.AreEqual("CompanyName", queryItems[0].ColumnName);
                Assert.AreEqual("CompanyID", queryItems[1].ColumnName);
                Assert.AreEqual("000002", queryItems[1].Value);

                Assert.IsFalse(actual.IsTransactional);

            }
            finally
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
        }


        /// <summary>
        ///A test for DeserializeMultiDbConfiguration
        ///</summary>
        [TestMethod()]
        public void DeserializeMultiDbConfigurationTest_ReadFile()
        {
            string fileName = Path.GetTempFileName();
            try
            {
                string fileContents = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
                fileContents += "<ArrayOfServerData xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">";
                fileContents += @"
  <ServerData>
    <ServerName>(local)</ServerName>
        <Overrides>
        <DatabaseOverride>
            <DefaultDbTarget>SqlBuildTest</DefaultDbTarget>
            <OverrideDbTarget>master</OverrideDbTarget>
        </DatabaseOverride>
        </Overrides>
  </ServerData>
</ArrayOfServerData>";
                File.WriteAllText(fileName, fileContents);
                MultiDbData actual;
                actual = MultiDbHelper.DeserializeMultiDbConfiguration(fileName);
                actual.IsTransactional = false;

                Assert.AreEqual("(local)", actual[0].ServerName);
                DbOverrides seq = actual[0].Overrides;
                Assert.AreEqual("SqlBuildTest", seq[0].DefaultDbTarget);
                Assert.AreEqual("master", seq[0].OverrideDbTarget);
                Assert.IsFalse(actual.IsTransactional);

            }
            finally
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
        }
        #endregion

        #region ValidateMultiDatabaseData
        /// <summary>
        ///A test for ValidateMultiDatabaseData
        ///</summary>
        [TestMethod()]
        public void ValidateMultiDatabaseData_GoodConfuguration()
        {
            string[] fileContents = new string[] { "SERVER:default,target;default2,target2", "SERVER2:default,target;default2,target2" };
            MultiDbData dbData = MultiDbHelper.ImportMultiDbTextConfig(fileContents);
            bool expected = true;
            bool actual = MultiDbHelper.ValidateMultiDatabaseData(dbData);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for ValidateMultiDatabaseData
        ///</summary>
        [TestMethod()]
        public void ValidateMultiDatabaseData_BadConfuguration()
        {
            string[] fileContents = new string[] { "SERVER:default,target;default2,target2", "SERVER2:,;default2,target2" };
            MultiDbData dbData = MultiDbHelper.ImportMultiDbTextConfig(fileContents);
            bool expected = false;
            bool actual = MultiDbHelper.ValidateMultiDatabaseData(dbData);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for ValidateMultiDatabaseData
        ///</summary>
        [TestMethod()]
        public void ValidateMultiDatabaseData_NullOverrideSequence()
        {
            MultiDbData dbData = new MultiDbData()
            {
                new ServerData(){ ServerName = "server1", Overrides = new DbOverrides(new DatabaseOverride("server1", "default", "target"),new DatabaseOverride("server1", "default2", "target2")) },
                new ServerData(){ ServerName = "server2", Overrides = null! }
            };


            bool expected = false;
            bool actual = MultiDbHelper.ValidateMultiDatabaseData(dbData);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for ValidateMultiDatabaseData
        ///</summary>
        [TestMethod()]
        public void ValidateMultiDatabaseData_EmptyOverrideSequence()
        {
            MultiDbData dbData = new MultiDbData();

            ServerData srv1 = new ServerData();
            srv1.ServerName = "server1";
            ServerData srv2 = new ServerData();
            srv2.ServerName = "server2";


            DbOverrides ovr = new DbOverrides();
            ovr.Add(new DatabaseOverride("server1", "default", "target"));
            ovr.Add(new DatabaseOverride("server1", "default2", "target2"));
            srv1.Overrides = ovr;
            dbData.Add(srv1);

            DbOverrides ovr2 = new DbOverrides();
            ovr.Add(new DatabaseOverride("server2", "default", "target"));
            ovr.Add(new DatabaseOverride("server2", "", ""));
            srv2.Overrides = ovr2;
            dbData.Add(srv2);

            bool expected = false;
            bool actual = MultiDbHelper.ValidateMultiDatabaseData(dbData);
            Assert.AreEqual(expected, actual);

        }
        #endregion

        /// <summary>
        ///A test for ConvertMultiDbDataToTextConfig
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void ConvertMultiDbDataToTextConfigTest()
        {
            MultiDbData cfg = new MultiDbData
            {
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("ServerA","default1", "override1")) },
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("ServerA","default2", "override2")) },
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("ServerA","default0", "override0")) },
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("ServerA","defaultX", "overrideX"), new DatabaseOverride("ServerA","defaultY", "overrideY")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides( new DatabaseOverride("ServerB","default6", "override6")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides( new DatabaseOverride("ServerB","default7", "override7")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides( new DatabaseOverride("ServerB","default5", "override5")) },
            };

            string expected =
@"ServerA:default1,override1
ServerA:default2,override2
ServerA:default0,override0
ServerA:defaultX,overrideX;defaultY,overrideY
ServerB:default6,override6
ServerB:default7,override7
ServerB:default5,override5
";
            string actual;
            actual = MultiDbHelper.ConvertMultiDbDataToTextConfig(cfg);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void SerializeMultiDbAsXMl_Test()
        {
            MultiDbData cfg = new MultiDbData
            {
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("ServerA","default1", "override1")) },
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("ServerA","default2", "override2")) },
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("ServerA","default0", "override0")) },
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("ServerA","defaultX", "overrideX"), new DatabaseOverride("ServerA","defaultY", "overrideY")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides( new DatabaseOverride("ServerB","default6", "override6")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides( new DatabaseOverride("ServerB","default7", "override7")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides( new DatabaseOverride("ServerB","default5", "override5")) },
            };


            string actual;
            actual = MultiDbHelper.SerializeMultiDbConfigurationToXml(cfg);

            var expected = Properties.Resources.serialized_multidb_xml;
            Assert.AreEqual(expected, actual);

        }

        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void SerializeMultiDbAsJson_Test()
        {
            MultiDbData cfg = new MultiDbData
            {
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("ServerA","default1", "override1")) },
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("ServerA","default2", "override2")) },
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("ServerA","default0", "override0")) },
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("ServerA","defaultX", "overrideX"), new DatabaseOverride("ServerA","defaultY", "overrideY")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides( new DatabaseOverride("ServerB","default6", "override6")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides( new DatabaseOverride("ServerB","default7", "override7")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides( new DatabaseOverride("ServerB","default5", "override5")) },
            };


            string actual;
            actual = MultiDbHelper.SerializeMultiDbConfigurationToJson(cfg);
            var expected = Properties.Resources.serialized_multidb_json;
            Assert.AreEqual(expected, actual);

        }

        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void SerializeMultiDbWithTagAsJson_Test()
        {
            MultiDbData cfg = new MultiDbData
            {
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("ServerA","default1", "override1", "TagA")) },
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("ServerA","default2", "override2", "TagA")) },
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("ServerA","default0", "override0", "TagA")) },
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("ServerA","defaultX", "overrideX", "TagB"), new DatabaseOverride("ServerA","defaultY", "overrideY", "TagB")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides( new DatabaseOverride("ServerB","default6", "override6", "TagB")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides( new DatabaseOverride("ServerB","default7", "override7", "TagB")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides( new DatabaseOverride("ServerB","default5", "override5", "TagB")) },
            };


            string actual;
            actual = MultiDbHelper.SerializeMultiDbConfigurationToJson(cfg);
            var expected = Properties.Resources.serialized_multidb_json_withtag_json;
            Assert.AreEqual(expected, actual);

        }

        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void SerializeAndDeserializeMultiDbAsJson_Test()
        {
            MultiDbData cfg = new MultiDbData
            {
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("ServerA","default1", "override1")) },
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("ServerA","default2", "override2")) },
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("ServerA","default0", "override0")) },
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("ServerA","defaultX", "overrideX"), new DatabaseOverride("ServerA","defaultY", "overrideY")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides( new DatabaseOverride("ServerB","default6", "override6")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides( new DatabaseOverride("ServerB","default7", "override7")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides( new DatabaseOverride("ServerB","default5", "override5")) },
            };


            string actual;
            actual = MultiDbHelper.SerializeMultiDbConfigurationToJson(cfg);
            var expected = Properties.Resources.serialized_multidb_json;
            Assert.AreEqual(expected, actual);

            var deserialized = MultiDbHelper.DeserializeMultiDbConfigurationString(actual);

            Assert.AreEqual(cfg.Count, deserialized.Count);

        }

        #region .: Additional Parsing Tests :.

        [TestMethod]
        public void ImportMultiDbTextConfig_WithConcurrencyTag_ParsesTagCorrectly()
        {
            // Arrange
            string[] fileContents = new string[] { "SERVER:default,target#TagA" };

            // Act
            MultiDbData actual = MultiDbHelper.ImportMultiDbTextConfig(fileContents);

            // Assert
            Assert.AreEqual("SERVER", actual[0].ServerName);
            Assert.AreEqual("TagA", actual[0].Overrides[0].ConcurrencyTag);
        }

        [TestMethod]
        public void ImportMultiDbTextConfig_WithOnlyOverrideDatabase_ParsesCorrectly()
        {
            // Arrange - no default, only override
            string[] fileContents = new string[] { "SERVER:targetDb" };

            // Act
            MultiDbData actual = MultiDbHelper.ImportMultiDbTextConfig(fileContents);

            // Assert
            Assert.AreEqual("SERVER", actual[0].ServerName);
            Assert.AreEqual("", actual[0].Overrides[0].DefaultDbTarget);
            Assert.AreEqual("targetDb", actual[0].Overrides[0].OverrideDbTarget);
        }

        [TestMethod]
        public void ImportMultiDbTextConfig_WithSingleQuotesInDefault_StripsQuotes()
        {
            // Arrange
            string[] fileContents = new string[] { "SERVER:'defaultDb',targetDb" };

            // Act
            MultiDbData actual = MultiDbHelper.ImportMultiDbTextConfig(fileContents);

            // Assert
            Assert.AreEqual("defaultDb", actual[0].Overrides[0].DefaultDbTarget);
            Assert.AreEqual("targetDb", actual[0].Overrides[0].OverrideDbTarget);
        }

        [TestMethod]
        public void ImportMultiDbTextConfig_WithSpacesAroundValues_TrimsCorrectly()
        {
            // Arrange
            string[] fileContents = new string[] { "  SERVER  :  default  ,  target  " };

            // Act
            MultiDbData actual = MultiDbHelper.ImportMultiDbTextConfig(fileContents);

            // Assert
            Assert.AreEqual("SERVER", actual[0].ServerName);
            Assert.AreEqual("default", actual[0].Overrides[0].DefaultDbTarget);
            Assert.AreEqual("target", actual[0].Overrides[0].OverrideDbTarget);
        }

        [TestMethod]
        public void ImportMultiDbTextConfig_WithMultipleOverridesPerLine_ParsesAll()
        {
            // Arrange
            string[] fileContents = new string[] { "SERVER:db1,target1;db2,target2;db3,target3" };

            // Act
            MultiDbData actual = MultiDbHelper.ImportMultiDbTextConfig(fileContents);

            // Assert
            Assert.AreEqual(3, actual[0].Overrides.Count);
            Assert.AreEqual("db1", actual[0].Overrides[0].DefaultDbTarget);
            Assert.AreEqual("target1", actual[0].Overrides[0].OverrideDbTarget);
            Assert.AreEqual("db2", actual[0].Overrides[1].DefaultDbTarget);
            Assert.AreEqual("target2", actual[0].Overrides[1].OverrideDbTarget);
            Assert.AreEqual("db3", actual[0].Overrides[2].DefaultDbTarget);
            Assert.AreEqual("target3", actual[0].Overrides[2].OverrideDbTarget);
        }

        [TestMethod]
        public void ImportMultiDbTextConfig_WithHashButNoTag_HandlesGracefully()
        {
            // Arrange - hash with no actual tag value
            string[] fileContents = new string[] { "SERVER:default,target#" };

            // Act
            MultiDbData actual = MultiDbHelper.ImportMultiDbTextConfig(fileContents);

            // Assert
            Assert.AreEqual("SERVER", actual[0].ServerName);
            Assert.AreEqual("default", actual[0].Overrides[0].DefaultDbTarget);
            Assert.AreEqual("target", actual[0].Overrides[0].OverrideDbTarget);
        }

        #endregion

        #region .: SaveMultiDbConfigToFile Tests :.

        [TestMethod]
        public void SaveMultiDbConfigToFile_AsJson_CreatesValidJsonFile()
        {
            // Arrange
            string fileName = Path.GetTempFileName();
            try
            {
                MultiDbData cfg = new MultiDbData
                {
                    new ServerData() { ServerName = "Server1", Overrides = new DbOverrides(new DatabaseOverride("Server1", "default", "target")) }
                };

                // Act
                bool result = MultiDbHelper.SaveMultiDbConfigToFile(fileName, cfg, asXml: false);

                // Assert
                Assert.IsTrue(result);
                Assert.IsTrue(File.Exists(fileName));
                string content = File.ReadAllText(fileName);
                Assert.IsTrue(content.Contains("\"ServerName\": \"Server1\"") || content.Contains("\"serverName\": \"Server1\""));
            }
            finally
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
        }

        [TestMethod]
        public void SaveMultiDbConfigToFile_AsXml_CreatesValidXmlFile()
        {
            // Arrange
            string fileName = Path.GetTempFileName();
            try
            {
                MultiDbData cfg = new MultiDbData
                {
                    new ServerData() { ServerName = "Server1", Overrides = new DbOverrides(new DatabaseOverride("Server1", "default", "target")) }
                };

                // Act
                bool result = MultiDbHelper.SaveMultiDbConfigToFile(fileName, cfg, asXml: true);

                // Assert
                Assert.IsTrue(result);
                Assert.IsTrue(File.Exists(fileName));
                string content = File.ReadAllText(fileName);
                Assert.IsTrue(content.Contains("<ServerName>Server1</ServerName>"));
            }
            finally
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
        }

        #endregion

        #region .: ConvertMultiDbDataToTextConfig Tests :.

        [TestMethod]
        public void ConvertMultiDbDataToTextConfig_WithConcurrencyTag_FormatsCorrectly()
        {
            // Arrange
            MultiDbData cfg = new MultiDbData
            {
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("ServerA", "default", "override", "TagA")) }
            };

            // Act
            string actual = MultiDbHelper.ConvertMultiDbDataToTextConfig(cfg);

            // Assert - the format includes the tag after the semicolon
            Assert.IsTrue(actual.Contains("default,override"));
            Assert.IsTrue(actual.StartsWith("ServerA:"));
        }

        [TestMethod]
        public void ConvertMultiDbDataToTextConfig_WithEmptyConfig_ReturnsEmptyString()
        {
            // Arrange
            MultiDbData cfg = new MultiDbData();

            // Act
            string actual = MultiDbHelper.ConvertMultiDbDataToTextConfig(cfg);

            // Assert
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        public void ConvertMultiDbDataToTextConfig_WithMultipleServers_FormatsCorrectly()
        {
            // Arrange
            MultiDbData cfg = new MultiDbData
            {
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("ServerA", "db1", "target1")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides(new DatabaseOverride("ServerB", "db2", "target2")) }
            };

            // Act
            string actual = MultiDbHelper.ConvertMultiDbDataToTextConfig(cfg);

            // Assert
            Assert.IsTrue(actual.Contains("ServerA:db1,target1"));
            Assert.IsTrue(actual.Contains("ServerB:db2,target2"));
        }

        #endregion

        #region .: DeserializeMultiDbConfigurationString Tests :.

        [TestMethod]
        public void DeserializeMultiDbConfigurationString_WithValidJson_DeserializesCorrectly()
        {
            // Arrange
            string json = @"[{""ServerName"":""TestServer"",""Overrides"":[{""DefaultDbTarget"":""default"",""OverrideDbTarget"":""target"",""ConcurrencyTag"":""""}]}]";

            // Act
            MultiDbData actual = MultiDbHelper.DeserializeMultiDbConfigurationString(json);

            // Assert
            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual("TestServer", actual[0].ServerName);
            Assert.AreEqual("default", actual[0].Overrides[0].DefaultDbTarget);
            Assert.AreEqual("target", actual[0].Overrides[0].OverrideDbTarget);
        }

        [TestMethod]
        public void DeserializeMultiDbConfigurationString_CaseInsensitive_ParsesLowercase()
        {
            // Arrange
            string json = @"[{""servername"":""TestServer"",""overrides"":[{""defaultdbtarget"":""default"",""overridedbtarget"":""target""}]}]";

            // Act
            MultiDbData actual = MultiDbHelper.DeserializeMultiDbConfigurationString(json);

            // Assert
            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual("TestServer", actual[0].ServerName);
        }

        #endregion

        #region .: ValidateMultiDatabaseData Additional Tests :.

        [TestMethod]
        public void ValidateMultiDatabaseData_WithEmptyConfiguration_ReturnsTrue()
        {
            // Arrange
            MultiDbData dbData = new MultiDbData();

            // Act
            bool actual = MultiDbHelper.ValidateMultiDatabaseData(dbData);

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void ValidateMultiDatabaseData_WithValidOverrides_ReturnsTrue()
        {
            // Arrange
            MultiDbData dbData = new MultiDbData
            {
                new ServerData()
                {
                    ServerName = "server1",
                    Overrides = new DbOverrides(new DatabaseOverride("server1", "default", "target"))
                }
            };

            // Act
            bool actual = MultiDbHelper.ValidateMultiDatabaseData(dbData);

            // Assert
            Assert.IsTrue(actual);
        }

        #endregion

        #region .: SaveMultiDbQueryConfiguration and LoadMultiDbQueryConfiguration Tests :.

        [TestMethod]
        public void SaveAndLoadMultiDbQueryConfiguration_RoundTrip_PreservesData()
        {
            // Arrange
            string fileName = Path.GetTempFileName();
            try
            {
                var cfg = new MultiDbQueryConfig
                {
                    SourceServer = "SourceServer",
                    Database = "SourceDb",
                    Query = "SELECT ServerName, DatabaseName FROM Servers"
                };

                // Act
                bool saveResult = MultiDbHelper.SaveMultiDbQueryConfiguration(fileName, cfg);
                var loaded = MultiDbHelper.LoadMultiDbQueryConfiguration(fileName);

                // Assert
                Assert.IsTrue(saveResult);
                Assert.IsNotNull(loaded);
                Assert.AreEqual(cfg.SourceServer, loaded.SourceServer);
                Assert.AreEqual(cfg.Database, loaded.Database);
                Assert.AreEqual(cfg.Query, loaded.Query);
            }
            finally
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
        }

        [TestMethod]
        public void LoadMultiDbQueryConfiguration_WithNonExistentFile_ReturnsNull()
        {
            // Arrange
            string fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".xml");

            // Act
            var result = MultiDbHelper.LoadMultiDbQueryConfiguration(fileName);

            // Assert
            Assert.IsNull(result);
        }

        #endregion


    }
}
