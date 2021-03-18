CREATE TABLE [DemoCdc].[PostsOutbox] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [OutboxId] INT IDENTITY (1, 1) NOT NULL PRIMARY KEY CLUSTERED ([OutboxId] ASC),
  [CreatedDate] DATETIME NOT NULL,
  [PostsMinLsn] BINARY(10) NOT NULL,  -- Primary table: Legacy.Posts
  [PostsMaxLsn] BINARY(10) NOT NULL,
  [CommentsMinLsn] BINARY(10) NOT NULL,  -- Related table: Legacy.Comments
  [CommentsMaxLsn] BINARY(10) NOT NULL,
  [CommentsTagsMinLsn] BINARY(10) NOT NULL,  -- Related table: Legacy.Tags
  [CommentsTagsMaxLsn] BINARY(10) NOT NULL,
  [PostsTagsMinLsn] BINARY(10) NOT NULL,  -- Related table: Legacy.Tags
  [PostsTagsMaxLsn] BINARY(10) NOT NULL,
  [IsComplete] BIT NOT NULL,
  [CompletedDate] DATETIME NULL,
  [HasDataLoss] BIT NOT NULL
);