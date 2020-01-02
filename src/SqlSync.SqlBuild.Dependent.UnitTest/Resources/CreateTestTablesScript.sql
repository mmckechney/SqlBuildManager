IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TransactionTest' AND TABLE_SCHEMA = 'dbo')
BEGIN
	CREATE TABLE [dbo].[TransactionTest](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Message] [varchar](200) NULL,
	[Guid] [uniqueidentifier] NULL,
	[DateTimeStamp] [datetime] NULL
) ON [PRIMARY]
END