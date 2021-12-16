CREATE TABLE [Hr].[EventOutbox] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [EventOutboxId] BIGINT IDENTITY (1, 1) NOT NULL PRIMARY KEY NONCLUSTERED ([EventOutboxId] ASC),
  [EnqueuedDate] DATETIME2 NOT NULL,
  [PartitionKey] NVARCHAR(128) NULL,
  [DequeuedDate] DATETIME2 NULL,
  CONSTRAINT [IX_Hr_EventOutbox_DequeuedDate] UNIQUE CLUSTERED ([DequeuedDate], [EventOutboxId])
);