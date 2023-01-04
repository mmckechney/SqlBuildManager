using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.Connection;
using System.Collections.Generic;
namespace SqlSync.SqlBuild.UnitTest
{


    /// <summary>
    ///This is a test class for OverrideDataTest and is intended
    ///to contain all OverrideDataTest Unit Tests
    ///</summary>
    [TestClass()]
    public class OverrideDataTest
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
        ///A test for TargetDatabaseOverrides
        ///</summary>
        [TestMethod()]
        public void TargetDatabaseOverridesTest()
        {
            List<DatabaseOverride> expected = new List<DatabaseOverride>();
            expected.Add(new DatabaseOverride("default1", "override1"));
            expected.Add(new DatabaseOverride("default2", "override2"));
            List<DatabaseOverride> actual;
            OverrideData.TargetDatabaseOverrides = expected;
            actual = OverrideData.TargetDatabaseOverrides;
            Assert.AreEqual(expected, actual);
        }



        /// <summary>
        ///A test for OverrideData Constructor
        ///</summary>
        [TestMethod()]
        public void OverrideDataConstructorTest()
        {
            OverrideData target = new OverrideData();
            Assert.AreEqual(typeof(OverrideData), target.GetType());
        }
    }
}
