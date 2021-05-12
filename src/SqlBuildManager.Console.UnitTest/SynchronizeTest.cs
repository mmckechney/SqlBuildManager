using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using SqlSync.SqlBuild.Syncronizer;
using System;
namespace SqlBuildManager.Console.UnitTest
{


    /// <summary>
    ///This is a test class for SynchronizeTest and is intended
    ///to contain all SynchronizeTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SynchronizeTest
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


        /// <summary>
        ///A test for Synchronize Constructor
        ///</summary>
        [TestMethod()]
        public void SynchronizeConstructorTest()
        {
            Synchronize target = new Synchronize();
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///A test for GetDatabaseRunHistoryDifference
        ///</summary>
        [TestMethod()]
        public void GetDatabaseRunHistoryDifferenceTest()
        {
            string[] args = new string[]
                {
                    "getdifference",
                    "--database" , "SqlbuildTest_SyncTest2",
                    "--server" , "localhost\\sqlexpress",
                    "--golddatabase", "SqlbuildTest_SyncTest1",
                    "--goldserver" , "localhost\\sqlexpress"
                };
            string expected = string.Empty; 
            string actual;

            var cmdLine = CommandLineBuilder.ParseArguments(args);
 
            actual = Synchronize.GetDatabaseRunHistoryTextDifference(cmdLine);
            Assert.AreEqual(1, actual.Split(new string[]{"\r\n"},StringSplitOptions.None).Length);
        }

        /// <summary>
        ///A test for GetDatabaseRunHistoryDifference
        ///</summary>
        [TestMethod()]
        public void GetDatabaseRunHistoryDifferenceTest1()
        {
            CommandLineArgs cmdLine = null; 
            DatabaseRunHistory expected = null; 
            DatabaseRunHistory actual;
            actual = Synchronize.GetDatabaseRunHistoryDifference(cmdLine);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ParseAndValidateFlags
        ///</summary>
        [TestMethod()]
        [DeploymentItem("sbm.exe")]
        public void ParseAndValidateFlagsTest()
        {
            string[] args = new string[]
               {
                    "getdifference",
                    "--database" , "SqlbuildTest_SyncTest2"
                  
               };
            CommandLineArgs expected = null; 
            (CommandLineArgs actual, string message) = CommandLineBuilder.ParseArgumentsWithMessage(args);

            Assert.AreEqual(expected, actual);
            Assert.IsTrue(message.Contains("--golddatabase"));
        }
    }
}
