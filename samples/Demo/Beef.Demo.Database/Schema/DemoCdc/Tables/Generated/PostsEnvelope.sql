CREATE TABLE [DemoCdc].[PostsEnvelope] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [EnvelopeId] INT IDENTITY (1, 1) NOT NULL PRIMARY KEY CLUSTERED ([EnvelopeId] ASC),
  [CreatedDate] DATETIME NOT NULL,
  [PMinLsn] BINARY(10) NOT NULL,  -- Primary table: Legacy.Posts
  [PMaxLsn] BINARY(10) NOT NULL,
  [CMinLsn] BINARY(10) NOT NULL,  -- Related table: Legacy.Comments
  [CMaxLsn] BINARY(10) NOT NULL,
  [TMinLsn] BINARY(10) NOT NULL,  -- Related table: Legacy.Tags
  [TMaxLsn] BINARY(10) NOT NULL,
  [T2MinLsn] BINARY(10) NOT NULL,  -- Related table: Legacy.Tags
  [T2MaxLsn] BINARY(10) NOT NULL,
  [IsComplete] BIT NOT NULL,
  [CompletedDate] DATETIME NULL
);