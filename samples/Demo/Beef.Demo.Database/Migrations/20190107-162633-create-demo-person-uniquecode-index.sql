-- Migration Script

BEGIN TRANSACTION

CREATE UNIQUE INDEX [UQ_Person_UniqueCode] ON [Demo].[Person]([UniqueCode]) WHERE [UniqueCode] IS NOT NULL
	
COMMIT TRANSACTION