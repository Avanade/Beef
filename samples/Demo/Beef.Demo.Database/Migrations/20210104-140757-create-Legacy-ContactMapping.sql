-- Migration Script

BEGIN TRANSACTION

CREATE TABLE [Legacy].[ContactMapping] (
  [ContactMappingId] INT IDENTITY (1, 1) PRIMARY KEY NONCLUSTERED ([ContactMappingId] ASC),
  [ContactId] INT NOT NULL,
  [UniqueId] UNIQUEIDENTIFIER NOT NULL,
  CONSTRAINT [IX_Legacy_ContactMapping_ContactId] UNIQUE CLUSTERED ([ContactId]),
  CONSTRAINT [IX_Legacy_ContactMapping_UniqueId] UNIQUE ([UniqueId])
);

COMMIT TRANSACTION