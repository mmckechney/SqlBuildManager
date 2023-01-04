using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Enterprise.Policy;
namespace SqlBuildManager.Enterprise.UnitTest
{


    /// <summary>
    ///This is a test class for GrantExecutePolicyTest and is intended
    ///to contain all GrantExecutePolicyTest Unit Tests
    ///</summary>
    [TestClass()]
    public class GrantExecutePolicyTest
    {
        /// <summary>
        ///A test for PolicyId
        ///</summary>
        [TestMethod()]
        public void PolicyIdTest()
        {
            GrantExecutePolicy target = new GrantExecutePolicy();
            string actual;
            actual = target.PolicyId;
            string expected = "GrantExecutePolicy";
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

        #region .: CheckPolicy Tests :.
        /// <summary>
        ///A test for CheckPolicy where both the script and the GRANT EXECUTE have bracketed routine and schema names
        ///</summary>
        [TestMethod()]
        public void CheckPolicyTest_AllBracketed()
        {
            GrantExecutePolicy target = new GrantExecutePolicy();
            #region script...
            string script = @"/* 
Source Server:	(local)
Source Db:	AdventureWorks
Process Date:	12/11/2008 9:56:37 PM
Object Scripted:HumanResources.uspUpdateEmployeePersonalInfo
Object Type:	Stored Procedure
Scripted By:	mmckechn
Include Permissions: True
Script as ALTER: True
*/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HumanResources].[uspUpdateEmployeePersonalInfo]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'
	CREATE PROCEDURE [HumanResources].[uspUpdateEmployeePersonalInfo]
	    @EmployeeID [int], 
	    @NationalIDNumber [nvarchar](15), 
	    @BirthDate [datetime], 
	    @MaritalStatus [nchar](1), 
	    @Gender [nchar](1)
	WITH EXECUTE AS CALLER
	AS
BEGIN
	    SET NOCOUNT ON;
	
	    BEGIN TRY
	        UPDATE [HumanResources].[Employee] 
	        SET [NationalIDNumber] = @NationalIDNumber 
	            ,[BirthDate] = @BirthDate 
	            ,[MaritalStatus] = @MaritalStatus 
	            ,[Gender] = @Gender 
	        WHERE [EmployeeID] = @EmployeeID;
	    END TRY
	    BEGIN CATCH
	        EXECUTE [dbo].[uspLogError];
	    END CATCH;
	END;
	' 
	PRINT 'CREATED Stored Procedure: HumanResources.uspUpdateEmployeePersonalInfo'
END
ELSE
BEGIN
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HumanResources].[uspUpdateEmployeePersonalInfo]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'
	ALTER PROCEDURE [HumanResources].[uspUpdateEmployeePersonalInfo]
	    @EmployeeID [int], 
	    @NationalIDNumber [nvarchar](15), 
	    @BirthDate [datetime], 
	    @MaritalStatus [nchar](1), 
	    @Gender [nchar](1)
	WITH EXECUTE AS CALLER
	AS
BEGIN
	    SET NOCOUNT ON;
	
	    BEGIN TRY
	        UPDATE [HumanResources].[Employee] 
	        SET [NationalIDNumber] = @NationalIDNumber 
	            ,[BirthDate] = @BirthDate 
	            ,[MaritalStatus] = @MaritalStatus 
	            ,[Gender] = @Gender 
	        WHERE [EmployeeID] = @EmployeeID;
	    END TRY
	    BEGIN CATCH
	        EXECUTE [dbo].[uspLogError];
	    END CATCH;
	END;
	' 
	PRINT 'ALTERED Stored Procedure: HumanResources.uspUpdateEmployeePersonalInfo'
	END
END
GO

GRANT EXECUTE ON [HumanResources].[uspUpdateEmployeePersonalInfo] TO [public]
GO";
            #endregion
            string message;
            string messageExpected = string.Empty;
            bool expected = true;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        ///A test for CheckPolicy where both the script and the GRANT EXECUTE a mix of bracketed and non-bracketed syntax
        ///</summary>
        [TestMethod()]
        public void CheckPolicyTest_MixedBrackets()
        {
            GrantExecutePolicy target = new GrantExecutePolicy();
            #region script...
            string script = @"/* 
Source Server:	(local)
Source Db:	AdventureWorks
Process Date:	12/11/2008 9:56:37 PM
Object Scripted:HumanResources.uspUpdateEmployeePersonalInfo
Object Type:	Stored Procedure
Scripted By:	mmckechn
Include Permissions: True
Script as ALTER: True
*/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HumanResources].[uspUpdateEmployeePersonalInfo]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'
	CREATE PROCEDURE [HumanResources].[uspUpdateEmployeePersonalInfo]
	    @EmployeeID [int], 
	    @NationalIDNumber [nvarchar](15), 
	    @BirthDate [datetime], 
	    @MaritalStatus [nchar](1), 
	    @Gender [nchar](1)
	WITH EXECUTE AS CALLER
	AS
BEGIN
	    SET NOCOUNT ON;
	
