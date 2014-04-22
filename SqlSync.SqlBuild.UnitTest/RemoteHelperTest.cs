using SqlSync.SqlBuild.Remote;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using SqlSync.SqlBuild.MultiDb;
using System.Collections.Generic;

namespace SqlSync.SqlBuild.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for RemoteHelperTest and is intended
    ///to contain all RemoteHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class RemoteHelperTest
    {


        ///</summary>
        [TestMethod()]
        public void RemoteHelperConstructorTest()
        {
            RemoteHelper target = new RemoteHelper();
            Assert.IsNotNull(target);
        }

        #region BuildRemoteExecutionCommandlineTest
        /// <summary>
        ///A test for BuildRemoteExecutionCommandline
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void BuildRemoteExecutionCommandlineTest_EmptySbmFileName()
        {
            string sbmFileName = string.Empty; 
            string overrideSettingFile = @"C:\temp\test.cfg";
            string remoteExeServersFile = @"C:\temp\test.txt";
            string rootLoggingPath = @"C:\temp";
            string distributionType = "equal";
            bool isTrial = false; 
            bool isTransactional = false;
            string buildDescription = "Just testing";
            string expected = string.Empty; 
            string actual;
            actual = RemoteHelper.BuildRemoteExecutionCommandline(sbmFileName, overrideSettingFile, remoteExeServersFile, rootLoggingPath, distributionType, isTrial, isTransactional, buildDescription,0);
  
        }
        /// <summary>
        ///A test for BuildRemoteExecutionCommandline
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void BuildRemoteExecutionCommandlineTest_EmptyOverrideFileName()
        {
            string sbmFileName = @"C:\temp\test.sbm";
            string overrideSettingFile = string.Empty;
            string remoteExeServersFile = @"C:\temp\test.txt";
            string rootLoggingPath = @"C:\temp";
            string distributionType = "equal";
            bool isTrial = false;
            bool isTransactional = false;
            string buildDescription = "Just testing";
            string expected = string.Empty;
            string actual;
            actual = RemoteHelper.BuildRemoteExecutionCommandline(sbmFileName, overrideSettingFile, remoteExeServersFile, rootLoggingPath, distributionType, isTrial, isTransactional, buildDescription,0);

        }

        /// <summary>
        ///A test for BuildRemoteExecutionCommandline
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void BuildRemoteExecutionCommandlineTest_EmptyRemoteExeServersFileName()
        {
            string sbmFileName = @"C:\temp\test.sbm";
            string overrideSettingFile = @"C:\temp\test.cfg";
            string remoteExeServersFile = string.Empty;
            string rootLoggingPath = @"C:\temp";
            string distributionType = "equal";
            bool isTrial = false;
            bool isTransactional = false;
            string buildDescription = "Just testing";
            string expected = string.Empty;
            string actual;
            actual = RemoteHelper.BuildRemoteExecutionCommandline(sbmFileName, overrideSettingFile, remoteExeServersFile, rootLoggingPath, distributionType, isTrial, isTransactional, buildDescription,0);

        }
        /// <summary>
        ///A test for BuildRemoteExecutionCommandline
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void BuildRemoteExecutionCommandlineTest_MissingRootLoggingPath()
        {
            string sbmFileName = @"C:\temp\test.sbm";
            string overrideSettingFile = @"C:\temp\test.cfg";
            string remoteExeServersFile = @"C:\temp\test.txt";
            string rootLoggingPath = string.Empty;
            string distributionType = "equal";
            bool isTrial = false;
            bool isTransactional = false;
            string buildDescription = "Just testing";
            string expected = string.Empty;
            string actual;
            actual = RemoteHelper.BuildRemoteExecutionCommandline(sbmFileName, overrideSettingFile, remoteExeServersFile, rootLoggingPath, distributionType, isTrial, isTransactional, buildDescription,0);

        }

        /// <summary>
        ///A test for BuildRemoteExecutionCommandline
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void BuildRemoteExecutionCommandlineTest_MissingDistributionType()
        {
            string sbmFileName = @"C:\temp\test.sbm";
            string overrideSettingFile = @"C:\temp\test.cfg";
            string remoteExeServersFile = @"C:\temp\test.txt";
            string rootLoggingPath = @"C:\temp";
            string distributionType = string.Empty;
            bool isTrial = false;
            bool isTransactional = false;
            string buildDescription = "Just testing";
            string expected = string.Empty;
            string actual;
            actual = RemoteHelper.BuildRemoteExecutionCommandline(sbmFileName, overrideSettingFile, remoteExeServersFile, rootLoggingPath, distributionType, isTrial, isTransactional, buildDescription,0);

        }
        /// <summary>
        ///A test for BuildRemoteExecutionCommandline
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void BuildRemoteExecutionCommandlineTest_BadDistributionType()
        {
            string sbmFileName = @"C:\temp\test.sbm";
            string overrideSettingFile = @"C:\temp\test.cfg";
            string remoteExeServersFile = @"C:\temp\test.txt";
            string rootLoggingPath = @"C:\temp";
            string distributionType = "bad";
            bool isTrial = false;
            bool isTransactional = false;
            string buildDescription = "Just testing";
            string expected = string.Empty;
            string actual;
            actual = RemoteHelper.BuildRemoteExecutionCommandline(sbmFileName, overrideSettingFile, remoteExeServersFile, rootLoggingPath, distributionType, isTrial, isTransactional, buildDescription,0);

        }

        /// <summary>
        ///A test for BuildRemoteExecutionCommandline
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void BuildRemoteExecutionCommandlineTest_MissingBuildDescription()
        {
            string sbmFileName = @"C:\temp\test.sbm";
            string overrideSettingFile = @"C:\temp\test.cfg";
            string remoteExeServersFile = @"C:\temp\test.txt";
            string rootLoggingPath = @"C:\temp";
            string distributionType = "local";
            bool isTrial = false;
            bool isTransactional = false;
            string buildDescription = string.Empty;
            string expected = string.Empty;
            string actual;
            actual = RemoteHelper.BuildRemoteExecutionCommandline(sbmFileName, overrideSettingFile, remoteExeServersFile, rootLoggingPath, distributionType, isTrial, isTransactional, buildDescription,0);

        }

        /// <summary>
        ///A test for BuildRemoteExecutionCommandline
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void BuildRemoteExecutionCommandlineTest_BadTrialAndTransactionCombo()
        {
            string sbmFileName = @"C:\temp\test.sbm";
            string overrideSettingFile = @"C:\temp\test.cfg";
            string remoteExeServersFile = @"C:\temp\test.txt";
            string rootLoggingPath = @"C:\temp";
            string distributionType = "local";
            bool isTrial = true;
            bool isTransactional = false;
            string buildDescription = "Just testing";
            string expected = string.Empty;
            string actual;
            actual = RemoteHelper.BuildRemoteExecutionCommandline(sbmFileName, overrideSettingFile, remoteExeServersFile, rootLoggingPath, distributionType, isTrial, isTransactional, buildDescription,0);

        }

        /// <summary>
        ///A test for BuildRemoteExecutionCommandline
        ///</summary>
        [TestMethod()]
        public void BuildRemoteExecutionCommandlineTest_GoodSetup()
        {
            string sbmFileName = @"C:\temp\test.sbm";
            string overrideSettingFile = @"C:\temp\test.cfg";
            string remoteExeServersFile = @"C:\temp\test.txt";
            string rootLoggingPath = @"C:\temp";
            string distributionType = "local";
            bool isTrial = false;
            bool isTransactional = false;
            string buildDescription = "Just testing";
            string expected = "SqlBuildManager.Console.exe\" /remote=true /build=\"C:\\temp\\test.sbm\" /RemoteServers=\"C:\\temp\\test.txt\" /override=\"C:\\temp\\test.cfg\" /RootLoggingPath=\"C:\\temp\" /DistributionType=\"local\" /Description=\"Just testing\" /transactional=False /trial=False /TimeoutRetryCount=0";
            string actual;
            actual = RemoteHelper.BuildRemoteExecutionCommandline(sbmFileName, overrideSettingFile, remoteExeServersFile, rootLoggingPath, distributionType, isTrial, isTransactional, buildDescription,0);
            Assert.IsTrue(actual.IndexOf(expected) > -1);
        }
        #endregion

        #region GetMultiDbConfigLinesArrayTest
        /// <summary>
        ///A test for GetMultiDbConfigLinesArray
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(MultiDbConfigurationException))]
        public void GetMultiDbConfigLinesArrayTest_MissingFileException()
        {
            string fileName = "C:\\dont_exist_today";
            RemoteHelper.GetMultiDbConfigLinesArray(fileName);
        }
        /// <summary>
        ///A test for GetMultiDbConfigLinesArray
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(MultiDbConfigurationException))]
        public void GetMultiDbConfigLinesArrayTest_BadFileExtensionException()
        {
            string fileName = Path.GetTempFileName();
            RemoteHelper.GetMultiDbConfigLinesArray(fileName);
        }

        /// <summary>
        ///A test for GetMultiDbConfigLinesArray
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(MultiDbConfigurationException))]
        public void GetMultiDbConfigLinesArrayTest_MalformedMultiDbFileException()
        {
            string fileName = Path.GetTempFileName();
            File.Move(fileName, fileName + ".multiDb");
            fileName = fileName + ".multiDb";
            RemoteHelper.GetMultiDbConfigLinesArray(fileName);
        }

        /// <summary>
        ///A test for GetMultiDbConfigLinesArray
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(MultiDbConfigurationException))]
        public void GetMultiDbConfigLinesArrayTest_MalformedMultiDbQFileException()
        {
            string fileName = Path.GetTempFileName();
            File.Move(fileName, fileName + ".multiDbQ");
            fileName = fileName + ".multiDbQ";
            RemoteHelper.GetMultiDbConfigLinesArray(fileName);
        }

        /// <summary>
        ///A test for GetMultiDbConfigLinesArray
        ///</summary>
        [TestMethod()]
        public void GetMultiDbConfigLinesArrayTest_MalformedCfgFileException()
        {
            string fileName = Path.GetTempFileName();
            File.Move(fileName, fileName + ".cfg");
            fileName = fileName + ".cfg";
            string[] actual;
            actual = RemoteHelper.GetMultiDbConfigLinesArray(fileName);
            Assert.AreEqual(0, actual.Length);
        }

        /// <summary>
        ///A test for GetMultiDbConfigLinesArray
        ///</summary>
        [TestMethod()]
        public void GetMultiDbConfigLinesArrayTest_ValidMultiDbCfgFile()
        {
            string fileName = Path.GetTempFileName();
            File.Move(fileName, fileName + ".multiDb");
            fileName = fileName + ".multiDb";
            File.WriteAllBytes(fileName, Properties.Resources.NoTrans_MultiDb);
            string[] actual;
            actual = RemoteHelper.GetMultiDbConfigLinesArray(fileName);
            Assert.AreEqual(2, actual.Length);
            Assert.IsTrue(actual[0].StartsWith("(local)"));

            File.Delete(fileName);

        }

        /// <summary>
        ///A test for GetMultiDbConfigLinesArray
        ///</summary>
        [TestMethod()]
        public void GetMultiDbConfigLinesArrayTest_ValidCfgFile()
        {
            string fileName = Path.GetTempFileName();
            File.Move(fileName, fileName + ".cfg");
            fileName = fileName + ".cfg";
            File.WriteAllBytes(fileName, Properties.Resources.DBList);
            string[] actual;
            actual = RemoteHelper.GetMultiDbConfigLinesArray(fileName);
            Assert.AreEqual(9, actual.Length);
            Assert.IsTrue(actual[0].StartsWith("(local)"));
            Assert.IsTrue(actual[8].StartsWith("SERVER2"));
            File.Delete(fileName);

        }

        /// <summary>
        ///A test for GetMultiDbConfigLinesArray
        ///</summary>
        [TestMethod()]
        public void GetMultiDbConfigLinesArrayTest_ValidMultiDbQFile()
        {
            string fileName = Path.GetTempFileName();
            File.Move(fileName, fileName + ".multiDbQ");
            fileName = fileName + ".multiDbQ";
            File.WriteAllBytes(fileName, Properties.Resources.multi_query);
            string[] actual= new string[0];

            actual = RemoteHelper.GetMultiDbConfigLinesArray(fileName);

            Assert.AreEqual(1, actual.Length, "NOTE: Make sure you have a (local)\\SQLEXPRESS SQL Server running.");
            Assert.IsTrue(actual[0].StartsWith("(local)\\SQLEXPRESS",StringComparison.CurrentCultureIgnoreCase));
            File.Delete(fileName);

        }

        #endregion

        #region GetUniqueServerNamesFromMultiDbTes
        /// <summary>
        ///A test for GetUniqueServerNamesFromMultiDb
        ///</summary>
        [TestMethod()]
        public void GetUniqueServerNamesFromMultiDbTest_GoodConfig()
        {
            string fileName = Path.GetTempFileName();
            File.Move(fileName, fileName + ".cfg");
            fileName = fileName + ".cfg";
            File.WriteAllBytes(fileName, Properties.Resources.DBList);
            string[] configArray = RemoteHelper.GetMultiDbConfigLinesArray(fileName);

            string[] actual;
            actual = RemoteHelper.GetUniqueServerNamesFromMultiDb(configArray);
            Assert.AreEqual(3, actual.Length);
            Assert.AreEqual("SERVER2", actual[2]);

            File.Delete(fileName);
        }

        /// <summary>
        ///A test for GetUniqueServerNamesFromMultiDb
        ///</summary>
        [TestMethod()]
        public void GetUniqueServerNamesFromMultiDbTest_GoodConfigWithIPAddress()
        {
            string fileName = Path.GetTempFileName();
            File.Move(fileName, fileName + ".cfg");
            fileName = fileName + ".cfg";
            File.WriteAllBytes(fileName, Properties.Resources.DBList);
            string[] configArray = new string[] { "(local):default1,override1", "127.0.0.1:def,ovr", "127.0.0.1\\Inst_1:def,ovr", "10.10.4.3:def,ovr", "10.10.4.3:def,ovr" };

            string[] actual;
            actual = RemoteHelper.GetUniqueServerNamesFromMultiDb(configArray);
            Assert.AreEqual(3, actual.Length);
            Assert.AreEqual("10.10.4.3", actual[2]);

            File.Delete(fileName);
        }
        #endregion

        /// <summary>
        ///A test for GetRemoteExecutionServers
        ///</summary>
        [TestMethod()]
        public void GetRemoteExecutionServersTest_WellFormattedServerList()
        {

            string remoteServerValue = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".txt";
            File.WriteAllText(remoteServerValue, Properties.Resources.remote_server_list);

            string[] txtMultiCfg = new string[]{"Server1:default,override","Server2\\Instance:default,override","Server3\\Inst_X:default,override"};
            MultiDbData multiDb = MultiDbHelper.ImportMultiDbTextConfig(txtMultiCfg);


            List<string> actual;
            actual = RemoteHelper.GetRemoteExecutionServers(remoteServerValue, multiDb);
            File.Delete(remoteServerValue);

            Assert.IsTrue(actual.Count == 2, "There should be 2 exe servers pulled from the remote_server_list file");
            Assert.AreEqual("ExeServer1", actual[0]);
            Assert.AreEqual("ExeServer2", actual[1]);
        }

        /// <summary>
        ///A test for GetRemoteExecutionServers
        ///</summary>
        [TestMethod()]
        public void GetRemoteExecutionServersTest_ServerListWithTrailingSpaces()
        {

            string remoteServerValue = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".txt";
            File.WriteAllText(remoteServerValue, Properties.Resources.remote_server_list_with_spaces);

            string[] txtMultiCfg = new string[] { "Server1:default,override", "Server2\\Instance:default,override", "Server3\\Inst_X:default,override" };
            MultiDbData multiDb = MultiDbHelper.ImportMultiDbTextConfig(txtMultiCfg);


            List<string> actual;
            actual = RemoteHelper.GetRemoteExecutionServers(remoteServerValue, multiDb);
            File.Delete(remoteServerValue);

            Assert.IsTrue(actual.Count == 2, "There should be 2 exe servers pulled from the remote_server_list_with_spaces file");
            Assert.AreEqual("ExeServer3", actual[0]);
            Assert.AreEqual("ExeServer4", actual[1]);
        }

        /// <summary>
        ///A test for GetRemoteExecutionServers
        ///</summary>
        [TestMethod()]
        public void GetRemoteExecutionServersTest_ServerListWithTrailingSpacesAndBlankLines()
        {

            string remoteServerValue = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".txt";
            File.WriteAllText(remoteServerValue, Properties.Resources.remote_server_list_with_spaces_and_blank_lines);

            string[] txtMultiCfg = new string[] { "Server1:default,override", "Server2\\Instance:default,override", "Server3\\Inst_X:default,override" };
            MultiDbData multiDb = MultiDbHelper.ImportMultiDbTextConfig(txtMultiCfg);


            List<string> actual;
            actual = RemoteHelper.GetRemoteExecutionServers(remoteServerValue, multiDb);
            File.Delete(remoteServerValue);

            Assert.IsTrue(actual.Count == 2, "There should be 2 exe servers pulled from the remote_server_list_with_spaces_and_blank_lines file");
            Assert.AreEqual("ExeServer5", actual[0]);
            Assert.AreEqual("ExeServer6", actual[1]);


        }

        /// <summary>
        ///A test for GetRemoteExecutionServers
        ///</summary>
        [TestMethod()]
        public void GetRemoteExecutionServersTest_DeriveSetAndGoodConfig()
        {

            string remoteServerValue = "derive";

            string[] txtMultiCfg = new string[] { "Server1:default,override", "Server2\\Instance:default,override", "Server3\\Inst_X:default,override" };
            MultiDbData multiDb = MultiDbHelper.ImportMultiDbTextConfig(txtMultiCfg);


            List<string> actual;
            actual = RemoteHelper.GetRemoteExecutionServers(remoteServerValue, multiDb);
        
            Assert.IsTrue(actual.Count == 3, "There should be 3 exe servers pulled from the MultiDbData object");
            Assert.AreEqual("SERVER1", actual[0]);
            Assert.AreEqual("SERVER2", actual[1]);
            Assert.AreEqual("SERVER3", actual[2]);

        }

        /// <summary>
        ///A test for GetRemoteExecutionServers
        ///</summary>
        [TestMethod()]
        public void GetRemoteExecutionServersTest_DeriveSetAndExtraSpacesConfig()
        {

            string remoteServerValue = "derive";

            string[] txtMultiCfg = new string[] { "Server1  :default,override", "Server2  \\Instance:default,override", "Server3\\ Inst_X :default,override" };
            MultiDbData multiDb = MultiDbHelper.ImportMultiDbTextConfig(txtMultiCfg);


            List<string> actual;
            actual = RemoteHelper.GetRemoteExecutionServers(remoteServerValue, multiDb);

            Assert.IsTrue(actual.Count == 3, "There should be 3 exe servers pulled from the MultiDbData object");
            Assert.AreEqual("SERVER1", actual[0]);
            Assert.AreEqual("SERVER2", actual[1]);
            Assert.AreEqual("SERVER3", actual[2]);

        }

        /// <summary>
        ///A test for GetRemoteExecutionServers
        ///</summary>
        [TestMethod()]
        public void GetRemoteExecutionServersTest_DeriveWithMixedCase()
        {

            string remoteServerValue = "DeRivE";

            string[] txtMultiCfg = new string[] { "Server1:default,override", "Server2\\Instance:default,override", "Server3\\Inst_X:default,override" };
            MultiDbData multiDb = MultiDbHelper.ImportMultiDbTextConfig(txtMultiCfg);


            List<string> actual;
            actual = RemoteHelper.GetRemoteExecutionServers(remoteServerValue, multiDb);

            Assert.IsTrue(actual.Count == 3, "There should be 3 exe servers pulled from the MultiDbData object");
            Assert.AreEqual("SERVER1", actual[0]);
            Assert.AreEqual("SERVER2", actual[1]);
            Assert.AreEqual("SERVER3", actual[2]);

        }

        /// <summary>
        ///A test for GetRemoteExecutionServers
        ///</summary>
        [TestMethod()]
        public void GetRemoteExecutionServersTest_DeriveWithSpaces()
        {

            string remoteServerValue = " derive ";

            string[] txtMultiCfg = new string[] { "Server1:default,override", "Server2\\Instance:default,override", "Server3\\Inst_X:default,override" };
            MultiDbData multiDb = MultiDbHelper.ImportMultiDbTextConfig(txtMultiCfg);


            List<string> actual;
            actual = RemoteHelper.GetRemoteExecutionServers(remoteServerValue, multiDb);

            Assert.IsTrue(actual.Count == 3, "There should be 3 exe servers pulled from the MultiDbData object");
            Assert.AreEqual("SERVER1", actual[0]);
            Assert.AreEqual("SERVER2", actual[1]);
            Assert.AreEqual("SERVER3", actual[2]);

        }
    }
}
