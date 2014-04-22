USE [SqlCodeReview]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[CodeReview](
	[CodeReviewId] [uniqueidentifier] NOT NULL,
	[ScriptId] [varchar](50) NULL,
	[ReviewDate] [datetime] NULL,
	[ReviewBy] [varchar](50) NULL,
	[ReviewStatus] [int] NOT NULL,
	[Comment] [varchar](500) NULL,
	[ReviewNumber] [varchar](50) NULL,
	[CheckSum] [varchar](50) NULL,
	[ValidationKey] [varchar](50) NULL,
 CONSTRAINT [PK_CodeReview] PRIMARY KEY NONCLUSTERED 
(
	[CodeReviewId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

USE [SqlCodeReview]
GO

CREATE CLUSTERED INDEX [ScriptId_ClusteredIDX] ON [dbo].[CodeReview]
(
	[ScriptId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO


