using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Test.Common;
using System;

namespace SqlSync.Connection.Dependent.UnitTest
{
    /// <summary>
    /// Summary description for ConnectionHelperTest
    /// </summary>
    [TestClass]
    public class ConnectionHelperTest
    {
        private static string TestServerName => TestEnvironment.SqlServer;
        private static string TestSqlUser => TestEnvironment.SqlUser;
        private static string TestSqlPassword => TestEnvironment.SqlPassword;
        private static AuthenticationType TestAuthType => TestEnvironment.UseSqlAuth ? AuthenticationType.Password : AuthenticationType.Windows;

        public ConnectionHelperTest()
        {

        }

        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        /// <summary>
        ///A test for TestDatabaseConnection
        ///</summary>
        [TestMethod()]
        public void TestDatabaseConnectionTest_StringOverrideSuccess()
        {
            string dbName = "SqlBuildTest";
            string serverName = TestServerName;
            int scriptTimeOut = 20;
            bool expected = true;
            bool actual;
            actual = ConnectionHelper.TestDatabaseConnection(dbName, serverName, TestSqlUser ?? "", TestSqlPassword ?? "", TestAuthType, scriptTimeOut,"");
            Assert.AreEqual(expected, actual, $"NOTE: If this test fails, please make sure you have {serverName} instance running.");
        }

        /// <summary>
        ///A test for TestDatabaseConnection
        ///</summary>
        [TestMethod()]
        public void TestDatabaseConnectionTest1_ConnDataOverrideSuccess()
        {
            ConnectionData connData = new ConnectionData()
            {
                DatabaseName = "SqlBuildTest",
                ScriptTimeout = 20,
                SQLServerName = TestServerName,
                AuthenticationType = TestAuthType,
                UserId = TestSqlUser ?? "",
                Password = TestSqlPassword ?? ""
            };
            bool expected = true;
            bool actual;
            actual = ConnectionHelper.TestDatabaseConnection(connData);
            Assert.AreEqual(expected, actual, $"NOTE: If this test fails, please make sure you have {TestServerName} instance running.");
        }
    }
}