	    BEGIN TRY
	        UPDATE [HumanResources].[Employee] 
	        SET [NationalIDNumber] = @NationalIDNumber 
	            ,[BirthDate] = @BirthDate 
	            ,[MaritalStatus] = @MaritalStatus 
	            ,[Gender] = @Gender 
	        WHERE [EmployeeID] = @EmployeeID;
	    END TRY
	    BEGIN CATCH
	        EXECUTE [dbo].[uspLogError];
	    END CATCH;
	END;
	' 
	PRINT 'CREATED Stored Procedure: HumanResources.uspUpdateEmployeePersonalInfo'
END
ELSE
BEGIN
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HumanResources].[uspUpdateEmployeePersonalInfo]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'
	ALTER PROCEDURE [HumanResources].[uspUpdateEmployeePersonalInfo]
	    @EmployeeID [int], 
	    @NationalIDNumber [nvarchar](15), 
	    @BirthDate [datetime], 
	    @MaritalStatus [nchar](1), 
	    @Gender [nchar](1)
	WITH EXECUTE AS CALLER
	AS
BEGIN
	    SET NOCOUNT ON;
	
	    BEGIN TRY
	        UPDATE [HumanResources].[Employee] 
	        SET [NationalIDNumber] = @NationalIDNumber 
	            ,[BirthDate] = @BirthDate 
	            ,[MaritalStatus] = @MaritalStatus 
	            ,[Gender] = @Gender 
	        WHERE [EmployeeID] = @EmployeeID;
	    END TRY
	    BEGIN CATCH
	        EXECUTE [dbo].[uspLogError];
	    END CATCH;
	END;
	' 
	PRINT 'ALTERED Stored Procedure: HumanResources.uspUpdateEmployeePersonalInfo'
	END
END
GO

GRANT EXECUTE ON HumanResources.uspUpdateEmployeePersonalInfo TO [public]
GO";
            #endregion
            string message;
            string messageExpected = string.Empty;
            bool expected = true;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        public void CheckPolicyTest_ShortFormExec()
        {
            GrantExecutePolicy target = new GrantExecutePolicy();
            #region script...
            string script = @"/* 
Source Server:	(local)
Source Db:	AdventureWorks
Process Date:	12/11/2008 9:56:37 PM
Object Scripted:HumanResources.uspUpdateEmployeePersonalInfo
Object Type:	Stored Procedure
Scripted By:	mmckechn
Include Permissions: True
Script as ALTER: True
*/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HumanResources].[uspUpdateEmployeePersonalInfo]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'
	CREATE PROCEDURE [HumanResources].[uspUpdateEmployeePersonalInfo]
	    @EmployeeID [int], 
	    @NationalIDNumber [nvarchar](15), 
	    @BirthDate [datetime], 
	    @MaritalStatus [nchar](1), 
	    @Gender [nchar](1)
	WITH EXECUTE AS CALLER
	AS
BEGIN
	    SET NOCOUNT ON;
	
