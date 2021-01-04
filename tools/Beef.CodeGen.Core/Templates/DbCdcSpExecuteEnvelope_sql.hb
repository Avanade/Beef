{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
CREATE PROCEDURE [{{CdcSchema}}].[{{StoredProcedureName}}]
  @MaxQuerySize INT NULL = 100,
  @GetIncompleteEnvelope BIT NULL = 0,
  @EnvelopeIdToMarkComplete INT NULL = NULL,
  @CompleteTrackingList AS [{{Root.CdcSchema}}].[udt{{Root.CdcTrackingTableName}}List] READONLY
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
        FROM [{{CdcSchema}}].[{{EnvelopeTableName}}] AS [_env]
        WHERE EnvelopeId = @EnvelopeIdToMarkComplete 

      MERGE INTO [{{Root.CdcSchema}}].[{{Root.CdcTrackingTableName}}] WITH (HOLDLOCK) AS [_ct]
        USING @CompleteTrackingList AS [_list] ON ([_ct].[Schema] = '{{Schema}}' AND [_ct].[Table] = '{{Table}}' AND [_ct].[Key] = [_list].[Key])
        WHEN MATCHED AND EXISTS (
            SELECT [_list].[Key], [_list].[Hash]
            EXCEPT
            SELECT [_ct].[Key], [_ct].[Hash])
          THEN UPDATE SET [_ct].[Hash] = [_list].[Hash], [_ct].[EnvelopeId] = @EnvelopeIdToMarkComplete
        WHEN NOT MATCHED BY TARGET
          THEN INSERT ([Schema], [Table], [Key], [Hash], [EnvelopeId])
            VALUES ('{{Schema}}', '{{Table}}', [_list].[Key], [_list].[Hash], @EnvelopeIdToMarkComplete);

      SELECT [_env].[EnvelopeId], [_env].[CreatedDate], [_env].[IsComplete], [_env].[CompletedDate]
        FROM [{{CdcSchema}}].[{{EnvelopeTableName}}] AS [_env]
        WHERE [_env].EnvelopeId = @EnvelopeIdToMarkComplete

      COMMIT TRANSACTION
      RETURN 0;
    END

    -- Declare variables.
    DECLARE @{{pascal Name}}BaseMinLsn BINARY(10), @{{pascal Name}}MinLsn BINARY(10), @{{pascal Name}}MaxLsn BINARY(10)
{{#each Joins}}
    DECLARE @{{pascal Name}}BaseMinLsn BINARY(10), @{{pascal Name}}MinLsn BINARY(10), @{{pascal Name}}MaxLsn BINARY(10)
{{/each}}
    DECLARE @EnvelopeId INT

    -- Get the latest 'base' minimum.
    SET @{{pascal Name}}BaseMinLsn = sys.fn_cdc_get_min_lsn('{{Schema}}_{{Name}}');
{{#each Joins}}
    SET @{{pascal Name}}BaseMinLsn = sys.fn_cdc_get_min_lsn('{{Schema}}_{{TableName}}');
{{/each}}

    -- Where requesting for incomplete envelope, get first that is marked as incomplete.
    IF (@GetIncompleteEnvelope = 1)
    BEGIN
      SELECT TOP 1
          @{{pascal Name}}MinLsn = [_env].[{{pascal Name}}MinLsn],
          @{{pascal Name}}MaxLsn = [_env].[{{pascal Name}}MaxLsn],
{{#each Joins}}
          @{{pascal Name}}MinLsn = [_env].[{{pascal Name}}MinLsn],
          @{{pascal Name}}MaxLsn = [_env].[{{pascal Name}}MaxLsn],
{{/each}}
          @EnvelopeId = [EnvelopeId]
        FROM [{{CdcSchema}}].[{{EnvelopeTableName}}] AS [_env]
        WHERE [_env].[IsComplete] = 0 
        ORDER BY [_env].[EnvelopeId]

      -- Where no incomplete envelope is found then stop!
      IF (@EnvelopeId IS NULL)
      BEGIN
        COMMIT TRANSACTION
        RETURN 0;
      END

      SET @MaxQuerySize = 1000000  -- Override to a very large number to get all rows within the existing range!
    END
    ELSE
    BEGIN
      -- Check that there are no incomplete envelopes; if there are then stop with error; otherwise, continue!
      DECLARE @IsComplete BIT

      SELECT TOP 1
          @EnvelopeId = [_env].[EnvelopeId],
          @{{pascal Name}}MinLsn = [_env].[{{pascal Name}}MaxLsn],
{{#each Joins}}
          @{{pascal Name}}MinLsn = [_env].[{{pascal Name}}MaxLsn],
{{/each}}
          @IsComplete = [_env].IsComplete
        FROM [{{CdcSchema}}].[{{EnvelopeTableName}}] AS [_env]
        ORDER BY [_env].[IsComplete] ASC, [_env].[EnvelopeId] DESC

      IF (@IsComplete = 0) -- Cannot continue where there is an incomplete envelope; must be completed.
      BEGIN
        SELECT [_env].[EnvelopeId], [_env].[CreatedDate], [_env].[IsComplete], [_env].[CompletedDate]
          FROM [{{CdcSchema}}].[{{EnvelopeTableName}}] AS [_env]
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
        SET @{{pascal Name}}MinLsn = @{{pascal Name}}BaseMinLsn;
{{#each Joins}}
        SET @{{pascal Name}}MinLsn = @{{pascal Name}}BaseMinLsn;
{{/each}}
      END
      ELSE
      BEGIN
        -- Increment the minimum as the last has already been processed.
        SET @{{pascal Name}}MinLsn = sys.fn_cdc_increment_lsn(@{{pascal Name}}MinLsn)
{{#each Joins}}
        SET @{{pascal Name}}MinLsn = sys.fn_cdc_increment_lsn(@{{pascal Name}}MinLsn)
{{/each}}
      END

      -- Get the maximum LSN.
      SET @{{pascal Name}}MaxLsn = sys.fn_cdc_get_max_lsn();
{{#each Joins}}
      SET @{{pascal Name}}MaxLsn = @{{pascal Parent.Name}}MaxLsn
{{/each}}

      -- Verify the maximum query size and correct (reset) where applicable.
      IF (@MaxQuerySize IS NULL OR @MaxQuerySize < 1 OR @MaxQuerySize > 10000)
      BEGIN
        SET @MaxQuerySize = 100
      END
    END

    -- The minimum can not be less than the base or an error will occur, so realign where not correct.
    IF (@{{pascal Name}}MinLsn < @{{pascal Name}}BaseMinLsn) BEGIN SET @{{pascal Name}}MinLsn = @{{pascal Name}}BaseMinLsn END
{{#each Joins}}
    IF (@{{pascal Name}}MinLsn < @{{pascal Name}}BaseMinLsn) BEGIN SET @{{pascal Name}}MinLsn = @{{pascal Name}}BaseMinLsn END
{{/each}}

    -- Find changes on the root table: {{Schema}}.{{Name}} - this determines overall operation type: 'create', 'update' or 'delete'.
    DECLARE @hasChanges BIT
    SET @hasChanges = 0

    IF (@{{pascal Name}}MinLsn < @{{pascal Name}}MaxLsn)
    BEGIN
      SELECT TOP (@MaxQuerySize)
          [_cdc].[__$start_lsn] AS [_Lsn],
          [_cdc].[__$operation] AS [_Op],
{{#each PrimaryKeyColumns}}
          [_cdc].[{{Name}}] AS [{{NameAlias}}]{{#unless @last}},{{/unless}}
{{/each}}
        INTO #_changes
        FROM cdc.fn_cdc_get_net_changes_{{Schema}}_{{Name}}(@{{pascal Name}}MinLsn, @{{pascal Name}}MaxLsn, 'all') AS [_cdc]
        ORDER BY [_cdc].[__$start_lsn]

      IF (@@ROWCOUNT <> 0)
      BEGIN
        SET @hasChanges = 1
        SELECT @{{pascal Name}}MinLsn = MIN([_Lsn]), @{{pascal Name}}MaxLsn = MAX([_Lsn]) FROM #_changes
      END
    END

{{#each Joins}}
    -- Find changes on related table: {{Name}} ({{Schema}}.{{TableName}}) - assume all are 'update' operation (i.e. it doesn't matter).
    IF (@{{pascal Name}}MinLsn < @{{pascal Name}}MaxLsn)
    BEGIN
      SELECT TOP (@MaxQuerySize)
          [_cdc].[__$start_lsn] AS [_Lsn],
          4 AS [_Op],
  {{#each Parent.PrimaryKeyColumns}}
          [{{Parent.Alias}}].[{{Name}}] AS [{{NameAlias}}]{{#unless @last}},{{/unless}}
  {{/each}}
        INTO #{{Alias}}
        FROM cdc.fn_cdc_get_net_changes_{{Schema}}_{{TableName}}(@{{pascal Name}}MinLsn, @{{pascal Name}}MaxLsn, 'all') AS [_cdc]
  {{#each JoinHierarchy}}
        INNER JOIN [{{JoinToSchema}}].[{{JoinTo}}] AS [{{JoinToAlias}}] WITH (NOLOCK) ON ({{#each On}}{{#unless @first}} AND {{/unless}}{{#if Parent.IsFirstInJoinHierarchy}}[_cdc]{{else}}[{{Parent.Alias}}]{{/if}}.[{{Name}}] = {{#ifval ToStatement}}{{ToStatement}}{{else}}[{{Parent.JoinToAlias}}].[{{ToColumn}}]{{/ifval}}{{/each}})
  {{/each}}
        ORDER BY [_cdc].[__$start_lsn]

      IF (@@ROWCOUNT <> 0)
      BEGIN
        SET @hasChanges = 1
        SELECT @{{pascal Name}}MinLsn = MIN([_Lsn]), @{{pascal Name}}MaxLsn = MAX([_Lsn]) FROM #{{Alias}}

        INSERT INTO #_changes
          SELECT * 
            FROM #{{Alias}} AS [_{{Alias}}]{{setkv1 Alias}}
            WHERE NOT EXISTS (SELECT * FROM #_changes AS [_chg] WHERE {{#each Parent.PrimaryKeyColumns}}{{#unless @first}} AND {{/unless}}[_chg].[{{Name}}] = [_{{Root.KV1}}].[{{Name}}]{{/each}})
      END
    END

{{/each}}
    -- Create a new envelope where not processing an existing.
    IF (@EnvelopeId IS NULL AND @hasChanges = 1)
    BEGIN
      DECLARE @InsertedEnvelopeId TABLE([EnvelopeId] INT)

      INSERT INTO [{{CdcSchema}}].[{{EnvelopeTableName}}] (
          [{{pascal Name}}MinLsn],
          [{{pascal Name}}MaxLsn],
{{#each Joins}}
          [{{pascal Name}}MinLsn],
          [{{pascal Name}}MaxLsn],
{{/each}}
          [CreatedDate],
          [IsComplete]
        ) 
        OUTPUT inserted.EnvelopeId INTO @InsertedEnvelopeId
        VALUES (
          @{{pascal Name}}MinLsn,
          @{{pascal Name}}MaxLsn,
{{#each Joins}}
          @{{pascal Name}}MinLsn,
          @{{pascal Name}}MaxLsn,
{{/each}}
          GETUTCDATE(),
          0
        )

        SELECT @EnvelopeId = [EnvelopeId] FROM @InsertedEnvelopeId
    END

    -- Return the *latest* envelope data.
    SELECT [_env].[EnvelopeId], [_env].[CreatedDate], [_env].[IsComplete], [_env].[CompletedDate]
      FROM [{{CdcSchema}}].[{{EnvelopeTableName}}] AS [_env]
      WHERE [_env].EnvelopeId = @EnvelopeId 

    -- Exit here if there were no changes found.
    IF (@hasChanges = 0)
    BEGIN
      COMMIT TRANSACTION
      RETURN 0
    END

    -- Root table: {{Schema}}.{{Name}} - uses LEFT OUTER JOIN's to get the deleted records, as well as any previous Tracking Hash value.
    SELECT
        [_ct].[Hash] AS [_TrackingHash],
        [_chg].[_Op] AS [_OperationType],
{{#each PrimaryKeyColumns}}
        [_chg].[{{Name}}] AS [{{NameAlias}}]{{#ifne Parent.SelectedColumnsExcludingPrimaryKey.Count 0}},{{/ifne}}
{{/each}}
{{#each SelectedColumnsExcludingPrimaryKey}}
        [{{Parent.Alias}}].[{{Name}}] AS [{{NameAlias}}]{{#unless @last}},{{/unless}}
{{/each}}
      FROM #_changes AS [_chg]
      LEFT OUTER JOIN [{{Root.CdcSchema}}].[{{Root.CdcTrackingTableName}}] AS [_ct] WITH (NOLOCK) ON ([_ct].[Schema] = '{{Schema}}' AND [_ct].[Table] = '{{Name}}' AND [_ct].[Key] = {{#ifeq PrimaryKeyColumns.Count 1}}{{#each PrimaryKeyColumns}}CAST([_chg].[{{Name}}] AS NVARCHAR)){{/each}}{{else}}CONCAT({{#each PrimaryKeyColumns}}CAST([_chg].[{{Name}}] AS NVARCHAR)){{#unless @last}}, ',', {{/unless}}{{/each}}){{/ifeq}}
      LEFT OUTER JOIN [{{Schema}}].[{{Table}}] AS [{{Alias}}] WITH (NOLOCK) ON ({{#each PrimaryKeyColumns}}{{#unless @first}} AND {{/unless}}[{{Parent.Alias}}].[{{Name}}] = [_chg].[{{Name}}]{{/each}})

{{#each Joins}}
    -- Related table: {{Name}} ({{Schema}}.{{TableName}}) - only use INNER JOINS to get what is actually there right now.
    SELECT
  {{#each JoinHierarchyReverse}}
    {{#unless @last}}
      {{#each OnSelectColumns}}
        [{{Parent.JoinToAlias}}].[{{ToColumn}}] AS [{{pascal Parent.JoinTo}}_{{Name}}],  -- Additional joining column (informational).
      {{/each}}
    {{/unless}}
  {{/each}}
  {{#each Columns}}
        [{{Parent.Alias}}].[{{Name}}] AS [{{NameAlias}}]{{#unless @last}},{{/unless}}
  {{/each}}
      FROM #_changes AS [_chg]
      INNER JOIN [{{Parent.Schema}}].[{{Parent.Table}}] AS [{{Parent.Alias}}] WITH (NOLOCK) ON ({{#each Parent.PrimaryKeyColumns}}{{#unless @first}} AND {{/unless}}[{{Parent.Alias}}].[{{Name}}] = [_chg].[{{Name}}]{{/each}})
  {{#each JoinHierarchyReverse}}
      INNER JOIN [{{Schema}}].[{{TableName}}] AS [{{Alias}}] WITH (NOLOCK) ON ({{#each On}}{{#unless @first}} AND {{/unless}}[{{Parent.Alias}}].[{{Name}}] = {{#ifval ToStatement}}{{ToStatement}}{{else}}[{{Parent.JoinToAlias}}].[{{ToColumn}}]{{/ifval}}{{/each}})
  {{/each}}
      WHERE [_chg].[_Op] <> 1

{{/each}}
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