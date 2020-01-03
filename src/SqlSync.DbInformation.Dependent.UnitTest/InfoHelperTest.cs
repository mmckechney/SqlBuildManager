using SqlSync.DbInformation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.Connection;

namespace SqlSync.DbInformation.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for InfoHelperTest and is intended
    ///to contain all InfoHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class InfoHelperTest
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
        ///A test for GetColumnNames
        ///</summary>
        [TestMethod()]
        public void GetColumnNamesTest()
        {
            Initialization init = new Initialization();
            string tableName = "SqlBuild_Logging";
            ConnectionData connData = init.connData;
            string[] actual;
            actual = InfoHelper.GetColumnNames(tableName, connData);
            Assert.AreEqual(18, actual.Length);
            Assert.AreEqual("BuildFileName", actual[0]);
            Assert.AreEqual("TargetDatabase", actual[11]);
            Assert.AreEqual("BuildRequestedBy", actual[14]);
        }
    }
}
