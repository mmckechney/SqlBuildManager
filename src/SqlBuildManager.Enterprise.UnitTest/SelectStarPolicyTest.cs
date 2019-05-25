using SqlBuildManager.Enterprise.Policy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using SqlBuildManager.Interfaces.ScriptHandling.Policy;
namespace SqlBuildManager.Enterprise.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for SelectStarPolicyTest and is intended
    ///to contain all SelectStarPolicyTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SelectStarPolicyTest
    {
        private static List<IScriptPolicyArgument> policyArgs = new List<IScriptPolicyArgument>();
        private SelectStarPolicy target = new SelectStarPolicy();
        [TestInitialize()]
        public void SetUpPolicyArguments()
        {
            policyArgs.Clear();
            policyArgs.Add(new IScriptPolicyArgument() { Name = "SystemExceptionRegex", Value = @"SELECT\s*\*\s*FROM\s*sys" });
            policyArgs.Add(new IScriptPolicyArgument() { Name = "ViewExceptionRegex", Value = @"SELECT((\s*)|(\s*.*\.))\*\s*FROM((\s*)|(\s*.*\.))vw_\w" });

            target.Arguments = policyArgs;
        }

        /// <summary>
        ///A test for ShortDescription
        ///</summary>
        [TestMethod()]
        public void ShortDescriptionTest()
        {
            
            string actual;
            actual = target.ShortDescription;
            Assert.AreEqual("SELECT *", actual);
        }

        /// <summary>
        ///A test for PolicyId
        ///</summary>
        [TestMethod()]
        public void PolicyIdTest()
        {
            
            string actual;
            actual = target.PolicyId;
            Assert.AreEqual("SelectStarPolicy", actual);
        }

        /// <summary>
        ///A test for LongDescription
        ///</summary>
        [TestMethod()]
        public void LongDescriptionTest()
        {
           
            string actual;
            actual = target.LongDescription;
            Assert.AreEqual("Checks that no queries in the script use \"SELECT *\" but rather explicitly lists columns", actual);
        }

        /// <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void SelectStarPolicy_CheckPolicyTest_PassNoSelectStar()
        {
           
            string script = @"SELECT e.[EmployeeID], e.[ManagerID], c.[FirstName], c.[LastName], 0 -- Get the initial list of Employees for Manager n
	        FROM [HumanResources].[Employee] e 
	            INNER JOIN [Person].[Contact] c 
	            ON e.[ContactID] = c.[ContactID]
	        WHERE [ManagerID] = @ManagerID
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
        public void SelectStarPolicy_CheckPolicyTest_PassSelectStarOnSys()
        {
            string script = @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[uspGetManagerEmployees]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'
	CREATE PROCEDURE [dbo].[uspGetManagerEmployees]
	    @ManagerID [int]
	AS
BEGIN
	    SET NOCOUNT OFF;
	
	    -- Use recursive query to list out all Employees required for a particular Manager
	    WITH [EMP_cte]([EmployeeID], [ManagerID], [FirstName], [LastName], [RecursionLevel]) -- CTE name and columns
	    AS (
	        SELECT e.[EmployeeID], e.[ManagerID], c.[FirstName], c.[LastName], 0 -- Get the initial list of Employees for Manager n
	        FROM [HumanResources].[Employee] e 
	            INNER JOIN [Person].[Contact] c 
	            ON e.[ContactID] = c.[ContactID]
	        WHERE [ManagerID] = @ManagerID";

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
        public void SelectStarPolicy_CheckPolicyTest_FailComboSelectStarSysAndSelectStar()
        {
           
           
            string script = @"
SELECT * FROM dbo.MyTable

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[uspGetManagerEmployees]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'
	CREATE PROCEDURE [dbo].[uspGetManagerEmployees]
	    @ManagerID [int]
	AS
BEGIN
	    SET NOCOUNT OFF;
	    
	    -- Use recursive query to list out all Employees required for a particular Manager
	    WITH [EMP_cte]([EmployeeID], [ManagerID], [FirstName], [LastName], [RecursionLevel]) -- CTE name and columns
	    AS (
	        SELECT e.[EmployeeID], e.[ManagerID], c.[FirstName], c.[LastName], 0 -- Get the initial list of Employees for Manager n
	        FROM [HumanResources].[Employee] e 
	            INNER JOIN [Person].[Contact] c 
	            ON e.[ContactID] = c.[ContactID]
	        WHERE [ManagerID] = @ManagerID";

            string message = string.Empty;
            string messageExpected = "A SELECT using \"*\" was found on line 2. Please remove this wildcard and use explicit column names"; 
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
        public void SelectStarPolicy_CheckPolicyTest_FailSelectStar()
        {
           
               string script = @"SELECT * FROM MyTable";

            string message = string.Empty;
            string messageExpected = "A SELECT using \"*\" was found on line 1. Please remove this wildcard and use explicit column names";
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message.TrimEnd());
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void SelectStarPolicy_CheckPolicyTest_FailSelectStarWithQualifier()
        {
            string script = @"
                           SELECT hcc.*, c.Alias, c.sdr, c.ews
                                  FROM [dbo].[rew] hcc WITH (NOLOCK) 
                                  INNER JOIN dbo.cus c WITH (NOLOCK) ON hcc.CustId = c.CustId
                           WHERE hcc.[hub] = @codde AND hcc.[Client] = @Client' ";


            string message = string.Empty;
            string messageExpected = "A SELECT using \"*\" was found on line 2. Please remove this wildcard and use explicit column names";
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message.TrimEnd());
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void SelectStarPolicy_CheckPolicyTest_PassWithViewSelectStar()
        {

            string script = @"SELECT * FROM vw_MyTestView";
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
        public void SelectStarPolicy_CheckPolicyTest_PassWithMultiLineViewSelectStar()
        {

            string script = @"SELECT * 
                        FROM 
                        vw_MyTestView";
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
        public void SelectStarPolicy_CheckPolicyTest_PassWithQualifiedViewSelectStar()
        {

            string script = @"SELECT zyx.* FROM vw_MyTestView xyz";
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
        public void SelectStarPolicy_CheckPolicyTest_PassWithQualifiedViewAndSystemSelectStar()
        {

            string script = @"IF NOT EXISTS(Select * from sys.tables WHERE table_name = 'test')
BEGIN
    SELECT zyx.* FROM vw_MyTestView xyz
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
        public void SelectStarPolicy_CheckPolicyTest_FailWithViewButAlsoDisallowedSelectStar()
        {

            string script = @"IF NOT EXISTS(SELECT * FROM vw_MyTestView xyz where b=a)
BEGIN
    Select * from dbo.mytable WHERE columnvalue = 'test'
END";
            string message = string.Empty;
            string messageExpected = "A SELECT using \"*\" was found on line 3. Please remove this wildcard and use explicit column names";
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
        public void SelectStarPolicy_CheckPolicyTest_FailWithDisallowedAndViewSelectStar()
        {

            string script = @"IF NOT EXISTS(Select * from dbo.mytable WHERE columnvalue = 'test')
BEGIN
    SELECT * FROM vw_MyTestView xyz where b=a
END";
            string message = string.Empty;
            string messageExpected = "A SELECT using \"*\" was found on line 1. Please remove this wildcard and use explicit column names";
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }

    }
}
