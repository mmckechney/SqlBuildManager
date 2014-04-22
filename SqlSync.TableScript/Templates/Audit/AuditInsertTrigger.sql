IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[<<schema>>].[<<Trigger Name>>]'))
BEGIN

	EXEC dbo.sp_executesql @statement = N'ALTER TRIGGER [<<schema>>].[<<Trigger Name>>] ON [<<schema>>].[<<Table Name>>] FOR INSERT
	AS
	BEGIN
		DECLARE @count int
		SELECT @count = count(*) FROM inserted
	
		-- Insert the values of the deleted row into the audit table
		INSERT INTO [<<schema>>].[<<Audit Table Name>>] ([ModifyType],[RowsAffected],<<Column List>>)
			SELECT ''INSERT'',@count, <<Column List>> FROM inserted
	END
	'
	PRINT 'ALTERED Trigger: [<<schema>>].[<<Trigger Name>>]'
	
END
ELSE
BEGIN
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[<<schema>>].[<<Trigger Name>>]'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'CREATE TRIGGER [<<schema>>].[<<Trigger Name>>] ON [<<schema>>].[<<Table Name>>] FOR INSERT
	AS
	BEGIN
		DECLARE @count int
		SELECT @count = count(*) FROM inserted
	
		-- Insert the values of the deleted row into the audit table
		INSERT INTO [<<schema>>].[<<Audit Table Name>>] ([ModifyType],[RowsAffected],<<Column List>>) 
			SELECT ''INSERT'',@count, <<Column List>> FROM inserted
	END
	'
	PRINT 'CREATED Trigger: [<<schema>>].[<<Trigger Name>>]'
END
END
GO


