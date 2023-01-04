using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Enterprise.Policy;
namespace SqlBuildManager.Enterprise.UnitTest
{


    /// <summary>
    ///This is a test class for ReRunablePolicyTest and is intended
    ///to contain all ReRunablePolicyTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ReRunablePolicyTest
    {

        /// <summary>
        ///A test for PolicyId
        ///</summary>
        [TestMethod()]
        public void PolicyIdTest()
        {
            ReRunablePolicy target = new ReRunablePolicy();
            string actual;
            actual = target.PolicyId;
            string expected = "ReRunablePolicy";
            Assert.AreEqual(actual, expected);

        }

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



        /// <summary>
        ///A test for ShortDescription
        ///</summary>
        [TestMethod()]
        public void ShortDescriptionTest()
        {
            ReRunablePolicy target = new ReRunablePolicy();
            string actual;
            actual = target.ShortDescription;
            string expected = "Re-runable scripts";
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for LongDescription
        ///</summary>
        [TestMethod()]
        public void LongDescriptionTest()
        {
            ReRunablePolicy target = new ReRunablePolicy();
            string actual;
            actual = target.LongDescription;
            string expected = "Checks that scripts contain \"IF EXISTS\" or \"IF NOT EXISTS\" checks so they are potentially re-runable";
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void ReRunablePolicyTest_SimpleCheck()
        {
            ReRunablePolicy target = new ReRunablePolicy();
            string script = @"IF NOT EXISTS (SELECT * FROM [dbo].[AccrualType] WITH (NOLOCK) WHERE [AccrualTypeID]='0')
BEGIN
   INSERT INTO [dbo].[AccrualType] ([AccrualTypeID],[Description]) VALUES ('0','Percent Year Passed')
   PRINT 'Inserted Row 1'
END
ELSE
BEGIN
   UPDATE [dbo].[AccrualType] SET [Description]='Percent Year Passed' WHERE [AccrualTypeID]='0'
   PRINT 'Updated Row 1'
END
";
            string message = string.Empty;
            string messageExpected = string.Empty;
            bool expected = true;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);

        }
        /// <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void ReRunablePolicyTest_MissingCheck()
        {
            ReRunablePolicy target = new ReRunablePolicy();
            string script = @"INSERT INTO [dbo].[AccrualType] ([AccrualTypeID],[Description]) VALUES ('0','Percent Year Passed')";
            string message;
            string messageExpected = "Script contains no \"IF EXISTS\" or \"IF NOT EXISTS\" checks";
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);

        }

        [TestMethod()]
        public void ReRunablePolicyTest_SystemObjectNullCheck()
        {
            ReRunablePolicy target = new ReRunablePolicy();
            string script = @"if DATABASE_PRINCIPAL_ID('mynew_role') is null
CREATE ROLE [mynew_role] AUTHORIZATION [dbo]";
            string message;
            string messageExpected = "";
            bool expected = true;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);

        }
    }
}
