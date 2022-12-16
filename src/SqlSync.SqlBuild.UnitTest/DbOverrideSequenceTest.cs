using SqlSync.SqlBuild.MultiDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.DbInformation;
using System.Collections.Generic;
using SqlSync.Connection;
namespace SqlSync.SqlBuild.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for DbOverrideSequenceTest and is intended
    ///to contain all DbOverrideSequenceTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DbOverrideSequenceTest
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
        ///A test for Sort
        ///</summary>
        //[TestMethod()]
        //public void SortTest_Numeric()
        //{
        //    DbOverrides target = new DbOverrides(); 
        //    List<DatabaseOverride> tmp1 = new List<DatabaseOverride>();
        //    tmp1.Add(new DatabaseOverride("default1a","override1a"));
        //    tmp1.Add(new DatabaseOverride("default1b","override1b"));

        //    List<DatabaseOverride> tmp2 = new List<DatabaseOverride>();
        //    tmp2.Add(new DatabaseOverride("default2a", "override2a"));
        //    tmp2.Add(new DatabaseOverride("default2b", "override2b"));

        //    target.Add("2", tmp2);
        //    target.Add("1", tmp1);
        //    target.Add("4", tmp1);
        //    target.Add("3", tmp1);
        //    target.Sort();
        //    Dictionary<string,List<DatabaseOverride>>.Enumerator enumer = target.GetEnumerator();
        //    enumer.MoveNext();
        //    Assert.IsTrue("1" == enumer.Current.Key.ToString(), "Sorting of DbOverride sequence failed. Expected \"1\", returned " + enumer.Current.Key.ToString());
        //    enumer.MoveNext();
        //    Assert.IsTrue("2" == enumer.Current.Key.ToString(), "Sorting of DbOverride sequence failed. Expected \"2\", returned " + enumer.Current.Key.ToString());
        //    enumer.MoveNext();
        //    Assert.IsTrue("3" == enumer.Current.Key.ToString(), "Sorting of DbOverride sequence failed. Expected \"3\", returned " + enumer.Current.Key.ToString());
        //    enumer.MoveNext();
        //    Assert.IsTrue("4" == enumer.Current.Key.ToString(), "Sorting of DbOverride sequence failed. Expected \"4\", returned " + enumer.Current.Key.ToString());
     
        //}
        /// <summary>
        ///A test for Sort
        ///</summary>
        //[TestMethod()]
        //public void SortTest_Alpha()
        //{
        //    DbOverrides target = new DbOverrides();
        //    List<DatabaseOverride> tmp1 = new List<DatabaseOverride>();
        //    tmp1.Add(new DatabaseOverride("default1a", "override1a"));
        //    tmp1.Add(new DatabaseOverride("default1b", "override1b"));

        //    List<DatabaseOverride> tmp2 = new List<DatabaseOverride>();
        //    tmp2.Add(new DatabaseOverride("default2a", "override2a"));
        //    tmp2.Add(new DatabaseOverride("default2b", "override2b"));

        //    target.Add("z", tmp2);
        //    target.Add("a", tmp1);
        //    target.Add("m", tmp2);
        //    target.Sort();
        //    Dictionary<string, List<DatabaseOverride>>.Enumerator enumer = target.GetEnumerator();
        //    enumer.MoveNext();
        //    Assert.IsTrue("a" == enumer.Current.Key.ToString(), "Sorting of DbOverride sequence failed. Expected \"a\", returned " + enumer.Current.Key.ToString());
        //    enumer.MoveNext();
        //    Assert.IsTrue("m" == enumer.Current.Key.ToString(), "Sorting of DbOverride sequence failed. Expected \"m\", returned " + enumer.Current.Key.ToString());
        //    enumer.MoveNext();
        //    Assert.IsTrue("z" == enumer.Current.Key.ToString(), "Sorting of DbOverride sequence failed. Expected \"z\", returned " + enumer.Current.Key.ToString());

        //}
        /// <summary>
        ///A test for Sort
        ///</summary>
        //[TestMethod()]
        //public void SortTest_MixedAlphaNumeric()
        //{
        //    DbOverrides target = new DbOverrides();
        //    List<DatabaseOverride> tmp1 = new List<DatabaseOverride>();
        //    tmp1.Add(new DatabaseOverride("default1a", "override1a"));
        //    tmp1.Add(new DatabaseOverride("default1b", "override1b"));

        //    List<DatabaseOverride> tmp2 = new List<DatabaseOverride>();
        //    tmp2.Add(new DatabaseOverride("default2a", "override2a"));
        //    tmp2.Add(new DatabaseOverride("default2b", "override2b"));

        //    target.Add("z", tmp2);
        //    target.Add("2", tmp1);
        //    target.Add("m", tmp2);
        //    target.Add("1", tmp2);
        //    target.Add("3", tmp2);
        //    target.Sort();
        //    Dictionary<string, List<DatabaseOverride>>.Enumerator enumer = target.GetEnumerator();
        //    enumer.MoveNext();
        //    Assert.IsTrue("m" == enumer.Current.Key.ToString(), "Sorting of DbOverride sequence failed. Expected \"m\", returned " + enumer.Current.Key.ToString());
        //    enumer.MoveNext();
        //    Assert.IsTrue("z" == enumer.Current.Key.ToString(), "Sorting of DbOverride sequence failed. Expected \"z\", returned " + enumer.Current.Key.ToString());
        //    enumer.MoveNext();
        //    Assert.IsTrue("1" == enumer.Current.Key.ToString(), "Sorting of DbOverride sequence failed. Expected \"1\", returned " + enumer.Current.Key.ToString());
        //    enumer.MoveNext();
        //    Assert.IsTrue("2" == enumer.Current.Key.ToString(), "Sorting of DbOverride sequence failed. Expected \"2\", returned " + enumer.Current.Key.ToString());
        //    enumer.MoveNext();
        //    Assert.IsTrue("3" == enumer.Current.Key.ToString(), "Sorting of DbOverride sequence failed. Expected \"3\", returned " + enumer.Current.Key.ToString());

        //}

        /// <summary>
        ///A test for GetSequenceId
        ///</summary>
        //[TestMethod()]
        //public void GetSequenceIdTest()
        //{
        //    DbOverrides target = new DbOverrides();
        //    List<DatabaseOverride> tmp1 = new List<DatabaseOverride>();
        //    tmp1.Add(new DatabaseOverride("default1a", "override1a"));
        //    tmp1.Add(new DatabaseOverride("default1b", "override1b"));

        //    List<DatabaseOverride> tmp2 = new List<DatabaseOverride>();
        //    tmp2.Add(new DatabaseOverride("default2a", "override2a"));
        //    tmp2.Add(new DatabaseOverride("default2b", "override2b"));

        //    target.Add("z", tmp1);
        //    target.Add("2", tmp2);

        //    string actual;
        //    actual = target.GetSequenceId("default2b", "override2b");
        //    Assert.AreEqual("2", actual);

        //    actual = target.GetSequenceId("default1a", "override1a");
        //    Assert.AreEqual("z", actual);

        //    actual = target.GetSequenceId("defaultXXX", "override1a");
        //    Assert.AreEqual("", actual);

        //    actual = target.GetSequenceId("default1a", "overrideXX");
        //    Assert.AreEqual("", actual);

        //}

        /// <summary>
        ///A test for GetOverrideDatabaseNameList
        ///</summary>
        [TestMethod()]
        public void GetOverrideDatabaseNameListTest()
        {
            DbOverrides target = new DbOverrides();
            List<DatabaseOverride> tmp1 = new List<DatabaseOverride>();
            tmp1.Add(new DatabaseOverride("default1a", "override1a"));
            tmp1.Add(new DatabaseOverride("default1b", "override1b"));

            List<DatabaseOverride> tmp2 = new List<DatabaseOverride>();
            tmp2.Add(new DatabaseOverride("default2a", "override2a"));
            tmp2.Add(new DatabaseOverride("default2b", "override2b"));
            target.AddRange(tmp1);
            target.AddRange(tmp2);

            List<string> actual = target.GetOverrideDatabaseNameList();
            Assert.AreEqual("override1a", actual[0]);
            Assert.AreEqual("override1b", actual[1]);
            Assert.AreEqual("override2a", actual[2]);
            Assert.AreEqual("override2b", actual[3]);
        }

        /// <summary>
        ///A test for Add
        ///</summary>
        [TestMethod()]
        public void AddTest()
        {
            DbOverrides target = new DbOverrides();
            List<DatabaseOverride> tmp1 = new List<DatabaseOverride>();
            tmp1.Add(new DatabaseOverride("default1a", "override1a"));
            tmp1.Add(new DatabaseOverride("default1b", "override1b"));
            target.AddRange(tmp1);

            Assert.AreEqual(2, target.Count);
        }

        /// <summary>
        ///A test for DbOverrideSequence Constructor
        ///</summary>
        [TestMethod()]
        public void DbOverrideSequenceConstructorTest()
        {
            DbOverrides target = new DbOverrides();
            Assert.AreEqual(0, target.Count);
        }
    }
}
