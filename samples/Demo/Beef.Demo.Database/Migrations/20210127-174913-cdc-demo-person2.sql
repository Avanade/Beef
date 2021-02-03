-- Enable Change Data Capture (CDC) for table: Demo.Person2

EXEC sys.sp_cdc_enable_table
  @source_schema = N'Demo',
  @source_name   = N'Person2',
  @role_name     = null,
  @supports_net_changes = 1