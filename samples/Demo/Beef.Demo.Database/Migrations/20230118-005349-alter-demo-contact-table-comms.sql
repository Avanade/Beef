-- Alter table: [Demo].[Contact]

BEGIN TRANSACTION

ALTER TABLE [Demo].[Contact]
  ADD [Comms] NVARCHAR(MAX) NULL

COMMIT TRANSACTION