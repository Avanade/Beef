{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
CREATE TABLE [{{CdcSchema}}].[{{CdcTrackingTableName}}] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [{{CdcTrackingTableName}}Id] INT IDENTITY (1, 1) NOT NULL PRIMARY KEY NONCLUSTERED ([{{CdcTrackingTableName}}Id] ASC),
  [Schema] VARCHAR(50) NOT NULL,
  [Table] VARCHAR(128) NOT NULL,
  [Key] NVARCHAR(128) NOT NULL,
  [Hash] NVARCHAR(32) NOT NULL,
  [EnvelopeId] INT NOT NULL,
  CONSTRAINT [IX_{{CdcSchema}}_{{CdcTrackingTableName}}_SchemaTableKey] UNIQUE CLUSTERED ([Schema], [Table], [Key])
);