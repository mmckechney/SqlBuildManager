${Title: Add Unique Index <<Index Name>>}
IF NOT  EXISTS (SELECT 1 FROM dbo.sysindexes WITH (NOLOCK) WHERE name = '<<Index Name>>'  AND OBJECT_NAME(id) = N'<<Table Name>>')
BEGIN
	 CREATE  UNIQUE  INDEX [<<Index Name>>] ON [<<Table Schema>>].[<<Table Name>>](<<Column Names>>) ON [PRIMARY]

END
GO