using SqlBuildManager.Enterprise.Policy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SqlBuildManager.Interfaces.ScriptHandling.Policy;
using System.Collections.Generic;

namespace SqlBuildManager.Enterprise.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for ScriptSyntaxCheckPolicyTest and is intended
    ///to contain all ScriptSyntaxCheckPolicyTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ScriptSyntaxCheckPolicyTest
    {
        
        
  


        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            execAsArgsNoException.Add(new IScriptPolicyArgument() { Name = "ExecAs", Value = "EXECUTE AS" });

            execAsArgsWithGlobalException.Add(new IScriptPolicyArgument() { Name = "ExecAs", Value = "EXECUTE AS", });
            execAsArgsWithGlobalException.Add(new IScriptPolicyArgument() { Name = "ExecAsException", Value = "EXECUTE AS Exception:", IsGlobalException = true });

            selectStarNoException.Add(new IScriptPolicyArgument() { Name = "SelectStar", Value = @"(SELECT\s*\*)|(SELECT\s*.*\.\*)" });

            selectStarLineException.Add(new IScriptPolicyArgument() { Name = "SelectStar", Value = @"(SELECT\s*\*)|(SELECT\s*.*\.\*)", });
            selectStarLineException.Add(new IScriptPolicyArgument() { Name = "SelectStarException", Value = @"SELECT((\s*)|(\s*.*\.))\*\s*FROM((\s*)|(\s*.*\.))vw_\w", IsLineException = true });

            dropTableArgsNoException.Add(new IScriptPolicyArgument() { Name = "DropTableRegex", Value = @"DROP\s+TABLE" });

  
        }
        
     

        /// <summary>
        ///A test for ScriptSyntaxCheckPolicy Constructor
        ///</summary>
        [TestMethod()]
        public void ScriptSyntaxCheckPolicyConstructorTest()
        {
            ScriptSyntaxCheckPolicy target = new ScriptSyntaxCheckPolicy();
            Assert.IsNotNull(target);
        }

        #region CheckPolicy - using EXECUTE AS"
        #region EXECUTE AS fields
        private static List<IScriptPolicyArgument> execAsArgsNoException = new List<IScriptPolicyArgument>();
        private static List<IScriptPolicyArgument> execAsArgsWithGlobalException = new List<IScriptPolicyArgument>();
        private const string execAsErrorMessage = "EXECUTE AS directive found on line " + PolicyHelper.LineNumberToken + ". This is not allowed.";
        private const string execAsShortDesc = "EXECUTE AS";
        private const string execAsLongDesc = "Checks to ensure there are no EXECUTE AS directives in the script.";
        #endregion

        /// <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void CheckPolicyTest_Pass()
        {
            ScriptSyntaxCheckPolicy target = new ScriptSyntaxCheckPolicy();
            target.LongDescription = execAsLongDesc;
            target.ShortDescription = execAsShortDesc;
            target.Arguments = execAsArgsNoException;
            target.ErrorMessage = execAsErrorMessage;

            string script = @"SELECT test FROM dbo.MyTable";
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
        public void CheckPolicyTest_FailWithExecuteAs()
        {
            ScriptSyntaxCheckPolicy target = new ScriptSyntaxCheckPolicy();
            target.LongDescription = execAsLongDesc;
            target.ShortDescription = execAsShortDesc;
            target.Arguments = execAsArgsNoException;
            target.ErrorMessage = execAsErrorMessage;

            string script = @"USE AdventureWorks2008R2;
GO
CREATE PROCEDURE HumanResources.uspEmployeesInDepartment 
@DeptValue int
WITH EXECUTE AS OWNER
AS
    SET NOCOUNT ON;
    SELECT e.BusinessEntityID, c.LastName, c.FirstName, e.JobTitle
    FROM Person.Person AS c 
    INNER JOIN HumanResources.Employee AS e
        ON c.BusinessEntityID = e.BusinessEntityID
    INNER JOIN HumanResources.EmployeeDepartmentHistory AS edh
        ON e.BusinessEntityID = edh.BusinessEntityID
    WHERE edh.DepartmentID = @DeptValue
    ORDER BY c.LastName, c.FirstName;
GO

";
            string message = string.Empty;
            string messageExpected = "EXECUTE AS directive found on line 5. This is not allowed.";
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
        public void CheckPolicyTest_PassWithExecuteAsAndGlobalException()
        {
            ScriptSyntaxCheckPolicy target = new ScriptSyntaxCheckPolicy();
            target.LongDescription = execAsLongDesc;
            target.ShortDescription = execAsShortDesc;
            target.Arguments = execAsArgsWithGlobalException;
            target.ErrorMessage = execAsErrorMessage;

            string script = @"USE AdventureWorks2008R2;
GO
CREATE PROCEDURE HumanResources.uspEmployeesInDepartment 
@DeptValue int
WITH EXECUTE AS OWNER
AS
    SET NOCOUNT ON;
    SELECT e.BusinessEntityID, c.LastName, c.FirstName, e.JobTitle
    FROM Person.Person AS c 
    INNER JOIN HumanResources.Employee AS e
        ON c.BusinessEntityID = e.BusinessEntityID
    INNER JOIN HumanResources.EmployeeDepartmentHistory AS edh
        ON e.BusinessEntityID = edh.BusinessEntityID
    WHERE edh.DepartmentID = @DeptValue
    ORDER BY c.LastName, c.FirstName;
GO
--EXECUTE AS Exception: Just testing
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
        #endregion

        #region CheckPolicy - using SELECT * and LineException"
        #region SELECT * fields
        private static List<IScriptPolicyArgument> selectStarNoException = new List<IScriptPolicyArgument>();
        private static List<IScriptPolicyArgument> selectStarLineException = new List<IScriptPolicyArgument>();
        private const string selectStarAsErrorMessage = "A SELECT * was found on line " + PolicyHelper.LineNumberToken + ". This is not allowed.";
        private const string selectStarShortDesc = "SELECT *";
        private const string selectStarLongDesc = "Checks to make sure you don't use SELECT * in your queries";

        #endregion

        [TestMethod()]
        public void CheckPolicyTest_PassWithLineException()
        {
            ScriptSyntaxCheckPolicy target = new ScriptSyntaxCheckPolicy();
            target.LongDescription = selectStarLongDesc;
            target.ShortDescription = selectStarShortDesc;
            target.Arguments = selectStarLineException;
            target.ErrorMessage = selectStarAsErrorMessage;

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

        [TestMethod()]
        public void CheckPolicyTest_FailWithNoLineException()
        {
            ScriptSyntaxCheckPolicy target = new ScriptSyntaxCheckPolicy();
            target.LongDescription = selectStarLongDesc;
            target.ShortDescription = selectStarShortDesc;
            target.Arguments = selectStarNoException;
            target.ErrorMessage = selectStarAsErrorMessage;

            string script = @"SELECT zyx.* FROM vw_MyTestView xyz";
            string message = string.Empty;
            string messageExpected = "A SELECT * was found on line 1. This is not allowed.";
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);

        }

        [TestMethod()]
        public void CheckPolicyTest_FailWithLineException()
        {
            ScriptSyntaxCheckPolicy target = new ScriptSyntaxCheckPolicy();
            target.LongDescription = selectStarLongDesc;
            target.ShortDescription = selectStarShortDesc;
            target.Arguments = selectStarLineException;
            target.ErrorMessage = selectStarAsErrorMessage;

            string script = @"IF EXISTS (SELECT zyx.* FROM vw_MyTestView xyz)
BEGIN
    SELECT * FROM MyTable
END
";
            string message = string.Empty;
            string messageExpected = "A SELECT * was found on line 3. This is not allowed.";
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);

        }

        [TestMethod()]
        public void CheckPolicyTest_PassWithViolationInComment()
        {
            ScriptSyntaxCheckPolicy target = new ScriptSyntaxCheckPolicy();
            target.LongDescription = selectStarLongDesc;
            target.ShortDescription = selectStarShortDesc;
            target.Arguments = selectStarLineException;
            target.ErrorMessage = selectStarAsErrorMessage;

            string script = @"IF EXISTS (SELECT zyx.* FROM vw_MyTestView xyz)
BEGIN
    -- SELECT * FROM MyTable
    SELECT top 5 col FROM MyTable
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

        [TestMethod()]
        public void CheckPolicyTest_PassWithNoMatches()
        {
            ScriptSyntaxCheckPolicy target = new ScriptSyntaxCheckPolicy();
            target.LongDescription = selectStarLongDesc;
            target.ShortDescription = selectStarShortDesc;
            target.Arguments = selectStarLineException;
            target.ErrorMessage = selectStarAsErrorMessage;

            string script = @"SELECT a,b,c,d, FROM MyTable
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


        #endregion

        #region CheckPolicy - using DROP TABLE
        #region DROP TABLE fields
        private static List<IScriptPolicyArgument> dropTableArgsNoException = new List<IScriptPolicyArgument>();
        /*Simulates config of...
         * 
            <ScriptPolicy PolicyId="ScriptSyntaxCheckPolicy">
		        <ScriptPolicyDescription 
				    LongDescription="Checks to ensure there any DROP TABLE directives are reviewed."
				    ShortDescription="DROP TABLE"
				    ErrorMessage="DROP TABLE directive found on line {lineNumber}. Please make sure this is reviewed prior to release."/>
		        <Argument Name="DropTableRegex" Value="DROP\s+TABLE" />
	        </ScriptPolicy>
         */
        private const string dropTableErrorMessage = "DROP TABLE directive found on line " + PolicyHelper.LineNumberToken + ". Please make sure this is reviewed prior to release.";
        private const string dropTableAsShortDesc = "DROP TABLE";
        private const string dropTableAsLongDesc = "Checks to ensure there any DROP TABLE directives are reviewed.";
        #endregion
        /// <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void CheckPolicyTest_FailWithDropTable()
        {
            ScriptSyntaxCheckPolicy target = new ScriptSyntaxCheckPolicy();
            target.LongDescription = dropTableAsLongDesc;
            target.ShortDescription = dropTableAsShortDesc;
            target.Arguments = dropTableArgsNoException;
            target.ErrorMessage = dropTableErrorMessage;

            string script = @"DROP TABLE MyTable

";
            string message = string.Empty;
            string messageExpected = "DROP TABLE directive found on line 1. Please make sure this is reviewed prior to release.";
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
        public void CheckPolicyTest_FailWithDropTableOnSeparateLine()
        {
            ScriptSyntaxCheckPolicy target = new ScriptSyntaxCheckPolicy();
            target.LongDescription = dropTableAsLongDesc;
            target.ShortDescription = dropTableAsShortDesc;
            target.Arguments = dropTableArgsNoException;
            target.ErrorMessage = dropTableErrorMessage;

            string script = @"DROP 
TABLE MyTable

";
            string message = string.Empty;
            string messageExpected = "DROP TABLE directive found on line 1. Please make sure this is reviewed prior to release.";
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
        public void CheckPolicyTest_DropTablePass()
        {
            ScriptSyntaxCheckPolicy target = new ScriptSyntaxCheckPolicy();
            target.LongDescription = dropTableAsLongDesc;
            target.ShortDescription = dropTableAsShortDesc;
            target.Arguments = dropTableArgsNoException;
            target.ErrorMessage = dropTableErrorMessage;

            string script = @"SELECT * FROM MyTable 
--DROP TABLE in comment

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
        #endregion


        #region Property tests
        /// <summary>
        ///A test for Arguments
        ///</summary>
        [TestMethod()]
        public void ArgumentsTest()
        {
            ScriptSyntaxCheckPolicy target = new ScriptSyntaxCheckPolicy();
            List<IScriptPolicyArgument> expected = execAsArgsWithGlobalException;
            List<IScriptPolicyArgument> actual;
            target.Arguments = expected;
            actual = target.Arguments;
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected[1].Value, actual[1].Value);
        }

        /// <summary>
        ///A test for ErrorMessage
        ///</summary>
        [TestMethod()]
        public void ErrorMessageTest()
        {
            string expected = "Here is my error"; 
            ScriptSyntaxCheckPolicy target = new ScriptSyntaxCheckPolicy();
            target.ErrorMessage = expected;
            
            string actual;
            actual = target.ErrorMessage;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for LongDescription
        ///</summary>
        [TestMethod()]
        public void LongDescriptionTest()
        {
            ScriptSyntaxCheckPolicy target = new ScriptSyntaxCheckPolicy();
            string expected = "This is the long description of my policy";
            string actual;
            target.LongDescription = expected;
            actual = target.LongDescription;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for PolicyId
        ///</summary>
        [TestMethod()]
        public void PolicyIdTest()
        {
            ScriptSyntaxCheckPolicy target = new ScriptSyntaxCheckPolicy(); 
            string actual;
            actual = target.PolicyId;
            Assert.AreEqual(target.PolicyId, actual);
        }

        /// <summary>
        ///A test for ShortDescription
        ///</summary>
        [TestMethod()]
        public void ShortDescriptionTest()
        {
            ScriptSyntaxCheckPolicy target = new ScriptSyntaxCheckPolicy(); // TODO: Initialize to an appropriate value
            string expected = "My Short Desc";
            string actual;
            target.ShortDescription = expected;
            actual = target.ShortDescription;
            Assert.AreEqual(expected, actual);

        }
        #endregion
    }
}
