${Title: Drop Column  <<Table Name>>.<<Column Name>>}
IF EXISTS(SELECT 1 FROM s WITH (NOLOCK) WHERE TABLE_NAME = '<<Table Name>>' AND TABLE_SCHEMA = '<<Table Schema>>'AND COLUMN_NAME = '<<Column Name>>')
BEGIN
	ALTER TABLE [<<Table Schema>>].[<<Table Name>>] DROP COLUMN <<Column Name>>
END
GO
