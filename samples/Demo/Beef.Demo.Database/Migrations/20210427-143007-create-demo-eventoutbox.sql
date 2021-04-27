CREATE TABLE [Demo].[EventOutbox] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [EventOutboxId] BIGINT IDENTITY (1, 1) NOT NULL PRIMARY KEY NONCLUSTERED ([EventOutboxId] ASC),
  [EnqueuedDate] DATETIME NOT NULL,
  [DequeuedDate] DATETIME NULL,
  CONSTRAINT [IX_Demo__DequeuedDate] UNIQUE CLUSTERED ([DequeuedDate], [EventOutboxId])
);