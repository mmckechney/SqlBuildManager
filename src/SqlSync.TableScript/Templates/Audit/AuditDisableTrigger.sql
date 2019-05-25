IF EXISTS (SELECT 1 FROM sys.objects WHERE name = '<<Trigger Name>>' AND type = 'TR')
BEGIN
	ALTER TABLE [<<schema>>].[<<Table Name>>] DISABLE TRIGGER <<Trigger Name>>
		Print '<<Trigger Name>> :: Disabled'
END
GO
