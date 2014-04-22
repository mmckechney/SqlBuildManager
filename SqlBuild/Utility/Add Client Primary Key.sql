${Title: Add Client PK <<PK Name>>}
IF NOT  EXISTS (SELECT 1 FROM sys.objects o with(nolock) INNER JOIN sys.objects po with(nolock) ON o.parent_object_id = po.object_id
				WHERE o.name = '<<PK Name>>' AND po.name = '<<Table Name>>')
BEGIN
	 
	 ALTER TABLE [<<Table Schema>>].[<<Table Name>>] ADD CONSTRAINT [<<PK Name>>] PRIMARY KEY CLUSTERED 
	(
		[<<Column Names>>] ASC
	) WITH (PAD_INDEX  = ON, 
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
