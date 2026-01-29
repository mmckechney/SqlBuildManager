using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.Connection;
using SqlSync.SqlBuild.Models;
using System.Collections.Generic;

#nullable enable
namespace SqlSync.SqlBuild.UnitTest
{


    /// <summary>
    ///This is a test class for OverrideDataTest and is intended
    ///to contain all OverrideDataTest Unit Tests
    ///</summary>
    [TestClass()]
    public class OverrideDataTest
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
        ///A test for TargetDatabaseOverrides
        ///</summary>
        [TestMethod()]
        public void TargetDatabaseOverridesTest()
        {
            List<DatabaseOverride> expected = new List<DatabaseOverride>();
            expected.Add(new DatabaseOverride("server1", "default1", "override1"));
            expected.Add(new DatabaseOverride("server1", "default2", "override2"));
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
