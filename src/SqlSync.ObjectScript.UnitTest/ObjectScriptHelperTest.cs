using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.Connection;
using SqlSync.ObjectScript.Hash;
using System;
using System.Collections.Specialized;
using System.Text;

namespace SqlSync.ObjectScript.UnitTest
{


    /// <summary>
    ///This is a test class for ObjectScriptHelperTest and is intended
    ///to contain all ObjectScriptHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ObjectScriptHelperTest
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
        ///A test for GetDatabaseObjectHashes
        ///</summary>
        [TestMethod()]
        public void GetDatabaseObjectHashesTest_BadDatabase()
        {
            Initialization init = new Initialization();
            ConnectionData data = init.connData;
            data.DatabaseName = "BADNAME";
            ObjectScriptHelper target = new ObjectScriptHelper(data);
            ObjectScriptHashData actual;
            actual = target.GetDatabaseObjectHashes();
            Assert.IsNull(actual);
        }

        #region CollateScriptWithSchemaCheckTest
        /// <summary>
        ///A test for CollateScriptWithSchemaCheck
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.ObjectScript.dll")]
        public void CollateScriptWithSchemaCheckTest_NothingToDo()
        {
            ObjectScriptHelper target = new ObjectScriptHelper(new ConnectionData());
            StringCollection coll = new StringCollection();
            coll.Add("This does not start with IF NOT EXISTS");
            coll.Add("Neither does this");
            string expected = String.Format(@"{0}
GO


{1}
GO


", coll[0], coll[1]);
            string schema = "dbo";
            StringBuilder sb = new StringBuilder();

            target.CollateScriptWithSchemaCheck(coll, schema, ref sb);
            Assert.AreEqual(expected, sb.ToString());
        }

        /// <summary>
        ///A test for CollateScriptWithSchemaCheck
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.ObjectScript.dll")]
        public void CollateScriptWithSchemaCheckTest_HasObjectIdButNothingToDo()
        {
            ObjectScriptHelper target = new ObjectScriptHelper(new ConnectionData());
            StringCollection coll = new StringCollection();
            coll.Add(@"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MyObject]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[MyObject](
	[MyObjectID] [int] IDENTITY(1,1) NOT NULL
	");
            string expected = String.Format(@"{0}
GO


", coll[0]);
            string schema = "dbo";
            StringBuilder sb = new StringBuilder();

            target.CollateScriptWithSchemaCheck(coll, schema, ref sb);
            Assert.AreEqual(expected, sb.ToString());
        }

        /// <summary>
        ///A test for CollateScriptWithSchemaCheck
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.ObjectScript.dll")]
        public void CollateScriptWithSchemaCheckTest_NeedsSchema()
        {
            ObjectScriptHelper target = new ObjectScriptHelper(new ConnectionData());
            StringCollection coll = new StringCollection();
            coll.Add(@"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[MyObject]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[MyObject](
	[MyObjectID] [int] IDENTITY(1,1) NOT NULL
	");

            string schema = "dbo";
            StringBuilder sb = new StringBuilder();

            target.CollateScriptWithSchemaCheck(coll, schema, ref sb);
            Assert.IsTrue(sb.ToString().IndexOf("OBJECT_ID(N'[dbo].[MyObject]')") > -1);
        }

        /// <summary>
        ///A test for CollateScriptWithSchemaCheck
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.ObjectScript.dll")]
        public void CollateScriptWithSchemaCheckTest_MultipleGoodWithSchema()
        {
            ObjectScriptHelper target = new ObjectScriptHelper(new ConnectionData());
            StringCollection coll = new StringCollection();
            coll.Add(@"IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MyObject_Fk1]') AND parent_object_id = OBJECT_ID(N'[dbo].[MyObject]'))
ALTER TABLE [dbo].[MyObject]  WITH CHECK ADD  CONSTRAINT [FK_MyObject_Fk1] FOREIGN KEY([MyObjectType])
REFERENCES [MyObjectType] ([MyObjectTypeID])");

            string expected = String.Format(@"{0}
GO


", coll[0]);
            string schema = "dbo";
            StringBuilder sb = new StringBuilder();

            target.CollateScriptWithSchemaCheck(coll, schema, ref sb);
            Assert.AreEqual(expected, sb.ToString());
        }

        /// <summary>
        ///A test for CollateScriptWithSchemaCheck
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.ObjectScript.dll")]
        public void CollateScriptWithSchemaCheckTest_MultipleNeedingSchema()
        {
            ObjectScriptHelper target = new ObjectScriptHelper(new ConnectionData());
            StringCollection coll = new StringCollection();
            coll.Add(@"IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_MyObject_Fk1]') AND parent_object_id = OBJECT_ID(N'[MyObject]'))
ALTER TABLE [dbo].[MyObject]  WITH CHECK ADD  CONSTRAINT [FK_MyObject_Fk1] FOREIGN KEY([MyObjectType])
REFERENCES [MyObjectType] ([MyObjectTypeID])");

            string expected = String.Format(@"{0}
GO


", coll[0]);
            string schema = "dbo";
            StringBuilder sb = new StringBuilder();

            target.CollateScriptWithSchemaCheck(coll, schema, ref sb);
            Assert.IsTrue(sb.ToString().IndexOf("OBJECT_ID(N'[dbo].[FK_MyObject_Fk1]')") > -1);
            Assert.IsTrue(sb.ToString().IndexOf("OBJECT_ID(N'[dbo].[MyObject]')") > -1);
        }
        #endregion
    }
}
