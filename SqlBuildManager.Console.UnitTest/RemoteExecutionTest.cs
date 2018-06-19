using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console;
using SqlBuildManager.ServiceClient;
using SqlBuildManager.ServiceClient.Sbm.BuildService;
namespace SqlBuildManager.Console.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for RemoteExecutionTest and is intended
    ///to contain all RemoteExecutionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class RemoteExecutionTest
    {
        
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            Initialization i =  new Initialization();
        }
        [ClassCleanup]
        public static void MyClassCleanup()
        {
            Initialization.CleanUp();
        }
        #region DeserializeBuildSettingsFileTest

        /// <summary>
        ///A test for DeserializeBuildSettingsFile
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Console.exe")]
        public void DeserializeBuildSettingsFileTest_GoodFile()
        {
            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName,SqlBuildManager.Console.UnitTest.Properties.Resources.remote_execution_file);
            BuildSettings actual;
            actual = RemoteExecution.DeserializeBuildSettingsFile(fileName);
            File.Delete(fileName);
            Assert.IsNotNull(actual);
        }

        /// <summary>
        ///A test for DeserializeBuildSettingsFile
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Console.exe")]
        public void DeserializeBuildSettingsFileTest_MissingFile()
        {
            string fileName = @"C:\I_dont_exist.txt";
            BuildSettings actual;
            actual = RemoteExecution.DeserializeBuildSettingsFile(fileName);
            Assert.IsNull(actual);
        }

        /// <summary>
        ///A test for DeserializeBuildSettingsFile
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Console.exe")]
        public void DeserializeBuildSettingsFileTest_BadFile()
        {
            string fileName = Path.GetTempFileName();
            BuildSettings actual;
            actual = RemoteExecution.DeserializeBuildSettingsFile(fileName);
            File.Delete(fileName);
            Assert.IsNull(actual);
        }
        #endregion

        #region DigestCommandLineArgumentsTest
        /// <summary>
        ///A test for DigestCommandLineArguments
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Console.exe")]
        public void DigestCommandLineArgumentsTest_FailBasicValidation()
        {
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.NoTrans_MultiDb_sbm);


            string[] args = new string[8];
            args[0] = "/Action=remote";
            args[1] = "/transactional=false";
            args[2] = "/trial=true";
            args[3] = "/override=\"C\\temp\\multicfg.cfg\"";
            args[4] = "/packagename=\"" + sbmFileName + "\"";
            args[5] = "/RemoteServers=\"C:\\temp\remote_servers.txt\"";
            args[6] = "/DistributionType=equal";
            args[7] = "/RootLoggingPath=\"C:\temp\"";

            BuildSettings setting = null;
            int expected = (int)ExecutionReturn.InvalidTransactionAndTrialCombo;
            int actual;
            actual = RemoteExecution.DigestAndValidateCommandLineArguments(args, out setting);
            File.Delete(sbmFileName);
            Assert.IsNull(setting);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for DigestCommandLineArguments
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Console.exe")]
        public void DigestCommandLineArgumentsTest_NoRemoteFileArgs()
        {
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.NoTrans_MultiDb_sbm);

            string[] args = new string[7];
            args[0] = "/Action=remote";
            args[1] = "/RootLoggingPath=\"C:\temp\"";
            args[2] = "/transactional=true";
            args[3] = "/trial=true";
            args[4] = "/override=\"" + Initialization.DbConfigFileName + "\"";
            args[5] = "/packagename=\"" + sbmFileName + "\"";
            //args[6] = "/RemoteServers=\"C:\\temp\remote_servers.txt\"";
            args[6] = "/DistributionType=equal";

            BuildSettings setting = null;
            int expected = -700;
            int actual;
            actual = RemoteExecution.DigestAndValidateCommandLineArguments(args, out setting);
            File.Delete(sbmFileName);
            Assert.IsNull(setting);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for DigestCommandLineArguments
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Console.exe")]
        public void DigestCommandLineArgumentsTest_MissingRemoteFileArgs()
        {
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.NoTrans_MultiDb_sbm);

            string[] args = new string[8];
            args[0] = "/Action=remote";
            args[1] = "/RootLoggingPath=\"C:\temp\"";
            args[2] = "/transactional=true";
            args[3] = "/trial=true";
            args[4] = "/override=\"" + Initialization.DbConfigFileName + "\"";
            args[5] = "/packagename=\"" + sbmFileName + "\"";
            args[6] = "/RemoteServers=\"C:\\temp\remote_servers.txt\"";
            args[7] = "/DistributionType=equal";

            BuildSettings setting = null; 
            int expected = -701; 
            int actual;
            actual = RemoteExecution.DigestAndValidateCommandLineArguments(args, out setting);
            File.Delete(sbmFileName);

            Assert.IsNull(setting);
            Assert.AreEqual(expected, actual);
        }

     

        /// <summary>
        ///A test for DigestCommandLineArguments
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Console.exe")]
        public void DigestCommandLineArgumentsTest_RemoteSetToDerive()
        {
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.NoTrans_MultiDb_sbm);

            string cfgContents = @"localhost\Instance_1:Default,MyDatabaseOverride
