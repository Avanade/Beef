-- Alter table: Demo.Contact

BEGIN TRANSACTION

ALTER TABLE [Demo].[Contact]
  ADD [StatusCode] NVARCHAR(50) NULL

COMMIT TRANSACTION