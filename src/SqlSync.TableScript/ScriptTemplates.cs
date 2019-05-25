using System;

namespace SqlSync.TableScript
{
	/// <summary>
	/// Summary description for ScriptTemplates.
	/// </summary>
	public class ScriptTemplates
	{

		public const string ADD_UPDATE_ID_COLUMN =
@"
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = '<<tableName>>' AND TABLE_SCHEMA = '<<schema>>' AND COLUMN_NAME = '<<columnName>>')
BEGIN
	ALTER TABLE [<<schema>>].[<<tableName>>] ADD [<<columnName>>] varchar(50) NULL
END
GO

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = '<<tableName>>' AND TABLE_SCHEMA = '<<schema>>' AND COLUMN_NAME = '<<columnName>> ' AND COLUMN_DEFAULT IS NULL)
BEGIN
	ALTER TABLE [<<schema>>].[<<tableName>>] ADD CONSTRAINT [DF_<<tableName>>_<<columnName>>] DEFAULT SYSTEM_USER FOR [<<columnName>>]
END
GO

UPDATE [<<schema>>].[<<tableName>>] SET [<<columnName>>]='<<userName>>' WHERE [<<columnName>>] IS NULL
GO

ALTER TABLE [<<schema>>].[<<tableName>>] ALTER COLUMN [<<columnName>>] varchar(50) NOT NULL
GO";

        public const string ADD_CREATE_ID_COLUMN =
@"
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = '<<tableName>>' AND TABLE_SCHEMA = '<<schema>>' AND COLUMN_NAME = '<<columnName>>')
BEGIN
	ALTER TABLE [<<schema>>].[<<tableName>>] ADD [<<columnName>>] varchar(50) NULL
END
GO

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = '<<tableName>>' AND TABLE_SCHEMA = '<<schema>>' AND COLUMN_NAME = '<<columnName>> ' AND COLUMN_DEFAULT IS NULL)
BEGIN
	ALTER TABLE [<<schema>>].[<<tableName>>] ADD CONSTRAINT [DF_<<tableName>>_<<columnName>>] DEFAULT SYSTEM_USER FOR [<<columnName>>]
END
GO

UPDATE [<<schema>>].[<<tableName>>] SET [<<columnName>>]='<<userName>>' WHERE [<<columnName>>] IS NULL
GO

ALTER TABLE [<<schema>>].[<<tableName>>] ALTER COLUMN [<<columnName>>] varchar(50) NOT NULL
GO";

		public const string ADD_UPDATE_DATE_COLUMN =
@"
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = '<<tableName>>' AND TABLE_SCHEMA = '<<schema>>' AND COLUMN_NAME = '<<columnName>>')
BEGIN
	ALTER TABLE [<<schema>>].[<<tableName>>] ADD [<<columnName>>] datetime NULL
END
GO

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = '<<tableName>>' AND TABLE_SCHEMA = '<<schema>>' AND COLUMN_NAME = '<<columnName>> ' AND COLUMN_DEFAULT IS NULL)
BEGIN
	ALTER TABLE [<<schema>>].[<<tableName>>] ADD CONSTRAINT [DF_<<tableName>>_<<columnName>>] DEFAULT getdate() FOR [<<columnName>>]
END
GO

UPDATE [<<schema>>].[<<tableName>>] SET [<<columnName>>]= '<<date>>' WHERE [<<columnName>>] IS NULL
GO

ALTER TABLE [<<schema>>].[<<tableName>>] ALTER COLUMN [<<columnName>>] datetime NOT NULL
GO";

        public const string ADD_CREATE_DATE_COLUMN =
@"
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = '<<tableName>>' AND TABLE_SCHEMA = '<<schema>>' AND COLUMN_NAME = '<<columnName>>')
BEGIN
	ALTER TABLE [<<schema>>].[<<tableName>>] ADD [<<columnName>>] datetime NULL
END
GO

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = '<<tableName>>' AND TABLE_SCHEMA = '<<schema>>' AND COLUMN_NAME = '<<columnName>> ' AND COLUMN_DEFAULT IS NULL)
BEGIN
	ALTER TABLE [<<schema>>].[<<tableName>>] ADD CONSTRAINT [DF_<<tableName>>_<<columnName>>] DEFAULT getdate() FOR [<<columnName>>]
END
GO

UPDATE [<<schema>>].[<<tableName>>] SET [<<columnName>>]= '<<date>>' WHERE [<<columnName>>] IS NULL
GO

ALTER TABLE [<<schema>>].[<<tableName>>] ALTER COLUMN [<<columnName>>] datetime NOT NULL
GO";

		public const string DropExistingDefaultConstraint =
@"
DECLARE @Name varchar(255)
DECLARE @sql nvarchar(500)
SELECT @Name = Name from sys.objects where id = (select cdefault FROM sys.columns where name='<<columnName>>' AND ID = (select id from sys.objects where name='<<tableName>>'))
IF(@Name IS NOT NULL AND @Name <> '')
BEGIN
	SET @sql = 'ALTER TABLE [<<schema>>].[<<tableName>>] DROP CONSTRAINT '+ @Name
	EXEC sp_executesql @Sql
END
GO

ALTER TABLE [<<schema>>].[<<tableName>>] ADD CONSTRAINT [DF_<<tableName>>_<<columnName>>] DEFAULT <<defaultValue>> FOR [<<columnName>>]
GO
";
	}

}
