CREATE TABLE [DemoCdc].[ContactOutbox] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [OutboxId] INT IDENTITY (1, 1) NOT NULL PRIMARY KEY CLUSTERED ([OutboxId] ASC),
  [CreatedDate] DATETIME NOT NULL,
  [ContactMinLsn] BINARY(10) NULL,  -- Primary table: Legacy.Contact
  [ContactMaxLsn] BINARY(10) NULL,
  [AddressMinLsn] BINARY(10) NULL,  -- Related table: Legacy.Address
  [AddressMaxLsn] BINARY(10) NULL,
  [IsComplete] BIT NOT NULL,
  [CompletedDate] DATETIME NULL,
  [CorrelationId] NVARCHAR(64) NULL,
  [HasDataLoss] BIT NOT NULL
);