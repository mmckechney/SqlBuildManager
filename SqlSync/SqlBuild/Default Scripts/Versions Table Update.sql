IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'versions' AND TABLE_SCHEMA = 'dbo')
	BEGIN
		INSERT INTO dbo.versions 
			(versionnumber,versiondate,State,Comment) 
		VALUES
			('#BuildDescription#',getdate(),'Completed','#BuildFileName#')
	END
ELSE
	BEGIN
		RAISERROR('The dbo.versions table was not found in the database!', 16, 1)
	END
GO