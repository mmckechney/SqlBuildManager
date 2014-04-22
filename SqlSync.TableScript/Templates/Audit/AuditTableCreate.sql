IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.Tables WHERE TABLE_NAME = '<<Audit Table Name>>'  AND TABLE_SCHEMA = '<<schema>>' AND TABLE_TYPE = 'BASE TABLE')
BEGIN
	CREATE TABLE [<<schema>>].[<<Audit Table Name>>](
		[<<Audit Table Name>>ID] [bigint] IDENTITY(1,1) NOT NULL,
		[ModifiedBy] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL CONSTRAINT [DF_<<Audit Table Name>>_ModifiedBy]  DEFAULT (suser_sname()),
		[ModifiedDate] datetime NOT NULL CONSTRAINT [DF_<<Audit Table Name>>_ModifiedDate]  DEFAULT (getdate()),
		[ModifyType] [varchar](50) NOT NULL,
		[RowsAffected] [int] NOT NULL,
		[WorkStation] [varchar](128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL CONSTRAINT [DF_<<Audit Table Name>>_WorkStation]  DEFAULT (host_name()),
		[Application] [varchar](128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL CONSTRAINT [DF_<<Audit Table Name>>_ApplicationName]  DEFAULT (app_name()),
		[SessionUser] [char](128) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL CONSTRAINT [DF_<<Audit Table Name>>_SessionUser]  DEFAULT (user_name()),
		)
	Print ' [<<schema>>].[<<Audit Table Name>>] :: Created Table'

END
GO

