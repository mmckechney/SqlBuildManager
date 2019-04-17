using SqlSync.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data.SqlClient;
using System;

namespace SqlSync.Connection.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for ConnectionHelperTest and is intended
    ///to contain all ConnectionHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ConnectionHelperTest
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

        private static string appNameString;
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            var x = new ConnectionHelper();
            appNameString = ConnectionHelper.appName;
        }
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
        ///A test for GetConnectionString
        ///</summary>
        [TestMethod()]
        public void GetConnectionStringTest_FromConnectionDataObj()
        {
            ConnectionData connData = new ConnectionData("myserver", "mydatabase");
            string expected = String.Format($"Data Source=myserver;Initial Catalog=mydatabase;Integrated Security=True;Pooling=False;Connect Timeout=20;Application Name=\"{appNameString}\";ConnectRetryCount=3;ConnectRetryInterval=10");
            string actual;
            actual = ConnectionHelper.GetConnectionString(connData);
            Assert.AreEqual(expected, actual);

        }
        /// <summary>
        ///A test for GetConnectionString
        ///</summary>
        [TestMethod()]
        public void GetConnectionStringTest_FromConnectionDataObj_WithTimeout()
        {
            ConnectionData connData = new ConnectionData("myserver", "mydatabase");
            connData.ScriptTimeout = 40;
            string expected = String.Format($"Data Source=myserver;Initial Catalog=mydatabase;Integrated Security=True;Pooling=False;Connect Timeout=40;Application Name=\"{appNameString}\";ConnectRetryCount=3;ConnectRetryInterval=10");
            string actual;
            actual = ConnectionHelper.GetConnectionString(connData);
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        ///A test for GetConnectionString
        ///</summary>
        [TestMethod()]
        public void GetConnectionStringTest_FromConnectionDataObj_WithPassword()
        {
            ConnectionData connData = new ConnectionData("myserver", "mydatabase");
            connData.AuthenticationType = AuthenticationType.Password;
            connData.UserId = "User";
            connData.Password = "Password";
            string expected = String.Format($"Data Source=myserver;Initial Catalog=mydatabase;User ID=User;Password=Password;Pooling=False;Connect Timeout=20;Authentication=\"Sql Password\";Application Name=\"{appNameString}\";ConnectRetryCount=3;ConnectRetryInterval=10");
            string actual;
            actual = ConnectionHelper.GetConnectionString(connData);
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        ///A test for GetConnectionString
        ///</summary>
        [TestMethod()]
        public void GetConnectionStringTest_FromConnectionDataObj_WithPassordAndTimeout()
        {
            ConnectionData connData = new ConnectionData("myserver", "mydatabase");
            connData.AuthenticationType = AuthenticationType.Password;
            connData.UserId = "User";
            connData.Password = "Password";
            connData.ScriptTimeout = 30;
            string expected = string.Format($"Data Source=myserver;Initial Catalog=mydatabase;User ID=User;Password=Password;Pooling=False;Connect Timeout=30;Authentication=\"Sql Password\";Application Name=\"{appNameString}\";ConnectRetryCount=3;ConnectRetryInterval=10");
            string actual = ConnectionHelper.GetConnectionString(connData);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetConnectionString
        ///</summary>
        [TestMethod()]
        public void GetConnectionStringTest_FromExplicitValues()
        {
            string dbName = "mydatabase";
            string serverName = "myserver";
            string uid = "userid";
            string pw = "password";
            int scriptTimeOut = 100;
            string expected = string.Format($"Data Source=myserver;Initial Catalog=mydatabase;User ID=userid;Password=password;Pooling=False;Connect Timeout=100;Authentication=\"Sql Password\";Application Name=\"{appNameString}\";ConnectRetryCount=3;ConnectRetryInterval=10");
            string actual;
            actual = ConnectionHelper.GetConnectionString(dbName, serverName, uid, pw, AuthenticationType.Password, scriptTimeOut);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetConnection
        ///</summary>
        [TestMethod()]
        public void GetConnectionTest_WithExplicitValues()
        {
            string dbName = "mydatabase";
            string serverName = "myserver";
            string uid = "userid";
            string pw = "password";
            int scriptTimeOut = 100;
            string expected = string.Format($"Data Source=myserver;Initial Catalog=mydatabase;User ID=userid;Password=password;Pooling=False;Connect Timeout=100;Authentication=\"Sql Password\";Application Name=\"{appNameString}\";ConnectRetryCount=3;ConnectRetryInterval=10");
            SqlConnection actual;
            actual = ConnectionHelper.GetConnection(dbName, serverName, uid, pw, AuthenticationType.Password, scriptTimeOut);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual.ConnectionString);
            Assert.AreEqual(System.Data.ConnectionState.Closed, actual.State);
            Assert.AreEqual("myserver",actual.DataSource);
           
        }

        /// <summary>
        ///A test for GetConnection
        ///</summary>
        [TestMethod()]
        public void GetConnectionTest_FromConnectionDataObj()
        {
            ConnectionData connData = new ConnectionData("myserver", "mydatabase");
            string expected = string.Format($"Data Source=myserver;Initial Catalog=mydatabase;Integrated Security=True;Pooling=False;Connect Timeout=20;Application Name=\"{appNameString}\";ConnectRetryCount=3;ConnectRetryInterval=10");
            SqlConnection actual;
            actual = ConnectionHelper.GetConnection(connData);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual.ConnectionString);
            Assert.AreEqual(System.Data.ConnectionState.Closed, actual.State);
            Assert.AreEqual("myserver", actual.DataSource);

        }

        /// <summary>
        ///A test for GetTargetDatabase
        ///</summary>
        [TestMethod()]
        public void GetTargetDatabaseTest()
        {
            List<DatabaseOverride> overrides = new List<DatabaseOverride>();
            overrides.Add(new DatabaseOverride("default1", "override1"));
            overrides.Add(new DatabaseOverride("default2", "override2"));
            overrides.Add(new DatabaseOverride("default4", "default4"));
            overrides.Add(new DatabaseOverride("MixedCASE", "override5"));

            string actual = ConnectionHelper.GetTargetDatabase("default2", overrides);
            Assert.AreEqual("override2", actual);

            actual = ConnectionHelper.GetTargetDatabase("default1", overrides);
            Assert.AreEqual("override1", actual);

            actual = ConnectionHelper.GetTargetDatabase("default4", overrides);
            Assert.AreEqual("default4", actual);

        }
        /// <summary>
        ///A test for GetTargetDatabase
        ///</summary>
        [TestMethod()]
        public void GetTargetDatabaseTest_EmptyDefault()
        {
            List<DatabaseOverride> overrides = new List<DatabaseOverride>();
            overrides.Add(new DatabaseOverride("default1", "override1"));
            overrides.Add(new DatabaseOverride("default2", "override2"));

            string actual = ConnectionHelper.GetTargetDatabase("", overrides);
            Assert.AreEqual("override1", actual);

           
        }
        /// <summary>
        ///A test for GetTargetDatabase
        ///</summary>
        [TestMethod()]
        public void GetTargetDatabaseTest_NullDefault()
        {
            List<DatabaseOverride> overrides = new List<DatabaseOverride>();
            overrides.Add(new DatabaseOverride("default1", "override1"));
            overrides.Add(new DatabaseOverride("default2", "override2"));

            string actual = ConnectionHelper.GetTargetDatabase(null, overrides);
            Assert.AreEqual("override1", actual);


        }
        /// <summary>
        ///A test for GetTargetDatabase
        ///</summary>
        [TestMethod()]
        public void GetTargetDatabaseTest_NoOverrideFound()
        {
            List<DatabaseOverride> overrides = new List<DatabaseOverride>();
            overrides.Add(new DatabaseOverride("default1", "override1"));
            overrides.Add(new DatabaseOverride("default2", "override2"));

            string actual = ConnectionHelper.GetTargetDatabase("default3", overrides);
            Assert.AreEqual("default3", actual);


        }
        /// <summary>
        ///A test for GetTargetDatabase
        ///</summary>
        [TestMethod()]
        public void GetTargetDatabaseTest_CaseInsensitive()
        {
            List<DatabaseOverride> overrides = new List<DatabaseOverride>();
            overrides.Add(new DatabaseOverride("default1", "override1"));
            overrides.Add(new DatabaseOverride("default2", "override2"));
            overrides.Add(new DatabaseOverride("default4", "default4"));
            overrides.Add(new DatabaseOverride("MixedCASE", "override5"));

            string actual = ConnectionHelper.GetTargetDatabase("mixedCaSe", overrides);
            Assert.AreEqual("override5", actual);


        }

        /// <summary>
        ///A test for ValidateDatabaseOverrides
        ///</summary>
        [TestMethod()]
        public void ValidateDatabaseOverridesTest_Good()
        {
            List<DatabaseOverride> overrides = new List<DatabaseOverride>();
            overrides.Add(new DatabaseOverride("default1", "override1"));
            overrides.Add(new DatabaseOverride("default2", "override2"));
            overrides.Add(new DatabaseOverride("default4", "default4"));
            overrides.Add(new DatabaseOverride("MixedCASE", "override5"));

            bool expected = true; 
            bool actual;
            actual = ConnectionHelper.ValidateDatabaseOverrides(overrides);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ValidateDatabaseOverrides
        ///</summary>
        [TestMethod()]
        public void ValidateDatabaseOverridesTest_MissingSetting()
        {
            List<DatabaseOverride> overrides = new List<DatabaseOverride>();
            overrides.Add(new DatabaseOverride("default1", "override1"));
            overrides.Add(new DatabaseOverride("default2", "override2"));
            overrides.Add(new DatabaseOverride("", ""));
            overrides.Add(new DatabaseOverride("MixedCASE", "override5"));

            bool expected = false;
            bool actual;
            actual = ConnectionHelper.ValidateDatabaseOverrides(overrides);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ValidateDatabaseOverrides
        ///</summary>
        [TestMethod()]
        public void ValidateDatabaseOverridesTest_NullList()
        {
            List<DatabaseOverride> overrides = null;
            bool expected = false;
            bool actual;
            actual = ConnectionHelper.ValidateDatabaseOverrides(overrides);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ValidateDatabaseOverrides
        ///</summary>
        [TestMethod()]
        public void ValidateDatabaseOverridesTest_NullOverride()
        {
            List<DatabaseOverride> overrides = new List<DatabaseOverride>();
            overrides.Add(new DatabaseOverride("default1", "override1"));
            overrides.Add(new DatabaseOverride("default2", "override2"));
            overrides.Add(null);
            overrides.Add(new DatabaseOverride("MixedCASE", "override5"));

            bool expected = false;
            bool actual;
            actual = ConnectionHelper.ValidateDatabaseOverrides(overrides);
            Assert.AreEqual(expected, actual);
        }

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
            actual = ConnectionHelper.TestDatabaseConnection(dbName, serverName,"","",AuthenticationType.Windows, scriptTimeOut);
            Assert.AreEqual(expected, actual, "NOTE: If this test fails, please make sure you have localhost\\SQLEXPRESS instance running.");
        }
        /// <summary>
        ///A test for TestDatabaseConnection
        ///</summary>
        [TestMethod()]
        public void TestDatabaseConnectionTest_StringOverrideFailure()
        {
            string dbName = "SqlBuildTest";
            string serverName = "BadServerName";
            int scriptTimeOut = 20;
            bool expected = false;
            bool actual;
            actual = ConnectionHelper.TestDatabaseConnection(dbName, serverName, "", "", AuthenticationType.Windows, scriptTimeOut);
            Assert.AreEqual(expected, actual);
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
        /// <summary>
        ///A test for TestDatabaseConnection
        ///</summary>
        [TestMethod()]
        public void TestDatabaseConnectionTest1_ConnDataOverrideFailure()
        {
            ConnectionData connData = new ConnectionData()
            {
                DatabaseName = "BadDatabaseNAme",
                ScriptTimeout = 20,
                SQLServerName = "localhost\\SQLEXPRESS",
                AuthenticationType = AuthenticationType.Windows
            };
            bool expected = false;
            bool actual;
            actual = ConnectionHelper.TestDatabaseConnection(connData);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ConnectionHelper Constructor
        ///</summary>
        [TestMethod()]
        public void ConnectionHelperConstructorTest()
        {
            ConnectionHelper target = new ConnectionHelper();
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(ConnectionHelper));
        }
    }
}
