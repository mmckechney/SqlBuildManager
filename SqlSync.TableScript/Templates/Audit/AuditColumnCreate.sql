IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = '<<Audit Table Name>>' AND TABLE_SCHEMA = '<<schema>>' AND COLUMN_NAME = '<<Column Name>>')
BEGIN
	ALTER TABLE [<<schema>>].[<<Audit Table Name>>] ADD [<<Column Name>>] [<<Column Type>>] <<Char Length>>
	Print '<<schema>>.<<Audit Table Name>> :: Added Column <<Column Name>> [<<Column Type>>] <<Char Length>>'
END
GO

