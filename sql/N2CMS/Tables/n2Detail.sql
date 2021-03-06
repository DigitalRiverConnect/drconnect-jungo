--USE [n2msft] /* INT environment's N2CMS database is not consistent, called n2cms_msft */
--GO 

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_n2Detail_ItemID' AND OBJECT_NAME(OBJECT_ID) = 'n2Detail')
BEGIN  
	CREATE NONCLUSTERED INDEX [IX_n2Detail_ItemID] ON [dbo].[n2Detail] 
	( 
	[ItemID] ASC 
	)WITH (ONLINE = ON) ON [PRIMARY]
END
GO
