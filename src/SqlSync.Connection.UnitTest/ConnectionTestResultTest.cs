using SqlSync.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SqlSync.Connection.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for ConnectionTestResultTest and is intended
    ///to contain all ConnectionTestResultTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ConnectionTestResultTest
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
        ///A test for ConnectionTestResult Constructor
        ///</summary>
        [TestMethod()]
        public void ConnectionTestResultConstructorTest()
        {
            ConnectionTestResult target = new ConnectionTestResult();
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(ConnectionTestResult));
        }

        /// <summary>
        ///A test for DatabaseName
        ///</summary>
        [TestMethod()]
        public void DatabaseNameTest()
        {
            ConnectionTestResult target = new ConnectionTestResult();
            string expected = "MyDatabaseName";
            string actual;
            target.DatabaseName = expected;
            actual = target.DatabaseName;
            Assert.AreEqual(expected, actual);
         }

        /// <summary>
        ///A test for ServerName
        ///</summary>
        [TestMethod()]
        public void ServerNameTest()
        {
            ConnectionTestResult target = new ConnectionTestResult();
            string expected = "TheServerName";
            string actual;
            target.ServerName = expected;
            actual = target.ServerName;
            Assert.AreEqual(expected, actual);
            
        }

        /// <summary>
        ///A test for Successful
        ///</summary>
        [TestMethod()]
        public void SuccessfulTest()
        {
            ConnectionTestResult target = new ConnectionTestResult(); 
            bool expected = false; 
            bool actual;
            target.Successful = expected;
            actual = target.Successful;
            Assert.AreEqual(expected, actual);
            
        }
    }
}
