using SqlSync.SqlBuild;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace SqlSync.SqlBuild.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for ClearScriptDataTest and is intended
    ///to contain all ClearScriptDataTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ClearScriptDataTest
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
        ///A test for ClearScriptData Constructor
        ///</summary>
        [TestMethod()]
        public void ClearScriptDataConstructorTest()
        {
            string[] selectedScriptIds = new string[] { "1", "3", "5" };
            SqlSyncBuildData buildData = new SqlSyncBuildData();
            string projectFileName = "MyProjectFile";
            string buildZipFileName = "MyZipFileName.sbm";
            ClearScriptData target = new ClearScriptData(selectedScriptIds, buildData, projectFileName, buildZipFileName);
            Assert.AreEqual(selectedScriptIds, target.SelectedScriptIds);
            Assert.AreEqual(buildData, target.BuildData);
            Assert.AreEqual(projectFileName, target.ProjectFileName);
            Assert.AreEqual(buildZipFileName, target.BuildZipFileName);
        }
    }
}
