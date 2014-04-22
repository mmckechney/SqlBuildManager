${Title: Add Constraint <<Constraint Name>>}
IF NOT EXISTS(SELECT 1 FROM dbo.sysobjects WITH (NOLOCK) WHERE name = '<<Constraint Name>>' AND type = 'D') AND
	EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.Columns WITH (NOLOCK) WHERE TABLE_NAME = '<<Table Name>>' AND TABLE_SCHEMA = '<<Table Schema>>' AND COLUMN_NAME = '<<Column Name>>' AND COLUMN_DEFAULT IS NULL)
BEGIN
	ALTER TABLE [<<Table Schema>>].[<<Table Name>>] ADD CONSTRAINT <<Constraint Name>> <<Constraint Value>> FOR <<Column Name>>
END
GO
