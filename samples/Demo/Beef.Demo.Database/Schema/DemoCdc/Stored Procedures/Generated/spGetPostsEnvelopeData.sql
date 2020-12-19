CREATE PROCEDURE [DemoCdc].[spGetPostsEnvelopeData]
  @EnvelopeIdToMarkComplete INT NULL = NULL,
  @ReturnIncompleteBatches BIT NULL = 0,
  @MaxBatchSize INT NULL = 100
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost.
   */

  SET NOCOUNT ON;

  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

    -- Mark the batch as complete; then return the updated envelope data and stop!
    IF (@EnvelopeIdToMarkComplete IS NOT NULL)
    BEGIN
      UPDATE [_outbox] SET
          [_outbox].[IsComplete] = 1,
          [_outbox].[CompletedDate] = GETUTCDATE()
        FROM [DemoCdc].[PostsEnvelope] AS [_outbox]
        WHERE EnvelopeId = @EnvelopeIdToMarkComplete 

      SELECT [_outbox].[EnvelopeId], [_outbox].[CreatedDate], [_outbox].[IsComplete], [_outbox].[CompletedDate]
        FROM [DemoCdc].[PostsEnvelope] AS [_outbox]
        WHERE [_outbox].EnvelopeId = @EnvelopeIdToMarkComplete 

      COMMIT TRANSACTION
      RETURN 0;
    END

    -- Declare variables.
    DECLARE @pMinLsn BINARY(10), @pMaxLsn BINARY(10)
    DECLARE @cMinLsn BINARY(10), @cMaxLsn BINARY(10)
    DECLARE @tMinLsn BINARY(10), @tMaxLsn BINARY(10)
    DECLARE @t2MinLsn BINARY(10), @t2MaxLsn BINARY(10)
    DECLARE @EnvelopeId INT

    -- Where requesting for incomplete envelope, get first that is marked as incomplete.
    IF (@ReturnIncompleteBatches = 1)
    BEGIN
      SELECT TOP 1
          @pMinLsn = [_outbox].[PMinLsn],
          @pMaxLsn = [_outbox].[PMaxLsn],
          @cMinLsn = [_outbox].[CMinLsn],
          @cMaxLsn = [_outbox].[CMaxLsn],
          @tMinLsn = [_outbox].[TMinLsn],
          @tMaxLsn = [_outbox].[TMaxLsn],
          @t2MinLsn = [_outbox].[T2MinLsn],
          @t2MaxLsn = [_outbox].[T2MaxLsn],
          @EnvelopeId = [EnvelopeId]
        FROM [DemoCdc].[PostsEnvelope] AS [_outbox]
        WHERE [_outbox].[EnvelopeId] = @EnvelopeIdToMarkComplete 
        ORDER BY [_outbox].[EnvelopeId]

      -- Where no incomplete envelope is found then stop!
      IF (@EnvelopeId IS NULL)
      BEGIN
        COMMIT TRANSACTION
        RETURN 0;
      END

      SET @MaxBatchSize = 1000000  -- Override to a very large number.
    END
    ELSE
    BEGIN
      -- Check that there are no incomplete envelopes; if there are then stop with error; otherwise, continue!
      DECLARE @IsComplete BIT

      SELECT TOP 1
          @EnvelopeId = [_outbox].[EnvelopeId],
          @pMinLsn = [_outbox].[PMaxLsn],
          @cMinLsn = [_outbox].[CMaxLsn],
          @tMinLsn = [_outbox].[TMaxLsn],
          @t2MinLsn = [_outbox].[T2MaxLsn],
          @IsComplete = [_outbox].IsComplete
        FROM [DemoCdc].[PostsEnvelope] AS [_outbox]
        ORDER BY [_outbox].[IsComplete] ASC, [_outbox].[EnvelopeId] DESC

      IF (@IsComplete = 0)
      BEGIN
        SELECT [_outbox].[EnvelopeId], [_outbox].[CreatedDate], [_outbox].[IsComplete], [_outbox].[CompletedDate]
          FROM [DemoCdc].[PostsEnvelope] AS [_outbox]
          WHERE [_outbox].EnvelopeId = @EnvelopeId 

        COMMIT TRANSACTION
        RETURN -1;
      END

      IF (@IsComplete IS NULL)
      BEGIN
        SET @pMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Posts');
        SET @cMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Comments');
        SET @tMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Tags');
        SET @t2MinLsn = sys.fn_cdc_get_min_lsn('Legacy_Tags');
      END

      SET @pMaxLsn = sys.fn_cdc_get_max_lsn();
      SET @cMaxLsn = @pMaxLsn
      SET @tMaxLsn = @pMaxLsn
      SET @t2MaxLsn = @pMaxLsn

      IF (@MaxBatchSize IS NULL OR @MaxBatchSize < 1 OR @MaxBatchSize > 10000)
      BEGIN
        SET @MaxBatchSize = 100  -- Reset where the value appears invalid. 
      END
    END

    -- Find changes on the root table: Legacy.Posts - this determines overall operation type: 'create', 'update' or 'delete'.
    SELECT TOP (@MaxBatchSize)
        [_cdc].[__$start_lsn] AS [_Lsn],
        [_cdc].[__$operation] AS [_Op],
        [_cdc].[PostsId] AS [PostsId]
      INTO #_changes
      FROM cdc.fn_cdc_get_net_changes_Legacy_Posts(@pMinLsn, @pMaxLsn, 'all') AS [_cdc]

    IF (@@ROWCOUNT <> 0)
    BEGIN
      SELECT @pMinLsn = MIN([_Lsn]), @pMaxLsn = MAX([_Lsn]) FROM #_changes
    END

    -- Find changes on related table: Legacy.Comments - assume all are 'update' operation (i.e. it doesn't matter).
    SELECT TOP (@MaxBatchSize)
        [_cdc].[__$start_lsn] AS [_Lsn],
        4 AS [_Op],
        [p].[PostsId] AS [PostsId]
      INTO #c
      FROM cdc.fn_cdc_get_net_changes_Legacy_Comments(@cMinLsn, @cMaxLsn, 'all') AS [_cdc]
      INNER JOIN [Legacy].[Posts] AS [p] WITH (NOLOCK) ON ([_cdc].[PostsId] = [p].[PostsId])

    IF (@@ROWCOUNT <> 0)
    BEGIN
      SELECT @cMinLsn = MIN([_Lsn]), @cMaxLsn = MAX([_Lsn]) FROM #c
    END

    INSERT INTO #_changes
      SELECT * 
        FROM #c AS [_c]
        WHERE NOT EXISTS (SELECT * FROM #_changes AS [_chg] WHERE [_chg].[PostsId] = [_c].[PostsId])

    -- Find changes on related table: Legacy.Tags - assume all are 'update' operation (i.e. it doesn't matter).
    SELECT TOP (@MaxBatchSize)
        [_cdc].[__$start_lsn] AS [_Lsn],
        4 AS [_Op],
        [p].[PostsId] AS [PostsId]
      INTO #t
      FROM cdc.fn_cdc_get_net_changes_Legacy_Tags(@tMinLsn, @tMaxLsn, 'all') AS [_cdc]
      INNER JOIN [Legacy].[Comments] AS [c] WITH (NOLOCK) ON ([_cdc].[ParentType] = 'C' AND [_cdc].[ParentId] = [c].[CommentsId])
      INNER JOIN [Legacy].[Posts] AS [p] WITH (NOLOCK) ON ([c].[PostsId] = [p].[PostsId])

    IF (@@ROWCOUNT <> 0)
    BEGIN
      SELECT @tMinLsn = MIN([_Lsn]), @tMaxLsn = MAX([_Lsn]) FROM #t
    END

    INSERT INTO #_changes
      SELECT * 
        FROM #t AS [_t]
        WHERE NOT EXISTS (SELECT * FROM #_changes AS [_chg] WHERE [_chg].[PostsId] = [_t].[PostsId])

    -- Find changes on related table: Legacy.Tags - assume all are 'update' operation (i.e. it doesn't matter).
    SELECT TOP (@MaxBatchSize)
        [_cdc].[__$start_lsn] AS [_Lsn],
        4 AS [_Op],
        [p].[PostsId] AS [PostsId]
      INTO #t2
      FROM cdc.fn_cdc_get_net_changes_Legacy_Tags(@t2MinLsn, @t2MaxLsn, 'all') AS [_cdc]
      INNER JOIN [Legacy].[Posts] AS [p] WITH (NOLOCK) ON ([_cdc].[ParentType] = 'P' AND [_cdc].[ParentId] = [p].[PostsId])

    IF (@@ROWCOUNT <> 0)
    BEGIN
      SELECT @t2MinLsn = MIN([_Lsn]), @t2MaxLsn = MAX([_Lsn]) FROM #t2
    END

    INSERT INTO #_changes
      SELECT * 
        FROM #t2 AS [_t2]
        WHERE NOT EXISTS (SELECT * FROM #_changes AS [_chg] WHERE [_chg].[PostsId] = [_t2].[PostsId])

    -- Create a new envelope where not processing an existing.
    IF (@EnvelopeId IS NULL)
    BEGIN
      DECLARE @InsertedEnvelopeId TABLE([EnvelopeId] INT)

      INSERT INTO [DemoCdc].[PostsEnvelope] (
          [PMinLsn],
          [PMaxLsn],
          [CMinLsn],
          [CMaxLsn],
          [TMinLsn],
          [TMaxLsn],
          [T2MinLsn],
          [T2MaxLsn],
          [CreatedDate],
          [IsComplete]
        ) 
        OUTPUT inserted.EnvelopeId INTO @InsertedEnvelopeId
        VALUES (
          @pMinLsn,
          @pMaxLsn,
          @cMinLsn,
          @cMaxLsn,
          @tMinLsn,
          @tMaxLsn,
          @t2MinLsn,
          @t2MaxLsn,
          GETUTCDATE(),
          0
        )

        SELECT @EnvelopeId = [EnvelopeId] FROM @InsertedEnvelopeId
    END

    -- Return the *latest* envelope data.
    SELECT [_outbox].[EnvelopeId], [_outbox].[CreatedDate], [_outbox].[IsComplete], [_outbox].[CompletedDate]
      FROM [DemoCdc].[PostsEnvelope] AS [_outbox]
      WHERE [_outbox].EnvelopeId = @EnvelopeId 

    -- Root table: Legacy.Posts - uses LEFT OUTER JOIN so we get the deleted records too.
    SELECT
        [_chg].[_Op] AS [_OperationType],
        [_chg].[PostsId] AS [PostsId],
        [p].[Text] AS [Text],
        [p].[Date] AS [Date]
      FROM #_changes AS [_chg]
      LEFT OUTER JOIN [Legacy].[Posts] AS [p] WITH (NOLOCK) ON ([p].[PostsId] = [_chg].[PostsId])

    -- Related table: Legacy.Comments - only use INNER JOINS to get what is actually there right now.
    SELECT
        [c].[CommentsId] AS [CommentsId],
        [c].[PostsId] AS [PostsId],
        [c].[Text] AS [Text],
        [c].[Date] AS [Date]
      FROM #_changes AS [_chg]
      INNER JOIN [Legacy].[Posts] AS [p] WITH (NOLOCK) ON ([p].[PostsId] = [_chg].[PostsId])
      INNER JOIN [Legacy].[Comments] AS [c] WITH (NOLOCK) ON ([c].[PostsId] = [p].[PostsId])
      WHERE [_chg].[_Op] <> 1

    -- Related table: Legacy.Tags - only use INNER JOINS to get what is actually there right now.
    SELECT
        [t].[TagsId] AS [TagsId],
        [t].[ParentType] AS [ParentType],
        [t].[ParentId] AS [ParentId],
        [t].[Text] AS [Text]
      FROM #_changes AS [_chg]
      INNER JOIN [Legacy].[Posts] AS [p] WITH (NOLOCK) ON ([p].[PostsId] = [_chg].[PostsId])
      INNER JOIN [Legacy].[Comments] AS [c] WITH (NOLOCK) ON ([c].[PostsId] = [p].[PostsId])
      INNER JOIN [Legacy].[Tags] AS [t] WITH (NOLOCK) ON ([t].[ParentType] = 'C' AND [t].[ParentId] = [c].[CommentsId])
      WHERE [_chg].[_Op] <> 1

    -- Related table: Legacy.Tags - only use INNER JOINS to get what is actually there right now.
    SELECT
        [t2].[TagsId] AS [TagsId],
        [t2].[ParentType] AS [ParentType],
        [t2].[ParentId] AS [ParentId],
        [t2].[Text] AS [Text]
      FROM #_changes AS [_chg]
      INNER JOIN [Legacy].[Posts] AS [p] WITH (NOLOCK) ON ([p].[PostsId] = [_chg].[PostsId])
      INNER JOIN [Legacy].[Tags] AS [t2] WITH (NOLOCK) ON ([t2].[ParentType] = 'P' AND [t2].[ParentId] = [p].[PostsId])
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