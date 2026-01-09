using Microsoft.VisualStudio.TestTools.UnitTesting;

#nullable enable

namespace SqlSync.SqlBuild.UnitTest
{


    /// <summary>
    ///This is a test class for StatusReportingTest and is intended
    ///to contain all StatusReportingTest Unit Tests
    ///</summary>
    [TestClass()]
    public class StatusReportingTest
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


        ///// <summary>
        /////A test for GetScriptStatus
        /////</summary>
        //[TestMethod()]
        //public void GetScriptStatusTest()
        //{
        //    Initialization init = new Initialization();

        //    SqlSyncBuildData buildData = null; 
        //    MultiDbData multiDbData = null; 
        //    string projectFilePath = string.Empty; 
        //    StatusReporting target = new StatusReporting(buildData, multiDbData, projectFilePath); 
        //    target.GetScriptStatus();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}
    }
}
