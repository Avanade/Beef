-- Alter table: Legacy.Contact

BEGIN TRANSACTION

ALTER TABLE [Legacy].[Contact]
  ADD [legacy_system_code] NVARCHAR(10) NULL

COMMIT TRANSACTION