CREATE TABLE [Outbox].[EventOutboxData] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [EventOutboxDataId] BIGINT NOT NULL PRIMARY KEY CLUSTERED ([EventOutboxDataId] ASC) CONSTRAINT FK_Outbox_EventOutboxData_EventOutbox FOREIGN KEY REFERENCES [Outbox].[EventOutbox] ([EventOutboxId]) ON DELETE CASCADE,
  [EventId] NVARCHAR(127),
  [Destination] NVARCHAR(127),
  [Subject] NVARCHAR(511) NULL,
  [Action] NVARCHAR(255) NULL,
  [Type] NVARCHAR(1023) NULL,
  [Source] NVARCHAR(1023) NULL,
  [Timestamp] DATETIMEOFFSET,
  [CorrelationId] NVARCHAR(127),
  [TenantId] NVARCHAR(127),
  [PartitionKey] NVARCHAR(127),
  [ETag] NVARCHAR(127),
  [Attributes] VARBINARY(MAX) NULL,
  [Data] VARBINARY(MAX) NULL
);