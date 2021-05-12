{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
CREATE TYPE [{{Schema}}].[udt{{EventOutboxTableName}}List] AS TABLE (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [EventId] UNIQUEIDENTIFIER,
  [Subject] NVARCHAR(1024),
  [Action] NVARCHAR(128) NULL,
  [CorrelationId] NVARCHAR(64) NULL,
  [TenantId] UNIQUEIDENTIFIER NULL,
  [ValueType] NVARCHAR(1024),
  [EventData] VARBINARY(MAX)
)