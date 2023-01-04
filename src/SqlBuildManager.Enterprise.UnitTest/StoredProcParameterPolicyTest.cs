using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Enterprise.Policy;
using SqlBuildManager.Interfaces.ScriptHandling.Policy;
using System.Collections.Generic;
namespace SqlBuildManager.Enterprise.UnitTest
{


    /// <summary>
    ///This is a test class for StoredProcParameterPolicyTest and is intended
    ///to contain all StoredProcParameterPolicyTest Unit Tests
    ///</summary>
    [TestClass()]
    public class StoredProcParameterPolicyTest
    {




        /// <summary>
        ///A test for ShortDescription
        ///</summary>
        [TestMethod()]
        public void ShortDescriptionTest()
        {
            StoredProcParameterPolicy target = new StoredProcParameterPolicy();
            string actual;
            actual = target.ShortDescription;
            Assert.AreEqual("Stored Proc Parameter", actual);
        }
        /// <summary>
        ///A test for ShortDescription
        ///</summary>
        [TestMethod()]
        public void ShortDescriptionTest_WithParameter()
        {
            StoredProcParameterPolicy target = new StoredProcParameterPolicy();
            IScriptPolicyArgument args = new IScriptPolicyArgument() { Name = "Parameter", Value = "@Test" };
            target.Arguments.Add(args);
            string actual;
            actual = target.ShortDescription;
            Assert.AreEqual("Stored Proc Parameter - @Test", actual);
        }
        /// <summary>
        ///A test for ShortDescription
        ///</summary>
        [TestMethod()]
        public void ShortDescriptionTest_WithParameterAndSchema()
        {
            StoredProcParameterPolicy target = new StoredProcParameterPolicy();
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "Parameter", Value = "@Test" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "Schema", Value = "abc" });
            string actual;
            actual = target.ShortDescription;
            Assert.AreEqual("Stored Proc Parameter - abc/@Test", actual);
        }

        /// <summary>
        ///A test for PolicyId
        ///</summary>
        [TestMethod()]
        public void PolicyIdTest()
        {
            StoredProcParameterPolicy target = new StoredProcParameterPolicy();
            string actual;
            actual = target.PolicyId;
            Assert.AreEqual("StoredProcParameterPolicy", actual);
        }

        /// <summary>
        ///A test for LongDescription
        ///</summary>
        [TestMethod()]
        public void LongDescriptionTest()
        {
            StoredProcParameterPolicy target = new StoredProcParameterPolicy();
            string actual;
            actual = target.LongDescription;
            Assert.AreEqual("Makes certain that new stored procs have the proper default parameters", actual);
        }

        /// <summary>
        ///A test for Arguments
        ///</summary>
        [TestMethod()]
        public void ArgumentsTest()
        {
            StoredProcParameterPolicy target = new StoredProcParameterPolicy();
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "Schema", Value = "testSchema" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "Parameter", Value = "testParameter" });

            List<IScriptPolicyArgument> actual = target.Arguments;
            Assert.AreEqual("testSchema", actual[0].Value);
            Assert.AreEqual("testParameter", actual[1].Value);

        }

        /// <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void StoredProcParameterPolicy_CheckPolicyTest_PassWithParameterAndType()
        {

            StoredProcParameterPolicy target = new StoredProcParameterPolicy();
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "Schema", Value = "HumanResources" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "Parameter", Value = "@EmployeeID" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "SqlType", Value = "int" });

            string script = @"EXEC dbo.sp_executesql @statement = N'
	CREATE PROCEDURE [HumanResources].[uspUpdateEmployeeHireInfo]
	    @EmployeeID [int], 
	    @Title [nvarchar](50), 
	    @HireDate [datetime], 
	    @RateChangeDate [datetime], 
	    @Rate [money], 
	    @PayFrequency [tinyint], 
	    @CurrentFlag [dbo].[Flag] 
	WITH EXECUTE AS CALLER
	AS
BEGIN
	    SET NOCOUNT ON;
	
	    BEGIN TRY
	        BEGIN TRANSACTION;
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
        public void StoredProcParameterPolicy_CheckPolicyTest_PassNoMatchingSchema()
        {

            StoredProcParameterPolicy target = new StoredProcParameterPolicy();
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "Schema", Value = "TestSchema" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "Parameter", Value = "@Hobo" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "SqlType", Value = "int" });
            string script = @"EXEC dbo.sp_executesql @statement = N'
	CREATE PROCEDURE [HumanResources].[uspUpdateEmployeeHireInfo]
	    @EmployeeID [int], 
	    @Title [nvarchar](50), 
	    @HireDate [datetime], 
	    @RateChangeDate [datetime], 
	    @Rate [money], 
	    @PayFrequency [tinyint], 
	    @CurrentFlag [dbo].[Flag] 
	WITH EXECUTE AS CALLER
	AS
