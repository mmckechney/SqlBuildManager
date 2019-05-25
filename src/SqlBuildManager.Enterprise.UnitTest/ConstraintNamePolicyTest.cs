using SqlBuildManager.Enterprise.Policy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace SqlBuildManager.Enterprise.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for ConstraintNamePolicyTest and is intended
    ///to contain all ConstraintNamePolicyTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ConstraintNamePolicyTest
    {

        /// <summary>
        ///A test for PolicyId
        ///</summary>
        [TestMethod()]
        public void PolicyIdTest()
        {
            ConstraintNamePolicy target = new ConstraintNamePolicy();
            string actual;
            actual = target.PolicyId;
            string expected = "ConstraintNamePolicy";
            Assert.AreEqual(actual, expected);

        }
        /// <summary>
        ///A test for LongDescription
        ///</summary>
        [TestMethod()]
        public void LongDescriptionTest()
        {
            ConstraintNamePolicy target = new ConstraintNamePolicy();
            string actual;
            actual = target.LongDescription;
            string expected = "Checks that constraints contain the name of the table they are applied to.";
            Assert.AreEqual(actual, expected);

        }

        /// <summary>
        ///A test for ShortDescription
        ///</summary>
        [TestMethod()]
        public void ShortDescriptionTest()
        {
            ConstraintNamePolicy target = new ConstraintNamePolicy();
            string actual;
            actual = target.ShortDescription;
            string expected = "Constraint Naming (beta)";
            Assert.AreEqual(actual, expected);
        }


        /// <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void CheckPolicy_NoConstraints()
        {
            ConstraintNamePolicy target = new ConstraintNamePolicy();
            string script = @"ALTER TABLE [dbo].[SqlBuild_Logging] ADD COLUMN MyColumn varchar(50)";
            string message;
            string messageExpected = "No constraints found";
            bool expected = true;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }


        #region Default Constraints
        /// <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void CheckPolicy_DefaultNoName()
        {
            ConstraintNamePolicy target = new ConstraintNamePolicy();
            string script = @"ALTER TABLE [dbo].[SqlBuild_Logging] ADD DEFAULT ((1)) FOR [AllowScriptBlock]";
            string message;
            string messageExpected = "No constraint name specified. Default constraint names not allowed.";
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
        public void CheckPolicy_DefaultIncludesTableName()
        {
            ConstraintNamePolicy target = new ConstraintNamePolicy();
            string script = @"ALTER TABLE [dbo].[AWBuildVersion] ADD  CONSTRAINT [DF_AWBuildVersion_ModifiedDate]  DEFAULT (getdate()) FOR [ModifiedDate]";
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
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void CheckPolicy_DefaultBadNameTableName()
        {
            ConstraintNamePolicy target = new ConstraintNamePolicy();
            string script = @"ALTER TABLE [dbo].[AWBuildVersion] ADD  CONSTRAINT [DF_NoTableName_ModifiedDate]  DEFAULT (getdate()) FOR [ModifiedDate]";
            string message;
            string messageExpected = "The default constraint name '[DF_NoTableName_ModifiedDate]' does not contain the referenced table name 'AWBuildVersion'.";
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
        public void CheckPolicy_DefaultIncludesTableNameNotQualified()
        {
            ConstraintNamePolicy target = new ConstraintNamePolicy();
            string script = @"ALTER TABLE [AWBuildVersion] ADD  CONSTRAINT [DF_AWBuildVersion_ModifiedDate]  DEFAULT (getdate()) FOR [ModifiedDate]";
            string message;
            string messageExpected = "";
            bool expected = true;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }
        #endregion

        #region ForeignKey Constraints

        /// <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void CheckPolicy_FKIncludesTableName()
        {
            ConstraintNamePolicy target = new ConstraintNamePolicy();
            string script = @"ALTER TABLE [HumanResources].[EmployeePayHistory]  WITH CHECK ADD CONSTRAINT [FK_EmployeePayHistory_Employee_EmployeeID] FOREIGN KEY([EmployeeID])
REFERENCES [HumanResources].[Employee] ([EmployeeID])
";
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
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void CheckPolicy_FKDoesNotIncludeTableName()
        {
            ConstraintNamePolicy target = new ConstraintNamePolicy();
            string script = @"ALTER TABLE [HumanResources].[EmployeePayHistory]  WITH CHECK ADD CONSTRAINT [FK_DummyTable_Employee_EmployeeID] FOREIGN KEY([EmployeeID])
REFERENCES [HumanResources].[Employee] ([EmployeeID])
";
            string message;
            string messageExpected = "The foreign key name '[FK_DummyTable_Employee_EmployeeID]' does not contain the referenced table name 'EmployeePayHistory'.";
            bool expected = false;
            bool actual;
            System.Collections.Generic.List<System.Text.RegularExpressions.Match> commentCollection = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            actual = target.CheckPolicy(script, commentCollection, out message);
            Assert.AreEqual(messageExpected, message);
            Assert.AreEqual(expected, actual);
        }

        
        #endregion
        /// <summary>
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void CheckPolicy_CheckConstraintIncludeTableName()
        {
            ConstraintNamePolicy target = new ConstraintNamePolicy();
            string script = @"ALTER TABLE [Person].[Contact]  WITH CHECK ADD  CONSTRAINT [CK_Contact_EmailPromotion] CHECK  (([EmailPromotion]>=(0) AND [EmailPromotion]<=(2)))";
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
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void CheckPolicy_CheckConstraintNotIncludeTableName()
        {
            ConstraintNamePolicy target = new ConstraintNamePolicy();
            string script = @"ALTER TABLE [Person].[Contact]  
WITH CHECK ADD  CONSTRAINT 
[CK_NoTable_EmailPromotion] CHECK  (([EmailPromotion]>=(0) AND [EmailPromotion]<=(2)))
";
            string message;
            string messageExpected = "The check constraint name '[CK_NoTable_EmailPromotion]' does not contain the referenced table name 'Contact'.";
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
        public void CheckPolicy_PrimaryKeyIncludesTableName()
        {
            ConstraintNamePolicy target = new ConstraintNamePolicy();
            string script = @" ALTER TABLE [Person].[Contact] ADD  CONSTRAINT [PK_Contact_ContactID] PRIMARY KEY CLUSTERED 
(
	[ContactID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

";
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
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void CheckPolicy_PrimaryKeyNotIncludeTableName()
        {
            ConstraintNamePolicy target = new ConstraintNamePolicy();
            string script = @" ALTER TABLE [Person].[Contact] ADD  CONSTRAINT [PK_BadTableName_MyPkID] PRIMARY KEY CLUSTERED 
(
	[ContactID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

";
            string message;
            string messageExpected = "The primary key name '[PK_BadTableName_MyPkID]' does not contain the referenced table name 'Contact'.";
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
        public void CheckPolicy_ForeignKeyIncludesTableName()
        {
            ConstraintNamePolicy target = new ConstraintNamePolicy();
            string script = @"IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WITH (NOLOCK) WHERE TABLE_NAME = 'Customers' AND TABLE_SCHEMA = 'dbo' AND COLUMN_NAME = 'ClientTypeId')
BEGIN
	ALTER TABLE [dbo].[Customers] ADD ClientTypeId [int]
	
	ALTER TABLE [dbo].[Customers]  WITH NOCHECK ADD  CONSTRAINT [FK_Customers_ClientType] FOREIGN KEY([ClientTypeId])
	REFERENCES [dbo].[ClientType] ([ClientTypeId])

	ALTER TABLE [dbo].[Customers] CHECK CONSTRAINT [FK_Customers_ClientType]
END
GO
";
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
        ///A test for CheckPolicy
        ///</summary>
        [TestMethod()]
        public void CheckPolicy_ForeignKeyNotIncludeTableName()
        {
            ConstraintNamePolicy target = new ConstraintNamePolicy();
            string script = @"IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WITH (NOLOCK) WHERE TABLE_NAME = 'Customers' AND TABLE_SCHEMA = 'dbo' AND COLUMN_NAME = 'ClientTypeId')
BEGIN
	ALTER TABLE [dbo].[Customers] ADD ClientTypeId [int]
	
	ALTER TABLE [dbo].[Customers]  WITH NOCHECK ADD  CONSTRAINT [FK_Custome_ClientType] FOREIGN KEY([ClientTypeId])
	REFERENCES [dbo].[ClientType] ([ClientTypeId])

	ALTER TABLE [dbo].[Customers] CHECK CONSTRAINT [FK_Customers_ClientType]
END
GO
";
            string message;
            string messageExpected = "The foreign key name '[FK_Custome_ClientType]' does not contain the referenced table name 'Customers'.";
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
        public void CheckPolicy_ForeignKeyNotIncludeTableNameInSecondConstraint()
        {
            ConstraintNamePolicy target = new ConstraintNamePolicy();
            string script = @"IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WITH (NOLOCK) WHERE TABLE_NAME = 'Customers' AND TABLE_SCHEMA = 'dbo' AND COLUMN_NAME = 'ClientTypeId')
BEGIN
	ALTER TABLE [dbo].[Customers] ADD ClientTypeId [int]
	
	ALTER TABLE [dbo].[Customers]  WITH NOCHECK ADD  CONSTRAINT [FK_Customers_ClientType] FOREIGN KEY([ClientTypeId])
	REFERENCES [dbo].[ClientType] ([ClientTypeId])

    ALTER TABLE [dbo].[Customers]  WITH NOCHECK ADD  CONSTRAINT [FK_Custome_ClientClass] FOREIGN KEY([ClientClass])
	REFERENCES [dbo].[ClientType] ([ClientClass])

	ALTER TABLE [dbo].[Customers] CHECK CONSTRAINT [FK_Customers_ClientType]
END
GO
";
            string message;
            string messageExpected = "The foreign key name '[FK_Custome_ClientClass]' does not contain the referenced table name 'Customers'.";
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
        public void CheckPolicy_EnablingConstraintNotIncludeTableName()
        {
            ConstraintNamePolicy target = new ConstraintNamePolicy();
            string script = @"IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WITH (NOLOCK) WHERE TABLE_NAME = 'Customers' AND TABLE_SCHEMA = 'dbo' AND COLUMN_NAME = 'ClientTypeId')
BEGIN
	ALTER TABLE [dbo].[Customers] ADD ClientTypeId [int]
	
	ALTER TABLE [dbo].[Customers]  WITH NOCHECK ADD  CONSTRAINT [FK_Customers_ClientType] FOREIGN KEY([ClientTypeId])
	REFERENCES [dbo].[ClientType] ([ClientTypeId])

	ALTER TABLE [dbo].[Customers] CHECK CONSTRAINT [FK_Custom_ClientType]
END
GO
";
            string message;
            string messageExpected = "An existing constraint enabled by your CHECK CONSTRAINT script does not contain the referenced table name 'Customers'.";
            bool expected = false;
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
        public void CheckPolicy_MissingSpaceBugFix()
        {
            ConstraintNamePolicy target = new ConstraintNamePolicy();
            string script = @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[clk].[ItemOperation]') AND type in (N'U'))
BEGIN
CREATE TABLE [clk].[ItemOperation](
       [Id] [tinyint] NOT NULL,
       [Description] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
       [CreatedDate] [datetime] NOT NULL,
       [CreatedBy] [varchar](75) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
       [ModifiedDate] [datetime] NULL,
       [ModifiedBy] [varchar](75) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
CONSTRAINT [PK_ItemOperation] PRIMARY KEY CLUSTERED 
(
       [Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
";

            string message;
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
