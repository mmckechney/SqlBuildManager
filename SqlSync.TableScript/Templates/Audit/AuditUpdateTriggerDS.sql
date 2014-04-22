IF EXISTS (SELECT name FROM sys.objects WHERE name = '<<Trigger Name>>' AND type = 'TR')
	DROP TRIGGER <<Trigger Name>>
	Print '<<Trigger Name>> :: Dropped'
GO

CREATE TRIGGER <<Trigger Name>> ON <<Table Name>> FOR UPDATE
AS
BEGIN
	DECLARE @TrxId uniqueidentifier
	SET @TrxId = newid()
	
	DECLARE @count int
	SELECT @count = count(*) FROM inserted
	
	DECLARE @InsertedBy int, @IndividualID int, @ObjectType int
	SELECT @InsertedBy = <<InsertByColumn>>,  @IndividualID = <<IndividualIDColumn>>, @ObjectType = <<ObjectTypeColumn>> FROM inserted
	
	INSERT INTO AuditTransactionMaster (TransId,TableName,ModifyType,RowsAffected,InsertById,IndividualIDPatient,ObjectType) 
		VALUES (@TrxId,'<<Table Name>>','UPDATE',@count,@InsertedBy,@IndividualID,@ObjectType)

	INSERT INTO <<Audit Table Name>> (TransId,<<Column List>>) SELECT @TrxId, <<Column List>> FROM inserted
END
GO
Print '<<Trigger Name>> :: Created'
GO

