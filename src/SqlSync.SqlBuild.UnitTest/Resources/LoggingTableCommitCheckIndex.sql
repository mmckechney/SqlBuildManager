--Add extra index to improve hash check performance
IF NOT  EXISTS (SELECT 1 FROM sysindexes WHERE name = 'IX_SqlBuild_Logging_CommitCheck'  AND OBJECT_NAME(id) = N'SqlBuild_Logging')
BEGIN
	 CREATE NONCLUSTERED INDEX [IX_SqlBuild_Logging_CommitCheck] ON [dbo].[SqlBuild_Logging] 
	(
		[ScriptId] ASC,
		[CommitDate] DESC
	)
	INCLUDE ( [ScriptFileHash],	[AllowScriptBlock]) 
	WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF) ON [PRIMARY]

END
