CREATE TABLE [DemoCdc].[PersonOutbox] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [OutboxId] INT IDENTITY (1, 1) NOT NULL PRIMARY KEY CLUSTERED ([OutboxId] ASC),
  [CreatedDate] DATETIME NOT NULL,
  [PersonMinLsn] BINARY(10) NULL,  -- Primary table: Demo.Person
  [PersonMaxLsn] BINARY(10) NULL,
  [IsComplete] BIT NOT NULL,
  [CompletedDate] DATETIME NULL,
  [HasDataLoss] BIT NOT NULL
);