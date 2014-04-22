using SqlSync.SqlBuild;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace SqlSync.SqlBuild.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for ScriptBatchTest and is intended
    ///to contain all ScriptBatchTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ScriptBatchTest
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
        ///A test for ScriptId
        ///</summary>
        [TestMethod()]
        public void ScriptBatchTest_All()
        {
            string scriptfileName = "MyFileName";
            string[] scriptBatchContents = new string[] { "batch1", "batch2", "batch3" };
            string scriptId = System.Guid.NewGuid().ToString();
            ScriptBatch target = new ScriptBatch(scriptfileName, scriptBatchContents, scriptId);
            Assert.AreEqual(scriptfileName, target.ScriptfileName);
            Assert.AreEqual(scriptBatchContents, target.ScriptBatchContents);
            Assert.AreEqual(scriptId, target.ScriptId);

            target.ScriptfileName = "ChangedName";
            string[] newBatch = new string[] { "batch4", "batch5" };
            target.ScriptBatchContents = newBatch;
            string newId = System.Guid.NewGuid().ToString();
            target.ScriptId = newId;
            Assert.AreEqual("ChangedName", target.ScriptfileName);
            Assert.AreEqual(newBatch, target.ScriptBatchContents);
            Assert.AreEqual(newId, target.ScriptId);
        }
    }
}
