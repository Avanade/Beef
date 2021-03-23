CREATE TYPE [DemoCdc].[udtCdcIdentityMappingList] AS TABLE (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [Schema] VARCHAR(50) NOT NULL,
  [Table] VARCHAR(128) NOT NULL,
  [Key] NVARCHAR(128) NOT NULL,
  [Identifier] NVARCHAR(128) NOT NULL
)