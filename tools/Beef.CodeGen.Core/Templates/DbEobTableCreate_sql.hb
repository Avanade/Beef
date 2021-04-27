{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
CREATE TABLE [{{Schema}}].[{{EventOutboxTableName}}] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [EventOutboxId] BIGINT IDENTITY (1, 1) NOT NULL PRIMARY KEY NONCLUSTERED ([EventOutboxId] ASC),
  [EnqueuedDate] DATETIME NOT NULL,
  [DequeuedDate] DATETIME NULL,
  CONSTRAINT [IX_{{Schema}}_{{EventOutboxName}}_DequeuedDate] UNIQUE CLUSTERED ([DequeuedDate], [EventOutboxId])
);