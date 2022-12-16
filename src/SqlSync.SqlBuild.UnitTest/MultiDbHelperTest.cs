using SqlSync.SqlBuild.MultiDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using SqlSync.Connection;
using System.Collections.Generic;
using System;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Resources;

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
            Assert.AreEqual(actual[0].ServerName, "SERVER");
            Assert.AreEqual(actual[1].ServerName, "SERVER2");

            DbOverrides seq = actual[0].Overrides;
            Assert.AreEqual(seq[0].DefaultDbTarget, "default");
            Assert.AreEqual(seq[1].OverrideDbTarget, "target2");
            seq = actual[1].Overrides;
            Assert.AreEqual(seq[1].DefaultDbTarget, "default2");
            Assert.AreEqual(seq[0].OverrideDbTarget, "target");
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
            Assert.AreEqual("SERVER",actual[0].ServerName); //Make sure the items with the same server only get on server entry.
            Assert.AreEqual("SERVER",actual[1].ServerName);
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
            string[] fileContents = new string[] { "SERVER:default,target;default2,target2","","SERVER2:default,target;default2,target2" };
            MultiDbData actual;
            actual = MultiDbHelper.ImportMultiDbTextConfig(fileContents);
            Assert.AreEqual(actual[0].ServerName, "SERVER");
            Assert.AreEqual(actual[1].ServerName, "SERVER2");

            DbOverrides seq = actual[0].Overrides;
            Assert.AreEqual(seq[0].DefaultDbTarget, "default");
            Assert.AreEqual(seq[1].OverrideDbTarget, "target2");
            seq = actual[1].Overrides;
            Assert.AreEqual(seq[1].DefaultDbTarget, "default2");
            Assert.AreEqual(seq[0].OverrideDbTarget, "target");
        }
        /// <summary>
        ///A test for ImportMultiDbTextConfig
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(MultiDbConfigurationException),"Error in configuration file line #2. Missing \":\" separator. This is needed to separate server from database override values.")]
        public void ImportMultiDbTextConfigTest_BadConfuguration_MissingColon()
        {
            string[] fileContents = new string[] { "SERVER:default,target;default2,target2", "SERVER2 default,target;default2,target2" };
            MultiDbData actual;
            actual = MultiDbHelper.ImportMultiDbTextConfig(fileContents);
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
            MultiDbData expected = null; 
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

                Assert.AreEqual(actual[0].ServerName, "SERVER");
                Assert.AreEqual(actual[1].ServerName, "SERVER2");

                DbOverrides seq = actual[0].Overrides;
                Assert.AreEqual(seq[0].DefaultDbTarget, "default");
                Assert.AreEqual(seq[1].OverrideDbTarget, "target2");
                seq = actual[1].Overrides;
                Assert.AreEqual(seq[1].DefaultDbTarget, "default2");
                Assert.AreEqual(seq[0].OverrideDbTarget, "target");
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
            MultiDbData expected = null; 
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

                Assert.AreEqual(actual[0].ServerName, "(local)");
                DbOverrides seq = actual[0].Overrides;
                Assert.AreEqual(seq[0].DefaultDbTarget, "SqlBuildTest");
                Assert.AreEqual(seq[0].OverrideDbTarget, "master");
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
                new ServerData(){ ServerName = "server1", Overrides = new DbOverrides(new DatabaseOverride("default", "target"),new DatabaseOverride("default2", "target2")) },
                new ServerData(){ ServerName = "server2", Overrides = null }
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
            ovr.Add(new DatabaseOverride("default", "target"));
            ovr.Add(new DatabaseOverride("default2", "target2"));
            srv1.Overrides = ovr;
            dbData.Add(srv1);

            DbOverrides ovr2 = new DbOverrides();
            ovr.Add(new DatabaseOverride("default", "target"));
            ovr.Add(new DatabaseOverride("", ""));
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
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("default1", "override1")) },
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("default2", "override2")) },
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("default0", "override0")) },
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("defaultX", "overrideX"), new DatabaseOverride("defaultY", "overrideY")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides( new DatabaseOverride("default6", "override6")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides( new DatabaseOverride("default7", "override7")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides( new DatabaseOverride("default5", "override5")) },
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
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("default1", "override1")) },
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("default2", "override2")) },
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("default0", "override0")) },
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("defaultX", "overrideX"), new DatabaseOverride("defaultY", "overrideY")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides( new DatabaseOverride("default6", "override6")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides( new DatabaseOverride("default7", "override7")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides( new DatabaseOverride("default5", "override5")) },
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
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("default1", "override1")) },
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("default2", "override2")) },
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("default0", "override0")) },
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("defaultX", "overrideX"), new DatabaseOverride("defaultY", "overrideY")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides( new DatabaseOverride("default6", "override6")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides( new DatabaseOverride("default7", "override7")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides( new DatabaseOverride("default5", "override5")) },
            };


            string actual;
            actual = MultiDbHelper.SerializeMultiDbConfigurationToJson(cfg);
            var expected = Properties.Resources.serialized_multidb_json;
            Assert.AreEqual(expected, actual);

        }

        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void SerializeAndDeserializeMultiDbAsJson_Test()
        {
            MultiDbData cfg = new MultiDbData
            {
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("default1", "override1")) },
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("default2", "override2")) },
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("default0", "override0")) },
                new ServerData() { ServerName = "ServerA", Overrides = new DbOverrides(new DatabaseOverride("defaultX", "overrideX"), new DatabaseOverride("defaultY", "overrideY")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides( new DatabaseOverride("default6", "override6")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides( new DatabaseOverride("default7", "override7")) },
                new ServerData() { ServerName = "ServerB", Overrides = new DbOverrides( new DatabaseOverride("default5", "override5")) },
            };


            string actual;
            actual = MultiDbHelper.SerializeMultiDbConfigurationToJson(cfg);
            var expected = Properties.Resources.serialized_multidb_json;
            Assert.AreEqual(expected, actual);

            var deserialized = MultiDbHelper.DeserializeMultiDbConfigurationString(actual);

            Assert.AreEqual(cfg.Count, deserialized.Count);

        }


    }
}
