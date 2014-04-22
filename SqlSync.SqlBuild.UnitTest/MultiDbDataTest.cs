using SqlSync.SqlBuild.MultiDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild;

namespace SqlSync.SqlBuild.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for MultiDbDataTest and is intended
    ///to contain all MultiDbDataTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MultiDbDataTest
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
        ///A test for RunAsTrial
        ///</summary>
        [TestMethod()]
        public void RunAsTrialTest()
        {
            MultiDbData target = new MultiDbData(); 
            bool expected = true; 
            bool actual;
            target.RunAsTrial = expected;
            actual = target.RunAsTrial;
            Assert.AreEqual(expected, actual);

            target.RunAsTrial = false;
            actual = target.RunAsTrial;
            Assert.AreEqual(false, actual);
        }

        /// <summary>
        ///A test for ProjectFileName
        ///</summary>
        [TestMethod()]
        public void ProjectFileNameTest()
        {
            MultiDbData target = new MultiDbData();
            string expected = "MyNewProjectName";
            string actual;
            target.ProjectFileName = expected;
            actual = target.ProjectFileName;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for MultiRunId
        ///</summary>
        [TestMethod()]
        public void MultiRunIdTest()
        {
            MultiDbData target = new MultiDbData();
            string expected = System.Guid.NewGuid().ToString();
            string actual;
            target.MultiRunId = expected;
            actual = target.MultiRunId;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsTransactional
        ///</summary>
        [TestMethod()]
        public void IsTransactionalTest()
        {
            MultiDbData target = new MultiDbData(); 
            bool expected = false; 
            bool actual;
            target.IsTransactional = expected;
            actual = target.IsTransactional;
            Assert.AreEqual(expected, actual);

            target.IsTransactional = true;
            actual = target.IsTransactional;
            Assert.AreEqual(true, actual);
        }

        /// <summary>
        ///A test for BuildFileName
        ///</summary>
        [TestMethod()]
        public void BuildFileNameTest()
        {
            MultiDbData target = new MultiDbData(); 
            string expected = "MyNewBuildFileName"; 
            string actual;
            target.BuildFileName = expected;
            actual = target.BuildFileName;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for BuildData
        ///</summary>
        [TestMethod()]
        public void BuildDataTest()
        {
            MultiDbData target = new MultiDbData(); // TODO: Initialize to an appropriate value
            SqlSyncBuildData expected = new SqlSyncBuildData();
            SqlSyncBuildData actual;
            target.BuildData = expected;
            actual = target.BuildData;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for MultiDbData Constructor
        ///</summary>
        [TestMethod()]
        public void MultiDbDataConstructorTest()
        {
            MultiDbData target = new MultiDbData();
            Assert.AreEqual(0, target.Count);
        }
    }
}
