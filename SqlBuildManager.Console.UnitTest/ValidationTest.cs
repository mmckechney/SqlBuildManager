using SqlBuildManager.Console;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.MultiDb;
using SqlSync.SqlBuild;
using SqlBuildManager.Interfaces.Console;
using System.IO;
namespace SqlBuildManager.Console.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for ValidationTest and is intended
    ///to contain all ValidationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ValidationTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

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
        public void ValidateCommonCommandLineArgsTest_MissingRootLoggingPath()
        {
            CommandLineArgs cmdLine = new CommandLineArgs();
            
            string[] errorMessages = null;
            int expected = -99; 
            int actual;
            actual = Validation.ValidateCommonCommandLineArgs(ref cmdLine, out errorMessages);
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
            cmdLine.RootLoggingPath = @"C\temp";
            cmdLine.Transactional = false;
            cmdLine.Trial = true;
            string[] errorMessages = null;
            int expected = (int)ExecutionReturn.InvalidTransactionAndTrialCombo;
            int actual;
            actual = Validation.ValidateCommonCommandLineArgs(ref cmdLine, out errorMessages);
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
            cmdLine.RootLoggingPath = @"C\temp";
            cmdLine.Transactional = true;
            cmdLine.Trial = true;
            string[] errorMessages = null;
            int expected = (int)ExecutionReturn.MissingOverrideFlag;
            int actual;
            actual = Validation.ValidateCommonCommandLineArgs(ref cmdLine, out errorMessages);
            Assert.AreEqual(2, errorMessages.Length);
            Assert.IsTrue(errorMessages[0].LastIndexOf("Missing /override setting.") > -1);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ValidateCommonCommandLineArgs
        ///</summary>
        [TestMethod()]
        public void ValidateCommonCommandLineArgsTest_MissingBuildFileName()
        {
            CommandLineArgs cmdLine = new CommandLineArgs();
            cmdLine.RootLoggingPath = @"C\temp";
            cmdLine.Transactional = true;
            cmdLine.Trial = true;
            cmdLine.MultiDbRunConfigFileName = @"C\temp\multicfg.multidb";
            string[] errorMessages = null;
            int expected = (int)ExecutionReturn.MissingBuildFlag;
            int actual;
            actual = Validation.ValidateCommonCommandLineArgs(ref cmdLine, out errorMessages);
            Assert.AreEqual(2, errorMessages.Length);
            Assert.IsTrue(errorMessages[0].LastIndexOf("Missing /build or /ScriptSrcDir setting.") > -1);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ValidateCommonCommandLineArgs
        ///</summary>
        [TestMethod()]
        public void ValidateCommonCommandLineArgsTest_MissingBuildFile()
        {
            CommandLineArgs cmdLine = new CommandLineArgs();
            cmdLine.RootLoggingPath = @"C\temp";
            cmdLine.Transactional = true;
            cmdLine.Trial = true;
            cmdLine.MultiDbRunConfigFileName = @"C\temp\multicfg.cfg";
            cmdLine.BuildFileName = @"C:\temp\not_here.sbm";
            string[] errorMessages = null;
            int expected = (int)ExecutionReturn.InvalidBuildFileNameValue;
            int actual;
            actual = Validation.ValidateCommonCommandLineArgs(ref cmdLine, out errorMessages);
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
            CommandLineArgs cmdLine = new CommandLineArgs();
            cmdLine.RootLoggingPath = @"C\temp";
            cmdLine.Transactional = true;
            cmdLine.Trial = true;
            cmdLine.MultiDbRunConfigFileName = @"C\temp\multicfg.BadExt";
            cmdLine.ScriptSrcDir = @"C:\temp";
            string[] errorMessages = null;
            int expected = (int)ExecutionReturn.InvalidOverrideFlag;
            int actual;
            actual = Validation.ValidateCommonCommandLineArgs(ref cmdLine, out errorMessages);
            Assert.AreEqual(2, errorMessages.Length);
            Assert.IsTrue(errorMessages[0].LastIndexOf("The '/override' setting file value must be") > -1);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ValidateCommonCommandLineArgs
        ///</summary>
        [TestMethod()]
        public void ValidateCommonCommandLineArgsTest_InvalidScriptScrDir()
        {
            CommandLineArgs cmdLine = new CommandLineArgs();
            cmdLine.RootLoggingPath = @"C\temp";
            cmdLine.Transactional = true;
            cmdLine.Trial = true;
            cmdLine.MultiDbRunConfigFileName = @"C\temp\multicfg.multidb";
            cmdLine.ScriptSrcDir = @"C:\I_DONT_EXIST";
            string[] errorMessages = null;
            int expected = (int)ExecutionReturn.InvalidScriptSourceDirectory;
            int actual;
            actual = Validation.ValidateCommonCommandLineArgs(ref cmdLine, out errorMessages);
            Assert.AreEqual(2, errorMessages.Length);
            Assert.IsTrue(errorMessages[0].LastIndexOf("Invalid /ScriptSrcDir setting.") > -1);
            Assert.AreEqual(expected, actual);
        }

         /// <summary>
        ///A test for ValidateCommonCommandLineArgs
        ///</summary>
        [TestMethod()]
        public void ValidateCommonCommandLineArgsTest_GoodConfig()
        {
            CommandLineArgs cmdLine = new CommandLineArgs();
            cmdLine.RootLoggingPath = @"C\temp";
            cmdLine.Transactional = true;
            cmdLine.Trial = true;
            cmdLine.MultiDbRunConfigFileName = @"C\temp\multicfg.cfg";
            cmdLine.ScriptSrcDir = @"C:\temp";
            string[] errorMessages = null;
            int expected = 0;
            int actual;
            actual = Validation.ValidateCommonCommandLineArgs(ref cmdLine, out errorMessages);
            Assert.AreEqual(0, errorMessages.Length);
            Assert.AreEqual(expected, actual);
        }
        #endregion

        #region ValidateAndLoadMultiDbData
        /// <summary>
        ///A test for ValidateAndLoadMultiDbData
        ///</summary>
        [TestMethod()]
        public void ValidateAndLoadMultiDbDataTest_MissingMultiDbFile()
        {
            string multiDbOverrideSettingFileName = @"C:\temp\test_not_here.multidb";
            MultiDbData multiData = null; 
            string[] errorMessages = null;
            int expected = (int)ExecutionReturn.NullMultiDbConfig;
            int actual;
            actual = Validation.ValidateAndLoadMultiDbData(multiDbOverrideSettingFileName, null, out multiData, out errorMessages);
            Assert.IsNull(multiData);
            Assert.AreEqual(2, errorMessages.Length);
            Assert.IsTrue(errorMessages[0].IndexOf("Unable to read in configuration file") > -1);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for ValidateAndLoadMultiDbData
        ///</summary>
        [TestMethod()]
        public void ValidateAndLoadMultiDbDataTest_MissingMultiDbQFile()
        {
            string multiDbOverrideSettingFileName = @"C:\temp\test_not_here.multidbq";
            MultiDbData multiData = null;
            string[] errorMessages = null;
            int expected = (int)ExecutionReturn.NullMultiDbConfig;
            int actual;
            actual = Validation.ValidateAndLoadMultiDbData(multiDbOverrideSettingFileName, null, out multiData, out errorMessages);
            Assert.IsNull(multiData);
            Assert.AreEqual(2, errorMessages.Length);
            Assert.IsTrue(errorMessages[0].IndexOf("Unable to read in configuration file") > -1);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for ValidateAndLoadMultiDbData
        ///</summary>
        [TestMethod()]
        public void ValidateAndLoadMultiDbDataTest_MissingCfgFile()
        {
            string multiDbOverrideSettingFileName = @"C:\temp\test_not_here.multidbq";
            MultiDbData multiData = null;
            string[] errorMessages = null;
            int expected = (int)ExecutionReturn.NullMultiDbConfig;
            int actual;
            actual = Validation.ValidateAndLoadMultiDbData(multiDbOverrideSettingFileName, null, out multiData, out errorMessages);
            Assert.IsNull(multiData);
            Assert.AreEqual(2, errorMessages.Length);
            Assert.IsTrue(errorMessages[0].IndexOf("Unable to read in configuration file") > -1);
            Assert.AreEqual(expected, actual);

        }

         /// <summary>
        ///A test for ValidateAndLoadMultiDbData
        ///</summary>
        [TestMethod()]
        public void ValidateAndLoadMultiDbDataTest_WellFormedMultiDbData()
        {
            string cfgContents = @"Server1\Instance_1:Default,MyDatabaseOverride";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName,cfgContents);
            MultiDbData multiData = null;
            string[] errorMessages = null;
            int expected = 0;
            int actual;
            actual = Validation.ValidateAndLoadMultiDbData(multiDbOverrideSettingFileName, null, out multiData, out errorMessages);
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
        //    MultiDbData multiData = null;
        //    string[] errorMessages = null;
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
