IF EXISTS (SELECT name FROM sys.objects WHERE name = '<<Trigger Name>>' AND type = 'TR')
	DROP TRIGGER <<Trigger Name>>
	Print '<<Trigger Name>> :: Dropped'
GO

CREATE TRIGGER <<Trigger Name>> ON <<Table Name>> FOR DELETE
AS
BEGIN
	DECLARE @TrxId uniqueidentifier
	SET @TrxId = newid()

	DECLARE @count int
	SELECT @count = count(*) FROM deleted
	
	DECLARE @DeletedBy as varchar(50)
	SELECT @DeletedBy = (SELECT convert(int,convert(varbinary(4),context_info)) 
			FROM master.dbo.sysprocesses WHERE spid = @@SPID)
			
	DECLARE @InsertedBy int, @IndividualID int, @ObjectType int
	SELECT @InsertedBy = <<InsertByColumn>>,  @IndividualID = <<IndividualIDColumn>>, @ObjectType = <<ObjectTypeColumn>> FROM deleted
	
	INSERT INTO AuditTransactionMaster (TransId,TableName,ModifiedBy,ModifyType,RowsAffected,InsertById,IndividualIDPatient,ObjectType)  
		VALUES (@TrxId,'<<Table Name>>',@DeletedBy,'DELETE',@count,@InsertedBy,@IndividualID,@ObjectType)
	INSERT INTO <<Audit Table Name>> (TransId,<<Column List>>) SELECT @TrxId, <<Column List>> FROM deleted
END
GO
Print '<<Trigger Name>> :: Created'
GO

