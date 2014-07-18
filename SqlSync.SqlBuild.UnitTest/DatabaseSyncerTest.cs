using SqlSync.SqlBuild.Syncronizer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SqlSync.Connection;

namespace SqlSync.SqlBuild.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for DatabaseSyncerTest and is intended
    ///to contain all DatabaseSyncerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DatabaseSyncerTest
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
        ///A test for SyncronizeDatabases
        ///</summary>
        [TestMethod()]
        public void SyncronizeDatabasesTest()
        {
            DatabaseSyncer target = new DatabaseSyncer(); // TODO: Initialize to an appropriate value
            ConnectionData gold = new ConnectionData()
            {
                DatabaseName = "SqlBuildTest",
                SQLServerName = @"localhost\SQLEXPRESS",
                UseWindowAuthentication = true
            };
            ConnectionData toUpdate = new ConnectionData()
            {
                DatabaseName = "SqlBuildTest1",
                SQLServerName = @"localhost\SQLEXPRESS",
                UseWindowAuthentication = true
            };
            target.SyncronizeDatabases(gold, toUpdate);

            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }
    }
}
