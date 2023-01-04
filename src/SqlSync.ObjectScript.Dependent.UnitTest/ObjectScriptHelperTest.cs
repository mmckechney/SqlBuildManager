using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.Connection;
using SqlSync.ObjectScript.Hash;

namespace SqlSync.ObjectScript.Dependent.UnitTest
{


    /// <summary>
    ///This is a test class for ObjectScriptHelperTest and is intended
    ///to contain all ObjectScriptHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ObjectScriptHelperTest
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
        ///A test for GetDatabaseObjectHashes
        ///</summary>
        [TestMethod()]
        public void GetDatabaseObjectHashesTest()
        {
            Initialization init = new Initialization();
            ConnectionData data = init.connData;
            ObjectScriptHelper target = new ObjectScriptHelper(data);
            ObjectScriptHashData actual;
            actual = target.GetDatabaseObjectHashes();
            Assert.IsNotNull(actual);
            Assert.AreEqual("C9D84C93D15E8D9ADF4F78BF8B97C051", actual.Tables["dbo.TransactionTest"].HashValue);
            Assert.AreEqual("Added", actual.Tables["dbo.TransactionTest"].ComparisonValue);
        }

    }
}
