-- Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

DECLARE t_cursor 
  CURSOR LOCAL READ_ONLY STATIC SCROLL FOR
  SELECT t.TABLE_SCHEMA, t.TABLE_NAME FROM (
	SELECT TABLE_SCHEMA, TABLE_NAME, (SELECT OBJECTPROPERTY(OBJECT_ID(''+TABLE_SCHEMA+ '.'+ TABLE_NAME +''), 'TableTemporalType')) AS TABLE_HISTORY 
	FROM INFORMATION_SCHEMA.TABLES as i
	WHERE i.TABLE_TYPE = 'BASE TABLE' AND i.TABLE_SCHEMA <> 'dbo' AND i.TABLE_SCHEMA <> 'cdc'
  ) AS [t] WHERE t.TABLE_HISTORY IN (0,2)

DECLARE @TableSchema VARCHAR(256)
DECLARE @TableName VARCHAR(256)
DECLARE @SqlCommand VARCHAR(2048)

OPEN t_cursor

-- Disable all foreign key constraints.
FETCH FIRST FROM t_cursor into @TableSchema, @TableName
WHILE @@FETCH_STATUS = 0
BEGIN
  SET @SqlCommand = 'ALTER TABLE [' + @TableSchema + '].[' + @TableName + '] NOCHECK CONSTRAINT ALL'
  PRINT @SqlCommand
  EXECUTE (@SqlCommand)
  FETCH NEXT FROM t_cursor into @TableSchema, @TableName
END

-- Delete data from all tables.
FETCH FIRST FROM t_cursor into @TableSchema, @TableName
WHILE @@FETCH_STATUS = 0
BEGIN
  SET @SqlCommand = 'SET QUOTED_IDENTIFIER ON; DELETE FROM [' + @TableSchema + '].[' + @TableName + ']'
  PRINT @SqlCommand
  EXECUTE (@SqlCommand)
  FETCH NEXT FROM t_cursor into @TableSchema, @TableName
END

-- Re-enable all constraints.
FETCH FIRST FROM t_cursor into @TableSchema, @TableName
WHILE @@FETCH_STATUS = 0
BEGIN
  SET @SqlCommand = 'ALTER TABLE [' + @TableSchema + '].[' + @TableName + '] WITH CHECK CHECK CONSTRAINT ALL'
  PRINT @SqlCommand
  EXECUTE (@SqlCommand)
  FETCH NEXT FROM t_cursor into @TableSchema, @TableName
END

CLOSE t_cursor
DEALLOCATE t_cursor