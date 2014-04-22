${Title: Enable trigger <<Trigger Name>>}
IF EXISTS (SELECT 1 FROM dbo.sysobjects WHERE name = '<<Trigger Name>>' AND type = 'TR')
BEGIN
	ALTER TABLE [<<Table Schema>>].[<<Table Name>>] ENABLE TRIGGER [<<Trigger Name>>]
	Print '<<Trigger Name>> :: Enabled'
END
GO
