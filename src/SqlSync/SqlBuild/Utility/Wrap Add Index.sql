${Title: Add Index <<Index Name>>}
IF NOT  EXISTS (SELECT 1 FROM dbo.sysindexes WHERE name = '<<Index Name>>'  AND OBJECT_NAME(id) = N'<<Table Name>>')
BEGIN
	 <<INSERT>>
END
GO