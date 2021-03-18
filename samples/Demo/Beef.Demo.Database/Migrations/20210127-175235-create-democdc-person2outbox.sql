CREATE TABLE [DemoCdc].[Person2Outbox] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [OutboxId] INT IDENTITY (1, 1) NOT NULL PRIMARY KEY CLUSTERED ([OutboxId] ASC),
  [CreatedDate] DATETIME NOT NULL,
  [Person2MinLsn] BINARY(10) NOT NULL,  -- Primary table: Demo.Person2
  [Person2MaxLsn] BINARY(10) NOT NULL,
  [IsComplete] BIT NOT NULL,
  [CompletedDate] DATETIME NULL,
  [HasDataLoss] BIT NOT NULL
);