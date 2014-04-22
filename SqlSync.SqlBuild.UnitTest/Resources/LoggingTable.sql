IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = 'SqlBuild_Logging' AND type = 'U')
BEGIN

	CREATE TABLE SqlBuild_Logging
	(
		[BuildFileName] varchar(300) NOT NULL,
		[ScriptFileName] varchar(300) NOT NULL,
		[ScriptId] uniqueidentifier,
		[ScriptFileHash] varchar(100),
		[CommitDate] datetime NOT NULL,
		[Sequence] int,
		[UserId]	varchar(50),
		[AllowScriptBlock] bit DEFAULT (1),
		[AllowBlockUpdateId] varchar(50)
	)
	
	CREATE NONCLUSTERED INDEX IX_SqlBuild_Logging ON dbo.SqlBuild_Logging
		(
		BuildFileName
		) ON [PRIMARY]
	
	CREATE NONCLUSTERED INDEX IX_SqlBuild_Logging_1 ON dbo.SqlBuild_Logging
		(
		ScriptFileName
		) ON [PRIMARY]
	
	CREATE CLUSTERED INDEX IX_SqlBuild_Logging_2 ON dbo.SqlBuild_Logging
		(
		ScriptId
		) ON [PRIMARY]
	
END

-- Append to table if needed
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = 'SqlBuild_Logging' AND COLUMN_NAME = 'ScriptText')
BEGIN
	ALTER TABLE SqlBuild_Logging ADD [ScriptText] Text NULL
END

-- Append to table if needed
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = 'SqlBuild_Logging' AND COLUMN_NAME = 'Tag')
BEGIN
	ALTER TABLE SqlBuild_Logging ADD [Tag] varchar(200) NULL
END

-- Append to table if needed
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = 'SqlBuild_Logging' AND COLUMN_NAME = 'TargetDatabase')
BEGIN
	ALTER TABLE SqlBuild_Logging ADD [TargetDatabase] varchar(200) NULL
END
