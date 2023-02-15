-- Migration Script

BEGIN TRANSACTION

CREATE TABLE [Legacy].[People] (
  [Id] INT NOT NULL IDENTITY(1, 1) PRIMARY KEY,
  [First] NVARCHAR(50) NULL,
  [Last] NVARCHAR(50) NULL,
  [BirthDate] DATE NULL,
  [Gender] NVARCHAR(1) NULL,
);

COMMIT TRANSACTION