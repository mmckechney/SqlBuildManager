using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Enterprise.Policy;
namespace SqlBuildManager.Enterprise.UnitTest
{


    /// <summary>
    ///This is a test class for QualifiedNamesPolicyTest and is intended
    ///to contain all QualifiedNamesPolicyTest Unit Tests
    ///</summary>
    [TestClass()]
    public class QualifiedNamesPolicyTest
    {

        /// <summary>
        ///A test for PolicyId
        ///</summary>
        [TestMethod()]
        public void PolicyIdTest()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string actual;
            actual = target.PolicyId;
            string expected = "QualifiedNamesPolicy";
            Assert.AreEqual(actual, expected);

        }
        /// <summary>
        ///A test for ShortDescription
        ///</summary>
        [TestMethod()]
        public void ShortDescriptionTest()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string actual;
            actual = target.ShortDescription;
            Assert.AreEqual("Qualified Names (beta)", actual);
        }

        /// <summary>
        ///A test for LongDescription
        ///</summary>
        [TestMethod()]
        public void LongDescriptionTest()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string actual;
            actual = target.LongDescription;
            Assert.AreEqual("Checks that object references are fully qualified (<schema>.<object name>) - in beta", actual);
        }

        /// <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void QualifiedNamesPolicyTest_MissingQualifierBracketed()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = Properties.Resources.MissingQualifierWithBrackets;
            string message = string.Empty;
            string messageExpected = "Missing schema qualifier on: [BOM_cte] cte (line: 30), [BOM_cte] b (line: 40)";
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
        public void QualifiedNamesPolicyTest_MissingQualifierBeforeGroupBy()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = Properties.Resources.MissingQualifierBeforeGroupBy;
            string message = string.Empty;
            string messageExpected = "Missing schema qualifier on: [BOM_cte] b (line: 39)";
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
        public void QualifiedNamesPolicyTest_FullyQualifiedBracketed()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = Properties.Resources.QualifiedWithBrackets;
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
        public void QualifiedNamesPolicyTest_NotQualifiedNotBracketed()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"ALTER PROCEDURE [dbo].[proc_Accruals]

	(
		@AccrualTemplateDetailID int
		
	)


AS
DELETE FROM AccrualRules
WHERE AccrualTemplateDetailID = @AccrualTEmplateDetailID";

            string message = string.Empty;
            string messageExpected = "Missing schema qualifier on: AccrualRules (line: 10)";
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
        public void QualifiedNamesPolicyTest_QualifiedNotBracketed()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"ALTER PROCEDURE [dbo].[proc_Accruals]

	(
		@AccrualTemplateDetailID int
		
	)


