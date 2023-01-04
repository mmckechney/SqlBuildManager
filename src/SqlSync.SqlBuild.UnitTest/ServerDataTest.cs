using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.MultiDb;

namespace SqlSync.SqlBuild.UnitTest
{


    /// <summary>
    ///This is a test class for ServerDataTest and is intended
    ///to contain all ServerDataTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ServerDataTest
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
        ///A test for ServerName
        ///</summary>
        [TestMethod()]
        public void ServerNameTest()
        {
            ServerData target = new ServerData();
            string expected = "MyServerName";
            string actual;
            target.ServerName = expected;
            actual = target.ServerName;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for OverrideSequence
        ///</summary>
        [TestMethod()]
        public void OverrideSequenceTest()
        {
            ServerData target = new ServerData();
            DbOverrides expected = new DbOverrides();
            DbOverrides actual;
            target.Overrides = expected;
            actual = target.Overrides;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ServerData Constructor
        ///</summary>
        [TestMethod()]
        public void ServerDataConstructorTest()
        {
            ServerData target = new ServerData();
            Assert.AreEqual(typeof(ServerData), target.GetType());
            Assert.IsTrue(target != null, "Error. ServerData object is null");
        }
    }
}
