CREATE PROCEDURE [XCdc].[spExecutePersonCdcOutbox]
  @MaxQuerySize INT = 100,            -- Maximum size of query to limit the number of changes to a manageable batch (performance vs failure trade-off).
  @CorrelationId NVARCHAR(64) = NULL, -- Correlation identifier to aid tracking of outbox execution and corresponding events.
  @ContinueWithDataLoss BIT = 0       -- Ignores data loss and continues; versus throwing an error.
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
    DECLARE @PersonAddressBaseMinLsn BINARY(10), @PersonAddressMinLsn BINARY(10), @PersonAddressMaxLsn BINARY(10)
    DECLARE @OutboxId INT

    -- Get the latest 'base' minimum.
    SET @PersonBaseMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Person');
    SET @PersonAddressBaseMinLsn = sys.fn_cdc_get_min_lsn('Legacy_PersonAddress');

    -- Check if there is already an incomplete batch and attempt to reprocess.
    SELECT
        @PersonMinLsn = [_outbox].[PersonMinLsn],
        @PersonMaxLsn = [_outbox].[PersonMaxLsn],
        @PersonAddressMinLsn = [_outbox].[PersonAddressMinLsn],
        @PersonAddressMaxLsn = [_outbox].[PersonAddressMaxLsn],
        @OutboxId = [OutboxId]
      FROM [XCdc].[PersonOutbox] AS [_outbox]
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
          @PersonMinLsn = [_outbox].[PersonMaxLsn],
          @PersonAddressMinLsn = [_outbox].[PersonAddressMaxLsn]
        FROM [XCdc].[PersonOutbox] AS [_outbox]
        ORDER BY [_outbox].[IsComplete] ASC, [_outbox].[OutboxId] DESC

      IF (@@ROWCOUNT = 0) -- No previous outbox; i.e. is the first time!
      BEGIN
        SET @PersonMinLsn = @PersonBaseMinLsn;
        SET @PersonAddressMinLsn = @PersonAddressBaseMinLsn;
      END
      ELSE
      BEGIN
        -- Increment the minimum as the last has already been processed.
        SET @PersonMinLsn = sys.fn_cdc_increment_lsn(@PersonMinLsn)
        SET @PersonAddressMinLsn = sys.fn_cdc_increment_lsn(@PersonAddressMinLsn)
      END

      -- Get the maximum LSN.
      SET @PersonMaxLsn = sys.fn_cdc_get_max_lsn();
      SET @PersonAddressMaxLsn = @PersonMaxLsn

      -- Verify the maximum query size and correct (reset) where applicable.
      IF (@MaxQuerySize IS NULL OR @MaxQuerySize < 1 OR @MaxQuerySize > 10000)
      BEGIN
        SET @MaxQuerySize = 100
      END
    END

    -- The minimum should _not_ be less than the base otherwise we have lost data; either continue with this data loss, or error and stop.
    DECLARE @hasDataLoss BIT
    SET @hasDataLoss = 0

    IF (@PersonMinLsn < @PersonBaseMinLsn) BEGIN IF (@ContinueWithDataLoss = 1) BEGIN SET @hasDataLoss = 1; SET @PersonMinLsn = @PersonBaseMinLsn END ELSE BEGIN ;THROW 56002, 'Unexpected data loss error for ''Legacy.Person''; this indicates that the CDC data has probably been cleaned up before being successfully processed.', 1; END END
    IF (@PersonAddressMinLsn < @PersonAddressBaseMinLsn) BEGIN IF (@ContinueWithDataLoss = 1) BEGIN SET @hasDataLoss = 1; SET @PersonAddressMinLsn = @PersonAddressBaseMinLsn END ELSE BEGIN ;THROW 56002, 'Unexpected data loss error for ''Legacy.PersonAddress''; this indicates that the CDC data has probably been cleaned up before being successfully processed.', 1; END END

    -- Find changes on the root table: Legacy.Person - this determines overall operation type: 'create', 'update' or 'delete'.
    DECLARE @hasChanges BIT
    SET @hasChanges = 0

    IF (@PersonMinLsn <= @PersonMaxLsn)
    BEGIN
      SELECT TOP (@MaxQuerySize)
          [_cdc].[__$start_lsn] AS [_Lsn],
          [_cdc].[__$operation] AS [_Op],
          [_cdc].[PersonId] AS [PersonId]
        INTO #_changes
        FROM cdc.fn_cdc_get_all_changes_Legacy_Person(@PersonMinLsn, @PersonMaxLsn, 'all') AS [_cdc]
        ORDER BY [_cdc].[__$start_lsn]

      IF (@@ROWCOUNT <> 0)
      BEGIN
        SET @hasChanges = 1
        SELECT @PersonMinLsn = MIN([_Lsn]), @PersonMaxLsn = MAX([_Lsn]) FROM #_changes
      END
    END

    -- Find changes on related table: PersonAddress (Legacy.PersonAddress) - assume all are 'update' operation (i.e. it doesn't matter).
    IF (@PersonAddressMinLsn <= @PersonAddressMaxLsn)
    BEGIN
      SELECT TOP (@MaxQuerySize)
          [_cdc].[__$start_lsn] AS [_Lsn],
          4 AS [_Op],
          [p].[PersonId] AS [PersonId]
        INTO #pa
        FROM cdc.fn_cdc_get_all_changes_Legacy_PersonAddress(@PersonAddressMinLsn, @PersonAddressMaxLsn, 'all') AS [_cdc]
        INNER JOIN [Legacy].[Person] AS [p] WITH (NOLOCK) ON ([_cdc].[PersonId] = [p].[PersonId])
        ORDER BY [_cdc].[__$start_lsn]

      IF (@@ROWCOUNT <> 0)
      BEGIN
        SET @hasChanges = 1
        SELECT @PersonAddressMinLsn = MIN([_Lsn]), @PersonAddressMaxLsn = MAX([_Lsn]) FROM #pa

        INSERT INTO #_changes
          SELECT * 
            FROM #pa AS [_pa]
            WHERE NOT EXISTS (SELECT * FROM #_changes AS [_chg] WHERE [_chg].[PersonId] = [_pa].[PersonId])
      END
    END

    -- Create a new outbox where not processing an existing.
    IF (@OutboxId IS NULL AND (@hasDataLoss = 1 OR @hasChanges = 1))
    BEGIN
      DECLARE @InsertedOutboxId TABLE([OutboxId] INT)

      INSERT INTO [XCdc].[PersonOutbox] (
          [PersonMinLsn],
          [PersonMaxLsn],
          [PersonAddressMinLsn],
          [PersonAddressMaxLsn],
          [CreatedDate],
          [IsComplete],
          [CorrelationId],
          [HasDataLoss]
        ) 
        OUTPUT inserted.OutboxId INTO @InsertedOutboxId
        VALUES (
          @PersonMinLsn,
          @PersonMaxLsn,
          @PersonAddressMinLsn,
          @PersonAddressMaxLsn,
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
        UPDATE [XCdc].[PersonOutbox] 
          SET [HasDataLoss] = @hasDataLoss,
              [CorrelationId] = @CorrelationId
          WHERE [OutboxId] = @OutboxId
      END
    END

    -- Return the *latest* outbox data.
    SELECT [_outbox].[OutboxId], [_outbox].[CreatedDate], [_outbox].[IsComplete], [_outbox].[CompletedDate], [_outbox].[CorrelationId], [_outBox].[HasDataLoss]
      FROM [XCdc].[PersonOutbox] AS [_outbox]
      WHERE [_outbox].OutboxId = @OutboxId 

    -- Exit here if there were no changes found.
    IF (@hasChanges = 0)
    BEGIN
      COMMIT TRANSACTION
      RETURN 0
    END

    -- Root table: Legacy.Person - uses LEFT OUTER JOIN's to get the deleted records, as well as any previous Tracking Hash value.
    SELECT
        [_chg].[_Op] AS [_OperationType],
        [_chg].[_Lsn] AS [_Lsn],
        [_ct].[Hash] AS [_TrackingHash],
        [_im].[GlobalId] AS [GlobalId],
        [_chg].[PersonId] AS [PersonId],
        [p].[PersonId] AS [TableKey_PersonId],
        [p].[FirstName] AS [FirstName],
        [p].[LastName] AS [LastName],
        [p].[Phone] AS [Phone],
        [p].[Email] AS [Email],
        [p].[Active] AS [Active]
      FROM #_changes AS [_chg]
      LEFT OUTER JOIN [Legacy].[Person] AS [p] ON ([p].[PersonId] = [_chg].[PersonId])
      LEFT OUTER JOIN [XCdc].[CdcIdentifierMapping] AS [_im] ON ([_im].[Schema] = 'Legacy' AND [_im].[Table] = 'Person' AND [_im].[Key] = CAST([_chg].[PersonId] AS NVARCHAR(128)))
      LEFT OUTER JOIN [XCdc].[CdcTracking] AS [_ct] ON ([_ct].[Schema] = 'Legacy' AND [_ct].[Table] = 'Person' AND [_ct].[Key] = _im.GlobalId)

    -- Related table: PersonAddress (Legacy.PersonAddress) - only use INNER JOINS to get what is actually there right now (where applicable).
    SELECT DISTINCT
        [pa].[Id] AS [Id],
        [pa].[PersonId] AS [PersonId],
        [pa].[AddressTypeId] AS [AddressTypeId],
        [pa].[Street1] AS [Street1],
        [pa].[Street2] AS [Street2],
        [pa].[City] AS [City],
        [pa].[State] AS [State],
        [pa].[PostalZipCode] AS [PostalZipCode],
        [at].[Code] AS [Code]
      FROM #_changes AS [_chg]
      INNER JOIN [Legacy].[Person] AS [p] ON ([p].[PersonId] = [_chg].[PersonId])
      INNER JOIN [Legacy].[PersonAddress] AS [pa] ON ([pa].[PersonId] = [p].[PersonId])
      LEFT OUTER JOIN [Legacy].[AddressType] AS [at] ON ([at].[AddressTypeId] = [pa].[AddressTypeId])
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