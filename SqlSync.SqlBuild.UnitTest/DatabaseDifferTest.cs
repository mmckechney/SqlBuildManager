using SqlSync.SqlBuild.Syncronizer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SqlSync.Connection;

namespace SqlSync.SqlBuild.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for DatabaseDifferTest and is intended
    ///to contain all DatabaseDifferTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DatabaseDifferTest
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
        ///A test for DatabaseDiffer Constructor
        ///</summary>
        [TestMethod()]
        public void DatabaseDifferConstructorTest()
        {
            DatabaseDiffer target = new DatabaseDiffer();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for GetDatabaseHistoryDifference
        ///</summary>
        [TestMethod()]
        public void GetDatabaseHistoryDifferenceTest()
        {
            DatabaseDiffer target = new DatabaseDiffer(); 
            ConnectionData goldenCopy = new ConnectionData()
                {
                    DatabaseName = "SqlBuildTest",
                    SQLServerName = @"localhost\SQLEXPRESS",
                    UseWindowAuthentication = true
                };
            ConnectionData toBeUpdated = new ConnectionData()
                {
                    DatabaseName = "SqlBuildTest1",
                    SQLServerName = @"localhost\SQLEXPRESS",
                    UseWindowAuthentication = true
                };
            DatabaseRunHistory expected = null; // TODO: Initialize to an appropriate value
            DatabaseRunHistory actual;
            actual = target.GetDatabaseHistoryDifference(goldenCopy, toBeUpdated);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetDatabaseRunHistory
        ///</summary>
        [TestMethod()]
        public void GetDatabaseRunHistoryTest()
        {
            DatabaseDiffer target = new DatabaseDiffer(); // TODO: Initialize to an appropriate value
            ConnectionData dbConnData = new ConnectionData()
                {
                    DatabaseName = "SqlBuildTest",
                    SQLServerName = @"localhost\SQLEXPRESS",
                    UseWindowAuthentication = true
                };
            DatabaseRunHistory expected = null; // TODO: Initialize to an appropriate value
            DatabaseRunHistory actual;
            actual = target.GetDatabaseRunHistory(dbConnData);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
