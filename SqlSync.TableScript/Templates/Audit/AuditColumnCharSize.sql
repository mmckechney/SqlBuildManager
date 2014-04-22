IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = '<<Audit Table Name>>' AND TABLE_SCHEMA = '<<schema>>' AND COLUMN_NAME = '<<Column Name>>' AND CHARACTER_MAXIMUM_LENGTH = <<Char Length>>) OR
	NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = '<<Table Name>>' AND TABLE_SCHEMA = '<<schema>>' AND COLUMN_NAME = '<<Column Name>>' AND CHARACTER_MAXIMUM_LENGTH = <<Char Length>>)
BEGIN
	DECLARE @length int
	SELECT @length = CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = '<<Table Name>>' AND TABLE_SCHEMA = '<<schema>>'AND COLUMN_NAME = '<<Column Name>>'
	
	DECLARE @sql nvarchar(800)
	SET @sql = 'ALTER TABLE [<<schema>>].[<<Audit Table Name>>] ALTER COLUMN [<<Column Name>>] [<<Column Type>>] ('+CAST(@length as varchar)+')'
	EXEC sp_executesql @sql
	Print '<<schema>>.<<Audit Table Name>> :: Altered Column <<Column Name>> [<<Column Type>>]('+CAST(@length as varchar)+')'
END
GO

