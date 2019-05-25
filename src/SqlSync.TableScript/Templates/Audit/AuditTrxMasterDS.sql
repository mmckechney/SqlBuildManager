IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.Tables WHERE TABLE_NAME = 'AuditTransactionMaster' AND TABLE_TYPE = 'BASE TABLE')
BEGIN
	CREATE TABLE AuditTransactionMaster(
		TransId uniqueidentifier NOT NULL ,
		TableName [varchar](128) NOT NULL,
		ModifiedBy [varchar](50) NOT NULL DEFAULT SYSTEM_USER,
		ModifiedDate datetime NOT NULL DEFAULT getdate(),
		ModifyType [varchar](50) NOT NULL,
		RowsAffected [int] NOT NULL,
		WorkStation [varchar](128) NOT NULL DEFAULT HOST_NAME(),
		Application [varchar](128) NOT NULL DEFAULT APP_NAME(),
		SessionUser [char](128) NOT NULL DEFAULT SESSION_USER,
		InsertById [int] NULL,
		IndividualIDPatient [int] NULL,
		ObjectType [int] NULL)
	
	Print 'AuditTransactionMaster :: Created Table'

END
ELSE
BEGIN
	IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = 'AuditTransactionMaster' AND COLUMN_NAME = 'SpId')
	BEGIN
		ALTER TABLE AuditTransactionMaster ADD SpId [int] NULL DEFAULT @@SPID
		Print 'AuditTransactionMaster :: Added SpId'
	END
	
	IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = 'AuditTransactionMaster' AND COLUMN_NAME = 'InsertById')
	BEGIN
		ALTER TABLE AuditTransactionMaster ADD InsertById [int] NULL
		Print 'AuditTransactionMaster :: Added InsertById'
	END
	
	IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = 'AuditTransactionMaster' AND COLUMN_NAME = 'IndividualIDPatient')
	BEGIN
		ALTER TABLE AuditTransactionMaster ADD IndividualIDPatient [int] NULL
		Print 'AuditTransactionMaster :: Added IndividualIDPatient'
	END
	
	IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = 'AuditTransactionMaster' AND COLUMN_NAME = 'ObjectType')
	BEGIN
		ALTER TABLE AuditTransactionMaster ADD ObjectType [int] NULL
		Print 'AuditTransactionMaster :: Added ObjectType'
	END
END
GO

