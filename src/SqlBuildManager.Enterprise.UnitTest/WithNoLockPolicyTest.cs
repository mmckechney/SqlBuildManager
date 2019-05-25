using SqlBuildManager.Enterprise.Policy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace SqlBuildManager.Enterprise.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for WithNoLockPolicyTest and is intended
    ///to contain all WithNoLockPolicyTest Unit Tests
    ///</summary>
    [TestClass()]
    public class WithNoLockPolicyTest
    {

        /// <summary>
        ///A test for PolicyId
        ///</summary>
        [TestMethod()]
        public void PolicyIdTest()
        {
            WithNoLockPolicy target = new WithNoLockPolicy();
            string actual;
            actual = target.PolicyId;
            string expected = "WithNoLockPolicy";
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
        ///A test for CheckPolicy FROM statement that has NOLOCK directive
        ///</summary>
        [TestMethod()]
        public void WithNoLockPolicyTest_FromHasNoLock()
        {
            WithNoLockPolicy target = new WithNoLockPolicy();
            string script = @"SELECT test FROM TestTable WITH (NOLOCK) WHERE Column1 = 'Hello' and Column3 = 'World'"; 
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
        ///A test for CheckPolicy FROM statement that does not have a NOLOCK directive
        ///</summary>
        [TestMethod()]
        public void WithNoLockPolicyTest_FromMissingNoLock()
        {
            WithNoLockPolicy target = new WithNoLockPolicy();
            string script = @"SELECT test FROM TestTable  WHERE Column1 = 'Hello' and Column3 = 'World'";
            string message = string.Empty;
            string messageExpected = WithNoLockPolicy.MissingNoLockMessage;
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script); 
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for CheckPolicy INNER JOIN  statement that is Missing NOLOCK directive
        ///</summary>
        [TestMethod()]
        public void WithNoLockPolicyTest_InnerJoinMissingNoLock()
        {
            WithNoLockPolicy target = new WithNoLockPolicy();
            string script = @"SELECT test FROM TestTable WITH (NOLOCK) 
INNER JOIN Table2 ON TestTable.Column 1= Table2.Column3 WHERE Column1 = 'Hello' and Column3 = 'World'";
            string message = string.Empty;
            string messageExpected = WithNoLockPolicy.MissingNoLockMessage;
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for CheckPolicy INNER JOIN  statement that has NOLOCK directive
        ///</summary>
        [TestMethod()]
        public void WithNoLockPolicyTest_InnerJoinHasNoLock()
        {
            WithNoLockPolicy target = new WithNoLockPolicy();
            string script = @"SELECT test FROM TestTable WITH (NOLOCK) 
INNER JOIN Table2 WITH (NOLOCK) ON TestTable.Column 1= Table2.Column3 WHERE Column1 = 'Hello' and Column3 = 'World'";
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
        ///A test for LongDescription
        ///</summary>
        [TestMethod()]
        public void LongDescriptionTest()
        {
            WithNoLockPolicy target = new WithNoLockPolicy();
            string actual;
            actual = target.LongDescription;
            string expected = "Checks that select scripts include WITH (NOLOCK) directive or have a [NOLOCK Exception: <table name> <reason>] tag"; 
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for ShortDescription
        ///</summary>
        [TestMethod()]
        public void ShortDescriptionTest()
        {
            WithNoLockPolicy target = new WithNoLockPolicy();
            string actual;
            actual = target.ShortDescription;
            string expected = "WITH (NOLOCK)";
            Assert.AreEqual(actual, expected);
        }

        /// <summary>
        ///A test for CheckPolicy FROM statement that has NOLOCK directive
        ///</summary>
        [TestMethod()]
        public void WithNoLockPolicyTest_HasExceptionTagWithName()
        {
            WithNoLockPolicy target = new WithNoLockPolicy();
            string script = @"--[NOLOCK Exception: TestTable doesn't need it here]
SELECT test FROM TestTable WHERE Column1 = 'Hello' and Column3 = 'World'";
            string message = string.Empty;
            string messageExpected = WithNoLockPolicy.FoundExceptionMessage; 
            bool expected = true;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for CheckPolicy FROM statement that has NOLOCK directive
        ///</summary>
        [TestMethod()]
        public void WithNoLockPolicyTest_HasExceptionTagWithAlias()
        {
            WithNoLockPolicy target = new WithNoLockPolicy();
            string script = @"--[NOLOCK Exception: tt doesn't need it here]
SELECT test FROM TestTable tt WHERE Column1 = 'Hello' and Column3 = 'World'";
            string message = string.Empty;
            string messageExpected = WithNoLockPolicy.FoundExceptionMessage;
            bool expected = true;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for CheckPolicy FROM statement that has NOLOCK directive
        ///</summary>
        [TestMethod()]
        public void WithNoLockPolicyTest_HasExceptionTagForOneTableButNotSecond()
        {
            WithNoLockPolicy target = new WithNoLockPolicy();
            string script = @"--[NOLOCK Exception: tt doesn't need it here]
SELECT test FROM TestTable tt
INNER JOIN Table2 t2 ON t2.Column1 = tt.Column1 WHERE Column1 = 'Hello' and Column3 = 'World'";
            string message = string.Empty;
            string messageExpected = WithNoLockPolicy.MissingNoLockMessage;
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for CheckPolicy FROM statement that has NOLOCK directive
        ///</summary>
        [TestMethod()]
        public void WithNoLockPolicyTest_HasExceptionTagForOneTableButNotSecond2()
        {
            WithNoLockPolicy target = new WithNoLockPolicy();
            string script = @"--[NOLOCK Exception: Table2 doesn't need it here]
SELECT test FROM TestTable tt
INNER JOIN Table2 t2 ON t2.Column1 = tt.Column1 WHERE Column1 = 'Hello' and Column3 = 'World'";
            string message = string.Empty;
            string messageExpected = WithNoLockPolicy.MissingNoLockMessage;
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for CheckPolicy FROM statement that has NOLOCK directive
        ///</summary>
        [TestMethod()]
        public void WithNoLockPolicyTest_HasExceptionTagForOneTableButNotSecond3()
        {
            WithNoLockPolicy target = new WithNoLockPolicy();
            string script = @"--[NOLOCK Exception: t2 doesn't need it here]
SELECT test FROM TestTable tt
INNER JOIN Table2 t2 ON t2.Column1 = tt.Column1 WHERE Column1 = 'Hello' and Column3 = 'World'";
            string message = string.Empty;
            string messageExpected = WithNoLockPolicy.MissingNoLockMessage;
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for CheckPolicy FROM statement that has NOLOCK directive
        ///</summary>
        [TestMethod()]
        public void WithNoLockPolicyTest_HasExceptionTagButNoTableName()
        {
            WithNoLockPolicy target = new WithNoLockPolicy();
            string script = @"--[NOLOCK Exception: doesn't have a table name]
SELECT test FROM TestTable WHERE Column1 = 'Hello' and Column3 = 'World'";
            string message = string.Empty;
            string messageExpected = WithNoLockPolicy.MissingNoLockMessage;
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for CheckPolicy FROM statement that has NOLOCK directive
        ///</summary>
        [TestMethod()]
        public void WithNoLockPolicyTest_HasExceptionTagButNoTableNameOrAlias()
        {
            WithNoLockPolicy target = new WithNoLockPolicy();
            string script = @"--[NOLOCK Exception: doesn't have a table name]
SELECT test FROM TestTable tt WHERE Column1 = 'Hello' and Column3 = 'World'";
            string message = string.Empty;
            string messageExpected = WithNoLockPolicy.MissingNoLockMessage;
            bool expected = false;
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
        public void WithNoLockPolicyTest_SysObjectSelect()
        {
            WithNoLockPolicy target = new WithNoLockPolicy();
            string script = @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[emailList]') AND type in (N'U'))";
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
        public void WithNoLockPolicyTest_InformationSchemaSelect()
        {
            WithNoLockPolicy target = new WithNoLockPolicy();
            string script = @"SELECT * FROM INFORMATION_SCHEMA.Routines WHERE ROUTINE_NAME='myproc'";
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
        public void WithNoLockPolicyTest_SysIndexesSelect()
        {
            WithNoLockPolicy target = new WithNoLockPolicy();
            string script = @"SELECT * FROM sysindexes WHERE NAME='myindex'";
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
        public void WithNoLockPolicyTest_SelectFromTableVariable()
        {
            WithNoLockPolicy target = new WithNoLockPolicy();
            string script = @"SELECT * FROM @MyTempTable WHERE NAME='myindex'";
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
        public void WithNoLockPolicyTest_WithDeleteButNoWhere()
        {
            WithNoLockPolicy target = new WithNoLockPolicy();
            string script = @"IF EXISTS (SELECT 1 FROM dbo.MyTable WITH (NOLOCK) WHERE TableId = 57 AND SecondId = 3)
	BEGIN
		DELETE FROM dbo.MyTable WHERE TableId = 57 AND SecondId = 3
		PRINT 'Deleted General Admin from dbo.MyTable for MyAspPage.aspx.'
	END";
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
        public void WithNoLockPolicyTest_WithInsertButNoWhere()
        {
            WithNoLockPolicy target = new WithNoLockPolicy();
             string script = @"IF EXISTS (SELECT 1 FROM dbo.MyTable WITH (NOLOCK) WHERE TableId = 57 AND SecondId = 3)
	BEGIN
		INSERT INTO dbo.MyTable WHERE TableId = 57 AND SecondId = 3
		PRINT 'Inserted General Admin from dbo.MyTable for MyAspPage.aspx.'
	END";
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
        public void WithNoLockPolicyTest_WithUpdateButNoWhere()
        {
            WithNoLockPolicy target = new WithNoLockPolicy();
            string script = @"IF EXISTS (SELECT 1 FROM dbo.MyTable WITH (NOLOCK) WHERE TableId = 57 AND SecondId = 3)
	BEGIN
		UPDATE dbo.MyTable WHERE TableId = 57 AND SecondId = 3
		PRINT 'UPDATE General Admin from dbo.MyTable for MyAspPage.aspx.'
	END";
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
        public void WithNoLockPolicyTest_FromInTheComments()
        {
            WithNoLockPolicy target = new WithNoLockPolicy();
            string script = @"CREATE PROCEDURE [dbo].[MyProc1]
            @MyId int,
            @RetryCount int,
            @ProcessStatusID int,
            @ProcessingStartDate DateTime,
            @ProcessingEndDate DateTime,
            @AgentID int,
            @DoDelete bit = 0
     AS
     
     /******************************************************************************
     **            Name: dbo.MyProc1.PRC
     **            Desc: For updating a record in dbo.MyTable
     **              
     **            Auth: author1
     **            Date: 12/12/2000
     *******************************************************************************
     **            Change History
     *******************************************************************************
     **            Date:         Author:              Description:
     **            -----         --------             --------------------------------
     **            01/30/2001    author            Added optional delete from MyTable
     *******************************************************************************/
     UPDATE [dbo].[MyTable] SET 
            RetryCount=@RetryCount,
            ProcessStatusID=@ProcessStatusID,
            ProcessingStartDate=@ProcessingStartDate,
            ProcessingEndDate=@ProcessingEndDate,
            AgentID=@AgentID
     WHERE MyId=@MyId

     IF @DoDelete = 1
     BEGIN
            DELETE FROM [dbo].[MyTable] WHERE MyId = @MyId
     END


GO
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

    }
}
