CREATE PROCEDURE [DemoCdc].[spExecutePostsCdcEnvelope]
  @MaxQuerySize INT NULL = 100,
  @GetIncompleteEnvelope BIT NULL = 0,
  @EnvelopeIdToMarkComplete INT NULL = NULL,
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

    -- Mark the envelope as complete and merge the tracking info; then return the updated envelope data and stop!
    IF (@EnvelopeIdToMarkComplete IS NOT NULL)
    BEGIN
      UPDATE [_env] SET
          [_env].[IsComplete] = 1,
          [_env].[CompletedDate] = GETUTCDATE()
        FROM [DemoCdc].[PostsEnvelope] AS [_env]
        WHERE EnvelopeId = @EnvelopeIdToMarkComplete 

      MERGE INTO [DemoCdc].[CdcTracking] WITH (HOLDLOCK) AS [_ct]
        USING @CompleteTrackingList AS [_list] ON ([_ct].[Schema] = 'Legacy' AND [_ct].[Table] = 'Posts' AND [_ct].[Key] = [_list].[Key])
        WHEN MATCHED AND EXISTS (
            SELECT [_list].[Key], [_list].[Hash]
            EXCEPT
            SELECT [_ct].[Key], [_ct].[Hash])
          THEN UPDATE SET [_ct].[Hash] = [_list].[Hash], [_ct].[EnvelopeId] = @EnvelopeIdToMarkComplete
        WHEN NOT MATCHED BY TARGET
          THEN INSERT ([Schema], [Table], [Key], [Hash], [EnvelopeId])
            VALUES ('Legacy', 'Posts', [_list].[Key], [_list].[Hash], @EnvelopeIdToMarkComplete);

      SELECT [_env].[EnvelopeId], [_env].[CreatedDate], [_env].[IsComplete], [_env].[CompletedDate]
        FROM [DemoCdc].[PostsEnvelope] AS [_env]
        WHERE [_env].EnvelopeId = @EnvelopeIdToMarkComplete

      COMMIT TRANSACTION
      RETURN 0;
    END

    -- Declare variables.
    DECLARE @PostsMinLsn BINARY(10), @PostsMaxLsn BINARY(10)
    DECLARE @CommentsMinLsn BINARY(10), @CommentsMaxLsn BINARY(10)
    DECLARE @CommentsTagsMinLsn BINARY(10), @CommentsTagsMaxLsn BINARY(10)
    DECLARE @PostsTagsMinLsn BINARY(10), @PostsTagsMaxLsn BINARY(10)
    DECLARE @EnvelopeId INT
    DECLARE @IncludeMin BIT

    -- Where requesting for incomplete envelope, get first that is marked as incomplete.
    IF (@GetIncompleteEnvelope = 1)
    BEGIN
      SELECT TOP 1
          @PostsMinLsn = [_env].[PostsMinLsn],
          @PostsMaxLsn = [_env].[PostsMaxLsn],
          @CommentsMinLsn = [_env].[CommentsMinLsn],
          @CommentsMaxLsn = [_env].[CommentsMaxLsn],
          @CommentsTagsMinLsn = [_env].[CommentsTagsMinLsn],
          @CommentsTagsMaxLsn = [_env].[CommentsTagsMaxLsn],
          @PostsTagsMinLsn = [_env].[PostsTagsMinLsn],
          @PostsTagsMaxLsn = [_env].[PostsTagsMaxLsn],
          @EnvelopeId = [EnvelopeId]
        FROM [DemoCdc].[PostsEnvelope] AS [_env]
        WHERE [_env].[IsComplete] = 0 
        ORDER BY [_env].[EnvelopeId]

      -- Where no incomplete envelope is found then stop!
      IF (@EnvelopeId IS NULL)
      BEGIN
        COMMIT TRANSACTION
        RETURN 0;
      END

      SET @MaxQuerySize = 1000000  -- Override to a very large number.
      SET @IncludeMin = 1
    END
    ELSE
    BEGIN
      -- Check that there are no incomplete envelopes; if there are then stop with error; otherwise, continue!
      DECLARE @IsComplete BIT

      SELECT TOP 1
          @EnvelopeId = [_env].[EnvelopeId],
          @PostsMinLsn = [_env].[PostsMaxLsn],
          @CommentsMinLsn = [_env].[CommentsMaxLsn],
          @CommentsTagsMinLsn = [_env].[CommentsTagsMaxLsn],
          @PostsTagsMinLsn = [_env].[PostsTagsMaxLsn],
          @IsComplete = [_env].IsComplete
        FROM [DemoCdc].[PostsEnvelope] AS [_env]
        ORDER BY [_env].[IsComplete] ASC, [_env].[EnvelopeId] DESC

      IF (@IsComplete = 0) -- Cannot continue where there is an incomplete envelope; must be completed.
      BEGIN
        SELECT [_env].[EnvelopeId], [_env].[CreatedDate], [_env].[IsComplete], [_env].[CompletedDate]
          FROM [DemoCdc].[PostsEnvelope] AS [_env]
          WHERE [_env].EnvelopeId = @EnvelopeId 

        COMMIT TRANSACTION
        RETURN -1;
      END
      ELSE
      BEGIN
        SET @EnvelopeId = null -- Force creation of a new envelope.
      END

      IF (@IsComplete IS NULL) -- No previous envelope; i.e. is the first time!
      BEGIN
        SET @PostsMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Posts');
        SET @CommentsMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Comments');
        SET @CommentsTagsMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Tags');
        SET @PostsTagsMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Tags');
        SET @IncludeMin = 1
      END

      SET @PostsMaxLsn = sys.fn_cdc_get_max_lsn();
      SET @CommentsMaxLsn = @PostsMaxLsn
      SET @CommentsTagsMaxLsn = @PostsMaxLsn
      SET @PostsTagsMaxLsn = @PostsMaxLsn

      IF (@MaxQuerySize IS NULL OR @MaxQuerySize < 1 OR @MaxQuerySize > 10000)
      BEGIN
        SET @MaxQuerySize = 100  -- Reset where the value appears invalid. 
      END
    END

    -- Find changes on the root table: Legacy.Posts - this determines overall operation type: 'create', 'update' or 'delete'.
    SELECT TOP (@MaxQuerySize)
        [_cdc].[__$start_lsn] AS [_Lsn],
        [_cdc].[__$operation] AS [_Op],
        [_cdc].[PostsId] AS [PostsId]
      INTO #_changes
      FROM cdc.fn_cdc_get_net_changes_Legacy_Posts(@PostsMinLsn, @PostsMaxLsn, 'all') AS [_cdc]
      WHERE @IncludeMin = 1 OR [_cdc].[__$start_lsn] > @PostsMinLsn
      ORDER BY [_cdc].[__$start_lsn]

    IF (@@ROWCOUNT <> 0)
    BEGIN
      SELECT @PostsMinLsn = MIN([_Lsn]), @PostsMaxLsn = MAX([_Lsn]) FROM #_changes
    END

    -- Find changes on related table: Comments (Legacy.Comments) - assume all are 'update' operation (i.e. it doesn't matter).
    SELECT TOP (@MaxQuerySize)
        [_cdc].[__$start_lsn] AS [_Lsn],
        4 AS [_Op],
        [p].[PostsId] AS [PostsId]
      INTO #c
      FROM cdc.fn_cdc_get_net_changes_Legacy_Comments(@CommentsMinLsn, @CommentsMaxLsn, 'all') AS [_cdc]
      INNER JOIN [Legacy].[Posts] AS [p] WITH (NOLOCK) ON ([_cdc].[PostsId] = [p].[PostsId])
      WHERE @IncludeMin = 1 OR [_cdc].[__$start_lsn] > @CommentsMinLsn
      ORDER BY [_cdc].[__$start_lsn]

    IF (@@ROWCOUNT <> 0)
    BEGIN
      SELECT @CommentsMinLsn = MIN([_Lsn]), @CommentsMaxLsn = MAX([_Lsn]) FROM #c
    END

    INSERT INTO #_changes
      SELECT * 
        FROM #c AS [_c]
        WHERE NOT EXISTS (SELECT * FROM #_changes AS [_chg] WHERE [_chg].[PostsId] = [_c].[PostsId])

    -- Find changes on related table: CommentsTags (Legacy.Tags) - assume all are 'update' operation (i.e. it doesn't matter).
    SELECT TOP (@MaxQuerySize)
        [_cdc].[__$start_lsn] AS [_Lsn],
        4 AS [_Op],
        [p].[PostsId] AS [PostsId]
      INTO #ct
      FROM cdc.fn_cdc_get_net_changes_Legacy_Tags(@CommentsTagsMinLsn, @CommentsTagsMaxLsn, 'all') AS [_cdc]
      INNER JOIN [Legacy].[Comments] AS [c] WITH (NOLOCK) ON ([_cdc].[ParentType] = 'C' AND [_cdc].[ParentId] = [c].[CommentsId])
      INNER JOIN [Legacy].[Posts] AS [p] WITH (NOLOCK) ON ([c].[PostsId] = [p].[PostsId])
      WHERE @IncludeMin = 1 OR [_cdc].[__$start_lsn] > @CommentsTagsMinLsn
      ORDER BY [_cdc].[__$start_lsn]

    IF (@@ROWCOUNT <> 0)
    BEGIN
      SELECT @CommentsTagsMinLsn = MIN([_Lsn]), @CommentsTagsMaxLsn = MAX([_Lsn]) FROM #ct
    END

    INSERT INTO #_changes
      SELECT * 
        FROM #ct AS [_ct]
        WHERE NOT EXISTS (SELECT * FROM #_changes AS [_chg] WHERE [_chg].[PostsId] = [_ct].[PostsId])

    -- Find changes on related table: PostsTags (Legacy.Tags) - assume all are 'update' operation (i.e. it doesn't matter).
    SELECT TOP (@MaxQuerySize)
        [_cdc].[__$start_lsn] AS [_Lsn],
        4 AS [_Op],
        [p].[PostsId] AS [PostsId]
      INTO #pt
      FROM cdc.fn_cdc_get_net_changes_Legacy_Tags(@PostsTagsMinLsn, @PostsTagsMaxLsn, 'all') AS [_cdc]
      INNER JOIN [Legacy].[Posts] AS [p] WITH (NOLOCK) ON ([_cdc].[ParentType] = 'P' AND [_cdc].[ParentId] = [p].[PostsId])
      WHERE @IncludeMin = 1 OR [_cdc].[__$start_lsn] > @PostsTagsMinLsn
      ORDER BY [_cdc].[__$start_lsn]

    IF (@@ROWCOUNT <> 0)
    BEGIN
      SELECT @PostsTagsMinLsn = MIN([_Lsn]), @PostsTagsMaxLsn = MAX([_Lsn]) FROM #pt
    END

    INSERT INTO #_changes
      SELECT * 
        FROM #pt AS [_pt]
        WHERE NOT EXISTS (SELECT * FROM #_changes AS [_chg] WHERE [_chg].[PostsId] = [_pt].[PostsId])

    -- Create a new envelope where not processing an existing.
    IF (@EnvelopeId IS NULL)
    BEGIN
      DECLARE @RowCount INT
      SELECT @RowCount = COUNT(*) FROM #_changes
      IF @RowCount = 0
      BEGIN
        COMMIT TRANSACTION
        RETURN 0;
      END

      DECLARE @InsertedEnvelopeId TABLE([EnvelopeId] INT)

      INSERT INTO [DemoCdc].[PostsEnvelope] (
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
        OUTPUT inserted.EnvelopeId INTO @InsertedEnvelopeId
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

        SELECT @EnvelopeId = [EnvelopeId] FROM @InsertedEnvelopeId
    END

    -- Return the *latest* envelope data.
    SELECT [_env].[EnvelopeId], [_env].[CreatedDate], [_env].[IsComplete], [_env].[CompletedDate]
      FROM [DemoCdc].[PostsEnvelope] AS [_env]
      WHERE [_env].EnvelopeId = @EnvelopeId 

    -- Root table: Legacy.Posts - uses LEFT OUTER JOIN so we get the deleted records too.
    SELECT
        [_ct].[Hash] AS [_TrackingHash],
        [_chg].[_Op] AS [_OperationType],
        [_chg].[PostsId] AS [PostsId],
        [p].[Text] AS [Text],
        [p].[Date] AS [Date]
      FROM #_changes AS [_chg]
      LEFT OUTER JOIN [DemoCdc].[CdcTracking] AS [_ct] WITH (NOLOCK) ON ([_ct].[Schema] = 'Legacy' AND [_ct].[Table] = 'Posts' AND [_ct].[Key] = CAST([_chg].[PostsId] AS NVARCHAR))
      LEFT OUTER JOIN [Legacy].[Posts] AS [p] WITH (NOLOCK) ON ([p].[PostsId] = [_chg].[PostsId])

    -- Related table: Comments (Legacy.Comments) - only use INNER JOINS to get what is actually there right now.
    SELECT
        [c].[CommentsId] AS [CommentsId],
        [c].[PostsId] AS [PostsId],
        [c].[Text] AS [Text],
        [c].[Date] AS [Date]
      FROM #_changes AS [_chg]
      INNER JOIN [Legacy].[Posts] AS [p] WITH (NOLOCK) ON ([p].[PostsId] = [_chg].[PostsId])
      INNER JOIN [Legacy].[Comments] AS [c] WITH (NOLOCK) ON ([c].[PostsId] = [p].[PostsId])
      WHERE [_chg].[_Op] <> 1

    -- Related table: CommentsTags (Legacy.Tags) - only use INNER JOINS to get what is actually there right now.
    SELECT
        [p].[PostsId] AS [Posts_PostsId],  -- Additional joining column (informational).
        [ct].[TagsId] AS [TagsId],
        [ct].[ParentId] AS [CommentsId],
        [ct].[Text] AS [Text]
      FROM #_changes AS [_chg]
      INNER JOIN [Legacy].[Posts] AS [p] WITH (NOLOCK) ON ([p].[PostsId] = [_chg].[PostsId])
      INNER JOIN [Legacy].[Comments] AS [c] WITH (NOLOCK) ON ([c].[PostsId] = [p].[PostsId])
      INNER JOIN [Legacy].[Tags] AS [ct] WITH (NOLOCK) ON ([ct].[ParentType] = 'C' AND [ct].[ParentId] = [c].[CommentsId])
      WHERE [_chg].[_Op] <> 1

    -- Related table: PostsTags (Legacy.Tags) - only use INNER JOINS to get what is actually there right now.
    SELECT
        [pt].[TagsId] AS [TagsId],
        [pt].[ParentId] AS [PostsId],
        [pt].[Text] AS [Text]
      FROM #_changes AS [_chg]
      INNER JOIN [Legacy].[Posts] AS [p] WITH (NOLOCK) ON ([p].[PostsId] = [_chg].[PostsId])
      INNER JOIN [Legacy].[Tags] AS [pt] WITH (NOLOCK) ON ([pt].[ParentType] = 'P' AND [pt].[ParentId] = [p].[PostsId])
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