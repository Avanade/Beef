{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
CREATE TABLE [{{CdcSchema}}].[{{CdcIdentifierMappingTableName}}] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [Schema] VARCHAR(50) NOT NULL,
  [Table] VARCHAR(128) NOT NULL,
  [Key] NVARCHAR(128) NOT NULL,
  [GlobalId] NVARCHAR(128) NOT NULL,
  CONSTRAINT [PK_{{CdcSchema}}_{{CdcIdentifierMappingTableName}}_SchemaTableKey] PRIMARY KEY CLUSTERED ([Schema], [Table], [Key]),
  CONSTRAINT [IX_{{CdcSchema}}_{{CdcIdentifierMappingTableName}}_SchemaTableGlobalId] UNIQUE ([Schema], [Table], [GlobalId])
);