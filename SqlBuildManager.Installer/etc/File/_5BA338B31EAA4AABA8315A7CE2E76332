${Title: Alter column <<Table Name>>.<<Column Name>>}
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WITH (NOLOCK) WHERE TABLE_NAME = '<<Table Name>>' AND TABLE_SCHEMA = '<<Table Schema>>' AND COLUMN_NAME = '<<Column Name>>')
BEGIN
	ALTER TABLE [<<Table Schema>>].[<<Table Name>>] ALTER COLUMN <<Column Name>> <<Column Definition>>
END
GO
