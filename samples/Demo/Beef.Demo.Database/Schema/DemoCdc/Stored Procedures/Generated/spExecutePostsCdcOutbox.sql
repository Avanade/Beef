CREATE PROCEDURE [DemoCdc].[spExecutePostsCdcOutbox]
  @MaxQuerySize INT NULL = 100,
  @GetIncompleteOutbox BIT NULL = 0,
  @OutboxIdToMarkComplete INT NULL = NULL,
  @CompleteTrackingList AS [DemoCdc].[udtCdcTrackingList] READONLY
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost.
   */

  SET NOCOUNT ON;

  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

    -- Mark the outbox as complete and merge the tracking info; then return the updated outbox data and stop!
    IF (@OutboxIdToMarkComplete IS NOT NULL)
    BEGIN
      UPDATE [_outbox] SET
          [_outbox].[IsComplete] = 1,
          [_outbox].[CompletedDate] = GETUTCDATE()
        FROM [DemoCdc].[PostsOutbox] AS [_outbox]
        WHERE OutboxId = @OutboxIdToMarkComplete 

      MERGE INTO [DemoCdc].[CdcTracking] WITH (HOLDLOCK) AS [_ct]
        USING @CompleteTrackingList AS [_list] ON ([_ct].[Schema] = 'Legacy' AND [_ct].[Table] = 'Posts' AND [_ct].[Key] = [_list].[Key])
        WHEN MATCHED AND EXISTS (
            SELECT [_list].[Key], [_list].[Hash]
            EXCEPT
            SELECT [_ct].[Key], [_ct].[Hash])
          THEN UPDATE SET [_ct].[Hash] = [_list].[Hash], [_ct].[OutboxId] = @OutboxIdToMarkComplete
        WHEN NOT MATCHED BY TARGET
          THEN INSERT ([Schema], [Table], [Key], [Hash], [OutboxId])
            VALUES ('Legacy', 'Posts', [_list].[Key], [_list].[Hash], @OutboxIdToMarkComplete);

      SELECT [_outbox].[OutboxId], [_outbox].[CreatedDate], [_outbox].[IsComplete], [_outbox].[CompletedDate]
        FROM [DemoCdc].[PostsOutbox] AS [_outbox]
        WHERE [_outbox].OutboxId = @OutboxIdToMarkComplete

      COMMIT TRANSACTION
      RETURN 0;
    END

    -- Declare variables.
    DECLARE @PostsBaseMinLsn BINARY(10), @PostsMinLsn BINARY(10), @PostsMaxLsn BINARY(10)
    DECLARE @CommentsBaseMinLsn BINARY(10), @CommentsMinLsn BINARY(10), @CommentsMaxLsn BINARY(10)
    DECLARE @CommentsTagsBaseMinLsn BINARY(10), @CommentsTagsMinLsn BINARY(10), @CommentsTagsMaxLsn BINARY(10)
    DECLARE @PostsTagsBaseMinLsn BINARY(10), @PostsTagsMinLsn BINARY(10), @PostsTagsMaxLsn BINARY(10)
    DECLARE @OutboxId INT

    -- Get the latest 'base' minimum.
    SET @PostsBaseMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Posts');
    SET @CommentsBaseMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Comments');
    SET @CommentsTagsBaseMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Tags');
    SET @PostsTagsBaseMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Tags');

    -- Where requesting for incomplete outbox, get first that is marked as incomplete.
    IF (@GetIncompleteOutbox = 1)
    BEGIN
      SELECT TOP 1
          @PostsMinLsn = [_outbox].[PostsMinLsn],
          @PostsMaxLsn = [_outbox].[PostsMaxLsn],
          @CommentsMinLsn = [_outbox].[CommentsMinLsn],
          @CommentsMaxLsn = [_outbox].[CommentsMaxLsn],
          @CommentsTagsMinLsn = [_outbox].[CommentsTagsMinLsn],
          @CommentsTagsMaxLsn = [_outbox].[CommentsTagsMaxLsn],
          @PostsTagsMinLsn = [_outbox].[PostsTagsMinLsn],
          @PostsTagsMaxLsn = [_outbox].[PostsTagsMaxLsn],
          @OutboxId = [OutboxId]
        FROM [DemoCdc].[PostsOutbox] AS [_outbox]
        WHERE [_outbox].[IsComplete] = 0 
        ORDER BY [_outbox].[OutboxId]

      -- Where no incomplete outbox is found then stop!
      IF (@OutboxId IS NULL)
      BEGIN
        COMMIT TRANSACTION
        RETURN 0;
      END

      SET @MaxQuerySize = 1000000  -- Override to a very large number to get all rows within the existing range!
    END
    ELSE
    BEGIN
      -- Check that there are no incomplete outboxes; if there are then stop with error; otherwise, continue!
      DECLARE @IsComplete BIT

      SELECT TOP 1
          @OutboxId = [_outbox].[OutboxId],
          @PostsMinLsn = [_outbox].[PostsMaxLsn],
          @CommentsMinLsn = [_outbox].[CommentsMaxLsn],
          @CommentsTagsMinLsn = [_outbox].[CommentsTagsMaxLsn],
          @PostsTagsMinLsn = [_outbox].[PostsTagsMaxLsn],
          @IsComplete = [_outbox].IsComplete
        FROM [DemoCdc].[PostsOutbox] AS [_outbox]
        ORDER BY [_outbox].[IsComplete] ASC, [_outbox].[OutboxId] DESC

      IF (@IsComplete = 0) -- Cannot continue where there is an incomplete outbox; must be completed.
      BEGIN
        SELECT [_outbox].[OutboxId], [_outbox].[CreatedDate], [_outbox].[IsComplete], [_outbox].[CompletedDate]
          FROM [DemoCdc].[PostsOutbox] AS [_outbox]
          WHERE [_outbox].OutboxId = @OutboxId 

        COMMIT TRANSACTION
        RETURN -1;
      END
      ELSE
      BEGIN
        SET @OutboxId = null -- Force creation of a new outbox.
      END

      IF (@IsComplete IS NULL) -- No previous outbox; i.e. is the first time!
      BEGIN
        SET @PostsMinLsn = @PostsBaseMinLsn;
        SET @CommentsMinLsn = @CommentsBaseMinLsn;
        SET @CommentsTagsMinLsn = @CommentsTagsBaseMinLsn;
        SET @PostsTagsMinLsn = @PostsTagsBaseMinLsn;
      END
      ELSE
      BEGIN
        -- Increment the minimum as the last has already been processed.
        SET @PostsMinLsn = sys.fn_cdc_increment_lsn(@PostsMinLsn)
        SET @CommentsMinLsn = sys.fn_cdc_increment_lsn(@CommentsMinLsn)
        SET @CommentsTagsMinLsn = sys.fn_cdc_increment_lsn(@CommentsTagsMinLsn)
        SET @PostsTagsMinLsn = sys.fn_cdc_increment_lsn(@PostsTagsMinLsn)
      END

      -- Get the maximum LSN.
      SET @PostsMaxLsn = sys.fn_cdc_get_max_lsn();
      SET @CommentsMaxLsn = @PostsMaxLsn
      SET @CommentsTagsMaxLsn = @PostsMaxLsn
      SET @PostsTagsMaxLsn = @PostsMaxLsn

      -- Verify the maximum query size and correct (reset) where applicable.
      IF (@MaxQuerySize IS NULL OR @MaxQuerySize < 1 OR @MaxQuerySize > 10000)
      BEGIN
        SET @MaxQuerySize = 100
      END
    END

    -- The minimum can not be less than the base or an error will occur, so realign where not correct.
    IF (@PostsMinLsn < @PostsBaseMinLsn) BEGIN SET @PostsMinLsn = @PostsBaseMinLsn END
    IF (@CommentsMinLsn < @CommentsBaseMinLsn) BEGIN SET @CommentsMinLsn = @CommentsBaseMinLsn END
    IF (@CommentsTagsMinLsn < @CommentsTagsBaseMinLsn) BEGIN SET @CommentsTagsMinLsn = @CommentsTagsBaseMinLsn END
    IF (@PostsTagsMinLsn < @PostsTagsBaseMinLsn) BEGIN SET @PostsTagsMinLsn = @PostsTagsBaseMinLsn END

    -- Find changes on the root table: Legacy.Posts - this determines overall operation type: 'create', 'update' or 'delete'.
    DECLARE @hasChanges BIT
    SET @hasChanges = 0

    IF (@PostsMinLsn < @PostsMaxLsn)
    BEGIN
      SELECT TOP (@MaxQuerySize)
          [_cdc].[__$start_lsn] AS [_Lsn],
          [_cdc].[__$operation] AS [_Op],
          [_cdc].[PostsId] AS [PostsId]
        INTO #_changes
        FROM cdc.fn_cdc_get_net_changes_Legacy_Posts(@PostsMinLsn, @PostsMaxLsn, 'all') AS [_cdc]
        ORDER BY [_cdc].[__$start_lsn]

      IF (@@ROWCOUNT <> 0)
      BEGIN
        SET @hasChanges = 1
        SELECT @PostsMinLsn = MIN([_Lsn]), @PostsMaxLsn = MAX([_Lsn]) FROM #_changes
      END
    END

    -- Find changes on related table: Comments (Legacy.Comments) - assume all are 'update' operation (i.e. it doesn't matter).
    IF (@CommentsMinLsn < @CommentsMaxLsn)
    BEGIN
      SELECT TOP (@MaxQuerySize)
          [_cdc].[__$start_lsn] AS [_Lsn],
          4 AS [_Op],
          [p].[PostsId] AS [PostsId]
        INTO #c
        FROM cdc.fn_cdc_get_net_changes_Legacy_Comments(@CommentsMinLsn, @CommentsMaxLsn, 'all') AS [_cdc]
        INNER JOIN [Legacy].[Posts] AS [p] WITH (NOLOCK) ON ([_cdc].[PostsId] = [p].[PostsId])
        ORDER BY [_cdc].[__$start_lsn]

      IF (@@ROWCOUNT <> 0)
      BEGIN
        SET @hasChanges = 1
        SELECT @CommentsMinLsn = MIN([_Lsn]), @CommentsMaxLsn = MAX([_Lsn]) FROM #c

        INSERT INTO #_changes
          SELECT * 
            FROM #c AS [_c]
            WHERE NOT EXISTS (SELECT * FROM #_changes AS [_chg] WHERE [_chg].[PostsId] = [_c].[PostsId])
      END
    END

    -- Find changes on related table: CommentsTags (Legacy.Tags) - assume all are 'update' operation (i.e. it doesn't matter).
    IF (@CommentsTagsMinLsn < @CommentsTagsMaxLsn)
    BEGIN
      SELECT TOP (@MaxQuerySize)
          [_cdc].[__$start_lsn] AS [_Lsn],
          4 AS [_Op],
          [p].[PostsId] AS [PostsId]
        INTO #ct
        FROM cdc.fn_cdc_get_net_changes_Legacy_Tags(@CommentsTagsMinLsn, @CommentsTagsMaxLsn, 'all') AS [_cdc]
        INNER JOIN [Legacy].[Comments] AS [c] WITH (NOLOCK) ON ([_cdc].[ParentType] = 'C' AND [_cdc].[ParentId] = [c].[CommentsId])
        INNER JOIN [Legacy].[Posts] AS [p] WITH (NOLOCK) ON ([c].[PostsId] = [p].[PostsId])
        ORDER BY [_cdc].[__$start_lsn]

      IF (@@ROWCOUNT <> 0)
      BEGIN
        SET @hasChanges = 1
        SELECT @CommentsTagsMinLsn = MIN([_Lsn]), @CommentsTagsMaxLsn = MAX([_Lsn]) FROM #ct

        INSERT INTO #_changes
          SELECT * 
            FROM #ct AS [_ct]
            WHERE NOT EXISTS (SELECT * FROM #_changes AS [_chg] WHERE [_chg].[PostsId] = [_ct].[PostsId])
      END
    END

    -- Find changes on related table: PostsTags (Legacy.Tags) - assume all are 'update' operation (i.e. it doesn't matter).
    IF (@PostsTagsMinLsn < @PostsTagsMaxLsn)
    BEGIN
      SELECT TOP (@MaxQuerySize)
          [_cdc].[__$start_lsn] AS [_Lsn],
          4 AS [_Op],
          [p].[PostsId] AS [PostsId]
        INTO #pt
        FROM cdc.fn_cdc_get_net_changes_Legacy_Tags(@PostsTagsMinLsn, @PostsTagsMaxLsn, 'all') AS [_cdc]
        INNER JOIN [Legacy].[Posts] AS [p] WITH (NOLOCK) ON ([_cdc].[ParentType] = 'P' AND [_cdc].[ParentId] = [p].[PostsId])
        ORDER BY [_cdc].[__$start_lsn]

      IF (@@ROWCOUNT <> 0)
      BEGIN
        SET @hasChanges = 1
        SELECT @PostsTagsMinLsn = MIN([_Lsn]), @PostsTagsMaxLsn = MAX([_Lsn]) FROM #pt

        INSERT INTO #_changes
          SELECT * 
            FROM #pt AS [_pt]
            WHERE NOT EXISTS (SELECT * FROM #_changes AS [_chg] WHERE [_chg].[PostsId] = [_pt].[PostsId])
      END
    END

    -- Create a new outbox where not processing an existing.
    IF (@OutboxId IS NULL AND @hasChanges = 1)
    BEGIN
      DECLARE @InsertedOutboxId TABLE([OutboxId] INT)

      INSERT INTO [DemoCdc].[PostsOutbox] (
          [PostsMinLsn],
          [PostsMaxLsn],
          [CommentsMinLsn],
          [CommentsMaxLsn],
          [CommentsTagsMinLsn],
          [CommentsTagsMaxLsn],
          [PostsTagsMinLsn],
          [PostsTagsMaxLsn],
          [CreatedDate],
          [IsComplete]
        ) 
        OUTPUT inserted.OutboxId INTO @InsertedOutboxId
        VALUES (
          @PostsMinLsn,
          @PostsMaxLsn,
          @CommentsMinLsn,
          @CommentsMaxLsn,
          @CommentsTagsMinLsn,
          @CommentsTagsMaxLsn,
          @PostsTagsMinLsn,
          @PostsTagsMaxLsn,
          GETUTCDATE(),
          0
        )

        SELECT @OutboxId = [OutboxId] FROM @InsertedOutboxId
    END

    -- Return the *latest* outbox data.
    SELECT [_outbox].[OutboxId], [_outbox].[CreatedDate], [_outbox].[IsComplete], [_outbox].[CompletedDate]
      FROM [DemoCdc].[PostsOutbox] AS [_outbox]
      WHERE [_outbox].OutboxId = @OutboxId 

    -- Exit here if there were no changes found.
    IF (@hasChanges = 0)
    BEGIN
      COMMIT TRANSACTION
      RETURN 0
    END

    -- Root table: Legacy.Posts - uses LEFT OUTER JOIN's to get the deleted records, as well as any previous Tracking Hash value.
    SELECT
        [_ct].[Hash] AS [_TrackingHash],
        [_chg].[_Op] AS [_OperationType],
        [_chg].[PostsId] AS [PostsId],
        [p].[Text] AS [Text],
        [p].[Date] AS [Date]
      FROM #_changes AS [_chg]
      LEFT OUTER JOIN [DemoCdc].[CdcTracking] AS [_ct] ON ([_ct].[Schema] = 'Legacy' AND [_ct].[Table] = 'Posts' AND [_ct].[Key] = CAST([_chg].[PostsId] AS NVARCHAR(128)))
      LEFT OUTER JOIN [Legacy].[Posts] AS [p] ON ([p].[PostsId] = [_chg].[PostsId])

    -- Related table: Comments (Legacy.Comments) - only use INNER JOINS to get what is actually there right now.
    SELECT
        [c].[CommentsId] AS [CommentsId],
        [c].[PostsId] AS [PostsId],
        [c].[Text] AS [Text],
        [c].[Date] AS [Date]
      FROM #_changes AS [_chg]
      INNER JOIN [Legacy].[Posts] AS [p] ON ([p].[PostsId] = [_chg].[PostsId])
      INNER JOIN [Legacy].[Comments] AS [c] ON ([c].[PostsId] = [p].[PostsId])
      WHERE [_chg].[_Op] <> 1

    -- Related table: CommentsTags (Legacy.Tags) - only use INNER JOINS to get what is actually there right now.
    SELECT
        [p].[PostsId] AS [Posts_PostsId],  -- Additional joining column (informational).
        [ct].[TagsId] AS [TagsId],
        [ct].[ParentId] AS [CommentsId],
        [ct].[Text] AS [Text]
      FROM #_changes AS [_chg]
      INNER JOIN [Legacy].[Posts] AS [p] ON ([p].[PostsId] = [_chg].[PostsId])
      INNER JOIN [Legacy].[Comments] AS [c] ON ([c].[PostsId] = [p].[PostsId])
      INNER JOIN [Legacy].[Tags] AS [ct] ON ([ct].[ParentType] = 'C' AND [ct].[ParentId] = [c].[CommentsId])
      WHERE [_chg].[_Op] <> 1

    -- Related table: PostsTags (Legacy.Tags) - only use INNER JOINS to get what is actually there right now.
    SELECT
        [pt].[TagsId] AS [TagsId],
        [pt].[ParentId] AS [PostsId],
        [pt].[Text] AS [Text]
      FROM #_changes AS [_chg]
      INNER JOIN [Legacy].[Posts] AS [p] ON ([p].[PostsId] = [_chg].[PostsId])
      INNER JOIN [Legacy].[Tags] AS [pt] ON ([pt].[ParentType] = 'P' AND [pt].[ParentId] = [p].[PostsId])
      WHERE [_chg].[_Op] <> 1

    -- Commit the transaction.
    COMMIT TRANSACTION
  END TRY
  BEGIN CATCH
    -- Rollback transaction and rethrow error.
    IF @@TRANCOUNT > 0
      ROLLBACK TRANSACTION;

    THROW;
  END CATCH
END