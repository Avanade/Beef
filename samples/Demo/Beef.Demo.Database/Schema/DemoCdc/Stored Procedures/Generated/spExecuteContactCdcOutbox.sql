CREATE PROCEDURE [DemoCdc].[spExecuteContactCdcOutbox]
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
    DECLARE @ContactBaseMinLsn BINARY(10), @ContactMinLsn BINARY(10), @ContactMaxLsn BINARY(10)
    DECLARE @AddressBaseMinLsn BINARY(10), @AddressMinLsn BINARY(10), @AddressMaxLsn BINARY(10)
    DECLARE @OutboxId INT

    -- Get the latest 'base' minimum.
    SET @ContactBaseMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Contact');
    SET @AddressBaseMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Address');

    -- Check if there is already an incomplete batch and attempt to reprocess.
    SELECT
        @ContactMinLsn = [_outbox].[ContactMinLsn],
        @ContactMaxLsn = [_outbox].[ContactMaxLsn],
        @AddressMinLsn = [_outbox].[AddressMinLsn],
        @AddressMaxLsn = [_outbox].[AddressMaxLsn],
        @OutboxId = [OutboxId]
      FROM [DemoCdc].[ContactOutbox] AS [_outbox]
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
          @ContactMinLsn = [_outbox].[ContactMaxLsn],
          @AddressMinLsn = [_outbox].[AddressMaxLsn]
        FROM [DemoCdc].[ContactOutbox] AS [_outbox]
        ORDER BY [_outbox].[IsComplete] ASC, [_outbox].[OutboxId] DESC

      IF (@@ROWCOUNT = 0) -- No previous outbox; i.e. is the first time!
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

    -- The minimum should _not_ be less than the base otherwise we have lost data; either continue with this data loss, or error and stop.
    DECLARE @hasDataLoss BIT
    SET @hasDataLoss = 0

    IF (@ContactMinLsn < @ContactBaseMinLsn) BEGIN IF (@ContinueWithDataLoss = 1) BEGIN SET @hasDataLoss = 1; SET @ContactMinLsn = @ContactBaseMinLsn END ELSE BEGIN ;THROW 56002, 'Unexpected data loss error for ''Legacy.Contact''; this indicates that the CDC data has probably been cleaned up before being successfully processed.', 1; END END
    IF (@AddressMinLsn < @AddressBaseMinLsn) BEGIN IF (@ContinueWithDataLoss = 1) BEGIN SET @hasDataLoss = 1; SET @AddressMinLsn = @AddressBaseMinLsn END ELSE BEGIN ;THROW 56002, 'Unexpected data loss error for ''Legacy.Address''; this indicates that the CDC data has probably been cleaned up before being successfully processed.', 1; END END

    -- Find changes on the root table: Legacy.Contact - this determines overall operation type: 'create', 'update' or 'delete'.
    DECLARE @hasChanges BIT
    SET @hasChanges = 0

    IF (@ContactMinLsn <= @ContactMaxLsn)
    BEGIN
      SELECT TOP (@MaxQuerySize)
          [_cdc].[__$start_lsn] AS [_Lsn],
          [_cdc].[__$operation] AS [_Op],
          [_cdc].[ContactId] AS [ContactId]
        INTO #_changes
        FROM cdc.fn_cdc_get_all_changes_Legacy_Contact(@ContactMinLsn, @ContactMaxLsn, 'all') AS [_cdc]
        ORDER BY [_cdc].[__$start_lsn]

      IF (@@ROWCOUNT <> 0)
      BEGIN
        SET @hasChanges = 1
        SELECT @ContactMinLsn = MIN([_Lsn]), @ContactMaxLsn = MAX([_Lsn]) FROM #_changes
      END
    END

    -- Find changes on related table: Address (Legacy.Address) - assume all are 'update' operation (i.e. it doesn't matter).
    IF (@AddressMinLsn <= @AddressMaxLsn)
    BEGIN
      SELECT TOP (@MaxQuerySize)
          [_cdc].[__$start_lsn] AS [_Lsn],
          4 AS [_Op],
          [c].[ContactId] AS [ContactId]
        INTO #a
        FROM cdc.fn_cdc_get_all_changes_Legacy_Address(@AddressMinLsn, @AddressMaxLsn, 'all') AS [_cdc]
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
    IF (@OutboxId IS NULL AND (@hasDataLoss = 1 OR @hasChanges = 1))
    BEGIN
      DECLARE @InsertedOutboxId TABLE([OutboxId] INT)

      INSERT INTO [DemoCdc].[ContactOutbox] (
          [ContactMinLsn],
          [ContactMaxLsn],
          [AddressMinLsn],
          [AddressMaxLsn],
          [CreatedDate],
          [IsComplete],
          [HasDataLoss]
        ) 
        OUTPUT inserted.OutboxId INTO @InsertedOutboxId
        VALUES (
          @ContactMinLsn,
          @ContactMaxLsn,
          @AddressMinLsn,
          @AddressMaxLsn,
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
        UPDATE [DemoCdc].[ContactOutbox] 
          SET [HasDataLoss] = @hasDataLoss
          WHERE [OutboxId] = @OutboxId
      END
    END

    -- Return the *latest* outbox data.
    SELECT [_outbox].[OutboxId], [_outbox].[CreatedDate], [_outbox].[IsComplete], [_outbox].[CompletedDate], [_outBox].[HasDataLoss]
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
        [_chg].[_Op] AS [_OperationType],
        [_chg].[_Lsn] AS [_Lsn],
        [_ct].[Hash] AS [_TrackingHash],
        [_im].[GlobalId] AS [GlobalId],
        [_chg].[ContactId] AS [CID],
        [c].[ContactId] AS [TableKey_CID],
        [c].[Name] AS [Name],
        [c].[Phone] AS [Phone],
        [c].[Email] AS [Email],
        [c].[Active] AS [Active],
        [c].[DontCallList] AS [DontCallList],
        [c].[AddressId] AS [AddressId],
        [c].[AlternateContactId] AS [AlternateContactId],
        [_im1].[GlobalId] AS [GlobalAlternateContactId],
        [c].[legacy_system_code] AS [LegacySystemCode],
        [cm].[UniqueId] AS [UniqueId]
      FROM #_changes AS [_chg]
      LEFT OUTER JOIN [Legacy].[Contact] AS [c] ON ([c].[ContactId] = [_chg].[ContactId])
      LEFT OUTER JOIN [Legacy].[ContactMapping] AS [cm] ON ([cm].[ContactId] = [c].[ContactId])
      LEFT OUTER JOIN [DemoCdc].[CdcIdentifierMapping] AS [_im] ON ([_im].[Schema] = 'Legacy' AND [_im].[Table] = 'Contact' AND [_im].[Key] = CAST([_chg].[ContactId] AS NVARCHAR(128)))
      LEFT OUTER JOIN [DemoCdc].[CdcIdentifierMapping] AS [_im1] ON ([_im1].[Schema] = 'Legacy' AND [_im1].[Table] = 'Contact' AND [_im1].[Key] = CAST([c].[AlternateContactId] AS NVARCHAR(128))) 
      LEFT OUTER JOIN [DemoCdc].[CdcTracking] AS [_ct] ON ([_ct].[Schema] = 'Legacy' AND [_ct].[Table] = 'Contact' AND [_ct].[Key] = _im.GlobalId)

    -- Related table: Address (Legacy.Address) - only use INNER JOINS to get what is actually there right now (where applicable).
    SELECT DISTINCT
        [_im].[GlobalId] AS [GlobalId],
        [a].[Id] AS [AID],
        [a].[Street1] AS [Street1],
        [a].[Street2] AS [Street2],
        [a].[City] AS [City],
        [a].[State] AS [State],
        [a].[PostalZipCode] AS [PostalZipCode],
        [a].[AlternateAddressId] AS [AlternateAddressId],
        [_im1].[GlobalId] AS [GlobalAlternateAddressId]
      FROM #_changes AS [_chg]
      INNER JOIN [Legacy].[Contact] AS [c] ON ([c].[ContactId] = [_chg].[ContactId])
      INNER JOIN [Legacy].[Address] AS [a] ON ([a].[Id] = [c].[AddressId])
      LEFT OUTER JOIN [DemoCdc].[CdcIdentifierMapping] AS [_im] ON ([_im].[Schema] = 'Legacy' AND [_im].[Table] = 'Address' AND [_im].[Key] = CAST([a].[Id] AS NVARCHAR(128)))
      LEFT OUTER JOIN [DemoCdc].[CdcIdentifierMapping] AS [_im1] ON ([_im1].[Schema] = 'Legacy' AND [_im1].[Table] = 'Address' AND [_im1].[Key] = CAST([a].[AlternateAddressId] AS NVARCHAR(128))) 
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