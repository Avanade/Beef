CREATE TABLE [Outbox].[EventOutbox] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [EventOutboxId] BIGINT IDENTITY (1, 1) NOT NULL PRIMARY KEY NONCLUSTERED ([EventOutboxId] ASC),
  [PartitionKey] NVARCHAR(127) NULL,
  [Destination] NVARCHAR(127) NULL,
  [EnqueuedDate] DATETIME2 NOT NULL,
  [DequeuedDate] DATETIME2 NULL,
  CONSTRAINT [IX_Outbox_EventOutbox_DequeuedDate] UNIQUE CLUSTERED ([PartitionKey], [Destination], [DequeuedDate], [EventOutboxId])
);