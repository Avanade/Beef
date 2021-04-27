CREATE TYPE [Demo].[udtEventOutboxList] AS TABLE (
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