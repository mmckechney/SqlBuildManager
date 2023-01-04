using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace SqlBuildManager.ScriptHandling.UnitTest
{


    /// <summary>
    ///This is a test class for ScriptOptimizationTest and is intended
    ///to contain all ScriptOptimizationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ScriptOptimizationTest
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

        /// <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_FromWithNoFollow()
        {
            string rawScript = "SELECT * FROM TestTable";
            string expected = "SELECT * FROM TestTable WITH (NOLOCK) ";
            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(rawScript);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_FromWithSameLineWhere()
        {
            string rawScript = "SELECT * FROM TestTable WHERE Column1='test'";
            string expected = "SELECT * FROM TestTable WITH (NOLOCK) WHERE Column1='test'";
            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(rawScript);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_FromWithEndWhiteSpace()
        {
            string rawScript = @"SELECT * FROM TestTable
    ";
            string expected = @"SELECT * FROM TestTable WITH (NOLOCK) 
";
            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(rawScript);
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_FromWithLineBreak()
        {
            string rawScript = @"SELECT * FROM TestTable
WHERE Test = 4
";
            string expected = @"SELECT * FROM TestTable WITH (NOLOCK) 
WHERE Test = 4
";
            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(rawScript);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_WithSysObjectsIgnoreObject()
        {
            #region rawscript ...
            string rawScript = @"IF NOT EXISTS (SELECT * FROMFROM sys.objects WITH  WHERE object_id = OBJECT_ID(N'[ats].[RuleTemplates_ReadByClockID]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [ats].[RuleTemplates_ReadByClockID] 
	(
		@Clock_Id INT
	) 
	AS
BEGIN
";
            #endregion
            string expected = rawScript;
            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(rawScript);
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_WithInfoSchemaIgnoreObject()
        {
            #region rawscript ...
            string rawScript = @"IF NOT EXISTS(SELECT * FROM information_schema.columns WHERE TABLE_NAME = 'TestTable' AND TABLE_SCHEMA = 'dbo' AND COLUMN_NAME = 'TestColumn')
BEGIN
	ALTER TABLE [dbo].[TestTable] ADD TestColumn BIT NOT NULL
END
GO
";
            #endregion
            string expected = rawScript;
            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(rawScript);
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_WithSysObjsObject()
        {
            #region rawscript ...
            string rawScript = @"SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ats].[RuleTemplates_ReadByClockID]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [ats].[RuleTemplates_ReadByClockID] 
		(
			@Clock_Id INT
		) 
		AS
BEGIN

";
            #endregion
            string expected = rawScript;
            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(rawScript);
            Assert.AreEqual(expected, actual);
        }


        /// <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_WithTwoSysObjsObject()
        {
            #region rawscript ...
            string rawScript = @"SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ats].[RuleTemplates_ReadByClockID]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [ats].[RuleTemplates_ReadByClockID] 
		(
			@Clock_Id INT
		) 
		AS

        SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ats].[RuleTemplates_ReadByClockID]') AND type in (N'P', N'PC'
BEGIN

";
            #endregion
            string expected = rawScript;
            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(rawScript);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_WithTwoSysObjsObjectNissingNoLock()
        {
            #region rawscript ...
            string rawScript = @"SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ats].[RuleTemplates_ReadByClockID]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [ats].[RuleTemplates_ReadByClockID] 
		(
			SELECT * FROM BadTable
		) 
		AS

        SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ats].[RuleTemplates_ReadByClockID]') AND type in (N'P', N'PC'
BEGIN

";
            #endregion
            string expected = @"SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ats].[RuleTemplates_ReadByClockID]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [ats].[RuleTemplates_ReadByClockID] 
		(
			SELECT * FROM BadTable WITH (NOLOCK) 
		) 
		AS

        SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ats].[RuleTemplates_ReadByClockID]') AND type in (N'P', N'PC'
BEGIN

";
            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(rawScript);
            Assert.AreEqual(expected, actual);
        }



        /// <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_WithOldSysObjsObject()
        {
            #region rawscript ...
            string rawScript = @"SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ats].[RuleTemplates_ReadByClockID]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [ats].[RuleTemplates_ReadByClockID] 
		(
			@Clock_Id INT
		) 
		AS
BEGIN

";
            #endregion
            string expected = rawScript;
            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(rawScript);
            Assert.AreEqual(expected, actual);
        }

        // <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_WithSysTriggersObject()
        {
            #region rawscript ...
            string rawScript = @"IF NOT EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[Person].[uAddressType]'))
EXEC dbo.sp_executesql @statement = N'
CREATE TRIGGER [Person].[uAddressType] ON [Person].[AddressType] 
AFTER UPDATE NOT FOR REPLICATION AS 
BEGIN
    SET NOCOUNT ON;

    UPDATE [Person].[AddressType]
    SET [Person].[AddressType].[ModifiedDate] = GETDATE()
    FROM inserted WITH (NOLOCK)
    WHERE inserted.[AddressTypeID] = [Person].[AddressType].[AddressTypeID];
END;
";
            #endregion
            string expected = rawScript;
            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(rawScript);
            Assert.AreEqual(expected, actual);
        }

        // <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_WithSysForeignKeyCols()
        {
            #region rawscript ...
            string rawScript = @"IF NOT EXISTS (SELECT * FROM 
                    sys.foreign_key_columns WHERE object_id = OBJECT_ID(N'[Person].[uAddressType]'))
";
            #endregion
            string expected = rawScript;
            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(rawScript);
            Assert.AreEqual(expected, actual);
        }
        // <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_WithSysIndexesObject()
        {
            #region rawscript ...
            string rawScript = @"IF NOT  EXISTS (SELECT * FROM sysindexes WHERE name = 'myIndex'  AND OBJECT_NAME(id) = N'myTable')
BEGIN
--	 some script here
END
GO
";
            #endregion
            string expected = rawScript;
            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(rawScript);
            Assert.AreEqual(expected, actual);
        }
        // <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_WithCursor()
        {
            #region rawscript ...
            string rawScript = @"DECLARE @TableName varchar(256)
SET @TableName = '<<Table Name>>'
SET @Schema = '<<Table Schema>>'
DECLARE @FkName varchar(500)
DECLARE fkCursor CURSOR FOR 
	SELECT CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS  
		WHERE Table_name = @TableName AND
			  CONSTRAINT_TYPE = 'FOREIGN KEY'

OPEN fkCursor
FETCH NEXT FROM fkCursor INTO @FkName 
WHILE @@FETCH_STATUS = 0
	BEGIN
		PRINT 'Droping Constraint '+@FkName +' off Table '+@Schema+'.'+@TableName
		EXEC ('ALTER TABLE '+@Schema+'.'+@TableName+' DROP CONSTRAINT '+@FkName)
		FETCH NEXT FROM fkCursor INTO @FkName 
	END
CLOSE fkCursor
DEALLOCATE fkCursor 
GO
";
            #endregion
            string expected = rawScript;
            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(rawScript);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for CheckPolicy - correction for Bug Fix 1
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_BugFix1()
        {
            string rawScript = Properties.Resources.NoLockBugFix1_Input;

            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(rawScript);
            Assert.AreEqual(Properties.Resources.NoLockBugFix1_Expected, actual);

        }

        // <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_InnerJoin()
        {
            string rawScript = @"SELECT 
              p.DepartmentPageSize
              , p.ColorBackgroundTable
              , p.AdminBenefitRequestState
              , p.ScheduleTemplatesShowPublic
              
       FROM
               MyTablePrefs p 
               Inner Join Themes t on 
               p.ThemeId = t.ThemeId
       WHERE
              p.CustomerID = @CustomerID
              AND p.UserID = @UserID
       ORDER BY p.MyTablePreferenceID DESC";


            string expected = @"SELECT 
              p.DepartmentPageSize
              , p.ColorBackgroundTable
              , p.AdminBenefitRequestState
              , p.ScheduleTemplatesShowPublic
              
       FROM
               MyTablePrefs p WITH (NOLOCK)  
               Inner Join Themes t WITH (NOLOCK) on 
               p.ThemeId = t.ThemeId
       WHERE
              p.CustomerID = @CustomerID
              AND p.UserID = @UserID
       ORDER BY p.MyTablePreferenceID DESC";
            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(rawScript);
            Assert.AreEqual(expected, actual);

        }
        // <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_MultipleJoins()
        {
            string rawScript = @"SELECT 
              p.DepartmentPageSize
              , p.ColorBackgroundTable
              , p.AdminBenefitRequestState
              , p.ScheduleTemplatesShowPublic
              
       FROM
               MyTablePrefs p 
               Inner Join Themes t on p.ThemeId = t.ThemeId
               RIGHT JOIN table2 t2 ON t2.Theme = p.Theme
               OUTER JOIN table3 t3 
                    ON t3.theme = t2.theme
       WHERE
              p.CustomerID = @CustomerID
              AND p.UserID = @UserID
       ORDER BY p.MyTablePreferenceID DESC";


            string expected = @"SELECT 
              p.DepartmentPageSize
              , p.ColorBackgroundTable
              , p.AdminBenefitRequestState
              , p.ScheduleTemplatesShowPublic
              
       FROM
               MyTablePrefs p WITH (NOLOCK)  
               Inner Join Themes t WITH (NOLOCK) on p.ThemeId = t.ThemeId
               RIGHT JOIN table2 t2 WITH (NOLOCK) ON t2.Theme = p.Theme
               OUTER JOIN table3 t3 WITH (NOLOCK)  
                    ON t3.theme = t2.theme
       WHERE
              p.CustomerID = @CustomerID
              AND p.UserID = @UserID
       ORDER BY p.MyTablePreferenceID DESC";
            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(rawScript);
            Assert.AreEqual(expected, actual);

        }


        // <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_WithDeleteFrom()
        {
            string rawScript = @"DELETE FROM MyTable mt WHERE mt.Address='1st Street'";


            string expected = rawScript;
            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(rawScript);
            Assert.AreEqual(expected, actual);

        }

        // <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_WithDeleteFromWithSubQuery()
        {
            string rawScript = @"DELETE FROM MyTable mt 
WHERE mt.Address='1st Street' AND
mt.LastName IN (SELECT LastName FROM Names WHERE FirstName = 'Mike'";


            string expected = @"DELETE FROM MyTable mt 
WHERE mt.Address='1st Street' AND
mt.LastName IN (SELECT LastName FROM Names WITH (NOLOCK) WHERE FirstName = 'Mike'"; ;
            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(rawScript);
            Assert.AreEqual(expected, actual);

        }

        // <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_WithUpdate()
        {
            string rawScript = @"UPDATE MyTable mt 
SET mt.Address='1st Street' AND
mt.LastName = 'Jones
WHERE mt.FirstName LIKE ('%Smith%')";


            string expected = rawScript;
            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(rawScript);
            Assert.AreEqual(expected, actual);

        }

        // <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_WithUpdateAndSubQuery()
        {
            string rawScript = @"UPDATE MyTable mt 
SET mt.Address='1st Street' AND
mt.LastName = 'Jones
WHERE mt.FirstName IN (SELECT FName FROM People p WHERE p.MiddleName = 'Harry')";


            string expected = @"UPDATE MyTable mt 
SET mt.Address='1st Street' AND
mt.LastName = 'Jones
WHERE mt.FirstName IN (SELECT FName FROM People p WITH (NOLOCK) WHERE p.MiddleName = 'Harry')";

            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(rawScript);
            Assert.AreEqual(expected, actual);

        }

        // <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_WithInsert()
        {
            string rawScript = @"INSERT INTO IPTableA
	            (IPTableAName, AllowX, AllowY)
	VALUES  (@IPTableAName,@AllowX,@AllowY)";


            string expected = rawScript;

            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(rawScript);
            Assert.AreEqual(expected, actual);

        }

        // <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_WithInsertAndSubQuery()
        {
            string rawScript = @"INSERT INTO IPTableA
	            (IPTableAName, AllowX, AllowY)
	VALUES  (
        (SELECT TOP 1 Name FROM MyTable WHERE Name LIKE 't%'), (SELECT Top 1 Allow FROM TableX), (SELECT Top 1 Allow FROM TableY)

)";


            string expected = @"INSERT INTO IPTableA
	            (IPTableAName, AllowX, AllowY)
	VALUES  (
        (SELECT TOP 1 Name FROM MyTable WITH (NOLOCK) WHERE Name LIKE 't%'), (SELECT Top 1 Allow FROM TableX WITH (NOLOCK) ), (SELECT Top 1 Allow FROM TableY WITH (NOLOCK) )

)"; ;

            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(rawScript);
            Assert.AreEqual(expected, actual);

        }

        // <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_BugFix2()
        {
            string rawScript = @"
        BEGIN
		-- SET NOCOUNT ON added to prevent extra result sets from
		-- interfering with SELECT statements.
		SET NOCOUNT ON;
	
		SELECT TOP 1
	        GlobalConfigID,
	        UseLDAP,
	        LdapPath,
	        LdapDomain,
	        IsLicensed
	    FROM
	        dbo.GlobalConfig
END";


            string expected = @"
        BEGIN
		-- SET NOCOUNT ON added to prevent extra result sets from
		-- interfering with SELECT statements.
		SET NOCOUNT ON;
	
		SELECT TOP 1
	        GlobalConfigID,
	        UseLDAP,
	        LdapPath,
	        LdapDomain,
	        IsLicensed
	    FROM
	        dbo.GlobalConfig WITH (NOLOCK) 
END";

            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(rawScript);
            Assert.AreEqual(expected, actual);

        }


        // <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_TempTableSelect()
        {
            string rawScript = @"		
				SELECT  Statistic,
					Today,
					Yesterday FROM
                    #CustomerStatistics
";


            string expected = rawScript;

            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(rawScript);
            Assert.AreEqual(expected, actual);

        }
        // <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_TempTableSelectWithWhere()
        {
            string script = @"		
				SELECT  Statistic,
					Today,
					Yesterday FROM
                    #CustomerStatistics WHERE
                Today = getdate()
";



            string expected = script;

            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(script);
            Assert.AreEqual(expected, actual);

        }

        // <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_TableVariableSelect()
        {
            string rawScript = @"		
				SELECT  Statistic,
					Today,
					Yesterday FROM
                    @CustomerStatistics
";


            string expected = rawScript;

            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(rawScript);
            Assert.AreEqual(expected, actual);

        }

        // <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_TableVariableSelectWithWhere()
        {
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


            string expected = script;

            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(script);
            Assert.AreEqual(expected, actual);

        }

        // <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_FunctionSelects()
        {
            string script = @"
				SELECT  Statistic,
					Today,
					Yesterday FROM
                dbo.func_GetMyDate('today') WHERE
                Today = getdate()
";


            string expected = script;

            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(script);
            Assert.AreEqual(expected, actual);

        }

        // <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_FunctionNoParams()
        {
            string script = @"
				SELECT  Statistic,
					Today,
					Yesterday FROM
                dbo.func_GetMyDate() f INNER JOIN dbo.myTable t WITH (NOLOCK) ON t.myCol = f.Yesterday WHERE
                Today = getdate()
";


            string expected = script;

            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(script);
            Assert.AreEqual(expected, actual);

        }

        // <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_FromInDashComment()
        {
            string script = @"
				SELECT  Statistic,
					Today,
                    -- just adding a FROM in an dash comment
					Yesterday FROM
                dbo.func_GetMyDate() f INNER JOIN dbo.myTable t WITH (NOLOCK) ON t.myCol = f.Yesterday WHERE
                Today = getdate()
";


            string expected = script;

            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(script);
            Assert.AreEqual(expected, actual);

        }

        // <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_FromInDashCommentAtEnd()
        {
            string script = @"
				SELECT  Statistic,
					Today,
                    
					Yesterday FROM
                dbo.func_GetMyDate() f INNER JOIN dbo.myTable t WITH (NOLOCK) ON t.myCol = f.Yesterday WHERE
                Today = getdate()
            -- just adding a FROM in an dash comment at end
";


            string expected = script;

            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(script);
            Assert.AreEqual(expected, actual);

        }

        // <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_FromInStarCommentAtEnd()
        {
            string script = @"
				SELECT  Statistic,
					Today,
                    
					Yesterday FROM
                dbo.func_GetMyDate() f INNER JOIN dbo.myTable t WITH (NOLOCK) ON t.myCol = f.Yesterday WHERE
                Today = getdate()
            /**************
            * This is a FROM comment
            ***************/
";


            string expected = script;

            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(script);
            Assert.AreEqual(expected, actual);

        }

        // <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_FromInStarComment()
        {
            string script = @"
                 /**************
                 * This is a FROM comment
                 ***************/
				SELECT  Statistic,
					Today,
                    
					Yesterday FROM
                dbo.func_GetMyDate() f INNER JOIN dbo.myTable t WITH (NOLOCK) ON t.myCol = f.Yesterday WHERE
                Today = getdate()
           
";


            string expected = script;

            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(script);
            Assert.AreEqual(expected, actual);

        }

        // <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_SelectFromTriggerInsertedIgnore()
        {
            string script = @"BEGIN
	DECLARE @TrxId uniqueidentifier
	SET @TrxId = newid()
	
	DECLARE @count int
	SELECT @count = count(*) FROM inserted
	
	INSERT INTO [dbo].[AuditTransactionMaster] (TransId,TableName,ModifyType,RowsAffected) VALUES (@TrxId,'Person.Address','UPDATE',@count)
	INSERT INTO [Person].[Address_Audit] (TransId,AddressID,AddressLine1,AddressLine2,City,StateProvinceID,PostalCode,rowguid,ModifiedDate) SELECT @TrxId, AddressID,AddressLine1,AddressLine2,City,StateProvinceID,PostalCode,rowguid,ModifiedDate FROM inserted
END
";
            string expected = script;

            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(script);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for ProcessNoLockOptimization
        ///</summary>
        [TestMethod()]
        public void ProcessNoLockOptimizationTest_SelectFromTriggerDeletedIgnore()
        {
            string script = @"BEGIN
	DECLARE @TrxId uniqueidentifier
	SET @TrxId = newid()

	DECLARE @count int
	SELECT @count = count(*) FROM deleted
	
	INSERT INTO [dbo].[AuditTransactionMaster] (TransId,TableName,ModifyType,RowsAffected) VALUES (@TrxId,'Person.Address','DELETE',@count)
	INSERT INTO [Person].[Address_Audit] (TransId,AddressID,AddressLine1,AddressLine2,City,StateProvinceID,PostalCode,rowguid,ModifiedDate) SELECT @TrxId, AddressID,AddressLine1,AddressLine2,City,StateProvinceID,PostalCode,rowguid,ModifiedDate FROM deleted
END
";


            string expected = script;

            string actual;
            actual = ScriptOptimization.ProcessNoLockOptimization(script);
            Assert.AreEqual(expected, actual);

        }

    }



}


