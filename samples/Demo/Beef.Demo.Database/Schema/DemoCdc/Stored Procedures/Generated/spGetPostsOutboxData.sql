CREATE PROCEDURE [DemoCdc].[spGetPostsOutboxData]
  @EnvelopeIdToMarkComplete INT NULL = NULL,
  @ReturnIncompleteBatches BIT = 0,
  @MaxBatchSize INT = 100
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost.
   */

  SET NOCOUNT ON;

  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

    -- Mark the batch as complete first.
    IF (@EnvelopeIdToMarkComplete IS NOT NULL)
    BEGIN
      UPDATE [DemoCdc].[PostsOutboxEnvelope] SET
        HasbeenCompleted = 1,
        ProcessedDate = GETUTCDATE()
        WHERE OutboxEnvelopeId = @EnvelopeIdToMarkComplete 
    END

    -- Where the batch size is 0 then no-op; this is used when closing down the service - invoked to mark the batch complete but _not_ get a new one.
    IF (@MaxBatchSize = 0) 
    BEGIN
      COMMIT TRANSACTION
      RETURN 0;
    END

    -- Declare required variables.
    DECLARE @existingBatchId INT 
    DECLARE @minPostsLsn BINARY(10)
    DECLARE @minCommentsLsn BINARY(10)
    DECLARE @maxlsn BINARY(10)

    -- This is a flag to specify if we need the min change. If this is a new batch, we want the change AFTER this change as this is set to the highest in the batch before.
    -- If this is a re-get of an existing batch, then we need to return it as its the first item in the batch
    DECLARE @includeMin BIT
    SET @includeMin = 0

    -- Where asking for any inprogress batches, we just need to find those and return them. 
    IF (@ReturnIncompleteBatches = 1)
    BEGIN
      -- Need to find the existing batch to return 
      SELECT TOP 1 
          @existingBatchId = [o].[OutboxEnvelopeId],
          @minPostsLsn = [o].[FirstProcessedLSN],
          @minCommentsLsn = [o].[FirstProcessedLSN],
          @maxlsn = [o].[LastProcessedLSN]
        FROM [DemoCdc].[PostsOutboxEnvelope] AS [o] 
        WHERE [o].[HasBeenCompleted] = 0 
        ORDER BY [o].[OutboxEnvelopeId]

      -- Where a batch is found then return
      IF @existingBatchId IS NOT NULL
      BEGIN
        -- Get the batch information from the table as this forms our first result set.
        SELECT * FROM [DemoCdc].[PostsOutboxEnvelope] AS [o] WHERE [o].[OutboxEnvelopeId] = @ExistingBatchId

        -- Mark the flag to include the first item in the batch too.
        SET @includeMin = 1
      END
    END

    -- Where null then there isnt an existing batch, so we need to get these numbers.
    IF @minPostsLsn IS NULL 
    BEGIN
      -- Get them from the last batch generated.
      SELECT TOP 1
          @minPostsLsn = [o].[LastProcessedLSN],
          @minCommentsLsn = [o].[LastProcessedLSN]
        FROM [DemoCdc].[PostsOutboxEnvelope] AS [o] 
        ORDER BY [o].[OutBoxEnvelopeId] DESC

      -- Where they are still NULL then this is the first batch ever, get them from the transaction log.
      IF @minPostsLsn IS NULL 
      BEGIN
        SET @minPostsLsn = sys.fn_cdc_get_min_lsn('Legacy_Posts') 
        SET @minCommentsLsn = sys.fn_cdc_get_min_lsn('Legacy_Comments') 

        -- Also include the first item here, because it should be in this batch.
        SET @includeMin = 1
      END

      -- Get the current max LSN from the transaction log
      SET @maxlsn = sys.fn_cdc_get_max_lsn()  
    END

    -- Do not go larger than the max batch size; put into a temp table so we can aggregate it later.
    SELECT TOP (@MaxBatchSize) * 
    INTO #changes
    FROM 
    (
        -- Get the [Legacy].[Posts] changes.
        SELECT TOP (@MaxBatchSize)
            'Posts' AS [__ChangeTable],
            [_chg].[__$start_lsn] AS [__StartLsn],
            [_chg].[__$operation] AS [__Operation],
            [_chg].[__$update_mask] AS [__UpdateMask],
            [_chg].[PostsId],
            [p].[Text],
            [p].[Date],
            [c].[CommentsId],
            [c].[Text] AS [CText],
            [c].[Date] AS [CDate]
          FROM cdc.fn_cdc_get_net_changes_legacy_posts(@minPostsLsn, @maxlsn, 'all') AS [_chg]
          LEFT OUTER JOIN [Legacy].[Posts] AS [p] ON ([p].[PostsId] = [_chg].[PostsId])
          LEFT OUTER JOIN [Legacy].[Comments] AS [c] ON ([c].[PostsId] = [p].[PostsId])
          WHERE (@includeMin = 1 OR [_chg].[__$start_lsn] > @minPostsLsn)
          ORDER BY [_chg].[__$start_lsn] ASC
      UNION
        -- Get the [Legacy].[Comments] changes.
        SELECT TOP (@MaxBatchSize)
            'Comments' AS [__ChangeTable],
            [_chg].[__$start_lsn] AS [__StartLsn],
            [_chg].[__$operation] AS [__Operation],
            [_chg].[__$update_mask] AS [__UpdateMask],
            [p].[PostsId],
            [p].[Text],
            [p].[Date],
            [c].[CommentsId],
            [c].[Text] AS [CText],
            [c].[Date] AS [CDate]
          FROM cdc.fn_cdc_get_net_changes_legacy_comments(@minCommentsLsn, @maxlsn, 'all') AS [_chg]
          INNER JOIN [Legacy].[Comments] AS [c] ON ([c].[CommentsId] = [_chg].[CommentsId])
          INNER JOIN [Legacy].[Posts] AS [p] ON ([c].[PostsId] = [p].[PostsId])
          WHERE (@includeMin = 1 OR [_chg].[__$start_lsn] > @minCommentsLsn)
          ORDER BY [_chg].[__$start_lsn] ASC
    ) AS [_changes]
    ORDER BY [__StartLsn] ASC -- We order by as we're taking TOP. We dont want to skip changes so we need them in LSN order

    -- Where there are changes we can make a batch, otherwise we just need to return the empty results.
    IF EXISTS (SELECT TOP 1 * from #changes)
    BEGIN
      -- If we need to create a new batch coz we arent returning an old one.
      IF ( @ExistingBatchId   IS NULL)
      BEGIN
        DECLARE @MinBatchLSN BINARY(10)
        DECLARE @MaxBatchLSN BINARY(10)

        -- Figure out the min and MAX LSN in this batch.
        SELECT @MinBatchLSN = MIN([__StartLsn]), @MaxBatchLSN = MAX([__StartLsn])
          FROM #changes

        -- Create the batch record in the table.
        INSERT INTO [DemoCdc].[PostsOutboxEnvelope] (
          [CreatedDate], 
          [FirstProcessedLSN],
          [LastProcessedLSN],
          [HasBeenCompleted]
        ) 
        VALUES (
          GETUTCDATE(),
          @MinBatchLSN,
          @MaxBatchLSN,
          0
        )

        -- Return as the first result set. If we arent making a new batch then we already returned it.
        SELECT * FROM [DemoCdc].[PostsOutboxEnvelope] WHERE [OutboxEnvelopeId] = @@IDENTITY
      END
    END
    ELSE
    BEGIN
      -- There are no changes so just return an empty result set.
      SELECT TOP 0 * FROM [DemoCdc].[PostsOutboxEnvelope]
    END

    -- Return the changes that we have.
    SELECT DISTINCT 
      [__Operation]
      [PostsId],
      [Text],
      [Date],
      [CommentsId],
      [CText],
      [CDate]
    FROM #changes

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