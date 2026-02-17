using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.Connection;
using SqlSync.SqlBuild.Synchronizer;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace SqlSync.SqlBuild.Dependent.UnitTest
{
    /// <summary>
    /// Integration tests for DatabaseDiffer that require database access
    /// NOTE: These tests require (local)\SQLEXPRESS with SqlBuildTest databases
    /// </summary>
    [TestClass()]
    [DoNotParallelize]
    public class DatabaseDifferTest
    {
        private static List<Initialization> _initColl = new();

        [ClassInitialize()]
        public static void Initilize(TestContext testContext)
        {
            _initColl = new List<Initialization>();
            // Create one to ensure databases exist
            _initColl.Add(new Initialization());
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            foreach (var init in _initColl)
            {
                init.Dispose();
            }
        }

        private Initialization GetInitialization()
        {
            var init = new Initialization();
            _initColl.Add(init);
            return init;
        }

        public TestContext TestContext { get; set; } = null!;

        #region Constructor Tests

        [TestMethod()]
        public void DatabaseDifferConstructorTest()
        {
            DatabaseDiffer target = new DatabaseDiffer();
            Assert.IsNotNull(target);
        }

        #endregion

        #region GetDatabaseRunHistory Tests

        [TestMethod()]
        public void GetDatabaseRunHistory_ReturnsHistory()
        {
            var init = GetInitialization();
            DatabaseDiffer target = new DatabaseDiffer();
            ConnectionData connData = init.CreateConnectionData(init.testDatabaseNames[0]);

            DatabaseRunHistory actual = target.GetDatabaseRunHistory(connData);

            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.BuildFileHistory);
            // May be empty if no builds have been run
        }

        [TestMethod()]
        public void GetDatabaseRunHistory_MultipleDbsReturnDifferentHistory()
        {
            var init = GetInitialization();
            DatabaseDiffer target = new DatabaseDiffer();
            
            ConnectionData connData1 = init.CreateConnectionData(init.testDatabaseNames[0]);
            
            ConnectionData connData2 = init.CreateConnectionData(
                init.testDatabaseNames.Count > 1 ? init.testDatabaseNames[1] : init.testDatabaseNames[0]);

            DatabaseRunHistory history1 = target.GetDatabaseRunHistory(connData1);
            DatabaseRunHistory history2 = target.GetDatabaseRunHistory(connData2);

            Assert.IsNotNull(history1);
            Assert.IsNotNull(history2);
        }

        [TestMethod()]
        public void GetDatabaseRunHistory_HandlesEmptyLoggingTable()
        {
            var init = GetInitialization();
            // Clean test databases should have empty logging tables
            DatabaseDiffer target = new DatabaseDiffer();
            ConnectionData connData = init.CreateConnectionData(init.testDatabaseNames[0]);

            DatabaseRunHistory actual = target.GetDatabaseRunHistory(connData);

            Assert.IsNotNull(actual);
            // Empty is valid - the table exists but has no data
        }

        #endregion

        #region GetDatabaseHistoryDifference Tests

        [TestMethod()]
        public void GetDatabaseHistoryDifferenceTest_SameSource()
        {
            var init = GetInitialization();
            DatabaseDiffer target = new DatabaseDiffer();
            ConnectionData goldenCopy = init.CreateConnectionData(init.testDatabaseNames[0]);
            ConnectionData toBeUpdated = init.CreateConnectionData(init.testDatabaseNames[0]);

            DatabaseRunHistory actual = target.GetDatabaseHistoryDifference(goldenCopy, toBeUpdated);

            Assert.AreEqual(0, actual.BuildFileHistory.Count, "Same database should have no differences");
        }

        [TestMethod()]
        public void GetDatabaseHistoryDifference_StringOverload_Works()
        {
            var init = GetInitialization();
            DatabaseDiffer target = new DatabaseDiffer();

            DatabaseRunHistory actual = target.GetDatabaseHistoryDifference(
                init.serverName, init.testDatabaseNames[0],
                init.serverName, init.testDatabaseNames[0]);

            Assert.AreEqual(0, actual.BuildFileHistory.Count);
        }

        [TestMethod()]
        public void GetDatabaseHistoryDifference_DifferentDatabases_ReturnsDifferences()
        {
            var init = GetInitialization();
            DatabaseDiffer target = new DatabaseDiffer();
            ConnectionData goldenCopy = init.CreateConnectionData(init.testDatabaseNames[0]);
            
            // Use a different database if available
            string otherDb = init.testDatabaseNames.Count > 1 ? init.testDatabaseNames[1] : init.testDatabaseNames[0];
            ConnectionData toBeUpdated = init.CreateConnectionData(otherDb);

            DatabaseRunHistory actual = target.GetDatabaseHistoryDifference(goldenCopy, toBeUpdated);

            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.BuildFileHistory);
            // Differences depend on what's been run on each database
        }

        [TestMethod(), Ignore("Don't have the setup scripts ready yet")]
        public void GetDatabaseHistoryDifferenceTest()
        {
            DatabaseDiffer target = new DatabaseDiffer();
            ConnectionData goldenCopy = new ConnectionData()
            {
                DatabaseName = "SqlBuildTest_SyncTest1",
                SQLServerName = @"localhost\SQLEXPRESS",
                AuthenticationType = AuthenticationType.Windows
            };
            ConnectionData toBeUpdated = new ConnectionData()
            {
                DatabaseName = "SqlBuildTest_SyncTest2",
                SQLServerName = @"localhost\SQLEXPRESS",
                AuthenticationType = AuthenticationType.Windows
            };
            DatabaseRunHistory actual;
            actual = target.GetDatabaseHistoryDifference(goldenCopy, toBeUpdated);
            Assert.AreEqual(3, actual.BuildFileHistory.Count);

            //check that they are in chron order
            Assert.IsTrue(actual.BuildFileHistory[0].CommitDate < actual.BuildFileHistory[1].CommitDate);
            Assert.IsTrue(actual.BuildFileHistory[1].CommitDate < actual.BuildFileHistory[2].CommitDate);
            Assert.AreEqual("7E704479328C9B2FDA48C7CF093B16F125EAB98A", actual.BuildFileHistory[2].BuildFileHash);
        }

        [TestMethod(), Ignore("Don't have the setup scripts ready yet")]
        public void GetDatabaseHistoryDifferenceTest_PartialSync()
        {
            DatabaseDiffer target = new DatabaseDiffer();
            ConnectionData goldenCopy = new ConnectionData()
            {
                DatabaseName = "SqlBuildTest_SyncTest1",
                SQLServerName = @"localhost\SQLEXPRESS",
                AuthenticationType = AuthenticationType.Windows
            };
            ConnectionData toBeUpdated = new ConnectionData()
            {
                DatabaseName = "SqlBuildTest_SyncTest3",
                SQLServerName = @"localhost\SQLEXPRESS",
                AuthenticationType = AuthenticationType.Windows
            };
            DatabaseRunHistory actual;
            actual = target.GetDatabaseHistoryDifference(goldenCopy, toBeUpdated);
            Assert.AreEqual(1, actual.BuildFileHistory.Count);

            //check that they are in chron order
            Assert.AreEqual("7E704479328C9B2FDA48C7CF093B16F125EAB98A", actual.BuildFileHistory[0].BuildFileHash);
        }

        [TestMethod(), Ignore("Don't have the setup scripts ready yet")]
        public void GetDatabaseRunHistoryTest_TestCountAndSelection()
        {
            DatabaseDiffer target = new DatabaseDiffer();
            ConnectionData dbConnData = new ConnectionData()
            {
                DatabaseName = "SqlBuildTest_SyncTest1",
                SQLServerName = @"localhost\SQLEXPRESS",
                AuthenticationType = AuthenticationType.Windows
            };

            DatabaseRunHistory actual;
            actual = target.GetDatabaseRunHistory(dbConnData);
            Assert.AreEqual(3, actual.BuildFileHistory.Count);

            Assert.AreEqual(DateTime.Parse("2014-07-21 13:58:07.880"),
                actual.BuildFileHistory.Where(x => x.BuildFileHash == "7651E282160CAF9C92CB923004D94B91181C077E").Select(y => y.CommitDate).FirstOrDefault());
        }

        #endregion

        #region BuildFileHistory Tests

        [TestMethod]
        public void BuildFileHistory_Properties_SetCorrectly()
        {
            var now = DateTime.Now;
            var history = new BuildFileHistory
            {
                BuildFileHash = "ABC123",
                BuildFileName = "Test.sbm",
                CommitDate = now
            };

            Assert.AreEqual("ABC123", history.BuildFileHash);
            Assert.AreEqual("Test.sbm", history.BuildFileName);
            Assert.AreEqual(now, history.CommitDate);
        }

        #endregion

        #region DatabaseRunHistory Tests

        [TestMethod]
        public void DatabaseRunHistory_Constructor_InitializesEmptyList()
        {
            var history = new DatabaseRunHistory();

            Assert.IsNotNull(history.BuildFileHistory);
            Assert.AreEqual(0, history.BuildFileHistory.Count);
        }

        [TestMethod]
        public void DatabaseRunHistory_AddRange_Works()
        {
            var history = new DatabaseRunHistory();
            var items = new List<BuildFileHistory>
            {
                new BuildFileHistory { BuildFileName = "Build1.sbm" },
                new BuildFileHistory { BuildFileName = "Build2.sbm" }
            };

            history.BuildFileHistory.AddRange(items);

            Assert.AreEqual(2, history.BuildFileHistory.Count);
        }

        #endregion
    }
}
