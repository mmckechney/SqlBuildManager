${Title: Add Client Index <<Index Name>>}
IF NOT  EXISTS (SELECT 1 FROM dbo.sysindexes WITH (NOLOCK) WHERE name = '<<Index Name>>'  AND OBJECT_NAME(id) = N'<<Table Name>>')
BEGIN
	 CREATE INDEX [<<Index Name>>] ON [<<Table Schema>>].[<<Table Name>>](<<Column Names>>) 
	 WITH (PAD_INDEX  = ON, 
	STATISTICS_NORECOMPUTE  = ON, 
	SORT_IN_TEMPDB = OFF, 
	IGNORE_DUP_KEY = OFF, 
	--DROP_EXISTING = OFF, 
	ONLINE = ON, 
	ALLOW_ROW_LOCKS  = ON, 
	ALLOW_PAGE_LOCKS  = ON, 
	FILLFACTOR = 90) ON [PRIMARY]

END
GO

