--USE mcmeta
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'ResourceVersionDeleted' AND type = 'U')
BEGIN 

CREATE TABLE [dbo].[ResourceVersionDeleted](
	[ID] [int] NOT NULL,
	[DeletionDateTime] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ) 


exec sp_dba_setpermission 'ResourceVersionDeleted'

END
GO

