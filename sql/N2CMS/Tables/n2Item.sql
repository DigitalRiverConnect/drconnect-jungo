--USE [n2msft] /* INT environment's N2CMS database is not consistent, called n2cms_msft */
--GO 

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_n2Item_Type' AND OBJECT_NAME(OBJECT_ID) = 'n2Item')
BEGIN  
	CREATE NONCLUSTERED INDEX [IX_n2Item_Type] ON [dbo].[n2Item] 
	( 
	[Type] ASC 
	)WITH (ONLINE = ON) ON [PRIMARY]
END
GO 
 
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_n2Item_ParentID' AND OBJECT_NAME(OBJECT_ID) = 'n2Item')
BEGIN  
	CREATE NONCLUSTERED INDEX [IX_n2Item_ParentID] ON [dbo].[n2Item] 
	( 
	[ParentID] ASC 
	)WITH (ONLINE = ON) ON [PRIMARY]
END
GO 

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_n2Item_ParentZone' AND OBJECT_NAME(OBJECT_ID) = 'n2Item')
BEGIN  
	CREATE NONCLUSTERED INDEX [IX_n2Item_ParentZone] ON [dbo].[n2Item] 
	( 
	[ParentID] ASC, 
	[ZoneName] ASC 
	)WITH (ONLINE = ON) ON [PRIMARY]
END
GO
