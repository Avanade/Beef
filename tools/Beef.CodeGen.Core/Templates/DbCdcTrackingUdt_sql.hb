{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
CREATE TYPE [{{CdcSchema}}].[udt{{CdcTrackingTableName}}List] AS TABLE (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [Key] NVARCHAR(128) NOT NULL,
  [Hash] NVARCHAR(32) NOT NULL
)