localhost:def,ovr";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string[] args = new string[8];
            args[0] = "/Action=remote";
            args[1] = "/RootLoggingPath=\"C:\temp\"";
            args[2] = "/transactional=true";
            args[3] = "/trial=true";
            args[4] = "/override=\""+ multiDbOverrideSettingFileName+ "\"";
            args[5] = "/packagename=\"" + sbmFileName + "\"";
            args[6] = "/RemoteServers=\"derive\"";
            args[7] = "/DistributionType=equal";

            BuildSettings setting = null;
            int expected = 0;
            int actual;
            actual = RemoteExecution.DigestAndValidateCommandLineArguments(args, out setting);
            File.Delete(sbmFileName);
            File.Delete(multiDbOverrideSettingFileName);
            if (actual <= -750 && actual >= -752)
            {
                Assert.Fail("Unable to validate command line agruments because no Remote Service process found on localhost. Please make sure you have a local service installed");
            }
            Assert.IsNotNull(setting, "Expected the setting variable not to be null. This should have been created in the DigestCommandLineArguments method");
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(1, setting.RemoteExecutionServers.Count);
            Assert.AreEqual("LOCALHOST", setting.RemoteExecutionServers[0].ServerName);
 
        }

        /// <summary>
        ///A test for DigestCommandLineArguments
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Console.exe")]
        public void DigestCommandLineArgumentsTest_MissingDistributionTypeArgs()
        {
            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, "localhost");

            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.NoTrans_MultiDb_sbm);

            string[] args = new string[7];
            args[0] = "/Action=remote";
            args[1] = "/RootLoggingPath=\"C:\temp\"";
            args[2] = "/transactional=true";
            args[3] = "/trial=true";
            args[4] = "/override=\"" + Initialization.DbConfigFileName + "\"";
            args[5] = "/packagename=\"" + sbmFileName + "\"";
            args[6] = "/RemoteServers=\"" + fileName + "\"";
            //args[7] = "/DistributionType=equal";

            BuildSettings setting = null;
            int expected = -702;
            int actual;
            actual = RemoteExecution.DigestAndValidateCommandLineArguments(args, out setting);
            File.Delete(fileName);
            File.Delete(sbmFileName);
            Assert.IsNull(setting);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for DigestCommandLineArguments
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Console.exe")]
        public void DigestCommandLineArgumentsTest_BadDistributionTypeArgs()
        {
            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, "localhost");

            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.NoTrans_MultiDb_sbm);


            string[] args = new string[8];
            args[0] = "/Action=remote";
            args[1] = "/RootLoggingPath=\"C:\temp\"";
            args[2] = "/transactional=true";
            args[3] = "/trial=true";
            args[4] = "/override=\"" + Initialization.DbConfigFileName + "\"";
            args[5] = "/packagename=\"" + sbmFileName + "\"";
            args[6] = "/RemoteServers=\"" + fileName + "\"";
            args[7] = "/DistributionType=BadValue";

            BuildSettings setting = null;
            int expected = -703;
            int actual;
            actual = RemoteExecution.DigestAndValidateCommandLineArguments(args, out setting);
            File.Delete(fileName);
            File.Delete(sbmFileName);

            Assert.IsNull(setting);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for DigestCommandLineArguments
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Console.exe")]
        public void DigestCommandLineArgumentsTest_BadMultiDbFile()
        {
            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, "localhost");

            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.NoTrans_MultiDb_sbm);


            string[] args = new string[8];
            args[0] = "/Action=remote";
            args[1] = "/RootLoggingPath=\"C:\temp\"";
            args[2] = "/transactional=true";
            args[3] = "/trial=true";
            args[4] = "/override=\"" + Initialization.DbConfigFileName + "\"";
            args[5] = "/packagename=\"" + sbmFileName + "\"";
            args[6] = "/RemoteServers=\"" + fileName + "\"";
            args[7] = "/DistributionType=equal";

            BuildSettings setting = null;
            int expected = (int)ExecutionReturn.NullMultiDbConfig;
            int actual;
            actual = RemoteExecution.DigestAndValidateCommandLineArguments(args, out setting);
            File.Delete(fileName);
            File.Delete(sbmFileName);
            Assert.IsNull(setting);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for DigestCommandLineArguments
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Console.exe")]
        public void DigestCommandLineArgumentsTest_BadHostName()
        {
            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, "badHost");

            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.NoTrans_MultiDb_sbm);


            string cfgContents = @"Server1\Instance_1:Default,MyDatabaseOverride";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string[] args = new string[8];
            args[0] = "/Action=remote";
            args[1] = "/RootLoggingPath=\"C:\temp\"";
            args[2] = "/transactional=true";
            args[3] = "/trial=true";
            args[4] = "/override=\"" + multiDbOverrideSettingFileName + "\"";
            args[5] = "/packagename=\"" + sbmFileName + "\"";
            args[6] = "/RemoteServers=\"" + fileName + "\"";
            args[7] = "/DistributionType=equal";

            BuildSettings setting = null;
            int expected = -750;
            int actual;
            actual = RemoteExecution.DigestAndValidateCommandLineArguments(args, out setting);
            File.Delete(fileName);
            File.Delete(sbmFileName);

            File.Delete(multiDbOverrideSettingFileName);
            Assert.IsNull(setting);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for DigestCommandLineArguments
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Console.exe")]
        public void DigestCommandLineArgumentsTest_GoodsHostNamePathEqualSplit()
        {
            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, "localhost");

            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.NoTrans_MultiDb_sbm);


            string cfgContents = @"Server1\Instance_1:Default,MyDatabaseOverride";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string[] args = new string[8];
            args[0] = "/Action=remote";
            args[1] = "/RootLoggingPath=\"C:\temp\"";
            args[2] = "/transactional=true";
            args[3] = "/trial=true";
            args[4] = "/override=\"" + multiDbOverrideSettingFileName + "\"";
            args[5] = "/packagename=\"" + sbmFileName + "\"";
            args[6] = "/RemoteServers=\"" + fileName + "\"";
            args[7] = "/DistributionType=equal";

            BuildSettings setting = null;
            int expected = 0;
            int actual;
            actual = RemoteExecution.DigestAndValidateCommandLineArguments(args, out setting);
            File.Delete(fileName);
            File.Delete(multiDbOverrideSettingFileName);
            File.Delete(sbmFileName);

            if (actual <= -750 && actual >= -752)
            {
                Assert.Fail("Unable to validate command line agruments because no Remote Service process found on localhost. Please make sure you have a local service installed");
            }

            Assert.IsNotNull(setting, "Expected the setting variable not to be null. This should have been created in the DigestCommandLineArguments method");
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for DigestCommandLineArguments
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Console.exe")]
        public void DigestCommandLineArgumentsTest_GoodsHostNamePathLocalSplit()
        {
            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, "localhost");

            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.NoTrans_MultiDb_sbm);


            string cfgContents = @"Server1\Instance_1:Default,MyDatabaseOverride";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string[] args = new string[8];
            args[0] = "/Action=remote";
            args[1] = "/RootLoggingPath=\"C:\temp\"";
            args[2] = "/transactional=true";
            args[3] = "/trial=true";
            args[4] = "/override=\"" + multiDbOverrideSettingFileName + "\"";
            args[5] = "/packagename=\"" + sbmFileName + "\"";
            args[6] = "/RemoteServers=\"" + fileName + "\"";
            args[7] = "/DistributionType=local";

            BuildSettings setting = null;
            int expected = 0;
            int actual;
            actual = RemoteExecution.DigestAndValidateCommandLineArguments(args, out setting);

            if(File.Exists(fileName))
                File.Delete(fileName);
            if(File.Exists(multiDbOverrideSettingFileName))
                File.Delete(multiDbOverrideSettingFileName);
            if(File.Exists(sbmFileName))
                File.Delete(sbmFileName);

            if (actual <= -750 && actual >= -752)
            {
                Assert.Fail("Unable to validate command line agruments because no Remote Service process found on localhost. Please make sure you have a local service installed");
            }
            Assert.IsNotNull(setting, "Expected the setting variable not to be null. This should have been created in the DigestCommandLineArguments method");
            Assert.AreEqual(expected, actual);
        }
 
        #endregion

        #region ValidateRemoteServerAvailabilityTest
        /// <summary>
        ///A test for ValidateRemoteServerAvailability
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Console.exe")]
        public void ValidateRemoteServerAvailabilityTest_LocalhostConnected()
        {
            List<string> remoteServers = new List<string>();
            remoteServers.Add("localhost");
            List<ServerConfigData> remoteServerData = null; 

            string[] errorMessages = null; 
            int expected = 0; 
            int actual;
            actual = RemoteExecution.ValidateRemoteServerAvailability(remoteServers, Protocol.Tcp, out remoteServerData, out errorMessages);
            Assert.AreEqual(0, errorMessages.Length, "NOTE: If test is failing, make sure you have a remote execution service running on this machine");
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ValidateRemoteServerAvailability
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Console.exe")]
        public void ValidateRemoteServerAvailabilityTest_BadHostName()
        {
            List<string> remoteServers = new List<string>();
            remoteServers.Add("badhost");
            List<ServerConfigData> remoteServerData = null;

            string[] errorMessages = null;
            int expected = -750;
            int actual;
            actual = RemoteExecution.ValidateRemoteServerAvailability(remoteServers, Protocol.Tcp, out remoteServerData, out errorMessages);
            Assert.AreEqual(1, errorMessages.Length);
            Assert.IsTrue(errorMessages[0].IndexOf("Remote service status for") > -1);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ValidateRemoteServerAvailability
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Console.exe")]
        public void ValidateRemoteServerAvailabilityTest_NullRemoteServers()
        {
            List<string> remoteServers = null;
            List<ServerConfigData> remoteServerData = null;
            string[] errorMessages = null;
            int expected = -752;
            int actual;
            actual = RemoteExecution.ValidateRemoteServerAvailability(remoteServers, Protocol.Tcp, out remoteServerData, out errorMessages);
            Assert.AreEqual(1, errorMessages.Length);
            Assert.IsTrue(errorMessages[0].IndexOf("No remote execution servers were specified") > -1);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ValidateRemoteServerAvailability
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Console.exe")]
        public void ValidateRemoteServerAvailabilityTest_EmptyRemoteServersList()
        {
            List<string> remoteServers = new List<string>();
            List<ServerConfigData> remoteServerData = null;
            string[] errorMessages = null;
            int expected = -752;
            int actual;
            actual = RemoteExecution.ValidateRemoteServerAvailability(remoteServers, Protocol.Tcp, out remoteServerData, out errorMessages);
            Assert.AreEqual(1, errorMessages.Length);
            Assert.IsTrue(errorMessages[0].IndexOf("No remote execution servers were specified") > -1);
            Assert.AreEqual(expected, actual);
        }

        
        #endregion

        #region ConstructorTest
        /// <summary>
        ///A test for RemoteExecution Constructor
        ///</summary>
        [TestMethod()]
        public void RemoteExecutionConstructorTest_WithArgs()
        {
            string[] arguments = new string[] { "test", "test2" };
            RemoteExecution target = new RemoteExecution(arguments);
            Assert.IsNotNull(target.args);
            Assert.AreEqual(2, target.args.Length);
        }

        /// <summary>
        ///A test for RemoteExecution Constructor
        ///</summary>
        [TestMethod()]
        public void RemoteExecutionConstructorTest_WithSettingsFile()
        {
            string settingsFile = @"C:\myfile.txt";
            RemoteExecution target = new RemoteExecution(settingsFile);
            Assert.IsNotNull(target.settingsFile);
            Assert.AreEqual(settingsFile, target.settingsFile);
        }
        #endregion

        #region ExecuteTest
        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public void ExecuteTest_SuccessTrial()
        {
            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, "localhost");

            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);


            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = new string[8];
            args[0] = "/Action=remote";
            args[1] = "/RootLoggingPath=\""+ loggingPath + "\"";
            args[2] = "/transactional=true";
            args[3] = "/trial=true";
            args[4] = "/override=\"" + multiDbOverrideSettingFileName + "\"";
            args[5] = "/packagename=\"" + sbmFileName + "\"";
            args[6] = "/RemoteServers=\"" + fileName + "\"";
            args[7] = "/DistributionType=equal";
            RemoteExecution target = new RemoteExecution(args); 
            int expected = 0; 
            int actual;
            actual = target.Execute();

            if(Directory.Exists(loggingPath))
                Directory.Delete(loggingPath, true);
            if(File.Exists(fileName))
                File.Delete(fileName);
            if(File.Exists(sbmFileName))
                File.Delete(sbmFileName);
            if(File.Exists(multiDbOverrideSettingFileName))
                File.Delete(multiDbOverrideSettingFileName);

            if (actual == -600)
                Assert.Fail("Unable to complete test. Please make sure you have a Remote Execution Service running on localhost");

            Assert.AreEqual(expected, actual,"Ensure that you have your service running on \"localhost\\SQLEXPRESS\"");
        }

        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public void ExecuteTest_SuccessTrialWithCodeReviewContents()
        {
            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, "localhost");

            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect_WithCodeReview);


            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = new string[8];
            args[0] = "/Action=remote";
            args[1] = "/RootLoggingPath=\"" + loggingPath + "\"";
            args[2] = "/transactional=true";
            args[3] = "/trial=true";
            args[4] = "/override=\"" + multiDbOverrideSettingFileName + "\"";
            args[5] = "/packagename=\"" + sbmFileName + "\"";
            args[6] = "/RemoteServers=\"" + fileName + "\"";
            args[7] = "/DistributionType=equal";
            RemoteExecution target = new RemoteExecution(args);
            int expected = 0;
            int actual;
            actual = target.Execute();

            if (Directory.Exists(loggingPath))
                Directory.Delete(loggingPath, true);
            if (File.Exists(fileName))
                File.Delete(fileName);
            if (File.Exists(sbmFileName))
                File.Delete(sbmFileName);
            if (File.Exists(multiDbOverrideSettingFileName))
                File.Delete(multiDbOverrideSettingFileName);

            if (actual == -600)
                Assert.Fail("Unable to complete test. Please make sure you have a Remote Execution Service running on localhost");

            Assert.AreEqual(expected, actual, "Ensure that you have your service running on \"localhost\\SQLEXPRESS\"");
        }

        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public void ExecuteTest_SuccessCommit()
        {
            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, "localhost");

            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);


            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = new string[7];
            args[0] = "/Action=remote";
            args[1] = "/RootLoggingPath=\"" + loggingPath + "\"";
            args[2] = "/transactional=true";
            args[3] = "/override=\"" + multiDbOverrideSettingFileName + "\"";
            args[4] = "/packagename=\"" + sbmFileName + "\"";
            args[5] = "/RemoteServers=\"" + fileName + "\"";
            args[6] = "/DistributionType=equal";
            RemoteExecution target = new RemoteExecution(args);
            int expected = 0;
            int actual;
            actual = target.Execute();

            if (Directory.Exists(loggingPath))
                Directory.Delete(loggingPath, true);
            if (File.Exists(fileName))
                File.Delete(fileName);
            if (File.Exists(sbmFileName))
                File.Delete(sbmFileName);
            if (File.Exists(multiDbOverrideSettingFileName))
                File.Delete(multiDbOverrideSettingFileName);

            if (actual == -600)
                Assert.Fail("Unable to complete test. Please make sure you have a Remote Execution Service running on localhost");

            Assert.AreEqual(expected, actual, "Ensure that you have your service running on \"localhost\\SQLEXPRESS\"");
        }

        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public void ExecuteTest_SuccessCommitWithLocalDist()
        {
            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, "localhost");

            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);


            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = new string[7];
            args[0] = "/Action=remote";
            args[1] = "/RootLoggingPath=\"" + loggingPath + "\"";
            args[2] = "/transactional=true";
            args[3] = "/override=\"" + multiDbOverrideSettingFileName + "\"";
            args[4] = "/packagename=\"" + sbmFileName + "\"";
            args[5] = "/RemoteServers=\"" + fileName + "\"";
            args[6] = "/DistributionType=local";
            RemoteExecution target = new RemoteExecution(args);
            int expected = 0;
            int actual;
            actual = target.Execute();

            if (Directory.Exists(loggingPath))
                Directory.Delete(loggingPath, true);
            if (File.Exists(fileName))
                File.Delete(fileName);
            if (File.Exists(sbmFileName))
                File.Delete(sbmFileName);
            if (File.Exists(multiDbOverrideSettingFileName))
                File.Delete(multiDbOverrideSettingFileName);

            if (actual == -600)
                Assert.Fail("Unable to complete test. Please make sure you have a Remote Execution Service running on localhost");

            Assert.AreEqual(expected, actual, "Ensure that you have your service running on \"localhost\\SQLEXPRESS\"");
        }

        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public void ExecuteTest_FailureWithLocalDist_UntaskedRemoteExeAndUnallocatedDbServer()
        {
            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, "localhost");

            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);


            string cfgContents = @"someotherhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = new string[7];
            args[0] = "/Action=remote";
            args[1] = "/RootLoggingPath=\"" + loggingPath + "\"";
            args[2] = "/transactional=true";
            args[3] = "/override=\"" + multiDbOverrideSettingFileName + "\"";
            args[4] = "/packagename=\"" + sbmFileName + "\"";
            args[5] = "/RemoteServers=\"" + fileName + "\"";
            args[6] = "/DistributionType=local";
            RemoteExecution target = new RemoteExecution(args);
            int expected = -698;
            int actual;
            actual = target.Execute();

            if (Directory.Exists(loggingPath))
                Directory.Delete(loggingPath, true);
            if (File.Exists(fileName))
                File.Delete(fileName);
            if (File.Exists(sbmFileName))
                File.Delete(sbmFileName);
            if (File.Exists(multiDbOverrideSettingFileName))
                File.Delete(multiDbOverrideSettingFileName);

            if (actual == -600)
                Assert.Fail("Unable to complete test. Please make sure you have a Remote Execution Service running on localhost");

            Assert.AreEqual(expected, actual, "Ensure that you have your service running on \"localhost\\SQLEXPRESS\"");
        }

        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public void ExecuteTest_FailureWithLocalDist_UntaskedRemoteExeServer()
        {
            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, "localhost\r\n127.0.0.1");

            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);


            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = new string[7];
            args[0] = "/Action=remote";
            args[1] = "/RootLoggingPath=\"" + loggingPath + "\"";
            args[2] = "/transactional=true";
            args[3] = "/override=\"" + multiDbOverrideSettingFileName + "\"";
            args[4] = "/packagename=\"" + sbmFileName + "\"";
            args[5] = "/RemoteServers=\"" + fileName + "\"";
            args[6] = "/DistributionType=local";
            RemoteExecution target = new RemoteExecution(args);
            int expected = 0;
            int actual;
            actual = target.Execute();

            if (Directory.Exists(loggingPath))
                Directory.Delete(loggingPath, true);
            if (File.Exists(fileName))
                File.Delete(fileName);
            if (File.Exists(sbmFileName))
                File.Delete(sbmFileName);
            if (File.Exists(multiDbOverrideSettingFileName))
                File.Delete(multiDbOverrideSettingFileName);

            if (actual == -600)
                Assert.Fail("Unable to complete test. Please make sure you have a Remote Execution Service running on localhost");

            Assert.AreEqual(expected, actual, "Ensure that you have your service running on \"localhost\\SQLEXPRESS\"");
        }

        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public void ExecuteTest_FailureWithLocalDist_UnassignedDBServer()
        {
            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, "localhost");

            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);


            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest
anotherhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = new string[7];
            args[0] = "/Action=remote";
            args[1] = "/RootLoggingPath=\"" + loggingPath + "\"";
            args[2] = "/transactional=true";
            args[3] = "/override=\"" + multiDbOverrideSettingFileName + "\"";
            args[4] = "/packagename=\"" + sbmFileName + "\"";
            args[5] = "/RemoteServers=\"" + fileName + "\"";
            args[6] = "/DistributionType=local";
            RemoteExecution target = new RemoteExecution(args);
            int expected = -698;
            int actual;
            actual = target.Execute();

            if (Directory.Exists(loggingPath))
                Directory.Delete(loggingPath, true);
            if (File.Exists(fileName))
                File.Delete(fileName);
            if (File.Exists(sbmFileName))
                File.Delete(sbmFileName);
            if (File.Exists(multiDbOverrideSettingFileName))
                File.Delete(multiDbOverrideSettingFileName);

            if (actual == -600)
                Assert.Fail("Unable to complete test. Please make sure you have a Remote Execution Service running on localhost");

            Assert.AreEqual(expected, actual, "Ensure that you have your service running on \"localhost\\SQLEXPRESS\"");
        }


        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public void ExecuteTest_ErrorInExecution()
        {
            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, "localhost");

            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.SyntaxError);


            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = new string[8];
            args[0] = "/Action=remote";
            args[1] = "/RootLoggingPath=\"" + loggingPath + "\"";
            args[2] = "/transactional=true";
            args[3] = "/trial=true";
            args[4] = "/override=\"" + multiDbOverrideSettingFileName + "\"";
            args[5] = "/packagename=\"" + sbmFileName + "\"";
            args[6] = "/RemoteServers=\"" + fileName + "\"";
            args[7] = "/DistributionType=equal";
            RemoteExecution target = new RemoteExecution(args);
            int expected = -601;
            int actual;
            actual = target.Execute();
            if(Directory.Exists(loggingPath))
                Directory.Delete(loggingPath, true);

            if(File.Exists(fileName))
                File.Delete(fileName);
            if(File.Exists(sbmFileName))
                File.Delete(sbmFileName);
            if(File.Exists(multiDbOverrideSettingFileName))
                File.Delete(multiDbOverrideSettingFileName);

            if (actual == -600)
                Assert.Fail("Unable to complete test. Please make sure you have a Remote Execution Service running on localhost");

            Assert.AreEqual(expected, actual, "Ensure that you have your service running on \"localhost\\SQLEXPRESS\"");
            Assert.IsTrue(Directory.GetFiles(SqlBuildManager.Logging.Configure.AppDataPath, "*.cfg").Length > 0); 
        }

        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public void ExecuteTest_UnableToCreateSettings()
        {
            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, "localhost");

            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.SyntaxError);


            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = new string[6];
            args[0] = "/Action=remote";
            args[1] = "/RootLoggingPath=\"" + loggingPath + "\"";
            args[2] = "/transactional=true";
            args[3] = "/trial=true";
            args[4] = "/override=\"" + multiDbOverrideSettingFileName + "\"";
            args[5] = "/PackageName=\"" + sbmFileName + "\"";
            //args[6] = "/RemoteServers=\"" + fileName + "\"";
           // args[7] = "/DistributionType=equal";
            RemoteExecution target = new RemoteExecution(args);
            int expected = -600;
            int actual;
            actual = target.Execute();
            File.Delete(fileName);
            File.Delete(sbmFileName);
            File.Delete(multiDbOverrideSettingFileName);
            Assert.AreEqual(expected, actual, "Ensure that you have your service running on \"localhost\\SQLEXPRESS\"");
        }

        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public void ExecuteTest_UsingRespFormattedSettingsFile()
        {

            string respFile = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".resp";
            File.WriteAllText(respFile, Properties.Resources.remote_execution_file);

            RemoteExecution target = new RemoteExecution(respFile);
            int expected = -601;
            int actual;
            actual = target.Execute();
            File.Delete(respFile);

            if (actual <= -750 && actual >= -752)
            {
                Assert.Fail("Unable to validate settings file because no Remote Service process found on localhost. Please make sure you have a local service installed");
            }
            Assert.AreEqual(expected, actual, "NOTE: If this test failed: Ensure that you have your service running on \"localhost\\SQLEXPRESS\"");
        }

        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public void ExecuteTest_UsingRespFormattedSettingsFileAndBadHost()
        {

            string respFile = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".resp";
            File.WriteAllText(respFile, Properties.Resources.remote_execution_file_bad_exeserver);

            RemoteExecution target = new RemoteExecution(respFile);
            int expected = -750;
            int actual;
            actual = target.Execute();
            File.Delete(respFile);
            Assert.AreEqual(expected, actual);
        }
        #endregion

        #region ExecuteTest with Retries
        [TestMethod()]
        public void ExecuteTest_CommitWithRetries()
        {
            int retryCount = 20;

            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, "localhost");

            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.InsertForThreadedTest);

            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest
localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest1";

            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = new string[9];
            args[0] = "/Action=remote";
            args[1] = "/RootLoggingPath=\"" + loggingPath + "\"";
            args[2] = "/transactional=true";
            args[3] = "/trial=false";
            args[4] = "/override=\"" + multiDbOverrideSettingFileName + "\"";
            args[5] = "/packagename=\"" + sbmFileName + "\"";
            args[6] = "/DistributionType=equal";
            args[7] = "/TimeoutRetryCount=" + retryCount.ToString();
            args[8] = "/RemoteServers=\"" + fileName + "\"";
            RemoteExecution target = new RemoteExecution(args);
            int expected = (int)ExecutionReturn.Successful;
            int actual;

            Thread THRInfinite = null;
            try
            {
                THRInfinite = new Thread(StartInfiniteLockingThread);
                THRInfinite.Start(600000);

                actual = target.Execute();

                if (actual == -600)
                    Assert.Fail("Unable to complete test. Please make sure you have a Remote Execution Service running on localhost");

                Assert.AreEqual(expected, actual, "Ensure that you have your service running on \"localhost\\SQLEXPRESS\"");

                //SqlBuildTest should still committed with at least one timeout message
                string[] logFiles = Directory.GetFiles(loggingPath + @"\localhost\SQLEXPRESS\SqlBuildTest\", "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest log file necessary to complete test");
                string logContents = File.ReadAllText(logFiles[0]);
                Regex regFindTimeout = new Regex("Error Message: Timeout expired");

                MatchCollection matches = regFindTimeout.Matches(logContents);
                Assert.IsTrue(matches.Count > 0, "No Timeout messages were encountered");
                Assert.IsTrue(matches.Count < retryCount + 1, String.Format("There were more Timeout retries than configured for! Allocated {0}; Found {1}", retryCount + 1, matches.Count));
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") > -1, "SqlBuildTest Log does not contain a 'COMMIT' message");
                Assert.IsTrue(logContents.IndexOf("ROLLBACK TRANSACTION") > -1, "SqlBuildTest does not contain a 'ROLLBACK' message. It should for the retries.");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest") > -1, "SqlBuildTest Log does not contain the proper database name");

                //SqlBuildTest1 should still committed with no timeout messages
                logFiles = Directory.GetFiles(loggingPath + @"\localhost\SQLEXPRESS\SqlBuildTest1\", "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest1 log file necessary to complete test");
                logContents = File.ReadAllText(logFiles[0]);
                Assert.IsTrue(logContents.IndexOf("Error Message: Timeout expired.") == -1, "SqlBuildTest1 Log contains a 'Timeout expired message'");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") > -1, "SqlBuildTest1 Log does not contain a 'COMMIT' message");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest1") > -1, "SqlBuildTest1 Log does not contain the proper database name");



            }
            finally
            {
                if (THRInfinite != null)
                    THRInfinite.Abort();

                if (Directory.Exists(loggingPath))
                    Directory.Delete(loggingPath, true);
                if (File.Exists(sbmFileName))
                    File.Delete(sbmFileName);
                if (File.Exists(multiDbOverrideSettingFileName))
                    File.Delete(multiDbOverrideSettingFileName);
            }
        }
        #endregion
        private void StartInfiniteLockingThread(object loopCount)
        {
            int loop = (int)loopCount;
            string connStr = string.Format(Initialization.ConnectionString, "SqlBuildTest");
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand cmd = new SqlCommand(String.Format(Properties.Resources.TableLockingScript, loop.ToString()), conn);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        ///A test for TestConnectivity
        ///</summary>
        [TestMethod()]
        public void TestConnectivityTest_Pass()
        {
            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, "localhost");

            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);


            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = new string[9];
            args[0] = "/Action=remote";
            args[1] = "/RootLoggingPath=\"" + loggingPath + "\"";
            args[2] = "/transactional=true";
            args[3] = "/trial=true";
            args[4] = "/override=\"" + multiDbOverrideSettingFileName + "\"";
            args[5] = "/PackageName=\"" + sbmFileName + "\"";
            args[6] = "/RemoteServers=\"" + fileName + "\"";
            args[7] = "/DistributionType=equal";
            args[8] = "/TestConnectivity=true";
            RemoteExecution target = new RemoteExecution(args);
            int expected = 0;
            int actual;
            actual = target.TestConnectivity();

            if (Directory.Exists(loggingPath))
                Directory.Delete(loggingPath, true);
            if (File.Exists(fileName))
                File.Delete(fileName);
            if (File.Exists(sbmFileName))
                File.Delete(sbmFileName);
            if (File.Exists(multiDbOverrideSettingFileName))
                File.Delete(multiDbOverrideSettingFileName);

            if (actual == -600)
                Assert.Fail("Unable to complete test. Please make sure you have a Remote Execution Service running on localhost");

            Assert.AreEqual(expected, actual, "Ensure that you have your service running on \"localhost\\SQLEXPRESS\"");
        }

        /// <summary>
        ///A test for TestConnectivity
        ///</summary>
        [TestMethod()]
        public void TestConnectivityTest_FailToConnectToOne()
        {
            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, "localhost");

            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);


            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest
