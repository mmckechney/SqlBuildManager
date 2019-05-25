IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.Tables WHERE TABLE_NAME = 'AuditTransactionMaster' AND TABLE_SCHEMA = 'dbo' AND TABLE_TYPE = 'BASE TABLE')
BEGIN
	CREATE TABLE dbo.AuditTransactionMaster(
		TransId uniqueidentifier NOT NULL ,
		TableName [varchar](128) NOT NULL,
		ModifiedBy [varchar](50) NOT NULL DEFAULT SYSTEM_USER,
		ModifiedDate datetime NOT NULL DEFAULT getdate(),
		ModifyType [varchar](50) NOT NULL,
		RowsAffected [int] NOT NULL,
		WorkStation [varchar](128) NOT NULL DEFAULT HOST_NAME(),
		Application [varchar](128) NOT NULL DEFAULT APP_NAME(),
		SessionUser [char](128) NOT NULL DEFAULT SESSION_USER)
	
	Print 'dbo.AuditTransactionMaster :: Created Table'

END
GO

