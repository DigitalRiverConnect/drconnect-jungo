--USE mcmeta
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'hibernate_unique_key' AND type = 'U')
BEGIN 

CREATE TABLE [dbo].[hibernate_unique_key](
	[next_hi] [int] NULL
) 

exec sp_dba_setpermission 'hibernate_unique_key'

END
GO

