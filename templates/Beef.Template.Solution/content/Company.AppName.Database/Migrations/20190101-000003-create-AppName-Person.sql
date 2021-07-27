﻿-- Migration Script

BEGIN TRANSACTION

CREATE TABLE [AppName].[Person](
   [PersonId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
   [FirstName] NVARCHAR(100) NULL,
   [LastName] NVARCHAR(100) NULL,
   [GenderCode] NVARCHAR(50) NULL,
   [Birthday] DATE NULL,
   [RowVersion] TIMESTAMP NOT NULL,
   [CreatedBy] NVARCHAR(250) NULL,
   [CreatedDate] DATETIME2 NULL,
   [UpdatedBy] NVARCHAR(250) NULL,
   [UpdatedDate] DATETIME2 NULL,
)
	
COMMIT TRANSACTION