localhost\SQLEXPRESS:DoesntExist,DoesntExist";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = new string[9];
            args[0] = "/Action=remote";
            args[1] = "/RootLoggingPath=\"" + loggingPath + "\"";
            args[2] = "/transactional=true";
            args[3] = "/trial=true";
            args[4] = "/override=\"" + multiDbOverrideSettingFileName + "\"";
            args[5] = "/PackageName=\"" + sbmFileName + "\"";
            args[6] = "/RemoteServers=\"" + fileName + "\"";
            args[7] = "/DistributionType=equal";
            args[8] = "/TestConnectivity=true";
            RemoteExecution target = new RemoteExecution(args);
            int expected = 1;
            int actual;
            actual = target.TestConnectivity();

            if (Directory.Exists(loggingPath))
                Directory.Delete(loggingPath, true);
            if (File.Exists(fileName))
                File.Delete(fileName);
            if (File.Exists(sbmFileName))
                File.Delete(sbmFileName);
            if (File.Exists(multiDbOverrideSettingFileName))
                File.Delete(multiDbOverrideSettingFileName);

            if (actual == -600)
                Assert.Fail("Unable to complete test. Please make sure you have a Remote Execution Service running on localhost");

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for TestConnectivity
        ///</summary>
        [TestMethod()]
        public void TestConnectivityTest_FailToConnectToTwo()
        {
            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, "localhost");

            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);


            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest
