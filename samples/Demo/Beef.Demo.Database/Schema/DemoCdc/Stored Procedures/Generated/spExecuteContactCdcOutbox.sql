CREATE PROCEDURE [DemoCdc].[spExecuteContactCdcOutbox]
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
        FROM [DemoCdc].[ContactOutbox] AS [_outbox]
        WHERE OutboxId = @OutboxIdToMarkComplete 

      MERGE INTO [DemoCdc].[CdcTracking] WITH (HOLDLOCK) AS [_ct]
        USING @CompleteTrackingList AS [_list] ON ([_ct].[Schema] = 'Legacy' AND [_ct].[Table] = 'Contact' AND [_ct].[Key] = [_list].[Key])
        WHEN MATCHED AND EXISTS (
            SELECT [_list].[Key], [_list].[Hash]
            EXCEPT
            SELECT [_ct].[Key], [_ct].[Hash])
          THEN UPDATE SET [_ct].[Hash] = [_list].[Hash], [_ct].[OutboxId] = @OutboxIdToMarkComplete
        WHEN NOT MATCHED BY TARGET
          THEN INSERT ([Schema], [Table], [Key], [Hash], [OutboxId])
            VALUES ('Legacy', 'Contact', [_list].[Key], [_list].[Hash], @OutboxIdToMarkComplete);

      SELECT [_outbox].[OutboxId], [_outbox].[CreatedDate], [_outbox].[IsComplete], [_outbox].[CompletedDate]
        FROM [DemoCdc].[ContactOutbox] AS [_outbox]
        WHERE [_outbox].OutboxId = @OutboxIdToMarkComplete

      COMMIT TRANSACTION
      RETURN 0;
    END

    -- Declare variables.
    DECLARE @ContactBaseMinLsn BINARY(10), @ContactMinLsn BINARY(10), @ContactMaxLsn BINARY(10)
    DECLARE @AddressBaseMinLsn BINARY(10), @AddressMinLsn BINARY(10), @AddressMaxLsn BINARY(10)
    DECLARE @OutboxId INT

    -- Get the latest 'base' minimum.
    SET @ContactBaseMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Contact');
    SET @AddressBaseMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Address');

    -- Where requesting for incomplete outbox, get first that is marked as incomplete.
    IF (@GetIncompleteOutbox = 1)
    BEGIN
      SELECT TOP 1
          @ContactMinLsn = [_outbox].[ContactMinLsn],
          @ContactMaxLsn = [_outbox].[ContactMaxLsn],
          @AddressMinLsn = [_outbox].[AddressMinLsn],
          @AddressMaxLsn = [_outbox].[AddressMaxLsn],
          @OutboxId = [OutboxId]
        FROM [DemoCdc].[ContactOutbox] AS [_outbox]
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
          @ContactMinLsn = [_outbox].[ContactMaxLsn],
          @AddressMinLsn = [_outbox].[AddressMaxLsn],
          @IsComplete = [_outbox].IsComplete
        FROM [DemoCdc].[ContactOutbox] AS [_outbox]
        ORDER BY [_outbox].[IsComplete] ASC, [_outbox].[OutboxId] DESC

      IF (@IsComplete = 0) -- Cannot continue where there is an incomplete outbox; must be completed.
      BEGIN
        SELECT [_outbox].[OutboxId], [_outbox].[CreatedDate], [_outbox].[IsComplete], [_outbox].[CompletedDate]
          FROM [DemoCdc].[ContactOutbox] AS [_outbox]
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
        SET @ContactMinLsn = @ContactBaseMinLsn;
        SET @AddressMinLsn = @AddressBaseMinLsn;
      END
      ELSE
      BEGIN
        -- Increment the minimum as the last has already been processed.
        SET @ContactMinLsn = sys.fn_cdc_increment_lsn(@ContactMinLsn)
        SET @AddressMinLsn = sys.fn_cdc_increment_lsn(@AddressMinLsn)
      END

      -- Get the maximum LSN.
      SET @ContactMaxLsn = sys.fn_cdc_get_max_lsn();
      SET @AddressMaxLsn = @ContactMaxLsn

      -- Verify the maximum query size and correct (reset) where applicable.
      IF (@MaxQuerySize IS NULL OR @MaxQuerySize < 1 OR @MaxQuerySize > 10000)
      BEGIN
        SET @MaxQuerySize = 100
      END
    END

    -- The minimum can not be less than the base or an error will occur, so realign where not correct.
    IF (@ContactMinLsn < @ContactBaseMinLsn) BEGIN SET @ContactMinLsn = @ContactBaseMinLsn END
    IF (@AddressMinLsn < @AddressBaseMinLsn) BEGIN SET @AddressMinLsn = @AddressBaseMinLsn END

    -- Find changes on the root table: Legacy.Contact - this determines overall operation type: 'create', 'update' or 'delete'.
    DECLARE @hasChanges BIT
    SET @hasChanges = 0

    IF (@ContactMinLsn < @ContactMaxLsn)
    BEGIN
      SELECT TOP (@MaxQuerySize)
          [_cdc].[__$start_lsn] AS [_Lsn],
          [_cdc].[__$operation] AS [_Op],
          [_cdc].[ContactId] AS [ContactId]
        INTO #_changes
        FROM cdc.fn_cdc_get_net_changes_Legacy_Contact(@ContactMinLsn, @ContactMaxLsn, 'all') AS [_cdc]
        ORDER BY [_cdc].[__$start_lsn]

      IF (@@ROWCOUNT <> 0)
      BEGIN
        SET @hasChanges = 1
        SELECT @ContactMinLsn = MIN([_Lsn]), @ContactMaxLsn = MAX([_Lsn]) FROM #_changes
      END
    END

    -- Find changes on related table: Address (Legacy.Address) - assume all are 'update' operation (i.e. it doesn't matter).
    IF (@AddressMinLsn < @AddressMaxLsn)
    BEGIN
      SELECT TOP (@MaxQuerySize)
          [_cdc].[__$start_lsn] AS [_Lsn],
          4 AS [_Op],
          [c].[ContactId] AS [ContactId]
        INTO #a
        FROM cdc.fn_cdc_get_net_changes_Legacy_Address(@AddressMinLsn, @AddressMaxLsn, 'all') AS [_cdc]
        INNER JOIN [Legacy].[Contact] AS [c] WITH (NOLOCK) ON ([_cdc].[Id] = [c].[AddressId])
        ORDER BY [_cdc].[__$start_lsn]

      IF (@@ROWCOUNT <> 0)
      BEGIN
        SET @hasChanges = 1
        SELECT @AddressMinLsn = MIN([_Lsn]), @AddressMaxLsn = MAX([_Lsn]) FROM #a

        INSERT INTO #_changes
          SELECT * 
            FROM #a AS [_a]
            WHERE NOT EXISTS (SELECT * FROM #_changes AS [_chg] WHERE [_chg].[ContactId] = [_a].[ContactId])
      END
    END

    -- Create a new outbox where not processing an existing.
    IF (@OutboxId IS NULL AND @hasChanges = 1)
    BEGIN
      DECLARE @InsertedOutboxId TABLE([OutboxId] INT)

      INSERT INTO [DemoCdc].[ContactOutbox] (
          [ContactMinLsn],
          [ContactMaxLsn],
          [AddressMinLsn],
          [AddressMaxLsn],
          [CreatedDate],
          [IsComplete]
        ) 
        OUTPUT inserted.OutboxId INTO @InsertedOutboxId
        VALUES (
          @ContactMinLsn,
          @ContactMaxLsn,
          @AddressMinLsn,
          @AddressMaxLsn,
          GETUTCDATE(),
          0
        )

        SELECT @OutboxId = [OutboxId] FROM @InsertedOutboxId
    END

    -- Return the *latest* outbox data.
    SELECT [_outbox].[OutboxId], [_outbox].[CreatedDate], [_outbox].[IsComplete], [_outbox].[CompletedDate]
      FROM [DemoCdc].[ContactOutbox] AS [_outbox]
      WHERE [_outbox].OutboxId = @OutboxId 

    -- Exit here if there were no changes found.
    IF (@hasChanges = 0)
    BEGIN
      COMMIT TRANSACTION
      RETURN 0
    END

    -- Root table: Legacy.Contact - uses LEFT OUTER JOIN's to get the deleted records, as well as any previous Tracking Hash value.
    SELECT
        [_ct].[Hash] AS [_TrackingHash],
        [_chg].[_Op] AS [_OperationType],
        [_chg].[ContactId] AS [ContactId],
        [c].[Name] AS [Name],
        [c].[Phone] AS [Phone],
        [c].[Email] AS [Email],
        [c].[Active] AS [Active],
        [c].[DontCallList] AS [DontCallList],
        [c].[AddressId] AS [AddressId],
        [cm].[UniqueId] AS [UniqueId]
      FROM #_changes AS [_chg]
      LEFT OUTER JOIN [DemoCdc].[CdcTracking] AS [_ct] ON ([_ct].[Schema] = 'Legacy' AND [_ct].[Table] = 'Contact' AND [_ct].[Key] = CAST([_chg].[ContactId] AS NVARCHAR(128)))
      LEFT OUTER JOIN [Legacy].[Contact] AS [c] ON ([c].[ContactId] = [_chg].[ContactId])
      INNER JOIN [Legacy].[ContactMapping] AS [cm] ON ([cm].[ContactId] = [c].[ContactId])

    -- Related table: Address (Legacy.Address) - only use INNER JOINS to get what is actually there right now.
    SELECT
        [a].[Id] AS [Id],
        [a].[Street1] AS [Street1],
        [a].[Street2] AS [Street2],
        [a].[City] AS [City],
        [a].[State] AS [State],
        [a].[PostalZipCode] AS [PostalZipCode]
      FROM #_changes AS [_chg]
      INNER JOIN [Legacy].[Contact] AS [c] ON ([c].[ContactId] = [_chg].[ContactId])
      INNER JOIN [Legacy].[Address] AS [a] ON ([a].[Id] = [c].[AddressId])
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