	    BEGIN TRY
	        UPDATE [HumanResources].[Employee] 
	        SET [NationalIDNumber] = @NationalIDNumber 
	            ,[BirthDate] = @BirthDate 
	            ,[MaritalStatus] = @MaritalStatus 
	            ,[Gender] = @Gender 
	        WHERE [EmployeeID] = @EmployeeID;
	    END TRY
	    BEGIN CATCH
	        EXECUTE [dbo].[uspLogError];
	    END CATCH;
	END;
	' 
	PRINT 'CREATED Stored Procedure: HumanResources.uspUpdateEmployeePersonalInfo'
END
ELSE
BEGIN
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HumanResources].[uspUpdateEmployeePersonalInfo]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'
	ALTER PROCEDURE [HumanResources].[uspUpdateEmployeePersonalInfo]
	    @EmployeeID [int], 
	    @NationalIDNumber [nvarchar](15), 
	    @BirthDate [datetime], 
	    @MaritalStatus [nchar](1), 
	    @Gender [nchar](1)
	WITH EXECUTE AS CALLER
	AS
BEGIN
	    SET NOCOUNT ON;
	
	    BEGIN TRY
	        UPDATE [HumanResources].[Employee] 
	        SET [NationalIDNumber] = @NationalIDNumber 
	            ,[BirthDate] = @BirthDate 
	            ,[MaritalStatus] = @MaritalStatus 
	            ,[Gender] = @Gender 
	        WHERE [EmployeeID] = @EmployeeID;
	    END TRY
	    BEGIN CATCH
	        EXECUTE [dbo].[uspLogError];
	    END CATCH;
	END;
	' 
	PRINT 'ALTERED Stored Procedure: HumanResources.uspUpdateEmployeePersonalInfo'
	END
END
GO

GRANT EXEC ON HumanResources.uspUpdateEmployeePersonalInfo TO [public]
GO";
            #endregion
            string message;
            string messageExpected = string.Empty;
            bool expected = true;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for CheckPolicy where the routine is missing a GRANT EXECUTE
        ///</summary>
        [TestMethod()]
        public void CheckPolicyTest_MissingExecute()
        {
            GrantExecutePolicy target = new GrantExecutePolicy();
            #region script...
            string script = @"/* 
Source Server:	(local)
Source Db:	AdventureWorks
Process Date:	12/11/2008 9:56:37 PM
Object Scripted:HumanResources.uspUpdateEmployeePersonalInfo
Object Type:	Stored Procedure
Scripted By:	mmckechn
Include Permissions: True
Script as ALTER: True
*/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HumanResources].[uspUpdateEmployeePersonalInfo]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'
	CREATE PROCEDURE [HumanResources].[uspUpdateEmployeePersonalInfo]
	    @EmployeeID [int], 
	    @NationalIDNumber [nvarchar](15), 
	    @BirthDate [datetime], 
	    @MaritalStatus [nchar](1), 
	    @Gender [nchar](1)
	WITH EXECUTE AS CALLER
	AS
BEGIN
	    SET NOCOUNT ON;
	
	    BEGIN TRY
	        UPDATE [HumanResources].[Employee] 
	        SET [NationalIDNumber] = @NationalIDNumber 
	            ,[BirthDate] = @BirthDate 
	            ,[MaritalStatus] = @MaritalStatus 
	            ,[Gender] = @Gender 
	        WHERE [EmployeeID] = @EmployeeID;
	    END TRY
	    BEGIN CATCH
	        EXECUTE [dbo].[uspLogError];
	    END CATCH;
	END;
	' 
	PRINT 'CREATED Stored Procedure: HumanResources.uspUpdateEmployeePersonalInfo'
END
ELSE
BEGIN
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HumanResources].[uspUpdateEmployeePersonalInfo]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'
	ALTER PROCEDURE [HumanResources].[uspUpdateEmployeePersonalInfo]
	    @EmployeeID [int], 
	    @NationalIDNumber [nvarchar](15), 
	    @BirthDate [datetime], 
	    @MaritalStatus [nchar](1), 
	    @Gender [nchar](1)
	WITH EXECUTE AS CALLER
	AS
BEGIN
	    SET NOCOUNT ON;
	
	    BEGIN TRY
	        UPDATE [HumanResources].[Employee] 
	        SET [NationalIDNumber] = @NationalIDNumber 
	            ,[BirthDate] = @BirthDate 
	            ,[MaritalStatus] = @MaritalStatus 
	            ,[Gender] = @Gender 
	        WHERE [EmployeeID] = @EmployeeID;
	    END TRY
	    BEGIN CATCH
	        EXECUTE [dbo].[uspLogError];
	    END CATCH;
	END;
	' 
	PRINT 'ALTERED Stored Procedure: HumanResources.uspUpdateEmployeePersonalInfo'
	END
