﻿using SqlSync.SqlBuild;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace SqlSync.SqlBuild.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for CommandLineTest and is intended
    ///to contain all CommandLineTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CommandLineTest
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


        /// <summary>
        ///A test for ParseCommandLineArg
        ///</summary>
        [TestMethod()]
        public void ParseCommandLineArgTest()
        {
            string[] args = new string[]{@"C:\Program Files\McKechney.com\Sql Build Manager\SqlBuildManager.Console.exe", 
                "/trial=\"True\"", 
                "/threaded=\"true\"", 
                "/LogAsText=\"false\"", 
                "/username=\"UserName1\"", 
                "/password=\"Password1\"", 
                "/LogToDatabaseName=\"AltLogDb\"", 
                "/build=\"c:\\Audit.sbm\"", 
                "/RootLoggingPath=\"C:\\Temp\"", 
                "/override=\"c:\\test.multiDb\"", 
                "/description=\"Just Testing\"",
                "/transactional=false"}; 
            CommandLineArgs actual;
            actual = CommandLine.ParseCommandLineArg(args);
            Assert.AreEqual(true, actual.Trial);
            Assert.AreEqual(false, actual.LogAsText);
            Assert.AreEqual("UserName1", actual.UserName);
            Assert.AreEqual("Password1", actual.Password);
            Assert.AreEqual("AltLogDb", actual.LogToDatabaseName);
            Assert.AreEqual("c:\\Audit.sbm", actual.BuildFileName);
            Assert.AreEqual("C:\\Temp", actual.RootLoggingPath);
            Assert.AreEqual(true, actual.OverrideDesignated);
            Assert.AreEqual("c:\\test.multiDb", actual.MultiDbRunConfigFileName);
            Assert.AreEqual("Just Testing", actual.Description);
            Assert.AreEqual(false, actual.Transactional);
            Assert.AreEqual(true, actual.BuildDesignated);
        }

        /// <summary>
        ///A test for ParseCommandLineArg
        ///</summary>
        [TestMethod()]
        public void ParseCommandLineArgTest_Alternate()
        {
            string[] args = new string[]{@"C:\Program Files\McKechney.com\Sql Build Manager\SqlBuildManager.Console.exe", 
                "/threaded=\"true\"", 
                "/LogToDatabaseName=\"AltLogDb\"", 
                "/scriptsrcdir=\"C:\\Temp\"", 
                "/server=\"MyServer\"",
                "/database=\"MyDatabase\"",
                "/description=\"Just Testing\""};
            CommandLineArgs actual;
            actual = CommandLine.ParseCommandLineArg(args);
            Assert.AreEqual(false, actual.Trial);
            Assert.AreEqual("MyServer", actual.Server);
            Assert.AreEqual("MyDatabase", actual.Database);
            Assert.AreEqual("AltLogDb", actual.LogToDatabaseName);
            Assert.AreEqual(false, actual.OverrideDesignated);
            Assert.AreEqual("Just Testing", actual.Description);
            Assert.AreEqual(true, actual.Transactional);
            Assert.AreEqual(false, actual.AutoScriptDesignated);
            Assert.AreEqual(false, actual.SprocTestDesignated);
        }
        [TestMethod()]
        public void ParseCommandLineArgTest_BackwardCompatability()
        {
            string[] args = new string[]{@"C:\Program Files\McKechney.com\Sql Build Manager\SqlBuildManager.Console.exe", 
                "/trial=\"True\"", 
                "/threaded=\"true\"", 
                "/b=\"c:\\Audit.sbm\"", 
                "/RootLoggingPath=\"C:\\Temp\"", 
                "/override=\"c:\\test.multiDb\"", 
                "/description=\"Just Testing\"",
                "/transactional=true"};
            CommandLineArgs actual;
            actual = CommandLine.ParseCommandLineArg(args);
            Assert.AreEqual(true, actual.Trial);
            Assert.AreEqual("c:\\Audit.sbm", actual.BuildFileName);
            Assert.AreEqual("C:\\Temp", actual.RootLoggingPath);
            Assert.AreEqual(true, actual.OverrideDesignated);
            Assert.AreEqual("c:\\test.multiDb", actual.MultiDbRunConfigFileName);
            Assert.AreEqual(true, actual.Transactional);
        }
        /// <summary>
        ///A test for ParseCommandLineArg
        ///</summary>
        [TestMethod()]
        public void ParseCommandLineArgTest_AutoScript()
        {
            string[] args = new string[]{@"C:\Program Files\McKechney.com\Sql Build Manager\SqlBuildManager.Console.exe", 
                "/auto=\"C:\test.sqlauto\""};
            CommandLineArgs actual;
            actual = CommandLine.ParseCommandLineArg(args);
            Assert.AreEqual(true, actual.AutoScriptDesignated);
            Assert.AreEqual("C:\test.sqlauto", actual.AutoScriptFileName);
        }
        /// <summary>
        ///A test for ParseCommandLineArg
        ///</summary>
        [TestMethod()]
        public void ParseCommandLineArgTest_SpTest()
        {
            string[] args = new string[]{@"C:\Program Files\McKechney.com\Sql Build Manager\SqlBuildManager.Console.exe", 
                "/test=\"C:\test.sptest\""};
            CommandLineArgs actual;
            actual = CommandLine.ParseCommandLineArg(args);
            Assert.AreEqual(true, actual.SprocTestDesignated);
            Assert.AreEqual("C:\test.sptest", actual.SpTestFile);
        }
        /// <summary>
        ///A test for ParseCommandLineArg
        ///</summary>
        [TestMethod()]
        public void ParseCommandLineArgTest_TheRestOfTheArgs()
        {
            string[] args = new string[]{@"C:\Program Files\McKechney.com\Sql Build Manager\SqlBuildManager.Console.exe", 
                "/trial",
                "/b=\"C:\test.sbm\"",
                "/override=\"default;override,default2,override2\"",
                "/log=\"log.log",
                "/scriptlogfile=\"C:\\Scriptlog.log\""};
            CommandLineArgs actual;
            actual = CommandLine.ParseCommandLineArg(args);
            Assert.AreEqual("C:\test.sbm", actual.BuildFileName);
            Assert.AreEqual(true, actual.OverrideDesignated);
            Assert.AreEqual("default;override,default2,override2", actual.ManualOverRideSets);
            Assert.AreEqual("log.log", actual.LogFileName);
            Assert.AreEqual("C:\\Scriptlog.log", actual.ScriptLogFileName);
        }
        /// <summary>
        ///A test for CommandLine Constructor
        ///</summary>
        [TestMethod()]
        public void CommandLineConstructorTest()
        {
            CommandLine target = new CommandLine();
            Assert.AreEqual(typeof(CommandLine), target.GetType());
        }
    }
}
