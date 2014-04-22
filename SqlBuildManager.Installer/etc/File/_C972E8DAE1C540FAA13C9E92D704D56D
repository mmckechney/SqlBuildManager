${Title: Drop Index <<Table Schema>>.<<Table Name>>.<<Index Name>>}
IF EXISTS (SELECT 1 FROM dbo.sysindexes WHERE name = '<<Index Name>>' AND OBJECT_NAME(id) = N'<<Table Name>>')
BEGIN
	DROP INDEX [<<Table Schema>>].[<<Table Name>>].<<Index Name>>
END 
GO