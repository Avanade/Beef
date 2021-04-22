CREATE PROCEDURE [DemoCdc].[spExecutePersonCdcOutbox]
  @MaxQuerySize INT = 100,                 -- Maximum size of query to limit the number of changes to a manageable batch (performance vs failure trade-off).
  @CorrelationId NVARCHAR(64) NULL = NULL, -- Correlation identifier to aid tracking of outbox execution and corresponding events.
  @ContinueWithDataLoss BIT = 0            -- Ignores data loss and continues; versus throwing an error.
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
    DECLARE @PersonBaseMinLsn BINARY(10), @PersonMinLsn BINARY(10), @PersonMaxLsn BINARY(10)
    DECLARE @OutboxId INT

    -- Get the latest 'base' minimum.
    SET @PersonBaseMinLsn = sys.fn_cdc_get_min_lsn('Demo_Person');

    -- Check if there is already an incomplete batch and attempt to reprocess.
    SELECT
        @PersonMinLsn = [_outbox].[PersonMinLsn],
        @PersonMaxLsn = [_outbox].[PersonMaxLsn],
        @OutboxId = [OutboxId]
      FROM [DemoCdc].[PersonOutbox] AS [_outbox]
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
          @PersonMinLsn = [_outbox].[PersonMaxLsn]
        FROM [DemoCdc].[PersonOutbox] AS [_outbox]
        ORDER BY [_outbox].[IsComplete] ASC, [_outbox].[OutboxId] DESC

      IF (@@ROWCOUNT = 0) -- No previous outbox; i.e. is the first time!
      BEGIN
        SET @PersonMinLsn = @PersonBaseMinLsn;
      END
      ELSE
      BEGIN
        -- Increment the minimum as the last has already been processed.
        SET @PersonMinLsn = sys.fn_cdc_increment_lsn(@PersonMinLsn)
      END

      -- Get the maximum LSN.
      SET @PersonMaxLsn = sys.fn_cdc_get_max_lsn();

      -- Verify the maximum query size and correct (reset) where applicable.
      IF (@MaxQuerySize IS NULL OR @MaxQuerySize < 1 OR @MaxQuerySize > 10000)
      BEGIN
        SET @MaxQuerySize = 100
      END
    END

    -- The minimum should _not_ be less than the base otherwise we have lost data; either continue with this data loss, or error and stop.
    DECLARE @hasDataLoss BIT
    SET @hasDataLoss = 0

    IF (@PersonMinLsn < @PersonBaseMinLsn) BEGIN IF (@ContinueWithDataLoss = 1) BEGIN SET @hasDataLoss = 1; SET @PersonMinLsn = @PersonBaseMinLsn END ELSE BEGIN ;THROW 56002, 'Unexpected data loss error for ''Demo.Person''; this indicates that the CDC data has probably been cleaned up before being successfully processed.', 1; END END

    -- Find changes on the root table: Demo.Person - this determines overall operation type: 'create', 'update' or 'delete'.
    DECLARE @hasChanges BIT
    SET @hasChanges = 0

    IF (@PersonMinLsn <= @PersonMaxLsn)
    BEGIN
      SELECT TOP (@MaxQuerySize)
          [_cdc].[__$start_lsn] AS [_Lsn],
          [_cdc].[__$operation] AS [_Op],
          [_cdc].[PersonId] AS [PersonId]
        INTO #_changes
        FROM cdc.fn_cdc_get_all_changes_Demo_Person(@PersonMinLsn, @PersonMaxLsn, 'all') AS [_cdc]
        ORDER BY [_cdc].[__$start_lsn]

      IF (@@ROWCOUNT <> 0)
      BEGIN
        SET @hasChanges = 1
        SELECT @PersonMinLsn = MIN([_Lsn]), @PersonMaxLsn = MAX([_Lsn]) FROM #_changes
      END
    END

    -- Create a new outbox where not processing an existing.
    IF (@OutboxId IS NULL AND (@hasDataLoss = 1 OR @hasChanges = 1))
    BEGIN
      DECLARE @InsertedOutboxId TABLE([OutboxId] INT)

      INSERT INTO [DemoCdc].[PersonOutbox] (
          [PersonMinLsn],
          [PersonMaxLsn],
          [CreatedDate],
          [IsComplete],
          [CorrelationId],
          [HasDataLoss]
        ) 
        OUTPUT inserted.OutboxId INTO @InsertedOutboxId
        VALUES (
          @PersonMinLsn,
          @PersonMaxLsn,
          GETUTCDATE(),
          0,
          @CorrelationId,
          @hasDataLoss
        )

        SELECT @OutboxId = [OutboxId] FROM @InsertedOutboxId
    END
    ELSE
    BEGIN
      IF (@OutboxId IS NOT NULL AND @hasDataLoss = 1)
      BEGIN
        UPDATE [DemoCdc].[PersonOutbox] 
          SET [HasDataLoss] = @hasDataLoss,
              [CorrelationId] = @CorrelationId
          WHERE [OutboxId] = @OutboxId
      END
    END

    -- Return the *latest* outbox data.
    SELECT [_outbox].[OutboxId], [_outbox].[CreatedDate], [_outbox].[IsComplete], [_outbox].[CompletedDate], [_outbox].[CorrelationId], [_outBox].[HasDataLoss]
      FROM [DemoCdc].[PersonOutbox] AS [_outbox]
      WHERE [_outbox].OutboxId = @OutboxId 

    -- Exit here if there were no changes found.
    IF (@hasChanges = 0)
    BEGIN
      COMMIT TRANSACTION
      RETURN 0
    END

    -- Root table: Demo.Person - uses LEFT OUTER JOIN's to get the deleted records, as well as any previous Tracking Hash value.
    SELECT
        [_chg].[_Op] AS [_OperationType],
        [_chg].[_Lsn] AS [_Lsn],
        [_ct].[Hash] AS [_TrackingHash],
        [_chg].[PersonId] AS [PersonId],
        [p].[PersonId] AS [TableKey_PersonId]
      FROM #_changes AS [_chg]
      LEFT OUTER JOIN [Demo].[Person] AS [p] ON ([p].[PersonId] = [_chg].[PersonId])
      LEFT OUTER JOIN [DemoCdc].[CdcTracking] AS [_ct] ON ([_ct].[Schema] = 'Demo' AND [_ct].[Table] = 'Person' AND [_ct].[Key] = CAST([_chg].[PersonId] AS NVARCHAR(128)))

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