AS
DELETE FROM dbo.AccrualRules
WHERE AccrualTemplateDetailID = @AccrualTEmplateDetailID";

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
        public void QualifiedNamesPolicyTest_UnqualifiedWithNoLock()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"SELECT
		  TelePunchPromptID,
		  Type,
		  ISNULL(Message, '''') as Message,
		  AllowPrevious,
		  AllowExit,
		  SingleDigit,
		  ISNULL(Digit, '''') as Digit,
		  ParentPrompt
		  
	FROM TelePunchPrompt WITH (NOLOCK) 
	WHERE 
		ParentPrompt Like @TelePunchPromptID
		AND Enabled = 1
	ORDER BY Type
";

            string message = string.Empty;
            string messageExpected = "Missing schema qualifier on: TelePunchPrompt (line: 11)";
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void QualifiedNamesPolicyTest_QualifiedWithNoLock()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"SELECT
		  TelePunchPromptID,
		  Type,
		  ISNULL(Message, '''') as Message,
		  AllowPrevious,
		  AllowExit,
		  SingleDigit,
		  ISNULL(Digit, '''') as Digit,
		  ParentPrompt
		  
	FROM dbo.TelePunchPrompt WITH (NOLOCK) 
	WHERE 
		ParentPrompt Like @TelePunchPromptID
		AND Enabled = 1
	ORDER BY Type
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
        public void QualifiedNamesPolicyTest_QualifiedAtEnd()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"SELECT
		  TelePunchPromptID,
		  Type,
		  ISNULL(Message, '''') as Message,
		  AllowPrevious,
		  AllowExit,
		  SingleDigit,
		  ISNULL(Digit, '''') as Digit,
		  ParentPrompt
		  
	FROM dbo.TelePunchPrompt
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
        public void QualifiedNamesPolicyTest_UnqualifiedAtEnd()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"SELECT
		  TelePunchPromptID,
		  Type,
		  ISNULL(Message, '''') as Message,
		  AllowPrevious,
		  AllowExit,
		  SingleDigit,
		  ISNULL(Digit, '''') as Digit,
		  ParentPrompt
		  
	FROM TelePunchPrompt
";

            string message = string.Empty;
            string messageExpected = "Missing schema qualifier on: TelePunchPrompt (line: 11)";
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void QualifiedNamesPolicyTest_DeleteStatementUnqualified()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"DELETE FROM 
        MyTable mt
        WHERE
        mt.Column = 'X'
";

            string message = string.Empty;
            string messageExpected = "Missing schema qualifier on: MyTable mt (line: 1)";
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void QualifiedNamesPolicyTest_DeleteStatementQualified()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"DELETE FROM 
        dbo.MyTable mt
        WHERE
        mt.Column = 'X'
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
        public void QualifiedNamesPolicyTest_UpdateStatementUnqualified()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"UPDATE  MyTable my
	SET       my.Column = '123' AND my.column2 = '345'
	WHERE  my.Column3 = 'hello;
";

            string message = string.Empty;
            string messageExpected = "Missing schema qualifier on: MyTable my (line: 1)";
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void QualifiedNamesPolicyTest_UpdateStatementQualified()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"UPDATE  dbo.MyTable my
	SET       my.Column = '123' AND my.column2 = '345'
	WHERE  my.Column3 = 'hello;
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

        // <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void QualifiedNamesPolicyTest_VariableTable()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"DECLARE @CustomerStatistics TABLE
			(
				Statistic	VARCHAR(50),
				Today		BIGINT,
				Yesterday	BIGINT
			)
		
				SELECT  Statistic,
					Today,
					Yesterday FROM
                @CustomerStatistics
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


        // <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void QualifiedNamesPolicyTest_VariableTableWithWhere()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"DECLARE @CustomerStatistics TABLE
			(
				Statistic	VARCHAR(50),
				Today		BIGINT,
				Yesterday	BIGINT
			)
		
				SELECT  Statistic,
					Today,
					Yesterday FROM
                @CustomerStatistics WHERE
                Today = getdate()
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

        // <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void QualifiedNamesPolicyTest_TempTableWithWhere()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"		
				SELECT  Statistic,
					Today,
					Yesterday FROM
                    #CustomerStatistics WHERE
                Today = getdate()
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

        // <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void QualifiedNamesPolicyTest_TempTable()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"		
				SELECT  Statistic,
					Today,
					Yesterday FROM
                    #CustomerStatistics
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

        // <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void QualifiedNamesPolicyTest_Cursor()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"		
				DECLARE dbname CURSOR FOR
	SELECT [name] FROM master.sys.sysdatabases WITH (NOLOCK)
	WHERE [name] LIKE ''me%'' OR [name] LIKE ''you%''
	ORDER BY [name]
	
	OPEN dbname;
	
	FETCH next
		FROM dbname 		INTO @dbname

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

        // <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void QualifiedNamesPolicyTest_FunctionSelect()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"		
				
	SELECT [name] FROM dbo.myFunction('test')
	WHERE [name] LIKE ''me%'' OR [name] LIKE ''you%''
	ORDER BY [name]
	
	OPEN dbname;
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

        // <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void CheckPolicy2Test_FromInDashComment()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"		
				--This is a FROM in a comment
	SELECT [name] FROM dbo.myFunction
	WHERE [name] LIKE ''me%''

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

        // <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void QualifiedNamesPolicyTest_FromInEndDashComment()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"		
				
	SELECT [name] FROM dbo.myFunction
	WHERE [name] LIKE ''me%''
    --This is a FROM in a comment

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

        // <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void QualifiedNamesPolicyTest_FromInStarComment()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"		
				/*
                    This is a FROM in a star comment
                */
	SELECT [name] FROM dbo.myFunction
	WHERE [name] LIKE ''me%''
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

        // <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void QualifiedNamesPolicyTest_FromInEndStarComment()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"		
				
	SELECT [name] FROM dbo.myFunction
	WHERE [name] LIKE ''me%''
/*
                    This is a FROM in a star comment
                */
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

        // <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void QualifiedNamesPolicyTest_TriggerInsertedTableIgnore()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"		

	DECLARE @TrxId uniqueidentifier
	SET @TrxId = newid()
	
	DECLARE @count int
	SELECT @count = count(*) FROM inserted
	
	INSERT INTO [dbo].[AuditTransactionMaster] (TransId,TableName,ModifyType,RowsAffected) VALUES (@TrxId,'Person.Address','UPDATE',@count)
	INSERT INTO [Person].[Address_Audit] (TransId,AddressID,AddressLine1,AddressLine2,City,StateProvinceID,PostalCode,rowguid,ModifiedDate) SELECT @TrxId, AddressID,AddressLine1,AddressLine2,City,StateProvinceID,PostalCode,rowguid,ModifiedDate FROM inserted
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

        // <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void QualifiedNamesPolicyTest_TriggerForUpdateASIgnore()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"		
				
	CREATE TRIGGER Address_AuditTrig_UPDATE ON [Person].[Address] FOR UPDATE
AS
BEGIN
	DECLARE @TrxId uniqueidentifier
	SET @TrxId = newid()
	
	DECLARE @count int
	SELECT @count = count(*) FROM inserted
	
	INSERT INTO [dbo].[AuditTransactionMaster] (TransId,TableName,ModifyType,RowsAffected) VALUES (@TrxId,'Person.Address','UPDATE',@count)
	INSERT INTO [Person].[Address_Audit] (TransId,AddressID,AddressLine1,AddressLine2,City,StateProvinceID,PostalCode,rowguid,ModifiedDate) SELECT @TrxId, AddressID,AddressLine1,AddressLine2,City,StateProvinceID,PostalCode,rowguid,ModifiedDate FROM inserted
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

        // <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void QualifiedNamesPolicyTest_TriggerDeletedTableIgnore()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"		
				
	CREATE TRIGGER Address_AuditTrig_DELETE ON [Person].[Address] FOR DELETE
AS
BEGIN
	DECLARE @TrxId uniqueidentifier
	SET @TrxId = newid()

	DECLARE @count int
	SELECT @count = count(*) FROM deleted
	
	INSERT INTO [dbo].[AuditTransactionMaster] (TransId,TableName,ModifyType,RowsAffected) VALUES (@TrxId,'Person.Address','DELETE',@count)
	INSERT INTO [Person].[Address_Audit] (TransId,AddressID,AddressLine1,AddressLine2,City,StateProvinceID,PostalCode,rowguid,ModifiedDate) SELECT @TrxId, AddressID,AddressLine1,AddressLine2,City,StateProvinceID,PostalCode,rowguid,ModifiedDate FROM deleted
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

        // <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void QualifiedNamesPolicyTest_UpdateQualifiedWithNoWhere()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"UPDATE dbo.CompanyInfo
SET
	[Name]=@Name
	,Address=@Address
	,City=@City
	,State=@State
	,Zip=@Zip
	,Phone=@Phone
	,Fax=@Fax
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

        // <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void QualifiedNamesPolicyTest_UpdateNonQualifiedWithNoWhere()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"UPDATE CompanyInfo
SET
	[Name]=@Name
	,Address=@Address
	,City=@City
	,State=@State
	,Zip=@Zip
	,Phone=@Phone
	,Fax=@Fax
";



            string message = string.Empty;
            string messageExpected = "Missing schema qualifier on: CompanyInfo (line: 1)";
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);

        }

        [TestMethod()]
        public void QualifiedNamesPolicyTest_IgnoreForeignKeyAction()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ExportBatch_ExportFileType')
BEGIN

	ALTER TABLE tlo.ExportBatch ADD CONSTRAINT
	FK_ExportBatch_ExportFileType FOREIGN KEY
	(
	ExportFileTypeId
	) REFERENCES tlo.ExportFileType
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
END
";



            string message = string.Empty;
            string messageExpected = "";
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
        public void QualifiedNamesPolicyTest_CommonTableEntitiesDeclaration()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[tlo].[ExportTimeCardLog_ListByPage]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [abc].[abv 122]
			@Page			int,
			@RowsPerPage	int,
			@FromDate		DateTime,
			@ToDate			DateTime
		AS
BEGIN
		/******************************************************************************
		**		Name: t[abc].[abv 122]
		**		Desc: qwerty
		**              
		**		Auth: werwer
		**		Date: 10/20/2009
		*******************************************************************************
		**		Change History
		*******************************************************************************
		**		Date:		Author:		Description:
		**		-----		--------	--------------------------------
		**		
		*******************************************************************************/
	
		DECLARE @startRowIndex int
		DECLARE @endRowIndex int
		DECLARE @nofPages int
	
		SELECT @nofPages = CEILING(count(1)/(@RowsPerPage*1.0))
		FROM abc.qwerty
		WITH(NOLOCK)
		WHERE (RequestDate >= @FromDate) AND (RequestDate <= @ToDate)
		
		IF (@Page > @nofPages)
	BEGIN
			SET @Page = @nofPages
	END

	SET @startRowIndex = ((@Page-1) * @RowsPerPage) + 1
	SET @endRowIndex = @startRowIndex+@RowsPerPage - 1

	;WITH PagingCTE 
     (
		Id,
		LogId,
		RequestDate,
		ServiceType,
		Status,
		StartDate,
		EndDate,
		ReportGuid,
		FileType,
		SequenceNumber,
		RequestingUri,
		RespondingUri,
		Message,
		RowNumber
	)
	AS 
	(
		SELECT 
			Id,
			LogId,
			RequestDate,
			ServiceType,
			Status,
			StartDate,
			EndDate,
			ReportGuid,
			FileType,
			SequenceNumber,
			RequestingUri,
			RespondingUri,
			Message,
			[RowNumber] = ROW_NUMBER() OVER(ORDER BY RequestDate Desc)
		FROM abc.qwerty
		WITH(NOLOCK)
		WHERE (RequestDate >= @FromDate) AND (RequestDate <= @ToDate)
	)

	SELECT
		Id,
		LogId,
		RequestDate,
		ServiceType,
		Status,
		StartDate,
		EndDate,
		ReportGuid,
		FileType,
		SequenceNumber,
		RequestingUri,
		RespondingUri,
		Message
	FROM PagingCTE
	WITH(NOLOCK)
	WHERE RowNumber >=  @startRowIndex  AND RowNumber <=  @endRowIndex
	ORDER By RowNumber

END
' 

END
ELSE
BEGIN
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[tlo].[ExportTimeCardLog_ListByPage]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'
		ALTER PROCEDURE [abc].[abc123]
			@Page			int,
			@RowsPerPage	int,
			@FromDate		DateTime,
			@ToDate			DateTime
		AS
BEGIN
		/******************************************************************************
		**		Name: abc.qwerty
		**		Desc: For use with TLO Integration with HRO/Preview
		**              
		**		Auth: qweree
		**		Date: 10/20/2009
		*******************************************************************************
		**		Change History
		*******************************************************************************
		**		Date:		Author:		Description:
		**		-----		--------	--------------------------------
		**		
		*******************************************************************************/
	
		DECLARE @startRowIndex int
		DECLARE @endRowIndex int
		DECLARE @nofPages int
	
		SELECT @nofPages = CEILING(count(1)/(@RowsPerPage*1.0))
		FROM abc.qwerty
		WITH(NOLOCK)
		WHERE (RequestDate >= @FromDate) AND (RequestDate <= @ToDate)
		
		IF (@Page > @nofPages)
	BEGIN
			SET @Page = @nofPages
	END

	SET @startRowIndex = ((@Page-1) * @RowsPerPage) + 1
	SET @endRowIndex = @startRowIndex+@RowsPerPage - 1

	;WITH PagingCTE 
     (
		Id,
		LogId,
		RequestDate,
		ServiceType,
		Status,
		StartDate,
		EndDate,
		ReportGuid,
		FileType,
		SequenceNumber,
		RequestingUri,
		RespondingUri,
		Message,
		RowNumber
	)
	AS 
	(
		SELECT 
			Id,
			LogId,
			RequestDate,
			ServiceType,
			Status,
			StartDate,
			EndDate,
			ReportGuid,
			FileType,
			SequenceNumber,
			RequestingUri,
			RespondingUri,
			Message,
			[RowNumber] = ROW_NUMBER() OVER(ORDER BY RequestDate Desc)
		FROM abc.qwerty
		WITH(NOLOCK)
		WHERE (RequestDate >= @FromDate) AND (RequestDate <= @ToDate)
	)

	SELECT
		Id,
		LogId,
		RequestDate,
		ServiceType,
		Status,
		StartDate,
		EndDate,
		ReportGuid,
		FileType,
		SequenceNumber,
		RequestingUri,
		RespondingUri,
		Message
	FROM PagingCTE
	WITH(NOLOCK)
	WHERE RowNumber >=  @startRowIndex  AND RowNumber <=  @endRowIndex
	ORDER By RowNumber

END
' 
	END
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


        [TestMethod()]
        public void QualifiedNamesPolicyTest_WhereInComments()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"
/* Since this is in a comment block we should ignore the SELECT me  FROM commentTest WHERE this equals that */
";



            string message = string.Empty;
            string messageExpected = "";
            bool expected = true;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);

        }

        [TestMethod()]
        public void QualifiedNamesPolicyTest_WhereInComments2()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"
/* Since this is in a 
comment block we should
ignore the SELECT me  FROM commentTest WHERE this equals that
*/
";

            string message = string.Empty;
            string messageExpected = "";
            bool expected = true;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);

        }

        [TestMethod()]
        public void QualifiedNamesPolicyTest_WhereInComments3()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"
/* Since this is in a 
comment block we should
ignore the SELECT me  FROM commentTest WHERE this equals that
*/

But this SELECT me  FROM test WHERE should fail
";

            string message = string.Empty;
            string messageExpected = "Missing schema qualifier on: test (line: 7)";
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);

        }

        [TestMethod()]
        public void QualifiedNamesPolicyTest_WhereInComments4()
        {
            QualifiedNamesPolicy target = new QualifiedNamesPolicy();
            string script = @"
-- Since this is in a comment block we should ignore the SELECT me  FROM commentTest WHERE this equals that 
";



            string message = string.Empty;
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
