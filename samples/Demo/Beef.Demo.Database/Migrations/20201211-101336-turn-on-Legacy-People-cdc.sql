EXEC sys.sp_cdc_enable_table  
	@source_schema = N'Legacy',  
	@source_name   = N'People',  
	@role_name     = null,
	@supports_net_changes = 1