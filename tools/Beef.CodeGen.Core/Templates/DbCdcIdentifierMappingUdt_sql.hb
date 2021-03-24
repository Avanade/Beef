{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
CREATE TYPE [{{CdcSchema}}].[udt{{CdcIdentifierMappingTableName}}List] AS TABLE (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [Schema] VARCHAR(50) NOT NULL,
  [Table] VARCHAR(128) NOT NULL,
  [Key] NVARCHAR(128) NOT NULL,
  [GlobalId] NVARCHAR(128) NOT NULL
)