${Title: Drop Table <<Table Name>>}
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '<<Table Name>>' AND TABLE_SCHEMA = '<<Table Schema>>')
BEGIN
	DROP TABLE [<<Table Schema>>].[<<Table Name>>]
END
GO