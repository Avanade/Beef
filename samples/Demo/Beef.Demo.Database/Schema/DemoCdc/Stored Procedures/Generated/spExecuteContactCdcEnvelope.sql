CREATE PROCEDURE [DemoCdc].[spExecuteContactCdcEnvelope]
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
        FROM [DemoCdc].[ContactEnvelope] AS [_env]
        WHERE EnvelopeId = @EnvelopeIdToMarkComplete 

      MERGE INTO [DemoCdc].[CdcTracking] WITH (HOLDLOCK) AS [_ct]
        USING @CompleteTrackingList AS [_list] ON ([_ct].[Schema] = 'Legacy' AND [_ct].[Table] = 'Contact' AND [_ct].[Key] = [_list].[Key])
        WHEN MATCHED AND EXISTS (
            SELECT [_list].[Key], [_list].[Hash]
            EXCEPT
            SELECT [_ct].[Key], [_ct].[Hash])
          THEN UPDATE SET [_ct].[Hash] = [_list].[Hash], [_ct].[EnvelopeId] = @EnvelopeIdToMarkComplete
        WHEN NOT MATCHED BY TARGET
          THEN INSERT ([Schema], [Table], [Key], [Hash], [EnvelopeId])
            VALUES ('Legacy', 'Contact', [_list].[Key], [_list].[Hash], @EnvelopeIdToMarkComplete);

      SELECT [_env].[EnvelopeId], [_env].[CreatedDate], [_env].[IsComplete], [_env].[CompletedDate]
        FROM [DemoCdc].[ContactEnvelope] AS [_env]
        WHERE [_env].EnvelopeId = @EnvelopeIdToMarkComplete

      COMMIT TRANSACTION
      RETURN 0;
    END

    -- Declare variables.
    DECLARE @ContactMinLsn BINARY(10), @ContactMaxLsn BINARY(10)
    DECLARE @AddressMinLsn BINARY(10), @AddressMaxLsn BINARY(10)
    DECLARE @EnvelopeId INT
    DECLARE @IncludeMin BIT

    -- Where requesting for incomplete envelope, get first that is marked as incomplete.
    IF (@GetIncompleteEnvelope = 1)
    BEGIN
      SELECT TOP 1
          @ContactMinLsn = [_env].[ContactMinLsn],
          @ContactMaxLsn = [_env].[ContactMaxLsn],
          @AddressMinLsn = [_env].[AddressMinLsn],
          @AddressMaxLsn = [_env].[AddressMaxLsn],
          @EnvelopeId = [EnvelopeId]
        FROM [DemoCdc].[ContactEnvelope] AS [_env]
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
          @ContactMinLsn = [_env].[ContactMaxLsn],
          @AddressMinLsn = [_env].[AddressMaxLsn],
          @IsComplete = [_env].IsComplete
        FROM [DemoCdc].[ContactEnvelope] AS [_env]
        ORDER BY [_env].[IsComplete] ASC, [_env].[EnvelopeId] DESC

      IF (@IsComplete = 0) -- Cannot continue where there is an incomplete envelope; must be completed.
      BEGIN
        SELECT [_env].[EnvelopeId], [_env].[CreatedDate], [_env].[IsComplete], [_env].[CompletedDate]
          FROM [DemoCdc].[ContactEnvelope] AS [_env]
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
        SET @ContactMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Contact');
        SET @AddressMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Address');
        SET @IncludeMin = 1
      END

      SET @ContactMaxLsn = sys.fn_cdc_get_max_lsn();
      SET @AddressMaxLsn = @ContactMaxLsn

      IF (@MaxQuerySize IS NULL OR @MaxQuerySize < 1 OR @MaxQuerySize > 10000)
      BEGIN
        SET @MaxQuerySize = 100  -- Reset where the value appears invalid. 
      END
    END

    -- Find changes on the root table: Legacy.Contact - this determines overall operation type: 'create', 'update' or 'delete'.
    SELECT TOP (@MaxQuerySize)
        [_cdc].[__$start_lsn] AS [_Lsn],
        [_cdc].[__$operation] AS [_Op],
        [_cdc].[ContactId] AS [ContactId]
      INTO #_changes
      FROM cdc.fn_cdc_get_net_changes_Legacy_Contact(@ContactMinLsn, @ContactMaxLsn, 'all') AS [_cdc]
      WHERE @IncludeMin = 1 OR [_cdc].[__$start_lsn] > @ContactMinLsn
      ORDER BY [_cdc].[__$start_lsn]

    IF (@@ROWCOUNT <> 0)
    BEGIN
      SELECT @ContactMinLsn = MIN([_Lsn]), @ContactMaxLsn = MAX([_Lsn]) FROM #_changes
    END

    -- Find changes on related table: Address (Legacy.Address) - assume all are 'update' operation (i.e. it doesn't matter).
    SELECT TOP (@MaxQuerySize)
        [_cdc].[__$start_lsn] AS [_Lsn],
        4 AS [_Op],
        [c].[ContactId] AS [ContactId]
      INTO #a
      FROM cdc.fn_cdc_get_net_changes_Legacy_Address(@AddressMinLsn, @AddressMaxLsn, 'all') AS [_cdc]
      INNER JOIN [Legacy].[Contact] AS [c] WITH (NOLOCK) ON ([_cdc].[Id] = [c].[AddressId])
      WHERE @IncludeMin = 1 OR [_cdc].[__$start_lsn] > @AddressMinLsn
      ORDER BY [_cdc].[__$start_lsn]

    IF (@@ROWCOUNT <> 0)
    BEGIN
      SELECT @AddressMinLsn = MIN([_Lsn]), @AddressMaxLsn = MAX([_Lsn]) FROM #a
    END

    INSERT INTO #_changes
      SELECT * 
        FROM #a AS [_a]
        WHERE NOT EXISTS (SELECT * FROM #_changes AS [_chg] WHERE [_chg].[ContactId] = [_a].[ContactId])

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

      INSERT INTO [DemoCdc].[ContactEnvelope] (
          [ContactMinLsn],
          [ContactMaxLsn],
          [AddressMinLsn],
          [AddressMaxLsn],
          [CreatedDate],
          [IsComplete]
        ) 
        OUTPUT inserted.EnvelopeId INTO @InsertedEnvelopeId
        VALUES (
          @ContactMinLsn,
          @ContactMaxLsn,
          @AddressMinLsn,
          @AddressMaxLsn,
          GETUTCDATE(),
          0
        )

        SELECT @EnvelopeId = [EnvelopeId] FROM @InsertedEnvelopeId
    END

    -- Return the *latest* envelope data.
    SELECT [_env].[EnvelopeId], [_env].[CreatedDate], [_env].[IsComplete], [_env].[CompletedDate]
      FROM [DemoCdc].[ContactEnvelope] AS [_env]
      WHERE [_env].EnvelopeId = @EnvelopeId 

    -- Root table: Legacy.Contact - uses LEFT OUTER JOIN so we get the deleted records too.
    SELECT
        [_ct].[Hash] AS [_TrackingHash],
        [_chg].[_Op] AS [_OperationType],
        [_chg].[ContactId] AS [ContactId],
        [c].[Name] AS [Name],
        [c].[Phone] AS [Phone],
        [c].[Email] AS [Email],
        [c].[Active] AS [Active],
        [c].[DontCallList] AS [DontCallList],
        [c].[AddressId] AS [AddressId]
      FROM #_changes AS [_chg]
      LEFT OUTER JOIN [DemoCdc].[CdcTracking] AS [_ct] WITH (NOLOCK) ON ([_ct].[Schema] = 'Legacy' AND [_ct].[Table] = 'Contact' AND [_ct].[Key] = CAST([_chg].[ContactId] AS NVARCHAR))
      LEFT OUTER JOIN [Legacy].[Contact] AS [c] WITH (NOLOCK) ON ([c].[ContactId] = [_chg].[ContactId])

    -- Related table: Address (Legacy.Address) - only use INNER JOINS to get what is actually there right now.
    SELECT
        [a].[Id] AS [Id],
        [a].[Street1] AS [Street1],
        [a].[Street2] AS [Street2],
        [a].[City] AS [City],
        [a].[State] AS [State],
        [a].[PostalZipCode] AS [PostalZipCode]
      FROM #_changes AS [_chg]
      INNER JOIN [Legacy].[Contact] AS [c] WITH (NOLOCK) ON ([c].[ContactId] = [_chg].[ContactId])
      INNER JOIN [Legacy].[Address] AS [a] WITH (NOLOCK) ON ([a].[Id] = [c].[AddressId])
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