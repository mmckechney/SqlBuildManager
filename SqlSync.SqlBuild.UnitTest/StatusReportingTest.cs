using SqlSync.SqlBuild.Status;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.MultiDb;

namespace SqlSync.SqlBuild.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for StatusReportingTest and is intended
    ///to contain all StatusReportingTest Unit Tests
    ///</summary>
    [TestClass()]
    public class StatusReportingTest
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


        ///// <summary>
        /////A test for GetScriptStatus
        /////</summary>
        //[TestMethod()]
        //public void GetScriptStatusTest()
        //{
        //    Initialization init = new Initialization();

        //    SqlSyncBuildData buildData = null; // TODO: Initialize to an appropriate value
        //    MultiDbData multiDbData = null; // TODO: Initialize to an appropriate value
        //    string projectFilePath = string.Empty; // TODO: Initialize to an appropriate value
        //    StatusReporting target = new StatusReporting(buildData, multiDbData, projectFilePath); // TODO: Initialize to an appropriate value
        //    target.GetScriptStatus();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}
    }
}
