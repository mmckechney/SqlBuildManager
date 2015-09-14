${Title: Drop Foreign Key <<Foreign Key Name>>}
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME = '<<Foreign Key Name>>')
BEGIN
	ALTER TABLE [<<Table Schema>>].[<<Table Name>>] DROP CONSTRAINT <<Foreign Key Name>>
END
GO