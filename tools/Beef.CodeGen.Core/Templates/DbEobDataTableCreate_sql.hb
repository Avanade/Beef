{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
CREATE TABLE [{{Schema}}].[{{EventOutboxTableName}}Data] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [EventOutboxId] BIGINT NOT NULL PRIMARY KEY CLUSTERED ([EventOutboxId] ASC),
  [EventId] UNIQUEIDENTIFIER,
  [Subject] NVARCHAR(1024),
  [Action] NVARCHAR(128) NULL,
  [CorrelationId] NVARCHAR(64) NULL,
  [TenantId] UNIQUEIDENTIFIER NULL,
  [ValueType] NVARCHAR(1024) NULL,
  [EventData] VARBINARY(MAX) NULL
);