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


        public TestContext TestContext { get; set; }

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
            Assert.IsTrue(actual.Tables.ContainsKey("dbo.TransactionTest"), "TransactionTest table not found in hash data");
            Assert.IsFalse(string.IsNullOrWhiteSpace(actual.Tables["dbo.TransactionTest"].HashValue), "TransactionTest hash should not be empty");
            Assert.AreEqual(32, actual.Tables["dbo.TransactionTest"].HashValue.Length, "Hash should be a 32-character MD5 hex string");
            Assert.AreEqual("Added", actual.Tables["dbo.TransactionTest"].ComparisonValue);
        }

    }
}