localhost\SQLEXPRESS:DoesntExist,DoesntExist
localhost\SQLEXPRESS:DoesntExist2,DoesntExist2";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = new string[9];
            args[0] = "/Action=remote";
            args[1] = "/RootLoggingPath=\"" + loggingPath + "\"";
            args[2] = "/transactional=true";
            args[3] = "/trial=true";
            args[4] = "/override=\"" + multiDbOverrideSettingFileName + "\"";
            args[5] = "/PackageName=\"" + sbmFileName + "\"";
            args[6] = "/RemoteServers=\"" + fileName + "\"";
            args[7] = "/DistributionType=equal";
            args[8] = "/TestConnectivity=true";
            RemoteExecution target = new RemoteExecution(args);
            int expected = 2;
            int actual;
            actual = target.TestConnectivity();

            if (Directory.Exists(loggingPath))
                Directory.Delete(loggingPath, true);
            if (File.Exists(fileName))
                File.Delete(fileName);
            if (File.Exists(sbmFileName))
                File.Delete(sbmFileName);
            if (File.Exists(multiDbOverrideSettingFileName))
                File.Delete(multiDbOverrideSettingFileName);

            if (actual == -600)
                Assert.Fail("Unable to complete test. Please make sure you have a Remote Execution Service running on localhost");

            Assert.AreEqual(expected, actual);
        }
    }
}
