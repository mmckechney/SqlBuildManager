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
			[AllowScriptBlock] bit CONSTRAINT DF_SqlBuildLogging_AllowScriptBlock DEFAULT (1) ,
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
	
		PRINT 'Created new SqlBuild_Logging table'
	END
ELSE
	BEGIN
		PRINT 'SqlBuild_Logging table already exists'
	END

-- Append to table if needed
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = 'SqlBuild_Logging' AND COLUMN_NAME = 'ScriptText')
	BEGIN
		ALTER TABLE SqlBuild_Logging ADD [ScriptText] Text NULL
		PRINT 'Added [ScriptText] column to SqlBuild_Logging table'
	END
ELSE
	BEGIN
		PRINT '[ScriptText] column already exists in SqlBuild_Logging table'
	END

-- Append to table if needed
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = 'SqlBuild_Logging' AND COLUMN_NAME = 'Tag')
	BEGIN
		ALTER TABLE SqlBuild_Logging ADD [Tag] varchar(200) NULL
		PRINT 'Added [Tag] column to SqlBuild_Logging table'
	END
ELSE
	BEGIN
		PRINT '[Tag] column already exists in SqlBuild_Logging table'
	END

-- Append to table if needed
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = 'SqlBuild_Logging' AND COLUMN_NAME = 'TargetDatabase')
BEGIN
	ALTER TABLE SqlBuild_Logging ADD [TargetDatabase] varchar(200) NULL
		PRINT 'Added [TargetDatabase] column to SqlBuild_Logging table'
	END
ELSE
	BEGIN
		PRINT '[TargetDatabase] column already exists in SqlBuild_Logging table'
	END


-- Append to table if needed
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = 'SqlBuild_Logging' AND COLUMN_NAME = 'RunWithVersion')
BEGIN
	ALTER TABLE SqlBuild_Logging ADD [RunWithVersion] varchar(20) NULL
		PRINT 'Added [RunWithVersion] column to SqlBuild_Logging table'
	END
ELSE
	BEGIN
		PRINT '[RunWithVersion] column already exists in SqlBuild_Logging table'
	END

-- Append to table if needed
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = 'SqlBuild_Logging' AND COLUMN_NAME = 'BuildProjectHash')
BEGIN
	ALTER TABLE SqlBuild_Logging ADD [BuildProjectHash] varchar(45) NULL
		PRINT 'Added [BuildProjectHash] column to SqlBuild_Logging table'
	END
ELSE
	BEGIN
		PRINT '[BuildProjectHash] column already exists in SqlBuild_Logging table'
END

-- Append to table if needed
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = 'SqlBuild_Logging' AND COLUMN_NAME = 'BuildRequestedBy')
BEGIN
	ALTER TABLE SqlBuild_Logging ADD [BuildRequestedBy] varchar(50) NULL
		PRINT 'Added [BuildRequestedBy] column to SqlBuild_Logging table'
	END
ELSE
	BEGIN
		PRINT '[BuildRequestedBy] column already exists in SqlBuild_Logging table'
END

-- Append to table if needed
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = 'SqlBuild_Logging' AND COLUMN_NAME = 'ScriptRunStart')
BEGIN
	ALTER TABLE SqlBuild_Logging ADD [ScriptRunStart] DateTime NULL
		PRINT 'Added [ScriptRunStart] column to SqlBuild_Logging table'
	END
ELSE
	BEGIN
		PRINT '[ScriptRunStart] column already exists in SqlBuild_Logging table'
END

-- Append to table if needed
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = 'SqlBuild_Logging' AND COLUMN_NAME = 'ScriptRunEnd')
BEGIN
	ALTER TABLE SqlBuild_Logging ADD [ScriptRunEnd] DateTime NULL
		PRINT 'Added [ScriptRunEnd] column to SqlBuild_Logging table'
	END
ELSE
	BEGIN
		PRINT '[ScriptRunEnd] column already exists in SqlBuild_Logging table'
END

-- Append to table if needed
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = 'SqlBuild_Logging' AND COLUMN_NAME = 'Description')
BEGIN
	ALTER TABLE SqlBuild_Logging ADD [Description] varchar(50) NULL
		PRINT 'Added [Description] column to SqlBuild_Logging table'
	END
ELSE
	BEGIN
		PRINT '[Description] column already exists in SqlBuild_Logging table'
END

-- Rename the default constraint to clean up the table
BEGIN TRY
	DECLARE @Constraint varchar(50)
	SELECT 
		@Constraint = c.name
	FROM sys.all_columns a
		INNER JOIN sys.tables b ON a.object_id = b.object_id
		INNER JOIN sys.default_constraints c ON a.default_object_id = c.object_id
	WHERE 
		b.name='SQLBuild_Logging' AND
		a.name = 'AllowScriptBlock'

	IF(@Constraint <> 'DF_SqlBuildLogging_AllowScriptBlock')
	BEGIN

			DECLARE @sql varchar(400)
			SET @sql = 'ALTER TABLE [dbo].[SqlBuild_Logging] DROP CONSTRAINT ' + @Constraint
			EXECUTE (@sql)
			print 'Dropped constraint ''' + @Constraint + ''
			ALTER TABLE [dbo].[SqlBuild_Logging] ADD CONSTRAINT DF_SqlBuildLogging_AllowScriptBlock DEFAULT(1) FOR AllowScriptBlock
			print 'Added contraint  DF_SqlBuildLogging_AllowScriptBlock'
	END
END TRY
BEGIN CATCH
	print @@ERROR
	print 'Error dropping and adding constraint'
END CATCH 