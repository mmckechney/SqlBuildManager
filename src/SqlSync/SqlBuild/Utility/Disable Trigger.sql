${Title: Disable Trigger <<Trigger Name>>}
IF EXISTS (SELECT 1 FROM dbo.sysobjects WITH (NOLOCK) WHERE name = '<<Trigger Name>>' AND type = 'TR')
BEGIN
	ALTER TABLE [<<Table Schema>>].[<<Table Name>>] DISABLE TRIGGER [<<Trigger Name>>]
	Print '<<Trigger Name>> :: Disabled'
END
