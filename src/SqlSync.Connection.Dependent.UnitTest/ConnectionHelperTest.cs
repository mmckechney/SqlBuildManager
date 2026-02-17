using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SqlSync.Connection.Dependent.UnitTest
{
    /// <summary>
    /// Summary description for ConnectionHelperTest
    /// </summary>
    [TestClass]
    public class ConnectionHelperTest
    {
        private static string TestServerName => Environment.GetEnvironmentVariable("SBM_TEST_SQL_SERVER") ?? @"localhost\SQLEXPRESS";
        private static string TestSqlUser => Environment.GetEnvironmentVariable("SBM_TEST_SQL_USER");
        private static string TestSqlPassword => Environment.GetEnvironmentVariable("SBM_TEST_SQL_PASSWORD");
        private static AuthenticationType TestAuthType => string.IsNullOrWhiteSpace(TestSqlUser) ? AuthenticationType.Windows : AuthenticationType.Password;

        public ConnectionHelperTest()
        {

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
