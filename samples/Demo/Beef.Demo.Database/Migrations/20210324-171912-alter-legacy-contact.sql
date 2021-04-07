-- Alter table: Legacy.Contact

BEGIN TRANSACTION

ALTER TABLE [Legacy].[Contact]
  ADD [AlternateContactId] INT NULL

COMMIT TRANSACTION