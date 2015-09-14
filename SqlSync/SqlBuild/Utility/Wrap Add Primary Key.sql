${Title: Add Primary Key <<Primary Key Name>>}
IF NOT  EXISTS (SELECT 1 FROM dbo.sysindexes WHERE name = <<Primary Key Name>>'  AND OBJECT_NAME(id) = N'<<Table Name>>')
BEGIN
	 <<INSERT>>
END
GO