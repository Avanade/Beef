-- Enable for the database.
IF (SELECT TOP 1 is_cdc_enabled FROM sys.databases WHERE [name] = N'XyzLegacy') = 0
BEGIN
  EXEC sp_changedbowner 'sa'
  EXEC sys.sp_cdc_enable_db
END

-- Enable for the Legacy.Person table.
IF (SELECT TOP 1 is_tracked_by_cdc FROM sys.tables WHERE [OBJECT_ID] = OBJECT_ID(N'Legacy.Person')) = 0
BEGIN
  EXEC sys.sp_cdc_enable_table  
    @source_schema = N'Legacy',  
    @source_name = N'Person',  
    @role_name = NULL,
    @supports_net_changes = 0
END

-- Enable for the Legacy.PersonAddress table.
IF (SELECT TOP 1 is_tracked_by_cdc FROM sys.tables WHERE [OBJECT_ID] = OBJECT_ID(N'Legacy.PersonAddress')) = 0
BEGIN
  EXEC sys.sp_cdc_enable_table  
    @source_schema = N'Legacy',  
    @source_name = N'PersonAddress',  
    @role_name = NULL,
    @supports_net_changes = 0
END