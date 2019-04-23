-- Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

-- Disable all foreign key constraints.
EXEC sp_msforeachtable @command1 = "ALTER TABLE ? NOCHECK CONSTRAINT ALL", @whereand = "and schema_name(syso.schema_id) <> 'dbo'"

-- Delete data from all tables.
EXEC sp_MSForEachTable @command1 = "SET QUOTED_IDENTIFIER ON; DELETE FROM ?", @whereand = "and schema_name(syso.schema_id) <> 'dbo'"

-- Re-enable all constraints.
EXEC sp_msforeachtable @command1 = "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL", @whereand = "and schema_name(syso.schema_id) <> 'dbo'"

-- Reset all identities (IDENT) to 0.
EXEC sp_MSforeachtable @command1 = "IF OBJECTPROPERTY(object_id('?'), 'TableHasIdentity') = 1 BEGIN DBCC CHECKIDENT ( '?', RESEED, 0) END", @whereand = "and schema_name(syso.schema_id) <> 'dbo'"