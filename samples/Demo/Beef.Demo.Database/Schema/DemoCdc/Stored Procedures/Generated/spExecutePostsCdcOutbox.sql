CREATE PROCEDURE [DemoCdc].[spExecutePostsCdcOutbox]
  @MaxQuerySize INT NULL = 100,             -- Maximum size of query to limit the number of changes to a manageable batch (performance vs failure trade-off).
  @ContinueWithDataLoss BIT NULL = 0        -- Ignores data loss and continues; versus throwing an error.
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost.
   */

  SET NOCOUNT ON;

  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

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

    -- Check if there is already an incomplete batch and attempt to reprocess.
    SELECT
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

    -- There should never be more than one incomplete outbox.
    IF @@ROWCOUNT > 1
    BEGIN
      ;THROW 56002, 'There are multiple incomplete outboxes; there should not be more than one incomplete outbox at any one time.', 1
    END

    -- Where there is no incomplete outbox then the next should be processed.
    IF (@OutboxId IS NULL)
    BEGIN
      -- Get the last outbox processed.
      SELECT TOP 1
          @PostsMinLsn = [_outbox].[PostsMaxLsn],
          @CommentsMinLsn = [_outbox].[CommentsMaxLsn],
          @CommentsTagsMinLsn = [_outbox].[CommentsTagsMaxLsn],
          @PostsTagsMinLsn = [_outbox].[PostsTagsMaxLsn]
        FROM [DemoCdc].[PostsOutbox] AS [_outbox]
        ORDER BY [_outbox].[IsComplete] ASC, [_outbox].[OutboxId] DESC

      IF (@@ROWCOUNT = 0) -- No previous outbox; i.e. is the first time!
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

    -- The minimum should _not_ be less than the base otherwise we have lost data; either continue with this data loss, or error and stop.
    DECLARE @hasDataLoss BIT
    SET @hasDataLoss = 0

    IF (@PostsMinLsn < @PostsBaseMinLsn) BEGIN IF (@ContinueWithDataLoss = 1) BEGIN SET @hasDataLoss = 1; SET @PostsMinLsn = @PostsBaseMinLsn END ELSE BEGIN ;THROW 56002, 'Unexpected data loss error for ''Legacy.Posts''; this indicates that the CDC data has probably been cleaned up before being successfully processed.', 1; END END
    IF (@CommentsMinLsn < @CommentsBaseMinLsn) BEGIN IF (@ContinueWithDataLoss = 1) BEGIN SET @hasDataLoss = 1; SET @CommentsMinLsn = @CommentsBaseMinLsn END ELSE BEGIN ;THROW 56002, 'Unexpected data loss error for ''Legacy.Comments''; this indicates that the CDC data has probably been cleaned up before being successfully processed.', 1; END END
    IF (@CommentsTagsMinLsn < @CommentsTagsBaseMinLsn) BEGIN IF (@ContinueWithDataLoss = 1) BEGIN SET @hasDataLoss = 1; SET @CommentsTagsMinLsn = @CommentsTagsBaseMinLsn END ELSE BEGIN ;THROW 56002, 'Unexpected data loss error for ''Legacy.Tags''; this indicates that the CDC data has probably been cleaned up before being successfully processed.', 1; END END
    IF (@PostsTagsMinLsn < @PostsTagsBaseMinLsn) BEGIN IF (@ContinueWithDataLoss = 1) BEGIN SET @hasDataLoss = 1; SET @PostsTagsMinLsn = @PostsTagsBaseMinLsn END ELSE BEGIN ;THROW 56002, 'Unexpected data loss error for ''Legacy.Tags''; this indicates that the CDC data has probably been cleaned up before being successfully processed.', 1; END END

    -- Find changes on the root table: Legacy.Posts - this determines overall operation type: 'create', 'update' or 'delete'.
    DECLARE @hasChanges BIT
    SET @hasChanges = 0

    IF (@PostsMinLsn <= @PostsMaxLsn)
    BEGIN
      SELECT TOP (@MaxQuerySize)
          [_cdc].[__$start_lsn] AS [_Lsn],
          [_cdc].[__$operation] AS [_Op],
          [_cdc].[PostsId] AS [PostsId]
        INTO #_changes
        FROM cdc.fn_cdc_get_all_changes_Legacy_Posts(@PostsMinLsn, @PostsMaxLsn, 'all') AS [_cdc]
        ORDER BY [_cdc].[__$start_lsn]

      IF (@@ROWCOUNT <> 0)
      BEGIN
        SET @hasChanges = 1
        SELECT @PostsMinLsn = MIN([_Lsn]), @PostsMaxLsn = MAX([_Lsn]) FROM #_changes
      END
    END

    -- Find changes on related table: Comments (Legacy.Comments) - assume all are 'update' operation (i.e. it doesn't matter).
    IF (@CommentsMinLsn <= @CommentsMaxLsn)
    BEGIN
      SELECT TOP (@MaxQuerySize)
          [_cdc].[__$start_lsn] AS [_Lsn],
          4 AS [_Op],
          [p].[PostsId] AS [PostsId]
        INTO #c
        FROM cdc.fn_cdc_get_all_changes_Legacy_Comments(@CommentsMinLsn, @CommentsMaxLsn, 'all') AS [_cdc]
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
    IF (@CommentsTagsMinLsn <= @CommentsTagsMaxLsn)
    BEGIN
      SELECT TOP (@MaxQuerySize)
          [_cdc].[__$start_lsn] AS [_Lsn],
          4 AS [_Op],
          [p].[PostsId] AS [PostsId]
        INTO #ct
        FROM cdc.fn_cdc_get_all_changes_Legacy_Tags(@CommentsTagsMinLsn, @CommentsTagsMaxLsn, 'all') AS [_cdc]
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
    IF (@PostsTagsMinLsn <= @PostsTagsMaxLsn)
    BEGIN
      SELECT TOP (@MaxQuerySize)
          [_cdc].[__$start_lsn] AS [_Lsn],
          4 AS [_Op],
          [p].[PostsId] AS [PostsId]
        INTO #pt
        FROM cdc.fn_cdc_get_all_changes_Legacy_Tags(@PostsTagsMinLsn, @PostsTagsMaxLsn, 'all') AS [_cdc]
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
    IF (@OutboxId IS NULL AND (@hasDataLoss = 1 OR @hasChanges = 1))
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
          [IsComplete],
          [HasDataLoss]
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
          0,
          @hasDataLoss
        )

        SELECT @OutboxId = [OutboxId] FROM @InsertedOutboxId
    END
    ELSE
    BEGIN
      IF (@OutboxId IS NOT NULL AND @hasDataLoss = 1)
      BEGIN
        UPDATE [DemoCdc].[PostsOutbox] 
          SET [HasDataLoss] = @hasDataLoss
          WHERE [OutboxId] = @OutboxId
      END
    END

    -- Return the *latest* outbox data.
    SELECT [_outbox].[OutboxId], [_outbox].[CreatedDate], [_outbox].[IsComplete], [_outbox].[CompletedDate], [_outBox].[HasDataLoss]
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
        [_chg].[_Op] AS [_OperationType],
        [_chg].[_Lsn] AS [_Lsn],
        [_ct].[Hash] AS [_TrackingHash],
        [_chg].[PostsId] AS [PostsId],
        [p].[PostsId] AS [TableKey_PostsId],
        [p].[Text] AS [Text],
        [p].[Date] AS [Date]
      FROM #_changes AS [_chg]
      LEFT OUTER JOIN [Legacy].[Posts] AS [p] ON ([p].[PostsId] = [_chg].[PostsId])
      LEFT OUTER JOIN [DemoCdc].[CdcTracking] AS [_ct] ON ([_ct].[Schema] = 'Legacy' AND [_ct].[Table] = 'Posts' AND [_ct].[Key] = CAST([_chg].[PostsId] AS NVARCHAR(128)))

    -- Related table: Comments (Legacy.Comments) - only use INNER JOINS to get what is actually there right now (where applicable).
    SELECT DISTINCT
        [c].[CommentsId] AS [CommentsId],
        [c].[PostsId] AS [PostsId],
        [c].[Text] AS [Text],
        [c].[Date] AS [Date]
      FROM #_changes AS [_chg]
      INNER JOIN [Legacy].[Posts] AS [p] ON ([p].[PostsId] = [_chg].[PostsId])
      INNER JOIN [Legacy].[Comments] AS [c] ON ([c].[PostsId] = [p].[PostsId])
      WHERE [_chg].[_Op] <> 1

    -- Related table: CommentsTags (Legacy.Tags) - only use INNER JOINS to get what is actually there right now (where applicable).
    SELECT DISTINCT
        [p].[PostsId] AS [Posts_PostsId],  -- Additional joining column (informational).
        [ct].[TagsId] AS [TagsId],
        [ct].[ParentId] AS [CommentsId],
        [ct].[Text] AS [Text]
      FROM #_changes AS [_chg]
      INNER JOIN [Legacy].[Posts] AS [p] ON ([p].[PostsId] = [_chg].[PostsId])
      INNER JOIN [Legacy].[Comments] AS [c] ON ([c].[PostsId] = [p].[PostsId])
      INNER JOIN [Legacy].[Tags] AS [ct] ON ([ct].[ParentType] = 'C' AND [ct].[ParentId] = [c].[CommentsId])
      WHERE [_chg].[_Op] <> 1

    -- Related table: PostsTags (Legacy.Tags) - only use INNER JOINS to get what is actually there right now (where applicable).
    SELECT DISTINCT
        [pt].[TagsId] AS [TagsId],
        [pt].[ParentId] AS [PostsId],
        [pt].[Text] AS [Text]
      FROM #_changes AS [_chg]
      INNER JOIN [Legacy].[Posts] AS [p] ON ([p].[PostsId] = [_chg].[PostsId])
      INNER JOIN [Legacy].[Tags] AS [pt] ON ([pt].[ParentType] = 'P' AND [pt].[ParentId] = [p].[PostsId])
      WHERE [_chg].[_Op] <> 1

    -- Commit the transaction.
    COMMIT TRANSACTION
    RETURN 0
  END TRY
  BEGIN CATCH
    -- Rollback transaction and rethrow error.
    IF @@TRANCOUNT > 0
      ROLLBACK TRANSACTION;

    THROW;
  END CATCH
END