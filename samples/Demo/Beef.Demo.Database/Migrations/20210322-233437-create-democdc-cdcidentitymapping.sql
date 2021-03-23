CREATE TABLE [DemoCdc].[CdcIdentityMapping] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [Schema] VARCHAR(50) NOT NULL,
  [Table] VARCHAR(128) NOT NULL,
  [Key] NVARCHAR(128) NOT NULL,
  [Identifier] NVARCHAR(128) NOT NULL,
  CONSTRAINT [PK_DemoCdc_CdcIdentityMapping_SchemaTableKey] PRIMARY KEY CLUSTERED ([Schema], [Table], [Key]),
  CONSTRAINT [IX_DemoCdc_CdcIdentityMapping_SchemaTableIdentifier] UNIQUE ([Schema], [Table], [Identifier])
);