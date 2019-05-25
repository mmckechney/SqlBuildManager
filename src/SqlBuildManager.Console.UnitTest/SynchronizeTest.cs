using SqlBuildManager.Console;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.Syncronizer;

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
        [TestMethod(), Ignore("Don't have the setup scripts ready yet")]
        public void GetDatabaseRunHistoryDifferenceTest()
        {
            string[] args = new string[]
                {
                    "/Database=SqlbuildTest_SyncTest2",
                    "/Server=localhost\\sqlexpress",
                    "/GoldDatabase=SqlbuildTest_SyncTest1",
                    "/GoldServer=localhost\\sqlexpress"
                };
            string expected = string.Empty; 
            string actual;
            var cmdLine = CommandLine.ParseCommandLineArg(args);
            actual = Synchronize.GetDatabaseRunHistoryTextDifference(cmdLine);
            Assert.AreEqual(3, actual.Split(new string[]{"\r\n"},StringSplitOptions.None).Length);
        }

        /// <summary>
        ///A test for GetDatabaseRunHistoryDifference
        ///</summary>
        [TestMethod(), Ignore("Don't have the setup scripts ready yet")]
        public void GetDatabaseRunHistoryDifferenceTest1()
        {
            CommandLineArgs cmdLine = null; // TODO: Initialize to an appropriate value
            DatabaseRunHistory expected = null; // TODO: Initialize to an appropriate value
            DatabaseRunHistory actual;
            actual = Synchronize.GetDatabaseRunHistoryDifference(cmdLine);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ParseAndValidateFlags
        ///</summary>
        [TestMethod(), Ignore("not complete")]
        [DeploymentItem("SqlBuildManager.Console.exe")]
        public void ParseAndValidateFlagsTest()
        {
            string[] args = null; // TODO: Initialize to an appropriate value
            CommandLineArgs expected = null; // TODO: Initialize to an appropriate value
            CommandLineArgs actual;
            var cmdLine = CommandLine.ParseCommandLineArg(args);
            actual = Synchronize.ValidateFlags(cmdLine);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
