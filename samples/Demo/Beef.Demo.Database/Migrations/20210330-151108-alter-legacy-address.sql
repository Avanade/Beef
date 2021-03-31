-- Alter table: Legacy.Contact

BEGIN TRANSACTION

ALTER TABLE [Legacy].[Address]
  ADD [AlternateAddressId] INT NULL

COMMIT TRANSACTION