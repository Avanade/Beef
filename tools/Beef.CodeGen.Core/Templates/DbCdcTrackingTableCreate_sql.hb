{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
CREATE TABLE [{{CdcSchema}}].[{{CdcTrackingTableName}}] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [{{CdcTrackingTableName}}Id] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY NONCLUSTERED ([{{CdcTrackingTableName}}Id] ASC),
  [Schema] VARCHAR(50) NOT NULL,
  [Table] VARCHAR(128) NOT NULL,
  [Key] NVARCHAR(128) NOT NULL,
  [Hash] NVARCHAR(32) NOT NULL,
  [OutboxId] INT NOT NULL,
  CONSTRAINT [IX_{{CdcSchema}}_{{CdcTrackingTableName}}_SchemaTableKey] UNIQUE CLUSTERED ([Schema], [Table], [Key])
);