IF NOT EXISTS(SELECT 1 FROM SqlBuild_Logging WHERE [ScriptId] = '{0}') 

BEGIN 

INSERT INTO SqlBuild_Logging(
	[BuildFileName],
	[ScriptFileName],
	[ScriptId],
	[ScriptFileHash],
	[CommitDate],
	[Sequence],
	[UserId],
	[AllowScriptBlock],
	[AllowBlockUpdateId],
	[ScriptText],
	[Tag],
	[TargetDatabase],
	[RunWithVersion],
	[BuildProjectHash],
	[BuildRequestedBy],
	[ScriptRunStart],
	[ScriptRunEnd],
	[Description]
)
VALUES(
	'TestPreRun-FileName',
	'TestPreRunScriptName',
	'{0}',
	'MadeUpHash',
	getdate(),
	1,
	'TestUser',
	1,
	'BlockUpdate',
	'Here is my script',
	'No Tag',
	'TestDatabase',
	'v10.4',
	'BuildFileHash',
	'requestuser',
	'2019-04-01 12:00:00',
	'2019-04-01 12:00:01',
	'My description'
) 

END