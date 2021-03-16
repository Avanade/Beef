CREATE TABLE [DemoCdc].[ContactOutbox] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [OutboxId] INT IDENTITY (1, 1) NOT NULL PRIMARY KEY CLUSTERED ([OutboxId] ASC),
  [CreatedDate] DATETIME NOT NULL,
  [ContactMinLsn] BINARY(10) NOT NULL,  -- Primary table: Legacy.Contact
  [ContactMaxLsn] BINARY(10) NOT NULL,
  [AddressMinLsn] BINARY(10) NOT NULL,  -- Related table: Legacy.Address
  [AddressMaxLsn] BINARY(10) NOT NULL,
  [IsComplete] BIT NOT NULL,
  [CompletedDate] DATETIME NULL,
  [HasDataLoss] BIT NOT NULL
);