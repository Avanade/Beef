{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
CREATE PROCEDURE [{{CdcSchema}}].[{{ExecuteStoredProcedureName}}]
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

    -- Check if there is already an incomplete batch and attempt to reprocess.
    SELECT
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

    -- There should never be more than one incomplete outbox.
    IF @@ROWCOUNT > 1
    BEGIN
      {{#if Root.HasBeefDbo}}EXEC spThrowBusinessException {{else}};THROW 56002, {{/if}}'There are multiple incomplete outboxes; there should not be more than one incomplete outbox at any one time.'{{#unless Root.HasBeefDbo}}, 1{{/unless}}
    END

    -- Where there is no incomplete outbox then the next should be processed.
    IF (@OutboxId IS NULL)
    BEGIN
      -- Get the last outbox processed.
      SELECT TOP 1
          @{{pascal Name}}MinLsn = [_outbox].[{{pascal Name}}MaxLsn]{{#ifne CdcJoins.Count 0}},{{/ifne}}
{{#each CdcJoins}}
          @{{pascal Name}}MinLsn = [_outbox].[{{pascal Name}}MaxLsn]{{#unless @last}},{{/unless}}
{{/each}}
        FROM [{{CdcSchema}}].[{{OutboxTableName}}] AS [_outbox]
        ORDER BY [_outbox].[IsComplete] ASC, [_outbox].[OutboxId] DESC

      IF (@@ROWCOUNT = 0) -- No previous outbox; i.e. is the first time!
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

    -- The minimum should _not_ be less than the base otherwise we have lost data; either continue with this data loss, or error and stop.
    DECLARE @hasDataLoss BIT
    SET @hasDataLoss = 0

    IF (@{{pascal Name}}MinLsn < @{{pascal Name}}BaseMinLsn) BEGIN IF (@ContinueWithDataLoss = 1) BEGIN SET @hasDataLoss = 1; SET @{{pascal Name}}MinLsn = @{{pascal Name}}BaseMinLsn END ELSE BEGIN{{#if Root.HasBeefDbo}} EXEC spThrowBusinessException{{else}} ;THROW 56002, {{/if}}'Unexpected data loss error for ''{{Schema}}.{{Name}}''; this indicates that the CDC data has probably been cleaned up before being successfully processed.'{{#unless Root.HasBeefDbo}}, 1;{{/unless}} END END
{{#each CdcJoins}}
    IF (@{{pascal Name}}MinLsn < @{{pascal Name}}BaseMinLsn) BEGIN IF (@ContinueWithDataLoss = 1) BEGIN SET @hasDataLoss = 1; SET @{{pascal Name}}MinLsn = @{{pascal Name}}BaseMinLsn END ELSE BEGIN{{#if Root.HasBeefDbo}} EXEC spThrowBusinessException{{else}} ;THROW 56002, {{/if}}'Unexpected data loss error for ''{{Schema}}.{{TableName}}''; this indicates that the CDC data has probably been cleaned up before being successfully processed.'{{#unless Root.HasBeefDbo}}, 1;{{/unless}} END END
{{/each}}

    -- Find changes on the root table: {{Schema}}.{{Name}} - this determines overall operation type: 'create', 'update' or 'delete'.
    DECLARE @hasChanges BIT
    SET @hasChanges = 0

    IF (@{{pascal Name}}MinLsn <= @{{pascal Name}}MaxLsn)
    BEGIN
      SELECT TOP (@MaxQuerySize)
          [_cdc].[__$start_lsn] AS [_Lsn],
          [_cdc].[__$operation] AS [_Op],
{{#each PrimaryKeyColumns}}
          [_cdc].[{{Name}}] AS [{{Name}}]{{#unless @last}},{{/unless}}
{{/each}}
        INTO #_changes
        FROM cdc.fn_cdc_get_all_changes_{{Schema}}_{{Name}}(@{{pascal Name}}MinLsn, @{{pascal Name}}MaxLsn, 'all') AS [_cdc]
        ORDER BY [_cdc].[__$start_lsn]

      IF (@@ROWCOUNT <> 0)
      BEGIN
        SET @hasChanges = 1
        SELECT @{{pascal Name}}MinLsn = MIN([_Lsn]), @{{pascal Name}}MaxLsn = MAX([_Lsn]) FROM #_changes
      END
    END

{{#each CdcJoins}}
    -- Find changes on related table: {{Name}} ({{Schema}}.{{TableName}}) - assume all are 'update' operation (i.e. it doesn't matter).
    IF (@{{pascal Name}}MinLsn <= @{{pascal Name}}MaxLsn)
    BEGIN
      SELECT TOP (@MaxQuerySize)
          [_cdc].[__$start_lsn] AS [_Lsn],
          4 AS [_Op],
  {{#each Parent.PrimaryKeyColumns}}
          [{{Parent.Alias}}].[{{Name}}] AS [{{Name}}]{{#unless @last}},{{/unless}}
  {{/each}}
        INTO #{{Alias}}
        FROM cdc.fn_cdc_get_all_changes_{{Schema}}_{{TableName}}(@{{pascal Name}}MinLsn, @{{pascal Name}}MaxLsn, 'all') AS [_cdc]
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
    IF (@OutboxId IS NULL AND (@hasDataLoss = 1 OR @hasChanges = 1))
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
          [IsComplete],
          [HasDataLoss]
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
          0,
          @hasDataLoss
        )

        SELECT @OutboxId = [OutboxId] FROM @InsertedOutboxId
    END
    ELSE
    BEGIN
      IF (@OutboxId IS NOT NULL AND @hasDataLoss = 1)
      BEGIN
        UPDATE [{{CdcSchema}}].[{{OutboxTableName}}] 
          SET [HasDataLoss] = @hasDataLoss
          WHERE [OutboxId] = @OutboxId
      END
    END

    -- Return the *latest* outbox data.
    SELECT [_outbox].[OutboxId], [_outbox].[CreatedDate], [_outbox].[IsComplete], [_outbox].[CompletedDate], [_outBox].[HasDataLoss]
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
        [_chg].[_Op] AS [_OperationType],
        [_chg].[_Lsn] AS [_Lsn],
        [_ct].[Hash] AS [_TrackingHash],
{{#if IdentifierMapping}}
        [_im].[GlobalId] AS [GlobalId],
{{/if}}
{{#each PrimaryKeyColumns}}
        [_chg].[{{Name}}] AS [{{NameAlias}}],
{{/each}}
{{#each PrimaryKeyColumns}}
        [{{Parent.Alias}}].[{{Name}}] AS [TableKey_{{NameAlias}}]{{#if @last}}{{#ifne Parent.SelectedColumnsExcludingPrimaryKey.Count 0}},{{/ifne}}{{else}},{{/if}}
{{/each}}
{{#each SelectedColumnsExcludingPrimaryKey}}
        [{{#ifval IdentifierMappingAlias}}{{IdentifierMappingAlias}}{{else}}{{Parent.Alias}}{{/ifval}}].[{{Name}}] AS [{{NameAlias}}]{{#unless @last}},{{else}}{{#ifne Parent.JoinNonCdcChildren.Count 0}},{{/ifne}}{{/unless}}
{{/each}}
{{#each JoinNonCdcChildren}}
  {{#each Columns}}
        [{{Parent.Alias}}].[{{Name}}] AS [{{NameAlias}}]{{#unless @last}},{{else}}{{#unless @../last}},{{/unless}}{{/unless}}
  {{/each}}
{{/each}}
      FROM #_changes AS [_chg]
      LEFT OUTER JOIN [{{Schema}}].[{{Table}}] AS [{{Alias}}] ON ({{#each PrimaryKeyColumns}}{{#unless @first}} AND {{/unless}}[{{Parent.Alias}}].[{{Name}}] = [_chg].[{{Name}}]{{/each}})
{{#each JoinNonCdcChildren}}
      {{JoinTypeSql}} [{{Schema}}].[{{TableName}}] AS [{{Alias}}] ON ({{#each On}}{{#unless @first}} AND {{/unless}}[{{Parent.Alias}}].[{{Name}}] = {{#ifval ToStatement}}{{ToStatement}}{{else}}[{{Parent.JoinToAlias}}].[{{ToColumn}}]{{/ifval}}{{/each}})
{{/each}}
{{#if IdentifierMapping}}
      LEFT OUTER JOIN [{{Root.CdcSchema}}].[{{Root.CdcIdentifierMappingTableName}}] AS [_im] ON ([_im].[Schema] = '{{Schema}}' AND [_im].[Table] = '{{Name}}' AND [_im].[Key] = {{#ifeq PrimaryKeyColumns.Count 1}}{{#each PrimaryKeyColumns}}CAST([_chg].[{{Name}}] AS NVARCHAR(128))){{/each}}{{else}}CONCAT({{#each PrimaryKeyColumns}}CAST([_chg].[{{Name}}] AS NVARCHAR(128))){{#unless @last}}, ',', {{/unless}}{{/each}}){{/ifeq}}
{{/if}}
{{#each SelectedColumnsExcludingPrimaryKey}}
  {{#ifval IdentifierMappingParent}}
      LEFT OUTER JOIN [{{Root.CdcSchema}}].[{{Root.CdcIdentifierMappingTableName}}] AS [{{IdentifierMappingAlias}}] ON ([{{IdentifierMappingAlias}}].[Schema] = '{{IdentifierMappingSchema}}' AND [{{IdentifierMappingAlias}}].[Table] = '{{IdentifierMappingTable}}' AND [{{IdentifierMappingAlias}}].[Key] = CAST([{{IdentifierMappingParent.Parent.Alias}}].[{{IdentifierMappingParent.Name}}] AS NVARCHAR(128))) 
  {{/ifval}}
{{/each}}
      LEFT OUTER JOIN [{{Root.CdcSchema}}].[{{Root.CdcTrackingTableName}}] AS [_ct] ON ([_ct].[Schema] = '{{Schema}}' AND [_ct].[Table] = '{{Name}}' AND [_ct].[Key] = {{#if IdentifierMapping}}_im.GlobalId){{else}}{{#ifeq PrimaryKeyColumns.Count 1}}{{#each PrimaryKeyColumns}}CAST([_chg].[{{Name}}] AS NVARCHAR(128))){{/each}}{{else}}CONCAT({{#each PrimaryKeyColumns}}CAST([_chg].[{{Name}}] AS NVARCHAR(128))){{#unless @last}}, ',', {{/unless}}{{/each}}){{/ifeq}}{{/if}}

{{#each CdcJoins}}
    -- Related table: {{Name}} ({{Schema}}.{{TableName}}) - only use INNER JOINS to get what is actually there right now (where applicable).
    SELECT DISTINCT
  {{#if IdentifierMapping}}
        [_im].[GlobalId] AS [GlobalId],
  {{/if}}
  {{#each JoinHierarchyReverse}}
    {{#unless @last}}
      {{#each OnSelectColumns}}
        [{{Parent.JoinToAlias}}].[{{ToColumn}}] AS [{{pascal Parent.JoinTo}}_{{Name}}],  -- Additional joining column (informational).
      {{/each}}
    {{/unless}}
  {{/each}}
  {{#each Columns}}
        [{{#ifval IdentifierMappingAlias}}{{IdentifierMappingAlias}}{{else}}{{Parent.Alias}}{{/ifval}}].[{{Name}}] AS [{{NameAlias}}]{{#unless @last}},{{else}}{{#ifne Parent.JoinNonCdcChildren.Count 0}},{{/ifne}}{{/unless}}
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
  {{#if IdentifierMapping}}
      LEFT OUTER JOIN [{{Root.CdcSchema}}].[{{Root.CdcIdentifierMappingTableName}}] AS [_im] ON ([_im].[Schema] = '{{Schema}}' AND [_im].[Table] = '{{Name}}' AND [_im].[Key] = {{#ifeq PrimaryKeyColumns.Count 1}}{{#each PrimaryKeyColumns}}CAST([{{Parent.Alias}}].[{{Name}}] AS NVARCHAR(128))){{/each}}{{else}}CONCAT({{#each PrimaryKeyColumns}}CAST([{{Parent.Alias}}].[{{Name}}] AS NVARCHAR(128))){{#unless @last}}, ',', {{/unless}}{{/each}}){{/ifeq}}
  {{/if}}
  {{#each Columns}}
    {{#ifval IdentifierMappingParent}}
      LEFT OUTER JOIN [{{Root.CdcSchema}}].[{{Root.CdcIdentifierMappingTableName}}] AS [{{IdentifierMappingAlias}}] ON ([{{IdentifierMappingAlias}}].[Schema] = '{{IdentifierMappingSchema}}' AND [{{IdentifierMappingAlias}}].[Table] = '{{IdentifierMappingTable}}' AND [{{IdentifierMappingAlias}}].[Key] = CAST([{{IdentifierMappingParent.Parent.Alias}}].[{{IdentifierMappingParent.Name}}] AS NVARCHAR(128))) 
    {{/ifval}}
  {{/each}}
      WHERE [_chg].[_Op] <> 1

{{/each}}
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