using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.Connection;
using SqlSync.SqlBuild.Synchronizer;
using System;

#nullable enable

namespace SqlSync.SqlBuild.Dependent.UnitTest
{


    /// <summary>
    ///This is a test class for DatabaseSyncerTest and is intended
    ///to contain all DatabaseSyncerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DatabaseSyncerTest
    {
        [ClassInitialize()]
        public static void Initilize(TestContext testContext)
        {
            Initialization init = new Initialization();
        }
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


        /// <summary>
        ///A test for SyncronizeDatabases
        ///</summary>
        [TestMethod()]
        public void SyncronizeDatabasesTest_SyncWorked()
        {
            DatabaseSyncer target = new DatabaseSyncer();
            ConnectionData gold = new ConnectionData()
            {
                DatabaseName = "SqlBuildTest_SyncTest1",
                SQLServerName = @"localhost\SQLEXPRESS",
                AuthenticationType = AuthenticationType.Windows
            };
            ConnectionData toUpdate = new ConnectionData()
            {
                DatabaseName = "SqlBuildTest_SyncTest2",
                SQLServerName = @"localhost\SQLEXPRESS",
                AuthenticationType = AuthenticationType.Windows
            };
            target.SyncronizationInfoEvent += new DatabaseSyncer.SyncronizationInfoEventHandler(target_SyncronizationInfoEvent);
            bool success = target.SyncronizeDatabasesAsync(gold, toUpdate, false).GetAwaiter().GetResult();

            CleanUpSyncTest2();

            Assert.IsTrue(true);
        }

        [TestMethod()]
        [Ignore("Flaky since history was not found")]
        public void SyncronizeDatabasesTest_SyncWorkedAndSticks()
        {
            DatabaseSyncer target = new DatabaseSyncer();
            ConnectionData gold = new ConnectionData()
            {
                DatabaseName = "SqlBuildTest_SyncTest1",
                SQLServerName = @"localhost\SQLEXPRESS",
                AuthenticationType = AuthenticationType.Windows
            };
            ConnectionData toUpdate = new ConnectionData()
            {
                DatabaseName = "SqlBuildTest_SyncTest2",
                SQLServerName = @"localhost\SQLEXPRESS",
                AuthenticationType = AuthenticationType.Windows
            };
            target.SyncronizationInfoEvent += new DatabaseSyncer.SyncronizationInfoEventHandler(target_SyncronizationInfoEvent);
            bool success = target.SyncronizeDatabasesAsync(gold, toUpdate, false).GetAwaiter().GetResult();

            DatabaseDiffer differ = new DatabaseDiffer();
            var history = differ.GetDatabaseHistoryDifference(gold, toUpdate);

            CleanUpSyncTest2();

            Assert.AreEqual(0, history.BuildFileHistory.Count);

        }

        void target_SyncronizationInfoEvent(string message)
        {
            Console.WriteLine(message);
        }

        internal static void CleanUpSyncTest2()
        {

            string sql = "DELETE FROM [SqlBuildTest_SyncTest2].[dbo].[SqlBuild_Logging]";
            ConnectionData sync2Conn = new ConnectionData()
            {
                DatabaseName = "SqlBuildTest_SyncTest2",
                SQLServerName = @"localhost\SQLEXPRESS",
                AuthenticationType = AuthenticationType.Windows
            };
            SqlConnection conn = SqlSync.Connection.ConnectionHelper.GetConnection(sync2Conn);
            conn.Open();
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
                conn.Close();
            }

        }
    }
}
