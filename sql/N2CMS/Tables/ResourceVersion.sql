--USE mcmeta
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'ResourceVersion' AND type = 'U')
BEGIN 

CREATE TABLE [dbo].[ResourceVersion](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ResourceType] [varchar](255) NOT NULL,
	[ResourceKey] [varchar](255) NOT NULL,
	[CompanyID] [varchar](50) NOT NULL,
	[Site] [varchar](50) NOT NULL,
	[CultureCode] [varchar](10) NOT NULL,
	[ResourceValue] [nvarchar](max) NULL,
	[Version] [int] NOT NULL,
	[IsPreview] [bit] NOT NULL,
	[LastModified] [datetime2](7) NOT NULL,
	[ModifiedBy] [varchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) )


ALTER TABLE [dbo].[ResourceVersion] ADD  DEFAULT ((0)) FOR [IsPreview]

exec sp_dba_setpermission 'ResourceVersion'

END
GO

