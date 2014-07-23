using SqlSync.SqlBuild.Syncronizer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
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

        [ClassInitialize()]
        public static void Initilize(TestContext testContext)
        {
            Initialization init = new Initialization();
        }

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
            Assert.IsNotNull(target);
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
                    DatabaseName = "SqlBuildTest_SyncTest1",
                    SQLServerName = @"localhost\SQLEXPRESS",
                    UseWindowAuthentication = true
                };
            ConnectionData toBeUpdated = new ConnectionData()
                {
                    DatabaseName = "SqlBuildTest_SyncTest2",
                    SQLServerName = @"localhost\SQLEXPRESS",
                    UseWindowAuthentication = true
                };
            DatabaseRunHistory actual;
            actual = target.GetDatabaseHistoryDifference(goldenCopy, toBeUpdated);
            Assert.AreEqual(3, actual.BuildFileHistory.Count);

            //check that they are in chron order
            Assert.IsTrue(actual.BuildFileHistory[0].CommitDate < actual.BuildFileHistory[1].CommitDate);
            Assert.IsTrue(actual.BuildFileHistory[1].CommitDate < actual.BuildFileHistory[2].CommitDate);
            Assert.AreEqual("7E704479328C9B2FDA48C7CF093B16F125EAB98A", actual.BuildFileHistory[2].BuildFileHash);
        }

        /// <summary>
        ///A test for GetDatabaseHistoryDifference
        ///</summary>
        [TestMethod()]
        public void GetDatabaseHistoryDifferenceTest_PartialSync()
        {
            DatabaseDiffer target = new DatabaseDiffer();
            ConnectionData goldenCopy = new ConnectionData()
            {
                DatabaseName = "SqlBuildTest_SyncTest1",
                SQLServerName = @"localhost\SQLEXPRESS",
                UseWindowAuthentication = true
            };
            ConnectionData toBeUpdated = new ConnectionData()
            {
                DatabaseName = "SqlBuildTest_SyncTest3",
                SQLServerName = @"localhost\SQLEXPRESS",
                UseWindowAuthentication = true
            };
            DatabaseRunHistory actual;
            actual = target.GetDatabaseHistoryDifference(goldenCopy, toBeUpdated);
            Assert.AreEqual(1, actual.BuildFileHistory.Count);

            //check that they are in chron order
            Assert.AreEqual("7E704479328C9B2FDA48C7CF093B16F125EAB98A", actual.BuildFileHistory[0].BuildFileHash);
        }

        /// <summary>
        ///A test for GetDatabaseHistoryDifference
        ///</summary>
        [TestMethod()]
        public void GetDatabaseHistoryDifferenceTest_SameSource()
        {
            DatabaseDiffer target = new DatabaseDiffer();
            ConnectionData goldenCopy = new ConnectionData()
            {
                DatabaseName = "SqlBuildTest_SyncTest1",
                SQLServerName = @"localhost\SQLEXPRESS",
                UseWindowAuthentication = true
            };
            ConnectionData toBeUpdated = new ConnectionData()
            {
                DatabaseName = "SqlBuildTest_SyncTest1",
                SQLServerName = @"localhost\SQLEXPRESS",
                UseWindowAuthentication = true
            };
            DatabaseRunHistory actual;
            actual = target.GetDatabaseHistoryDifference(goldenCopy, toBeUpdated);
            Assert.AreEqual(0, actual.BuildFileHistory.Count);

        }

        /// <summary>
        ///A test for GetDatabaseRunHistory
        ///</summary>
        [TestMethod()]
        public void GetDatabaseRunHistoryTest_TestCountAndSelection()
        {
            DatabaseDiffer target = new DatabaseDiffer(); 
            ConnectionData dbConnData = new ConnectionData()
                {
                    DatabaseName = "SqlBuildTest_SyncTest1",
                    SQLServerName = @"localhost\SQLEXPRESS",
                    UseWindowAuthentication = true
                };

            DatabaseRunHistory actual;
            actual = target.GetDatabaseRunHistory(dbConnData);
            Assert.AreEqual(3,actual.BuildFileHistory.Count);

            Assert.AreEqual(DateTime.Parse("2014-07-21 13:58:07.880"), 
                actual.BuildFileHistory.Where(x => x.BuildFileHash == "7651E282160CAF9C92CB923004D94B91181C077E").Select(y => y.CommitDate).FirstOrDefault());
        }
    }
}
