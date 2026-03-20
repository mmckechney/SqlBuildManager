using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Interfaces.Console;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.IO;

#nullable enable
namespace SqlBuildManager.Console.UnitTest
{


    /// <summary>
    ///This is a test class for ValidationTest and is intended
    ///to contain all ValidationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ValidationTest
    {


        public TestContext TestContext { get; set; } = null!;

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

        #region ValidateCommonCommandLineArgs
        /// <summary>
        ///A test for ValidateCommonCommandLineArgs
        ///</summary>
        [TestMethod()]
        [Ignore]
        public void ValidateCommonCommandLineArgsTest_MissingRootLoggingPath()
        {
            CommandLineArgs cmdLine = new CommandLineArgs();
            cmdLine.AuthenticationArgs.AuthenticationType = SqlSync.Connection.AuthenticationType.AzureADIntegrated;
            string[] errorMessages = null!;
            int expected = -99;
            int actual;
            actual = Validation.ValidateCommonCommandLineArgs(cmdLine, out errorMessages);
            Assert.AreEqual(0, errorMessages.Length);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ValidateCommonCommandLineArgs
        ///</summary>
        [TestMethod()]
        public void ValidateCommonCommandLineArgsTest_BadTransactionTrialCombo()
        {
            CommandLineArgs cmdLine = new CommandLineArgs();
            cmdLine.AuthenticationArgs.AuthenticationType = SqlSync.Connection.AuthenticationType.AzureADIntegrated;
            cmdLine.RootLoggingPath = Path.GetTempPath();
            cmdLine.Transactional = false;
            cmdLine.Trial = true;
            string[] errorMessages = null!;
            int expected = (int)ExecutionReturn.InvalidTransactionAndTrialCombo;
            int actual;
            actual = Validation.ValidateCommonCommandLineArgs(cmdLine, out errorMessages);
            Assert.AreEqual(2, errorMessages.Length);
            Assert.IsTrue(errorMessages[0].StartsWith("Invalid command line combination"));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ValidateCommonCommandLineArgs
        ///</summary>
        [TestMethod()]
        public void ValidateCommonCommandLineArgsTest_MissingOverrideSetting()
        {
            CommandLineArgs cmdLine = new CommandLineArgs();
            cmdLine.AuthenticationArgs.AuthenticationType = SqlSync.Connection.AuthenticationType.AzureADIntegrated;
            cmdLine.RootLoggingPath = Path.GetTempPath();
            cmdLine.Transactional = true;
            cmdLine.Trial = true;
            string[] errorMessages = null!;
            int expected = (int)ExecutionReturn.MissingOverrideFlag;
            int actual;
            actual = Validation.ValidateCommonCommandLineArgs(cmdLine, out errorMessages);
            Assert.AreEqual(2, errorMessages.Length);
            Assert.IsTrue(errorMessages[0].LastIndexOf("Missing --override option") > -1);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ValidateCommonCommandLineArgs
        ///</summary>
        [TestMethod()]
        public void ValidateCommonCommandLineArgsTest_MissingBuildFileName()
        {
            CommandLineArgs cmdLine = new CommandLineArgs();
            cmdLine.AuthenticationArgs.AuthenticationType = SqlSync.Connection.AuthenticationType.AzureADIntegrated;
            cmdLine.RootLoggingPath = Path.GetTempPath();
            cmdLine.Transactional = true;
            cmdLine.Trial = true;
            cmdLine.MultiDbRunConfigFileName = Path.Combine(Path.GetTempPath(), "temp","multicfg.multidb");
            string[] errorMessages = null!;
            int expected = (int)ExecutionReturn.MissingBuildFlag;
            int actual;
            actual = Validation.ValidateCommonCommandLineArgs(cmdLine, out errorMessages);
            Assert.AreEqual(2, errorMessages.Length);
            Assert.IsTrue(errorMessages[0].LastIndexOf("Invalid command line set. Missing --packagename, --platinumdacpac, --scriptsrcdir, or --platinumdbsource and --platinumserversource options.") > -1);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ValidateCommonCommandLineArgs
        ///</summary>
        [TestMethod()]
        public void ValidateCommonCommandLineArgsTest_MissingBuildFile()
        {
            CommandLineArgs cmdLine = new CommandLineArgs();
            cmdLine.AuthenticationArgs.AuthenticationType = SqlSync.Connection.AuthenticationType.AzureADIntegrated;
            cmdLine.RootLoggingPath = Path.GetTempPath();
            cmdLine.Transactional = true;
            cmdLine.Trial = true;
            cmdLine.MultiDbRunConfigFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(),"multicfg.cfg");
            cmdLine.BuildFileName = Path.Combine(Path.GetTempPath(), "temp/not_here.sbm");
            string[] errorMessages = null!;
            int expected = (int)ExecutionReturn.InvalidBuildFileNameValue;
            int actual;
            actual = Validation.ValidateCommonCommandLineArgs(cmdLine, out errorMessages);
            Assert.AreEqual(2, errorMessages.Length);
            Assert.IsTrue(errorMessages[0].LastIndexOf("Missing Build file. The build file specified:") > -1);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for ValidateCommonCommandLineArgs
        ///</summary>
        [TestMethod()]
        public void ValidateCommonCommandLineArgsTest_BadOverrideSetting()
        {
            var filePath = Path.Combine(Path.GetTempPath(), "multicfg.BadExt");
            if (!File.Exists(filePath))
                File.WriteAllText(filePath, "hi");
            CommandLineArgs cmdLine = new CommandLineArgs();
            cmdLine.RootLoggingPath = Path.GetTempPath();
            cmdLine.Transactional = true;
            cmdLine.Trial = true;
            cmdLine.MultiDbRunConfigFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "multicfg.BadExt");
            cmdLine.ScriptSrcDir = Path.GetTempPath();
            cmdLine.AuthenticationArgs.AuthenticationType = SqlSync.Connection.AuthenticationType.AzureADIntegrated;
            string[] errorMessages = null!;
            int expected = (int)ExecutionReturn.InvalidOverrideFlag;
            int actual;
            actual = Validation.ValidateCommonCommandLineArgs(cmdLine, out errorMessages);

            Assert.AreEqual(2, errorMessages.Length);
            Assert.IsTrue(errorMessages[0].LastIndexOf("Specified --override file does not exist") > -1);
            Assert.AreEqual(expected, actual);

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch
                { }
            }

        }

        /// <summary>
        ///A test for ValidateCommonCommandLineArgs
        ///</summary>
        [TestMethod()]
        public void ValidateCommonCommandLineArgsTest_InvalidScriptScrDir()
        {
            CommandLineArgs cmdLine = new CommandLineArgs();
            cmdLine.AuthenticationArgs.AuthenticationType = SqlSync.Connection.AuthenticationType.AzureADIntegrated;
            cmdLine.RootLoggingPath = Path.GetTempPath();
            cmdLine.Transactional = true;
            cmdLine.Trial = true;
            cmdLine.MultiDbRunConfigFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "multicfg.multidb");
            cmdLine.ScriptSrcDir = Path.Combine(Path.GetTempPath(), "I_DONT_EXIST");
            string[] errorMessages = null!;
            int expected = (int)ExecutionReturn.InvalidScriptSourceDirectory;
            int actual;
            actual = Validation.ValidateCommonCommandLineArgs(cmdLine, out errorMessages);
            Assert.AreEqual(2, errorMessages.Length);
            Assert.IsTrue(errorMessages[0].LastIndexOf("Invalid --scriptsrcdir setting.") > -1);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ValidateCommonCommandLineArgs
        ///</summary>
        [TestMethod()]
        public void ValidateCommonCommandLineArgsTest_GoodConfig()
        {
            var init = new Initialization();
            var multFile = SqlBuildManager.Test.Common.TestFileHelper.GetTrulyUniqueFile("cfg");
            File.WriteAllBytes(multFile, Properties.Resources.NoTrans_MultiDb_multidb);
            CommandLineArgs cmdLine = new CommandLineArgs();
            cmdLine.AuthenticationArgs.AuthenticationType = SqlSync.Connection.AuthenticationType.AzureADIntegrated;
            cmdLine.RootLoggingPath = Path.GetTempPath();
            cmdLine.Transactional = true;
            cmdLine.Trial = true;
            cmdLine.MultiDbRunConfigFileName = multFile;
            cmdLine.ScriptSrcDir = Path.GetTempPath();
            string[] errorMessages = null!;
            int expected = 0;
            int actual;
            try
            {
                actual = Validation.ValidateCommonCommandLineArgs(cmdLine, out errorMessages);
                Assert.AreEqual(0, errorMessages.Length);
                Assert.AreEqual(expected, actual);
            }
            finally
            {
                if (File.Exists(multFile)) File.Delete(multFile);
            }
        }
        #endregion

        #region ValidateAndLoadMultiDbData
        /// <summary>
        ///A test for ValidateAndLoadMultiDbData
        ///</summary>
        [TestMethod()]
        public void ValidateAndLoadMultiDbDataTest_MissingMultiDbFile()
        {
            string multiDbOverrideSettingFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "test_not_here.multidb");
            MultiDbData multiData = null!;
            string[] errorMessages = null!;
            int expected = (int)ExecutionReturn.MissingTargetDbOverrideSetting;
            int actual;
            actual = Validation.ValidateAndLoadMultiDbData(multiDbOverrideSettingFileName, null!, out multiData, out errorMessages);
            Assert.IsNull(multiData);
            Assert.AreEqual(1, errorMessages.Length);
            Assert.IsTrue(errorMessages[0].IndexOf("The Multi DB configuration file was not found") > -1);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for ValidateAndLoadMultiDbData
        ///</summary>
        [TestMethod()]
        public void ValidateAndLoadMultiDbDataTest_MissingMultiDbQFile()
        {
            string multiDbOverrideSettingFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "test_not_here.multidbq");
            MultiDbData multiData = null!;
            string[] errorMessages = null!;
            int expected = (int)ExecutionReturn.MissingTargetDbOverrideSetting;
            int actual;
            actual = Validation.ValidateAndLoadMultiDbData(multiDbOverrideSettingFileName, null!, out multiData, out errorMessages);
            Assert.IsNull(multiData);
            Assert.AreEqual(1, errorMessages.Length);
            Assert.IsTrue(errorMessages[0].IndexOf("The Multi DB configuration file was not found") > -1);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for ValidateAndLoadMultiDbData
        ///</summary>
        [TestMethod()]
        public void ValidateAndLoadMultiDbDataTest_MissingCfgFile()
        {
            string multiDbOverrideSettingFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(),"test_not_here.multidbq");
            MultiDbData multiData = null!;
            string[] errorMessages = null!;
            int expected = (int)ExecutionReturn.MissingTargetDbOverrideSetting;
            int actual;
            actual = Validation.ValidateAndLoadMultiDbData(multiDbOverrideSettingFileName, null!, out multiData, out errorMessages);
            Assert.IsNull(multiData);
            Assert.AreEqual(1, errorMessages.Length);
            Assert.IsTrue(errorMessages[0].IndexOf("The Multi DB configuration file was not found") > -1);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for ValidateAndLoadMultiDbData
        ///</summary>
        [TestMethod()]
        public void ValidateAndLoadMultiDbDataTest_WellFormedMultiDbData()
        {
            string cfgContents = @"Server1\Instance_1:Default,MyDatabaseOverride";
            string multiDbOverrideSettingFileName = Path.Combine(Path.GetTempPath(), $"{System.Guid.NewGuid().ToString()}.cfg");
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);
            MultiDbData multiData = null!;
            string[] errorMessages = null!;
            int expected = 0;
            int actual;
            actual = Validation.ValidateAndLoadMultiDbData(multiDbOverrideSettingFileName, null!, out multiData, out errorMessages);
            File.Delete(multiDbOverrideSettingFileName);

            Assert.IsNotNull(multiData);
            Assert.AreEqual(0, errorMessages.Length);
            Assert.AreEqual(expected, actual);


        }

        ///// <summary>
        /////A test for ValidateAndLoadMultiDbData
        /////</summary>
        //[TestMethod()]
        //public void ValidateAndLoadMultiDbDataTest_MalFormedMultiDbData()
        //{
        //    string cfgContents = @"Server1\Instance_1:Default,";
        //    string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
        //    File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);
        //    MultiDbData multiData = null!;
        //    string[] errorMessages = null!;
        //    int expected = 0;
        //    int actual;
        //    actual = Validation.ValidateAndLoadMultiDbData(multiDbOverrideSettingFileName, out multiData, out errorMessages);
        //    File.Delete(multiDbOverrideSettingFileName);

        //    Assert.IsNotNull(multiData);
        //    Assert.AreEqual(2, errorMessages.Length);
        //    Assert.IsTrue(errorMessages[0].IndexOf("One or more scripts is missing a default or target override database setting") > -1);

        //    Assert.AreEqual(expected, actual);


        //}


        #endregion


        /// <summary>
        ///A test for Validation Constructor
        ///</summary>
        [TestMethod()]
        public void ValidationConstructorTest()
        {
            Validation target = new Validation();
            Assert.IsNotNull(target);
        }
    }
}
