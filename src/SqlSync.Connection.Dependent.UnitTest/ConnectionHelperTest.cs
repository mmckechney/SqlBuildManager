using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlSync.Connection.Dependent.UnitTest
{
    /// <summary>
    /// Summary description for ConnectionHelperTest
    /// </summary>
    [TestClass]
    public class ConnectionHelperTest
    {
        public ConnectionHelperTest()
        {
            //
            // TODO: Add constructor logic here
            //
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
            string serverName = "localhost\\SQLEXPRESS";
            int scriptTimeOut = 20;
            bool expected = true;
            bool actual;
            actual = ConnectionHelper.TestDatabaseConnection(dbName, serverName, "", "", AuthenticationType.Windows, scriptTimeOut);
            Assert.AreEqual(expected, actual, "NOTE: If this test fails, please make sure you have localhost\\SQLEXPRESS instance running.");
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
                SQLServerName = "localhost\\SQLEXPRESS",
                AuthenticationType = AuthenticationType.Windows
            };
            bool expected = true;
            bool actual;
            actual = ConnectionHelper.TestDatabaseConnection(connData);
            Assert.AreEqual(expected, actual, "NOTE: If this test fails, please make sure you have localhost\\SQLEXPRESS instance running.");
        }
    }
}
