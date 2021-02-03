EXEC sys.sp_cdc_enable_table  
	@source_schema = N'Legacy',  
	@source_name   = N'Posts',  
	@role_name     = null,
	@supports_net_changes = 1

EXEC sys.sp_cdc_enable_table  
	@source_schema = N'Legacy',  
	@source_name   = N'Comments',  
	@role_name     = null,
	@supports_net_changes = 1

EXEC sys.sp_cdc_enable_table  
	@source_schema = N'Legacy',  
	@source_name   = N'Tags',  
	@role_name     = null,
	@supports_net_changes = 1