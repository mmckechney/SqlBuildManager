using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace SqlBuildManager.ScriptHandling.UnitTest
{


    /// <summary>
    ///This is a test class for ScriptWrappingTest and is intended
    ///to contain all ScriptWrappingTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ScriptWrappingTest
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


        #region .: ExtractTableNameFromScript Tests :.
        /// <summary>
        ///A test for ExtractTableNameFromScript successful with custom schema
        ///</summary>
        [TestMethod()]
        public void ExtractTableNameFromScriptTest_CustomSchema()
        {
            string rawScript = @"CREATE TABLE [testSchema].[TestTable] 
(
    column2 int,
    column3 varchar(50)
)";

            string schema;
            string schemaExpected = "testSchema";
            string tableName;
            string tableNameExpected = "TestTable";
            ScriptWrapping.ExtractTableNameFromScript(rawScript, out schema, out tableName);
            Assert.AreEqual(schemaExpected, schema);
            Assert.AreEqual(tableNameExpected, tableName);
        }
        /// <summary>
        ///A test for ExtractTableNameFromScript successful with default schema
        ///</summary>
        [TestMethod()]
        public void ExtractTableNameFromScriptTest_DefaultSchema()
        {
            string rawScript = @"ALTER TABLE [TestTable] ALTER COLUMN column3 varchar(50)";
            string schema;
            string schemaExpected = "dbo";
            string tableName;
            string tableNameExpected = "TestTable";
            ScriptWrapping.ExtractTableNameFromScript(rawScript, out schema, out tableName);
            Assert.AreEqual(schemaExpected, schema);
            Assert.AreEqual(tableNameExpected, tableName);
        }
        /// <summary>
        ///A test for ExtractTableNameFromScript unable to find table name
        ///</summary>
        [TestMethod()]
        public void ExtractTableNameFromScriptTest_UnableToFindTable()
        {
            string rawScript = @"CREATE TABL [TestTable] 
(
    column2 int,
    column3 varchar(50)
)";

            string schema;
            string schemaExpected = "dbo";
            string tableName;
            string tableNameExpected = "";
            ScriptWrapping.ExtractTableNameFromScript(rawScript, out schema, out tableName);
            Assert.AreEqual(schemaExpected, schema);
            Assert.AreEqual(tableNameExpected, tableName);
        }
        #endregion

        #region .: TransformCreateTableToAlterColumn Tests:.
        /// <summary>
        ///A test for TransformCreateTableToAlterColumn
        ///</summary>
        [TestMethod()]
        public void TransformCreateTableToAlterColumnTest()
        {
            string rawScript = @"CREATE TABLE [Person].[Contact](
	[ContactID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[NameStyle] [dbo].[NameStyle] NOT NULL CONSTRAINT [DF_Contact_NameStyle]  DEFAULT ((0)),
	[Title] [nvarchar](8) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[FirstName] [dbo].[Name] NOT NULL,
	[MiddleName] [dbo].[Name] NULL)";
            string schema = "Person";
            string tableName = "Contact";
            string changedScript;
            #region expected script ...
            string changedScriptExpected = @"IF EXISTS(SELECT 1 FROM information_schema.columns WHERE TABLE_NAME = 'Contact' AND TABLE_SCHEMA = 'Person' AND COLUMN_NAME = 'ContactID')
	ALTER TABLE [Person].[Contact] ALTER COLUMN [ContactID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL
ELSE
	ALTER TABLE [Person].[Contact] ADD [ContactID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL
GO

IF EXISTS(SELECT 1 FROM information_schema.columns WHERE TABLE_NAME = 'Contact' AND TABLE_SCHEMA = 'Person' AND COLUMN_NAME = 'NameStyle')
	ALTER TABLE [Person].[Contact] ALTER COLUMN 	[NameStyle] [dbo].[NameStyle] NOT NULL 
ELSE
	ALTER TABLE [Person].[Contact] ADD 	[NameStyle] [dbo].[NameStyle] NOT NULL 
GO

IF EXISTS(SELECT 1 FROM information_schema.columns WHERE TABLE_NAME = 'Contact' AND TABLE_SCHEMA = 'Person' AND COLUMN_NAME = 'Title')
	ALTER TABLE [Person].[Contact] ALTER COLUMN [Title] [nvarchar](8) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
ELSE
	ALTER TABLE [Person].[Contact] ADD [Title] [nvarchar](8) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
GO

IF EXISTS(SELECT 1 FROM information_schema.columns WHERE TABLE_NAME = 'Contact' AND TABLE_SCHEMA = 'Person' AND COLUMN_NAME = 'FirstName')
	ALTER TABLE [Person].[Contact] ALTER COLUMN [FirstName] [dbo].[Name] NOT NULL
ELSE
	ALTER TABLE [Person].[Contact] ADD [FirstName] [dbo].[Name] NOT NULL
GO

IF EXISTS(SELECT 1 FROM information_schema.columns WHERE TABLE_NAME = 'Contact' AND TABLE_SCHEMA = 'Person' AND COLUMN_NAME = 'MiddleName')
	ALTER TABLE [Person].[Contact] ALTER COLUMN [MiddleName] [dbo].[Name] NULL)
ELSE
	ALTER TABLE [Person].[Contact] ADD [MiddleName] [dbo].[Name] NULL)
GO

";
            #endregion

            List<string> expected = new List<string>();
            expected.AddRange(new string[] { "ContactID", "NameStyle", "Title", "FirstName", "MiddleName" });

            List<string> actual;
            actual = ScriptWrapping.TransformCreateTableToAlterColumn(rawScript, schema, tableName, out changedScript);
            Assert.AreEqual(changedScriptExpected, changedScript);
            Assert.AreEqual(string.Join(",", expected.ToArray()), string.Join(",", actual.ToArray()));

        }
        /// <summary>
        ///A test for TransformCreateTableToAlterColumn missing table name initiator
        ///</summary>
        [TestMethod()]
        public void TransformCreateTableToAlterColumnTest_MissingTableName()
        {
            string rawScript = @"CREATE TABLE [Person].[Contact](
	[ContactID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[NameStyle] [dbo].[NameStyle] NOT NULL CONSTRAINT [DF_Contact_NameStyle]  DEFAULT ((0)),
	[Title] [nvarchar](8) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[FirstName] [dbo].[Name] NOT NULL,
	[MiddleName] [dbo].[Name] NULL)";
            string schema = "Person";
            string tableName = "";
            string changedScript;
            string changedScriptExpected = rawScript;

            List<string> expected = new List<string>();

            List<string> actual;
            actual = ScriptWrapping.TransformCreateTableToAlterColumn(rawScript, schema, tableName, out changedScript);
            Assert.AreEqual(changedScriptExpected, changedScript);
            Assert.AreEqual(string.Join(",", expected.ToArray()), string.Join(",", actual.ToArray()));
        }

        #endregion

        /// <summary>
        ///A test for TransformCreateTableToResyncTable
        ///</summary>
        [TestMethod()]
        public void TransformCreateTableToResyncTableTest()
        {
            string rawScript = @"CREATE TABLE [Person].[Contact](
	[ContactID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[NameStyle] [dbo].[NameStyle] NOT NULL CONSTRAINT [DF_Contact_NameStyle]  DEFAULT ((0)),
	[Title] [nvarchar](8) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[FirstName] [dbo].[Name] NOT NULL,
	[MiddleName] [dbo].[Name] NULL)";
            string schema = "Person";
            string tableName = "Contact";
            #region expected ...
            string expected = @"IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Contact' AND TABLE_SCHEMA = 'Person')
	CREATE TABLE [Person].[Contact]  (temp_will_be_removed bit NULL)
GO

IF EXISTS(SELECT 1 FROM information_schema.columns WHERE TABLE_NAME = 'Contact' AND TABLE_SCHEMA = 'Person' AND COLUMN_NAME = 'ContactID')
	ALTER TABLE [Person].[Contact] ALTER COLUMN [ContactID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL
ELSE
	ALTER TABLE [Person].[Contact] ADD [ContactID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL
GO

IF EXISTS(SELECT 1 FROM information_schema.columns WHERE TABLE_NAME = 'Contact' AND TABLE_SCHEMA = 'Person' AND COLUMN_NAME = 'NameStyle')
	ALTER TABLE [Person].[Contact] ALTER COLUMN 	[NameStyle] [dbo].[NameStyle] NOT NULL 
ELSE
	ALTER TABLE [Person].[Contact] ADD 	[NameStyle] [dbo].[NameStyle] NOT NULL 
GO

IF EXISTS(SELECT 1 FROM information_schema.columns WHERE TABLE_NAME = 'Contact' AND TABLE_SCHEMA = 'Person' AND COLUMN_NAME = 'Title')
	ALTER TABLE [Person].[Contact] ALTER COLUMN [Title] [nvarchar](8) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
ELSE
	ALTER TABLE [Person].[Contact] ADD [Title] [nvarchar](8) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
GO

IF EXISTS(SELECT 1 FROM information_schema.columns WHERE TABLE_NAME = 'Contact' AND TABLE_SCHEMA = 'Person' AND COLUMN_NAME = 'FirstName')
	ALTER TABLE [Person].[Contact] ALTER COLUMN [FirstName] [dbo].[Name] NOT NULL
ELSE
	ALTER TABLE [Person].[Contact] ADD [FirstName] [dbo].[Name] NOT NULL
GO

IF EXISTS(SELECT 1 FROM information_schema.columns WHERE TABLE_NAME = 'Contact' AND TABLE_SCHEMA = 'Person' AND COLUMN_NAME = 'MiddleName')
	ALTER TABLE [Person].[Contact] ALTER COLUMN [MiddleName] [dbo].[Name] NULL)
ELSE
	ALTER TABLE [Person].[Contact] ADD [MiddleName] [dbo].[Name] NULL)
GO


IF EXISTS(SELECT 1 FROM information_schema.columns WHERE TABLE_NAME = 'Contact' AND TABLE_SCHEMA = 'Person' AND COLUMN_NAME = 'temp_will_be_removed')
	ALTER TABLE [Person].[Contact] DROP COLUMN temp_will_be_removed
GO

--Remove any obsolete columns
DECLARE @sql varchar(1000)
DECLARE @col varchar(250)
DECLARE @FK varchar(250)
DECLARE @tmp TABLE(columnName varchar(250))
DECLARE @tmpFK TABLE(constraintName varchar(250))
INSERT INTO @tmp SELECT COLUMN_NAME FROM information_schema.columns WHERE TABLE_NAME = 'Contact' AND TABLE_SCHEMA = 'Person' AND COLUMN_NAME NOT IN ('ContactID','NameStyle','Title','FirstName','MiddleName')
DECLARE curRemove CURSOR FOR SELECT columnName FROM @tmp
OPEN curRemove
FETCH NEXT FROM curRemove INTO @col
WHILE @@FETCH_STATUS = 0
BEGIN
	INSERT INTO @tmpFK SELECT CONSTRAINT_NAME FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE where column_name = @col AND Table_Name = 'Contact' AND TABLE_SCHEMA = 'Person'
	DECLARE curFK CURSOR FOR SELECT constraintName FROM @tmpFK
	OPEN curFK
	FETCH NEXT FROM curFK INTO @FK
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @sql = 'IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE WHERE CONSTRAINT_NAME = '''+@FK+''') ALTER TABLE [Person].[Contact] DROP CONSTRAINT ' +@FK
		PRINT @sql
		EXEC(@sql)
		FETCH NEXT FROM curFK INTO @FK
	END
	CLOSE curFK
	DEALLOCATE curFK 
	SET @sql = 'ALTER TABLE [Person].[Contact] DROP COLUMN ' +@col
	PRINT @sql
	EXEC(@sql)
	DELETE FROM @tmpFK
	FETCH NEXT FROM curRemove INTO @col
END
CLOSE curRemove
DEALLOCATE curRemove
GO

";
            #endregion
            string actual;
            actual = ScriptWrapping.TransformCreateTableToResyncTable(rawScript, schema, tableName);
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        ///A test for TransformCreateTableToResyncTable
        ///</summary>
        [TestMethod()]
        public void TransformCreateTableToResyncTableTest_MissingTableName()
        {
            string rawScript = @"CREATE TABLE [Person].[Contact](
	[ContactID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[NameStyle] [dbo].[NameStyle] NOT NULL CONSTRAINT [DF_Contact_NameStyle]  DEFAULT ((0)),
	[Title] [nvarchar](8) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[FirstName] [dbo].[Name] NOT NULL,
	[MiddleName] [dbo].[Name] NULL)";
            string schema = "Person";
            string tableName = "";
            #region expected ...
            string expected = rawScript;
            #endregion
            string actual;
            actual = ScriptWrapping.TransformCreateTableToResyncTable(rawScript, schema, tableName);
            Assert.AreEqual(expected, actual);
        }
    }
}
