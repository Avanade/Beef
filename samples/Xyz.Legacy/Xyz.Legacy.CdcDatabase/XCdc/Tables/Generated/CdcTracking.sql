CREATE TABLE [XCdc].[CdcTracking] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [CdcTrackingId] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY NONCLUSTERED ([CdcTrackingId] ASC),
  [Schema] VARCHAR(50) NOT NULL,
  [Table] VARCHAR(128) NOT NULL,
  [Key] NVARCHAR(128) NOT NULL,
  [Hash] NVARCHAR(32) NOT NULL,
  [OutboxId] INT NOT NULL,
  CONSTRAINT [IX_XCdc_CdcTracking_SchemaTableKey] UNIQUE CLUSTERED ([Schema], [Table], [Key])
);