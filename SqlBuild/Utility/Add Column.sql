${Title: Add <<Table Schema>>.<<Table Name>>.<<Column Name>> Column}
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WITH (NOLOCK) WHERE TABLE_NAME = '<<Table Name>>' AND TABLE_SCHEMA = '<<Table Schema>>' AND COLUMN_NAME = '<<Column Name>>')
BEGIN
	ALTER TABLE [<<Table Schema>>].[<<Table Name>>] ADD <<Column Name>> <<Column Definition>>
END
GO
