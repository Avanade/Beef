{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
CREATE TABLE [{{CdcSchema}}].[{{CdcIdentityMappingTableName}}] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [Schema] VARCHAR(50) NOT NULL,
  [Table] VARCHAR(128) NOT NULL,
  [Key] NVARCHAR(128) NOT NULL,
  [Identifier] NVARCHAR(128) NOT NULL,
  CONSTRAINT [PK_{{CdcSchema}}_{{CdcIdentityMappingTableName}}_SchemaTableKey] PRIMARY KEY CLUSTERED ([Schema], [Table], [Key]),
  CONSTRAINT [IX_{{CdcSchema}}_{{CdcIdentityMappingTableName}}_SchemaTableIdentifier] UNIQUE ([Schema], [Table], [Identifier])
);