END
GO
";
            #endregion
            string message;
            string messageExpected = "Missing execute on HumanResources.uspUpdateEmployeePersonalInfo";
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        ///A test for CheckPolicy where the routine does not have a defining schema nor a GRANT EXECUTE
        ///</summary>
        [TestMethod()]
        public void CheckPolicyTest_MissingExecuteNoSchema()
        {
            GrantExecutePolicy target = new GrantExecutePolicy();
            #region script...
            string script = @"/* 
Source Server:	(local)
Source Db:	AdventureWorks
Process Date:	12/11/2008 9:56:37 PM
Object Scripted:HumanResources.uspUpdateEmployeePersonalInfo
Object Type:	Stored Procedure
Scripted By:	mmckechn
Include Permissions: True
Script as ALTER: True
*/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[uspUpdateEmployeePersonalInfo]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'
	CREATE PROCEDURE [uspUpdateEmployeePersonalInfo]
	    @EmployeeID [int], 
	    @NationalIDNumber [nvarchar](15), 
	    @BirthDate [datetime], 
	    @MaritalStatus [nchar](1), 
	    @Gender [nchar](1)
	WITH EXECUTE AS CALLER
	AS
BEGIN
	    SET NOCOUNT ON;
	
	    BEGIN TRY
	        UPDATE [HumanResources].[Employee] 
	        SET [NationalIDNumber] = @NationalIDNumber 
	            ,[BirthDate] = @BirthDate 
	            ,[MaritalStatus] = @MaritalStatus 
	            ,[Gender] = @Gender 
	        WHERE [EmployeeID] = @EmployeeID;
	    END TRY
	    BEGIN CATCH
	        EXECUTE [dbo].[uspLogError];
	    END CATCH;
	END;
	' 
	PRINT 'CREATED Stored Procedure: uspUpdateEmployeePersonalInfo'
END
ELSE
BEGIN
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[uspUpdateEmployeePersonalInfo]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'
	ALTER PROCEDURE [uspUpdateEmployeePersonalInfo]
	    @EmployeeID [int], 
	    @NationalIDNumber [nvarchar](15), 
	    @BirthDate [datetime], 
	    @MaritalStatus [nchar](1), 
	    @Gender [nchar](1)
	WITH EXECUTE AS CALLER
	AS
BEGIN
	    SET NOCOUNT ON;
	
	    BEGIN TRY
	        UPDATE [HumanResources].[Employee] 
	        SET [NationalIDNumber] = @NationalIDNumber 
	            ,[BirthDate] = @BirthDate 
	            ,[MaritalStatus] = @MaritalStatus 
	            ,[Gender] = @Gender 
	        WHERE [EmployeeID] = @EmployeeID;
	    END TRY
	    BEGIN CATCH
	        EXECUTE [dbo].[uspLogError];
	    END CATCH;
	END;
	' 
	PRINT 'ALTERED Stored Procedure: uspUpdateEmployeePersonalInfo'
	END
END
GO
";
            #endregion
            string message;
            string messageExpected = "Missing execute on uspUpdateEmployeePersonalInfo";
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        ///A test for CheckPolicy where the routine does not have a defining schema with the bracket syntax
        ///</summary>
        [TestMethod()]
        public void CheckPolicyTest_NoSchemaWithBrackets()
        {
            GrantExecutePolicy target = new GrantExecutePolicy();
            #region script...
            string script = @"/* 
Source Server:	(local)
Source Db:	AdventureWorks
Process Date:	12/11/2008 9:56:37 PM
Object Scripted:HumanResources.uspUpdateEmployeePersonalInfo
Object Type:	Stored Procedure
Scripted By:	mmckechn
Include Permissions: True
Script as ALTER: True
*/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[uspUpdateEmployeePersonalInfo]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'
	CREATE PROCEDURE [uspUpdateEmployeePersonalInfo]
	    @EmployeeID [int], 
	    @NationalIDNumber [nvarchar](15), 
	    @BirthDate [datetime], 
	    @MaritalStatus [nchar](1), 
	    @Gender [nchar](1)
	WITH EXECUTE AS CALLER
	AS
BEGIN
	    SET NOCOUNT ON;
	
	    BEGIN TRY
	        UPDATE [HumanResources].[Employee] 
	        SET [NationalIDNumber] = @NationalIDNumber 
	            ,[BirthDate] = @BirthDate 
	            ,[MaritalStatus] = @MaritalStatus 
	            ,[Gender] = @Gender 
	        WHERE [EmployeeID] = @EmployeeID;
	    END TRY
	    BEGIN CATCH
	        EXECUTE [dbo].[uspLogError];
	    END CATCH;
	END;
	' 
	PRINT 'CREATED Stored Procedure: uspUpdateEmployeePersonalInfo'
END
ELSE
BEGIN
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[uspUpdateEmployeePersonalInfo]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'
	ALTER PROCEDURE [uspUpdateEmployeePersonalInfo]
	    @EmployeeID [int], 
	    @NationalIDNumber [nvarchar](15), 
	    @BirthDate [datetime], 
	    @MaritalStatus [nchar](1), 
	    @Gender [nchar](1)
	WITH EXECUTE AS CALLER
	AS
BEGIN
	    SET NOCOUNT ON;
	
	    BEGIN TRY
	        UPDATE [HumanResources].[Employee] 
	        SET [NationalIDNumber] = @NationalIDNumber 
	            ,[BirthDate] = @BirthDate 
	            ,[MaritalStatus] = @MaritalStatus 
	            ,[Gender] = @Gender 
	        WHERE [EmployeeID] = @EmployeeID;
	    END TRY
	    BEGIN CATCH
	        EXECUTE [dbo].[uspLogError];
	    END CATCH;
	END;
	' 
	PRINT 'ALTERED Stored Procedure: uspUpdateEmployeePersonalInfo'
	END
END
GO

GRANT EXECUTE ON uspUpdateEmployeePersonalInfo TO [public]
GO";
            #endregion
            string message;
            string messageExpected = "";
            bool expected = true;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        ///A test for CheckPolicy where the routine does not have a defining schema with the no bracket syntax
        ///</summary>
        [TestMethod()]
        public void CheckPolicyTest_NoSchemaNoBrackets()
        {
            GrantExecutePolicy target = new GrantExecutePolicy();
            #region script...
            string script = @"/* 
Source Server:	(local)
Source Db:	AdventureWorks
Process Date:	12/11/2008 9:56:37 PM
Object Scripted:uspUpdateEmployeePersonalInfo
Object Type:	Stored Procedure
Scripted By:	mmckechn
Include Permissions: True
Script as ALTER: True
*/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[uspUpdateEmployeePersonalInfo]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'
	CREATE PROCEDURE [uspUpdateEmployeePersonalInfo]
	    @EmployeeID [int], 
	    @NationalIDNumber [nvarchar](15), 
	    @BirthDate [datetime], 
	    @MaritalStatus [nchar](1), 
	    @Gender [nchar](1)
	WITH EXECUTE AS CALLER
	AS
BEGIN
	    SET NOCOUNT ON;
	
	    BEGIN TRY
	        UPDATE [Employee] 
	        SET [NationalIDNumber] = @NationalIDNumber 
	            ,[BirthDate] = @BirthDate 
	            ,[MaritalStatus] = @MaritalStatus 
	            ,[Gender] = @Gender 
	        WHERE [EmployeeID] = @EmployeeID;
	    END TRY
	    BEGIN CATCH
	        EXECUTE [dbo].[uspLogError];
	    END CATCH;
	END;
	' 
	PRINT 'CREATED Stored Procedure: uspUpdateEmployeePersonalInfo'
END
ELSE
BEGIN
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[uspUpdateEmployeePersonalInfo]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'
	ALTER PROCEDURE [uspUpdateEmployeePersonalInfo]
	    @EmployeeID [int], 
	    @NationalIDNumber [nvarchar](15), 
	    @BirthDate [datetime], 
	    @MaritalStatus [nchar](1), 
	    @Gender [nchar](1)
	WITH EXECUTE AS CALLER
	AS
BEGIN
	    SET NOCOUNT ON;
	
	    BEGIN TRY
	        UPDATE [Employee] 
	        SET [NationalIDNumber] = @NationalIDNumber 
	            ,[BirthDate] = @BirthDate 
	            ,[MaritalStatus] = @MaritalStatus 
	            ,[Gender] = @Gender 
	        WHERE [EmployeeID] = @EmployeeID;
	    END TRY
	    BEGIN CATCH
	        EXECUTE [dbo].[uspLogError];
	    END CATCH;
	END;
	' 
	PRINT 'ALTERED Stored Procedure: uspUpdateEmployeePersonalInfo'
	END
END
GO

GRANT EXECUTE ON uspUpdateEmployeePersonalInfo TO [public]
GO";
            #endregion
            string message;
            string messageExpected = string.Empty;
            bool expected = true;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        ///A test for CheckPolicy for a function 
        ///</summary>
        [TestMethod()]
        public void CheckPolicyTest_FunctionWithBrackets()
        {
            GrantExecutePolicy target = new GrantExecutePolicy();
            #region script...
            string script = @"SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufnGetAccountingEndDate]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
	execute dbo.sp_executesql @statement = N'
	CREATE FUNCTION [dbo].[ufnGetAccountingEndDate]()
	RETURNS [datetime] 
	AS 
BEGIN
	    RETURN DATEADD(millisecond, -2, CONVERT(datetime, ''2004-07-01'', 101));
	END;
	' 
	PRINT 'CREATED Function: dbo.ufnGetAccountingEndDate'
END
ELSE
BEGIN
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufnGetAccountingEndDate]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
	execute dbo.sp_executesql @statement = N'
	ALTER FUNCTION [dbo].[ufnGetAccountingEndDate]()
	RETURNS [datetime] 
	AS 
BEGIN
	    RETURN DATEADD(millisecond, -2, CONVERT(datetime, ''2004-07-01'', 101));
	END;
	' 
	PRINT 'ALTERED Function: dbo.ufnGetAccountingEndDate'
	END
END
GO

GRANT EXECUTE ON [dbo].[ufnGetAccountingEndDate] TO [public]
GO

";
            #endregion
            string message;
            string messageExpected = string.Empty;
            bool expected = true;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        ///A test for CheckPolicy where there is no routine defined (fall through case)
        ///</summary>
        [TestMethod()]
        public void CheckPolicyTest_NoRoutines()
        {
            GrantExecutePolicy target = new GrantExecutePolicy();
            #region script...
            string script = @"SET ANSI_NULLS ON
GO


SET QUOTED_IDENTIFIER ON
GO


IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Person].[Address]') AND type in (N'U'))
BEGIN
CREATE TABLE [Person].[Address](
	[AddressID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[AddressLine1] [nvarchar](60) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[AddressLine2] [nvarchar](60) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[City] [nvarchar](30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[StateProvinceID] [int] NOT NULL,
	[PostalCode] [nvarchar](15) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[rowguid] [uniqueidentifier] ROWGUIDCOL  NOT NULL CONSTRAINT [DF_Address_rowguid]  DEFAULT (newid()),
	[ModifiedDate] [datetime] NOT NULL CONSTRAINT [DF_Address_ModifiedDate]  DEFAULT (getdate()),
 CONSTRAINT [PK_Address_AddressID] PRIMARY KEY CLUSTERED 
(
	[AddressID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO



SET ANSI_NULLS ON
GO



SET QUOTED_IDENTIFIER ON
GO



IF NOT EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[Person].[Address_AuditTrig_DELETE]'))
EXEC dbo.sp_executesql @statement = N'
CREATE TRIGGER Address_AuditTrig_DELETE ON [Person].[Address] FOR DELETE
AS
BEGIN
	DECLARE @TrxId uniqueidentifier
	SET @TrxId = newid()

	DECLARE @count int
	SELECT @count = count(*) FROM deleted
	
	INSERT INTO [dbo].[AuditTransactionMaster] (TransId,TableName,ModifyType,RowsAffected) VALUES (@TrxId,''Person.Address'',''DELETE'',@count)
	INSERT INTO [Person].[Address_Audit] (TransId,AddressID,AddressLine1,AddressLine2,City,StateProvinceID,PostalCode,rowguid,ModifiedDate) SELECT @TrxId, AddressID,AddressLine1,AddressLine2,City,StateProvinceID,PostalCode,rowguid,ModifiedDate FROM deleted
END
' 
GO



SET ANSI_NULLS ON
GO



SET QUOTED_IDENTIFIER ON
GO



IF NOT EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[Person].[Address_AuditTrig_INSERT]'))
EXEC dbo.sp_executesql @statement = N'
CREATE TRIGGER Address_AuditTrig_INSERT ON [Person].[Address] FOR INSERT
AS
BEGIN
	DECLARE @TrxId uniqueidentifier
	SET @TrxId = newid()
	
	DECLARE @count int
	SELECT @count = count(*) FROM inserted
	
	INSERT INTO [dbo].[AuditTransactionMaster] (TransId,TableName,ModifyType,RowsAffected) VALUES (@TrxId,''Person.Address'',''INSERT'',@count)
	INSERT INTO [Person].[Address_Audit] (TransId,AddressID,AddressLine1,AddressLine2,City,StateProvinceID,PostalCode,rowguid,ModifiedDate) SELECT @TrxId, AddressID,AddressLine1,AddressLine2,City,StateProvinceID,PostalCode,rowguid,ModifiedDate FROM inserted
END
' 
GO



SET ANSI_NULLS ON
GO



SET QUOTED_IDENTIFIER ON
GO



IF NOT EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[Person].[Address_AuditTrig_UPDATE]'))
EXEC dbo.sp_executesql @statement = N'
CREATE TRIGGER Address_AuditTrig_UPDATE ON [Person].[Address] FOR UPDATE
AS
BEGIN
	DECLARE @TrxId uniqueidentifier
	SET @TrxId = newid()
	
	DECLARE @count int
	SELECT @count = count(*) FROM inserted
	
	INSERT INTO [dbo].[AuditTransactionMaster] (TransId,TableName,ModifyType,RowsAffected) VALUES (@TrxId,''Person.Address'',''UPDATE'',@count)
	INSERT INTO [Person].[Address_Audit] (TransId,AddressID,AddressLine1,AddressLine2,City,StateProvinceID,PostalCode,rowguid,ModifiedDate) SELECT @TrxId, AddressID,AddressLine1,AddressLine2,City,StateProvinceID,PostalCode,rowguid,ModifiedDate FROM inserted
END
' 
GO



SET ANSI_NULLS ON
GO



SET QUOTED_IDENTIFIER ON
GO



IF NOT EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[Person].[uAddress]'))
EXEC dbo.sp_executesql @statement = N'
CREATE TRIGGER [Person].[uAddress] ON [Person].[Address] 
AFTER UPDATE NOT FOR REPLICATION AS 
BEGIN
    SET NOCOUNT ON;

    UPDATE [Person].[Address]
    SET [Person].[Address].[ModifiedDate] = GETDATE()
    FROM inserted
    WHERE inserted.[AddressID] = [Person].[Address].[AddressID];
END;
' 
GO


IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[Person].[FK_Address_StateProvince_StateProvinceID]') AND parent_object_id = OBJECT_ID(N'[Person].[Address]'))
ALTER TABLE [Person].[Address]  WITH CHECK ADD  CONSTRAINT [FK_Address_StateProvince_StateProvinceID] FOREIGN KEY([StateProvinceID])
REFERENCES [StateProvince] ([StateProvinceID])
GO


ALTER TABLE [Person].[Address] CHECK CONSTRAINT [FK_Address_StateProvince_StateProvinceID]
GO



";
            #endregion
            string message;
            string messageExpected = "No routines found";
            bool expected = true;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }

        #endregion

        /// <summary>
        ///A test for LongDescription
        ///</summary>
        [TestMethod()]
        public void LongDescriptionTest()
        {
            GrantExecutePolicy target = new GrantExecutePolicy();
            string actual;
            actual = target.LongDescription;
            string expected = "Checks that Stored Procedure and Function scripts have at least one \"GRANT EXECUTE\" statement";
            Assert.AreEqual(actual, expected);

        }

        /// <summary>
        ///A test for ShortDescription
        ///</summary>
        [TestMethod()]
        public void ShortDescriptionTest()
        {
            GrantExecutePolicy target = new GrantExecutePolicy();
            string actual;
            actual = target.ShortDescription;
            string expected = "Check for GRANT EXECUTE";
            Assert.AreEqual(actual, expected);
        }
    }
}
