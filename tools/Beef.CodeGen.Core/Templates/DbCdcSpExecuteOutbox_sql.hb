{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
CREATE PROCEDURE [{{CdcSchema}}].[{{StoredProcedureName}}]
  @MaxQuerySize INT NULL = 100,
  @GetIncompleteOutbox BIT NULL = 0,
  @OutboxIdToMarkComplete INT NULL = NULL,
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

    -- Mark the outbox as complete and merge the tracking info; then return the updated outbox data and stop!
    IF (@OutboxIdToMarkComplete IS NOT NULL)
    BEGIN
      UPDATE [_outbox] SET
          [_outbox].[IsComplete] = 1,
          [_outbox].[CompletedDate] = GETUTCDATE()
        FROM [{{CdcSchema}}].[{{OutboxTableName}}] AS [_outbox]
        WHERE OutboxId = @OutboxIdToMarkComplete 

      MERGE INTO [{{Root.CdcSchema}}].[{{Root.CdcTrackingTableName}}] WITH (HOLDLOCK) AS [_ct]
        USING @CompleteTrackingList AS [_list] ON ([_ct].[Schema] = '{{Schema}}' AND [_ct].[Table] = '{{Table}}' AND [_ct].[Key] = [_list].[Key])
        WHEN MATCHED AND EXISTS (
            SELECT [_list].[Key], [_list].[Hash]
            EXCEPT
            SELECT [_ct].[Key], [_ct].[Hash])
          THEN UPDATE SET [_ct].[Hash] = [_list].[Hash], [_ct].[OutboxId] = @OutboxIdToMarkComplete
        WHEN NOT MATCHED BY TARGET
          THEN INSERT ([Schema], [Table], [Key], [Hash], [OutboxId])
            VALUES ('{{Schema}}', '{{Table}}', [_list].[Key], [_list].[Hash], @OutboxIdToMarkComplete);

      SELECT [_outbox].[OutboxId], [_outbox].[CreatedDate], [_outbox].[IsComplete], [_outbox].[CompletedDate]
        FROM [{{CdcSchema}}].[{{OutboxTableName}}] AS [_outbox]
        WHERE [_outbox].OutboxId = @OutboxIdToMarkComplete

      COMMIT TRANSACTION
      RETURN 0;
    END

    -- Declare variables.
    DECLARE @{{pascal Name}}BaseMinLsn BINARY(10), @{{pascal Name}}MinLsn BINARY(10), @{{pascal Name}}MaxLsn BINARY(10)
{{#each CdcJoins}}
    DECLARE @{{pascal Name}}BaseMinLsn BINARY(10), @{{pascal Name}}MinLsn BINARY(10), @{{pascal Name}}MaxLsn BINARY(10)
{{/each}}
    DECLARE @OutboxId INT

    -- Get the latest 'base' minimum.
    SET @{{pascal Name}}BaseMinLsn = sys.fn_cdc_get_min_lsn('{{Schema}}_{{Name}}');
{{#each CdcJoins}}
    SET @{{pascal Name}}BaseMinLsn = sys.fn_cdc_get_min_lsn('{{Schema}}_{{TableName}}');
{{/each}}

    -- Where requesting for incomplete outbox, get first that is marked as incomplete.
    IF (@GetIncompleteOutbox = 1)
    BEGIN
      SELECT TOP 1
          @{{pascal Name}}MinLsn = [_outbox].[{{pascal Name}}MinLsn],
          @{{pascal Name}}MaxLsn = [_outbox].[{{pascal Name}}MaxLsn],
{{#each CdcJoins}}
          @{{pascal Name}}MinLsn = [_outbox].[{{pascal Name}}MinLsn],
          @{{pascal Name}}MaxLsn = [_outbox].[{{pascal Name}}MaxLsn],
{{/each}}
          @OutboxId = [OutboxId]
        FROM [{{CdcSchema}}].[{{OutboxTableName}}] AS [_outbox]
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
          @{{pascal Name}}MinLsn = [_outbox].[{{pascal Name}}MaxLsn],
{{#each CdcJoins}}
          @{{pascal Name}}MinLsn = [_outbox].[{{pascal Name}}MaxLsn],
{{/each}}
          @IsComplete = [_outbox].IsComplete
        FROM [{{CdcSchema}}].[{{OutboxTableName}}] AS [_outbox]
        ORDER BY [_outbox].[IsComplete] ASC, [_outbox].[OutboxId] DESC

      IF (@IsComplete = 0) -- Cannot continue where there is an incomplete outbox; must be completed.
      BEGIN
        SELECT [_outbox].[OutboxId], [_outbox].[CreatedDate], [_outbox].[IsComplete], [_outbox].[CompletedDate]
          FROM [{{CdcSchema}}].[{{OutboxTableName}}] AS [_outbox]
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
        SET @{{pascal Name}}MinLsn = @{{pascal Name}}BaseMinLsn;
{{#each CdcJoins}}
        SET @{{pascal Name}}MinLsn = @{{pascal Name}}BaseMinLsn;
{{/each}}
      END
      ELSE
      BEGIN
        -- Increment the minimum as the last has already been processed.
        SET @{{pascal Name}}MinLsn = sys.fn_cdc_increment_lsn(@{{pascal Name}}MinLsn)
{{#each CdcJoins}}
        SET @{{pascal Name}}MinLsn = sys.fn_cdc_increment_lsn(@{{pascal Name}}MinLsn)
{{/each}}
      END

      -- Get the maximum LSN.
      SET @{{pascal Name}}MaxLsn = sys.fn_cdc_get_max_lsn();
{{#each CdcJoins}}
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
{{#each CdcJoins}}
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

{{#each CdcJoins}}
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
            FROM #{{Alias}} AS [_{{Alias}}]
            WHERE NOT EXISTS (SELECT * FROM #_changes AS [_chg] WHERE {{#each Parent.PrimaryKeyColumns}}{{#unless @first}} AND {{/unless}}[_chg].[{{Name}}] = [_{{../Alias}}].[{{Name}}]{{/each}})
      END
    END

{{/each}}
    -- Create a new outbox where not processing an existing.
    IF (@OutboxId IS NULL AND @hasChanges = 1)
    BEGIN
      DECLARE @InsertedOutboxId TABLE([OutboxId] INT)

      INSERT INTO [{{CdcSchema}}].[{{OutboxTableName}}] (
          [{{pascal Name}}MinLsn],
          [{{pascal Name}}MaxLsn],
{{#each CdcJoins}}
          [{{pascal Name}}MinLsn],
          [{{pascal Name}}MaxLsn],
{{/each}}
          [CreatedDate],
          [IsComplete]
        ) 
        OUTPUT inserted.OutboxId INTO @InsertedOutboxId
        VALUES (
          @{{pascal Name}}MinLsn,
          @{{pascal Name}}MaxLsn,
{{#each CdcJoins}}
          @{{pascal Name}}MinLsn,
          @{{pascal Name}}MaxLsn,
{{/each}}
          GETUTCDATE(),
          0
        )

        SELECT @OutboxId = [OutboxId] FROM @InsertedOutboxId
    END

    -- Return the *latest* outbox data.
    SELECT [_outbox].[OutboxId], [_outbox].[CreatedDate], [_outbox].[IsComplete], [_outbox].[CompletedDate]
      FROM [{{CdcSchema}}].[{{OutboxTableName}}] AS [_outbox]
      WHERE [_outbox].OutboxId = @OutboxId 

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
        [{{Parent.Alias}}].[{{Name}}] AS [{{NameAlias}}]{{#unless @last}},{{else}}{{#ifne Parent.JoinNonCdcChildren.Count 0}},{{/ifne}}{{/unless}}
{{/each}}
{{#each JoinNonCdcChildren}}
  {{#each Columns}}
        [{{Parent.Alias}}].[{{Name}}] AS [{{NameAlias}}]{{#unless @last}},{{else}}{{#unless @../last}},{{/unless}}{{/unless}}
  {{/each}}
{{/each}}
      FROM #_changes AS [_chg]
      LEFT OUTER JOIN [{{Root.CdcSchema}}].[{{Root.CdcTrackingTableName}}] AS [_ct] ON ([_ct].[Schema] = '{{Schema}}' AND [_ct].[Table] = '{{Name}}' AND [_ct].[Key] = {{#ifeq PrimaryKeyColumns.Count 1}}{{#each PrimaryKeyColumns}}CAST([_chg].[{{Name}}] AS NVARCHAR(128))){{/each}}{{else}}CONCAT({{#each PrimaryKeyColumns}}CAST([_chg].[{{Name}}] AS NVARCHAR(128))){{#unless @last}}, ',', {{/unless}}{{/each}}){{/ifeq}}
      LEFT OUTER JOIN [{{Schema}}].[{{Table}}] AS [{{Alias}}] ON ({{#each PrimaryKeyColumns}}{{#unless @first}} AND {{/unless}}[{{Parent.Alias}}].[{{Name}}] = [_chg].[{{Name}}]{{/each}})
{{#each JoinNonCdcChildren}}
      {{JoinTypeSql}} [{{Schema}}].[{{TableName}}] AS [{{Alias}}] ON ({{#each On}}{{#unless @first}} AND {{/unless}}[{{Parent.Alias}}].[{{Name}}] = {{#ifval ToStatement}}{{ToStatement}}{{else}}[{{Parent.JoinToAlias}}].[{{ToColumn}}]{{/ifval}}{{/each}})
{{/each}}

{{#each CdcJoins}}
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
        [{{Parent.Alias}}].[{{Name}}] AS [{{NameAlias}}]{{#unless @last}},{{else}}{{#ifne Parent.JoinNonCdcChildren.Count 0}},{{/ifne}}{{/unless}}
  {{/each}}
  {{#each JoinNonCdcChildren}}
    {{#each Columns}}
        [{{Parent.Alias}}].[{{Name}}] AS [{{NameAlias}}]{{#unless @last}},{{else}}{{#unless @../last}},{{/unless}}{{/unless}}
    {{/each}}
  {{/each}}
      FROM #_changes AS [_chg]
      INNER JOIN [{{Parent.Schema}}].[{{Parent.Table}}] AS [{{Parent.Alias}}] ON ({{#each Parent.PrimaryKeyColumns}}{{#unless @first}} AND {{/unless}}[{{Parent.Alias}}].[{{Name}}] = [_chg].[{{Name}}]{{/each}})
  {{#each JoinHierarchyReverse}}
      INNER JOIN [{{Schema}}].[{{TableName}}] AS [{{Alias}}] ON ({{#each On}}{{#unless @first}} AND {{/unless}}[{{Parent.Alias}}].[{{Name}}] = {{#ifval ToStatement}}{{ToStatement}}{{else}}[{{Parent.JoinToAlias}}].[{{ToColumn}}]{{/ifval}}{{/each}})
  {{/each}}
  {{#each JoinNonCdcChildren}}
      {{JoinTypeSql}} [{{Schema}}].[{{TableName}}] AS [{{Alias}}] ON ({{#each On}}{{#unless @first}} AND {{/unless}}[{{Parent.Alias}}].[{{Name}}] = {{#ifval ToStatement}}{{ToStatement}}{{else}}[{{Parent.JoinToAlias}}].[{{ToColumn}}]{{/ifval}}{{/each}})
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