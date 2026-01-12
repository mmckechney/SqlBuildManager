using Microsoft.VisualStudio.TestTools.UnitTesting;

#nullable enable
namespace SqlSync.SqlBuild.UnitTest
{


    /// <summary>
    ///This is a test class for ClearScriptDataTest and is intended
    ///to contain all ClearScriptDataTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ClearScriptDataTest
    {


        public TestContext TestContext { get; set; } = null!;

        

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
            var buildDataModel = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            string projectFileName = "MyProjectFile";
            string buildZipFileName = "MyZipFileName.sbm";
            ClearScriptData target = new ClearScriptData(selectedScriptIds, buildDataModel, projectFileName, buildZipFileName);
            Assert.AreEqual(selectedScriptIds, target.SelectedScriptIds);
            Assert.AreEqual(buildDataModel, target.BuildDataModel);
            Assert.AreEqual(projectFileName, target.ProjectFileName);
            Assert.AreEqual(buildZipFileName, target.BuildZipFileName);
        }
    }
}
