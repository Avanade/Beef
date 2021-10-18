-- Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

SELECT * 
   FROM INFORMATION_SCHEMA.TABLES as t
     INNER JOIN INFORMATION_SCHEMA.COLUMNS as c
	   ON t.TABLE_CATALOG = c.TABLE_CATALOG
		 AND t.TABLE_SCHEMA = c.TABLE_SCHEMA
		 AND t.TABLE_NAME = c.TABLE_NAME
   WHERE COLUMNPROPERTY(object_id(t.TABLE_SCHEMA+'.'+t.TABLE_NAME), c.COLUMN_NAME, 'IsComputed') in (1,2)