BEGIN
	    SET NOCOUNT ON;
	
	    BEGIN TRY
	        BEGIN TRANSACTION;
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
        public void StoredProcParameterPolicy_CheckPolicyTest_PassNoMatchingTargetDb()
        {

            StoredProcParameterPolicy target = new StoredProcParameterPolicy();
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "Schema", Value = "TestSchema" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "Parameter", Value = "@Hobo" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "SqlType", Value = "int" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "TargetDatabase", Value = "TestDb" });

            string script = @"Select test FROM dbo.ItJustDoesntMatter";
            string message = string.Empty;
            string targetDatabase = "MisMatch";
            string messageExpected = string.Empty;
            bool expected = true;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, targetDatabase, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void StoredProcParameterPolicy_CheckPolicyTest_PassNoTypeDefined()
        {

            StoredProcParameterPolicy target = new StoredProcParameterPolicy();
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "Schema", Value = "HumanResources" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "Parameter", Value = "@EmployeeID" });
            string script = @"EXEC dbo.sp_executesql @statement = N'
	CREATE PROCEDURE [HumanResources].[uspUpdateEmployeeHireInfo]
	    @EmployeeID [int], 
	    @Title [nvarchar](50), 
	    @HireDate [datetime], 
	    @RateChangeDate [datetime], 
	    @Rate [money], 
	    @PayFrequency [tinyint], 
	    @CurrentFlag [dbo].[Flag] 
	WITH EXECUTE AS CALLER
	AS
BEGIN
	    SET NOCOUNT ON;
	
	    BEGIN TRY
	        BEGIN TRANSACTION;
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
        public void StoredProcParameterPolicy_CheckPolicyTest_PassWithParameterNoBracketType()
        {

            StoredProcParameterPolicy target = new StoredProcParameterPolicy();
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "Schema", Value = "HumanResources" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "Parameter", Value = "@EmployeeID" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "SqlType", Value = "int" });
            string script = @"EXEC dbo.sp_executesql @statement = N'
	CREATE PROCEDURE [HumanResources].[uspUpdateEmployeeHireInfo]
	    @EmployeeID int, 
	    @Title [nvarchar](50), 
	    @HireDate [datetime], 
	    @RateChangeDate [datetime], 
	    @Rate [money], 
	    @PayFrequency [tinyint], 
	    @CurrentFlag [dbo].[Flag] 
	WITH EXECUTE AS CALLER
	AS
BEGIN
	    SET NOCOUNT ON;
	
	    BEGIN TRY
	        BEGIN TRANSACTION;
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
        public void StoredProcParameterPolicy_CheckPolicyTest_FailNoSchemaSet()
        {

            StoredProcParameterPolicy target = new StoredProcParameterPolicy();
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "Parameter", Value = "@EmployeeID" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "SqlType", Value = "int" });
            string script = @"It just doesn't matter";
            string message = string.Empty;
            string messageExpected = "Missing \"Schema\" argument in setup. Please check your Enterprise configuration";
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
        public void StoredProcParameterPolicy_CheckPolicyTest_FailNoParameterSet()
        {

            StoredProcParameterPolicy target = new StoredProcParameterPolicy();
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "Schema", Value = "HumanResources" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "SqlType", Value = "int" });

            string script = @"It just doesn't matter";
            string message = string.Empty;
            string messageExpected = "Missing \"Parameter\" argument in setup. Please check your Enterprise configuration";
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
        public void StoredProcParameterPolicy_CheckPolicyTest_FailNoArgumentsSet()
        {

            StoredProcParameterPolicy target = new StoredProcParameterPolicy();
            string script = @"It just doesn't matter";
            string message = string.Empty;
            string messageExpected = "Missing \"Schema\", \"Parameter\" arguments in setup. Please check your Enterprise configuration";
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
        public void StoredProcParameterPolicy_CheckPolicyTest_FailWithMissingParameter()
        {

            StoredProcParameterPolicy target = new StoredProcParameterPolicy();
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "Schema", Value = "HumanResources" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "SqlType", Value = "int" });
            target.Arguments.Add(new IScriptPolicyArgument() { Name = "Parameter", Value = "@EpicFail" });

            string script = @"EXEC dbo.sp_executesql @statement = N'
	CREATE PROCEDURE [HumanResources].[uspUpdateEmployeeHireInfo]
	    @EmployeeID [int], 
	    @Title [nvarchar](50), 
	    @HireDate [datetime], 
	    @RateChangeDate [datetime], 
	    @Rate [money], 
	    @PayFrequency [tinyint], 
	    @CurrentFlag [dbo].[Flag] 
	WITH EXECUTE AS CALLER
	AS
BEGIN
	    SET NOCOUNT ON;
	
	    BEGIN TRY
	        BEGIN TRANSACTION;
	";
            string message = string.Empty;
            string messageExpected = "The parameter \"@EpicFail\" is required for all procedures in the \"HumanResources\" schema.";
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }
    }
}
