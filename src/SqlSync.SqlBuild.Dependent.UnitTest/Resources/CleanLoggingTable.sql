IF EXISTS (SELECT 1 FROM sys.objects WHERE name = 'SqlBuild_Logging' AND type = 'U')
BEGIN
	IF EXISTS (SELECT 1 FROM dbo.SqlBuild_Logging WHERE CommitDate < '{0}')
	BEGIN
		DELETE FROM SqlBuild_Logging
	END
END