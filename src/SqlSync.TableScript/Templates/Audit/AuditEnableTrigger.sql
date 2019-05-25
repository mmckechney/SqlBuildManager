IF EXISTS (SELECT 1 FROM sys.objects WHERE name = '<<Trigger Name>>' AND type = 'TR')
BEGIN
	ALTER TABLE [<<schema>>].[<<Table Name>>] ENABLE TRIGGER <<Trigger Name>>
	Print '<<Trigger Name>> :: Enabled'
END
GO
