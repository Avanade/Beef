CREATE TABLE [DemoCdc].[CdcTracking] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [CdcTrackingId] INT IDENTITY (1, 1) NOT NULL PRIMARY KEY NONCLUSTERED ([CdcTrackingId] ASC),
  [Schema] VARCHAR(50) NOT NULL,
  [Table] VARCHAR(128) NOT NULL,
  [Key] NVARCHAR(128) NOT NULL,
  [Hash] NVARCHAR(32) NOT NULL,
  [EnvelopeId] INT NOT NULL,
  CONSTRAINT [IX_DemoCdc_CdcTracking_SchemaTableKey] UNIQUE CLUSTERED ([Schema], [Table], [Key])
);