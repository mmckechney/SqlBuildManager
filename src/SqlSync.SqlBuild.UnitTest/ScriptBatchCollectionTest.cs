using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace SqlSync.SqlBuild.UnitTest
{


    /// <summary>
    ///This is a test class for ScriptBatchCollectionTest and is intended
    ///to contain all ScriptBatchCollectionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ScriptBatchCollectionTest
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
        ///A test for GetScriptBatch
        ///</summary>
        [TestMethod()]
        public void GetScriptBatchTest()
        {
            string scriptfileName = "MyFileName";
            string[] scriptBatchContents = new string[] { "batch1", "batch2", "batch3" };
            string scriptId = System.Guid.NewGuid().ToString();
            ScriptBatch batch1 = new ScriptBatch(scriptfileName, scriptBatchContents, scriptId);

            string scriptfileName2 = "Batch2Name";
            string[] scriptBatchContents2 = new string[] { "batch4", "batch5", "batch5" };
            string scriptId2 = System.Guid.NewGuid().ToString();
            ScriptBatch batch2 = new ScriptBatch(scriptfileName2, scriptBatchContents2, scriptId2);

            ScriptBatchCollection target = new ScriptBatchCollection();
            target.Add(batch1);
            target.Add(batch2);

            ScriptBatch actual;
            actual = target.GetScriptBatch(scriptId);
            Assert.AreEqual(batch1, actual);
            actual = target.GetScriptBatch(scriptId2);
            Assert.AreEqual(batch2, actual);
            actual = target.GetScriptBatch("Can'tFindMe");
            Assert.IsNull(actual);
        }

        /// <summary>
        ///A test for ScriptBatchCollection Constructor
        ///</summary>
        [TestMethod()]
        public void ScriptBatchCollectionConstructorTest()
        {
            ScriptBatchCollection target = new ScriptBatchCollection();
            Assert.IsInstanceOfType(target, typeof(ScriptBatchCollection));
        }
    }
}
