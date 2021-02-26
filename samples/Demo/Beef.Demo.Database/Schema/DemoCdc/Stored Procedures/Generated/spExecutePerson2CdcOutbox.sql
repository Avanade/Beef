CREATE PROCEDURE [DemoCdc].[spExecutePerson2CdcOutbox]
  @MaxQuerySize INT NULL = 100,             -- Maximum size of query to limit the number of changes to a manageable batch (perf vs failure cost).
  @GetIncompleteOutbox BIT NULL = 0,        -- Gets incomplete outbox (batch) where existing.
  @ContinueWithDataLoss BIT NULL = 0,       -- Ignores data loss and continues; versus returning -2.
  @OutboxIdToMarkComplete INT NULL = NULL,  -- Marks the specified outbox as Complete; no further data is retrieved.
  @CompleteTrackingList AS [DemoCdc].[udtCdcTrackingList] READONLY  -- When Marking/Completing the corresponding tracking list should also be merged.
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost.
   */

  /* Return Codes:
   *   0 = Success
   *  -1 = Cannot continue where there is an incomplete outbox; must be completed.
   *  -2 = SQL Server has cleaned up CDC data that should have been included in the batch; i.e changes have been missed.
   *        (see https://docs.microsoft.com/en-us/sql/relational-databases/track-changes/administer-and-monitor-change-data-capture-sql-server)
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
        FROM [DemoCdc].[Person2Outbox] AS [_outbox]
        WHERE OutboxId = @OutboxIdToMarkComplete 

      MERGE INTO [DemoCdc].[CdcTracking] WITH (HOLDLOCK) AS [_ct]
        USING @CompleteTrackingList AS [_list] ON ([_ct].[Schema] = 'Demo' AND [_ct].[Table] = 'Person2' AND [_ct].[Key] = [_list].[Key])
        WHEN MATCHED AND EXISTS (
            SELECT [_list].[Key], [_list].[Hash]
            EXCEPT
            SELECT [_ct].[Key], [_ct].[Hash])
          THEN UPDATE SET [_ct].[Hash] = [_list].[Hash], [_ct].[OutboxId] = @OutboxIdToMarkComplete
        WHEN NOT MATCHED BY TARGET
          THEN INSERT ([Schema], [Table], [Key], [Hash], [OutboxId])
            VALUES ('Demo', 'Person2', [_list].[Key], [_list].[Hash], @OutboxIdToMarkComplete);

      SELECT [_outbox].[OutboxId], [_outbox].[CreatedDate], [_outbox].[IsComplete], [_outbox].[CompletedDate]
        FROM [DemoCdc].[Person2Outbox] AS [_outbox]
        WHERE [_outbox].OutboxId = @OutboxIdToMarkComplete

      COMMIT TRANSACTION
      RETURN 0;
    END

    -- Declare variables.
    DECLARE @Person2BaseMinLsn BINARY(10), @Person2MinLsn BINARY(10), @Person2MaxLsn BINARY(10)
    DECLARE @OutboxId INT

    -- Get the latest 'base' minimum.
    SET @Person2BaseMinLsn = sys.fn_cdc_get_min_lsn('Demo_Person2');

    -- Where requesting for incomplete outbox, get first that is marked as incomplete.
    IF (@GetIncompleteOutbox = 1)
    BEGIN
      SELECT TOP 1
          @Person2MinLsn = [_outbox].[Person2MinLsn],
          @Person2MaxLsn = [_outbox].[Person2MaxLsn],
          @OutboxId = [OutboxId]
        FROM [DemoCdc].[Person2Outbox] AS [_outbox]
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
          @Person2MinLsn = [_outbox].[Person2MaxLsn],
          @IsComplete = [_outbox].IsComplete
        FROM [DemoCdc].[Person2Outbox] AS [_outbox]
        ORDER BY [_outbox].[IsComplete] ASC, [_outbox].[OutboxId] DESC

      IF (@IsComplete = 0) -- Cannot continue where there is an incomplete outbox; must be completed.
      BEGIN
        SELECT [_outbox].[OutboxId], [_outbox].[CreatedDate], [_outbox].[IsComplete], [_outbox].[CompletedDate]
          FROM [DemoCdc].[Person2Outbox] AS [_outbox]
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
        SET @Person2MinLsn = @Person2BaseMinLsn;
      END
      ELSE
      BEGIN
        -- Increment the minimum as the last has already been processed.
        SET @Person2MinLsn = sys.fn_cdc_increment_lsn(@Person2MinLsn)
      END

      -- Get the maximum LSN.
      SET @Person2MaxLsn = sys.fn_cdc_get_max_lsn();

      -- Verify the maximum query size and correct (reset) where applicable.
      IF (@MaxQuerySize IS NULL OR @MaxQuerySize < 1 OR @MaxQuerySize > 10000)
      BEGIN
        SET @MaxQuerySize = 100
      END
    END

    -- The minimum should _not_ be less than the base otherwise we have lost data; either continue with this data loss, or error and stop.
    IF (@Person2MinLsn < @Person2BaseMinLsn) BEGIN IF (@ContinueWithDataLoss = 1) BEGIN SET @Person2MinLsn = @Person2BaseMinLsn END ELSE BEGIN COMMIT TRANSACTION RETURN -2 END END

    -- Find changes on the root table: Demo.Person2 - this determines overall operation type: 'create', 'update' or 'delete'.
    DECLARE @hasChanges BIT
    SET @hasChanges = 0

    IF (@Person2MinLsn < @Person2MaxLsn)
    BEGIN
      SELECT TOP (@MaxQuerySize)
          [_cdc].[__$start_lsn] AS [_Lsn],
          [_cdc].[__$operation] AS [_Op],
          [_cdc].[PersonId] AS [PersonId]
        INTO #_changes
        FROM cdc.fn_cdc_get_net_changes_Demo_Person2(@Person2MinLsn, @Person2MaxLsn, 'all') AS [_cdc]
        ORDER BY [_cdc].[__$start_lsn]

      IF (@@ROWCOUNT <> 0)
      BEGIN
        SET @hasChanges = 1
        SELECT @Person2MinLsn = MIN([_Lsn]), @Person2MaxLsn = MAX([_Lsn]) FROM #_changes
      END
    END

    -- Create a new outbox where not processing an existing.
    IF (@OutboxId IS NULL AND @hasChanges = 1)
    BEGIN
      DECLARE @InsertedOutboxId TABLE([OutboxId] INT)

      INSERT INTO [DemoCdc].[Person2Outbox] (
          [Person2MinLsn],
          [Person2MaxLsn],
          [CreatedDate],
          [IsComplete]
        ) 
        OUTPUT inserted.OutboxId INTO @InsertedOutboxId
        VALUES (
          @Person2MinLsn,
          @Person2MaxLsn,
          GETUTCDATE(),
          0
        )

        SELECT @OutboxId = [OutboxId] FROM @InsertedOutboxId
    END

    -- Return the *latest* outbox data.
    SELECT [_outbox].[OutboxId], [_outbox].[CreatedDate], [_outbox].[IsComplete], [_outbox].[CompletedDate]
      FROM [DemoCdc].[Person2Outbox] AS [_outbox]
      WHERE [_outbox].OutboxId = @OutboxId 

    -- Exit here if there were no changes found.
    IF (@hasChanges = 0)
    BEGIN
      COMMIT TRANSACTION
      RETURN 0
    END

    -- Root table: Demo.Person2 - uses LEFT OUTER JOIN's to get the deleted records, as well as any previous Tracking Hash value.
    SELECT
        [_ct].[Hash] AS [_TrackingHash],
        [_chg].[_Op] AS [_OperationType],
        [_chg].[PersonId] AS [PersonId],
        [p].[RowVersion] AS [RowVersion],
        [p].[IsDeleted] AS [IsDeleted]
      FROM #_changes AS [_chg]
      LEFT OUTER JOIN [DemoCdc].[CdcTracking] AS [_ct] ON ([_ct].[Schema] = 'Demo' AND [_ct].[Table] = 'Person2' AND [_ct].[Key] = CAST([_chg].[PersonId] AS NVARCHAR(128)))
      LEFT OUTER JOIN [Demo].[Person2] AS [p] ON ([p].[PersonId] = [_chg].[PersonId])

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