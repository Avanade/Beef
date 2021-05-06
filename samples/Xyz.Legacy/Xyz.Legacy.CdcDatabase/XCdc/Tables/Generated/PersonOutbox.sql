CREATE TABLE [XCdc].[PersonOutbox] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [OutboxId] INT IDENTITY (1, 1) NOT NULL PRIMARY KEY CLUSTERED ([OutboxId] ASC),
  [CreatedDate] DATETIME2 NOT NULL,
  [PersonMinLsn] BINARY(10) NULL,  -- Primary table: Legacy.Person
  [PersonMaxLsn] BINARY(10) NULL,
  [PersonAddressMinLsn] BINARY(10) NULL,  -- Related table: Legacy.PersonAddress
  [PersonAddressMaxLsn] BINARY(10) NULL,
  [IsComplete] BIT NOT NULL,
  [CompletedDate] DATETIME2 NULL,
  [CorrelationId] NVARCHAR(64) NULL,
  [HasDataLoss] BIT NOT NULL
);