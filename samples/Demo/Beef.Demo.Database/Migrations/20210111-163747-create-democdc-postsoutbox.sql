CREATE TABLE [DemoCdc].[PostsOutbox] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [OutboxId] INT IDENTITY (1, 1) NOT NULL PRIMARY KEY CLUSTERED ([OutboxId] ASC),
  [CreatedDate] DATETIME NOT NULL,
  [PostsMinLsn] BINARY(10) NULL,  -- Primary table: Legacy.Posts
  [PostsMaxLsn] BINARY(10) NULL,
  [CommentsMinLsn] BINARY(10) NULL,  -- Related table: Legacy.Comments
  [CommentsMaxLsn] BINARY(10) NULL,
  [CommentsTagsMinLsn] BINARY(10) NULL,  -- Related table: Legacy.Tags
  [CommentsTagsMaxLsn] BINARY(10) NULL,
  [PostsTagsMinLsn] BINARY(10) NULL,  -- Related table: Legacy.Tags
  [PostsTagsMaxLsn] BINARY(10) NULL,
  [IsComplete] BIT NOT NULL,
  [CompletedDate] DATETIME NULL,
  [CorrelationId] NVARCHAR(64) NULL,
  [HasDataLoss] BIT NOT NULL
);