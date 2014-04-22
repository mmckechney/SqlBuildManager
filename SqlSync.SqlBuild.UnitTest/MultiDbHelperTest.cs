using SqlSync.SqlBuild.MultiDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using SqlSync.Connection;
using System.Collections.Generic;
using System;

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

            DbOverrideSequence seq = actual[0].OverrideSequence;
            Assert.AreEqual(seq["0"][0].DefaultDbTarget, "default");
            Assert.AreEqual(seq["0"][1].OverrideDbTarget, "target2");
            seq = actual[1].OverrideSequence;
            Assert.AreEqual(seq["1"][1].DefaultDbTarget, "default2");
            Assert.AreEqual(seq["1"][0].OverrideDbTarget, "target");
        }

        /// <summary>
        ///A test for ImportMultiDbTextConfig
        ///</summary>
        [TestMethod()]
        public void ImportMultiDbTextConfigTest_MultipleOfSameServer()
        {
            string[] fileContents = new string[] { "SERVER:default,target;default2,target2", "SERVER:default,target;default2,target2", "SERVER1:default,target;default3,target3" };
            MultiDbData actual;
            actual = MultiDbHelper.ImportMultiDbTextConfig(fileContents);
            Assert.AreEqual("SERVER",actual[0].ServerName); //Make sure the items with the same server only get on server entry.
            Assert.AreEqual("SERVER1",actual[1].ServerName);
            Assert.AreEqual(2, actual[0].OverrideSequence.Count);
            Assert.AreEqual(1, actual[1].OverrideSequence.Count);
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

            DbOverrideSequence seq = actual[0].OverrideSequence;
            Assert.AreEqual(seq["0"][0].DefaultDbTarget, "default");
            Assert.AreEqual(seq["0"][1].OverrideDbTarget, "target2");
            seq = actual[1].OverrideSequence;
            Assert.AreEqual(seq["1"][1].DefaultDbTarget, "default2");
            Assert.AreEqual(seq["1"][0].OverrideDbTarget, "target");
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
        /// <summary>
        ///A test for ImportMultiDbTextConfig
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(MultiDbConfigurationException), "Error in configuration file line #1. Missing \",\" separator. This is needed to separate default and override database targets.")]
        public void ImportMultiDbTextConfigTest_BadConfuguration_Comma()
        {
            string[] fileContents = new string[] { "SERVER:defaulttarget;default2,target2", "SERVER2:default,target;default2,target2" };
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

                DbOverrideSequence seq = actual[0].OverrideSequence;
                Assert.AreEqual(seq["0"][0].DefaultDbTarget, "default");
                Assert.AreEqual(seq["0"][1].OverrideDbTarget, "target2");
                seq = actual[1].OverrideSequence;
                Assert.AreEqual(seq["1"][1].DefaultDbTarget, "default2");
                Assert.AreEqual(seq["1"][0].OverrideDbTarget, "target");
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
                   DbOverrideSequence seq = actual[1].OverrideSequence;
                   Assert.AreEqual("Default", seq["1"][0].DefaultDbTarget);
                   Assert.AreEqual("Db_0002", seq["1"][0].OverrideDbTarget);

                   List<QueryRowItem> queryItems = seq["1"][0].QueryRowData;
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
    <OverrideSequence>
      <item>
        <key>
          <string>1</string>
        </key>
        <value>
          <ArrayOfDatabaseOverride>
            <DatabaseOverride>
              <DefaultDbTarget>SqlBuildTest</DefaultDbTarget>
              <OverrideDbTarget>master</OverrideDbTarget>
            </DatabaseOverride>
          </ArrayOfDatabaseOverride>
        </value>
      </item>
    </OverrideSequence>
  </ServerData>
</ArrayOfServerData>";
                File.WriteAllText(fileName, fileContents);
                MultiDbData actual;
                actual = MultiDbHelper.DeserializeMultiDbConfiguration(fileName);
                actual.IsTransactional = false;

                Assert.AreEqual(actual[0].ServerName, "(local)");
                DbOverrideSequence seq = actual[0].OverrideSequence;
                Assert.AreEqual(seq["1"][0].DefaultDbTarget, "SqlBuildTest");
                Assert.AreEqual(seq["1"][0].OverrideDbTarget, "master");
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
            MultiDbData dbData = new MultiDbData();

            ServerData srv1 = new ServerData();
            srv1.ServerName = "server1";
            dbData["server1"] = srv1;
            ServerData srv2 = new ServerData();
            srv2.ServerName = "server2";
            dbData["server2"] = srv2;

            DbOverrideSequence ovr = new DbOverrideSequence();
            ovr.Add("1",new DatabaseOverride("default","target"));
            ovr.Add("2",new DatabaseOverride("default2","target2"));
            dbData["server1"].OverrideSequence = ovr;
            dbData["server2"].OverrideSequence = null;

            
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
            dbData["server1"] = srv1;
            ServerData srv2 = new ServerData();
            srv2.ServerName = "server2";
            dbData["server2"] = srv2;

            DbOverrideSequence ovr = new DbOverrideSequence();
            ovr.Add("1", new DatabaseOverride("default", "target"));
            ovr.Add("2", new DatabaseOverride("default2", "target2"));
            dbData["server1"].OverrideSequence = ovr;

            DbOverrideSequence ovr2 = new DbOverrideSequence();
            ovr.Add("3", new DatabaseOverride("default", "target"));
            ovr.Add("4", new DatabaseOverride("", ""));
            dbData["server2"].OverrideSequence = ovr2;


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
            DbOverrideSequence sequenceA = new DbOverrideSequence();
            sequenceA.Add("1", new DatabaseOverride("default1", "override1"));
            sequenceA.Add("2", new DatabaseOverride("default2", "override2"));
            sequenceA.Add("0", new DatabaseOverride("default0", "override0"));

            DatabaseOverride ovrX = new DatabaseOverride("defaultX", "overrideX");
            DatabaseOverride ovrY = new DatabaseOverride("defaultY", "overrideY");
            List<DatabaseOverride> lstOvr = new List<DatabaseOverride>();
            lstOvr.Add(ovrX);
            lstOvr.Add(ovrY);
            sequenceA.Add("M", lstOvr);

            ServerData serverA = new ServerData();
            serverA.OverrideSequence = sequenceA;
            serverA.ServerName = "ServerA";

            DbOverrideSequence sequenceB = new DbOverrideSequence();
            sequenceB.Add("6", new DatabaseOverride("default6", "override6"));
            sequenceB.Add("7", new DatabaseOverride("default7", "override7"));
            sequenceB.Add("5", new DatabaseOverride("default5", "override5"));
           
            ServerData serverB = new ServerData();
            serverB.OverrideSequence = sequenceB;
            serverB.ServerName = "ServerB";

            MultiDbData cfg = new MultiDbData();
            cfg.Add(serverA);
            cfg.Add(serverB);

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
            actual = MultiDbHelper_Accessor.ConvertMultiDbDataToTextConfig(cfg);
            Assert.AreEqual(expected, actual);
        }


    }
}
