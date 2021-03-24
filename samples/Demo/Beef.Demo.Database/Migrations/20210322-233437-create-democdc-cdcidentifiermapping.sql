CREATE TABLE [DemoCdc].[CdcIdentifierMapping] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [Schema] VARCHAR(50) NOT NULL,
  [Table] VARCHAR(128) NOT NULL,
  [Key] NVARCHAR(128) NOT NULL,
  [GlobalId] NVARCHAR(128) NOT NULL,
  CONSTRAINT [PK_DemoCdc_CdcIdentifierMapping_SchemaTableKey] PRIMARY KEY CLUSTERED ([Schema], [Table], [Key]),
  CONSTRAINT [IX_DemoCdc_CdcIdentifierMapping_SchemaTableGlobalId] UNIQUE ([Schema], [Table], [GlobalId])
);