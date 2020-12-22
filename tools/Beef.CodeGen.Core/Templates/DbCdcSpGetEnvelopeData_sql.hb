{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
CREATE PROCEDURE [{{CdcSchema}}].[{{StoredProcedureName}}]
  @EnvelopeIdToMarkComplete INT NULL = NULL,
  @ReturnIncompleteBatches BIT NULL = 0,
  @MaxBatchSize INT NULL = 100
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost.
   */

  SET NOCOUNT ON;

  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

    -- Mark the batch as complete; then return the updated envelope data and stop!
    IF (@EnvelopeIdToMarkComplete IS NOT NULL)
    BEGIN
      UPDATE [_outbox] SET
          [_outbox].[IsComplete] = 1,
          [_outbox].[CompletedDate] = GETUTCDATE()
        FROM [{{CdcSchema}}].[{{EnvelopeTableName}}] AS [_outbox]
        WHERE EnvelopeId = @EnvelopeIdToMarkComplete 

      SELECT [_outbox].[EnvelopeId], [_outbox].[CreatedDate], [_outbox].[IsComplete], [_outbox].[CompletedDate]
        FROM [{{CdcSchema}}].[{{EnvelopeTableName}}] AS [_outbox]
        WHERE [_outbox].EnvelopeId = @EnvelopeIdToMarkComplete 

      COMMIT TRANSACTION
      RETURN 0;
    END

    -- Declare variables.
    DECLARE @{{pascal Name}}MinLsn BINARY(10), @{{pascal Name}}MaxLsn BINARY(10)
{{#each Joins}}
    DECLARE @{{pascal Name}}MinLsn BINARY(10), @{{pascal Name}}MaxLsn BINARY(10)
{{/each}}
    DECLARE @EnvelopeId INT

    -- Where requesting for incomplete envelope, get first that is marked as incomplete.
    IF (@ReturnIncompleteBatches = 1)
    BEGIN
      SELECT TOP 1
          @{{pascal Name}}MinLsn = [_outbox].[{{pascal Name}}MinLsn],
          @{{pascal Name}}MaxLsn = [_outbox].[{{pascal Name}}MaxLsn],
{{#each Joins}}
          @{{pascal Name}}MinLsn = [_outbox].[{{pascal Name}}MinLsn],
          @{{pascal Name}}MaxLsn = [_outbox].[{{pascal Name}}MaxLsn],
{{/each}}
          @EnvelopeId = [EnvelopeId]
        FROM [{{CdcSchema}}].[{{EnvelopeTableName}}] AS [_outbox]
        WHERE [_outbox].[EnvelopeId] = @EnvelopeIdToMarkComplete 
        ORDER BY [_outbox].[EnvelopeId]

      -- Where no incomplete envelope is found then stop!
      IF (@EnvelopeId IS NULL)
      BEGIN
        COMMIT TRANSACTION
        RETURN 0;
      END

      SET @MaxBatchSize = 1000000  -- Override to a very large number.
    END
    ELSE
    BEGIN
      -- Check that there are no incomplete envelopes; if there are then stop with error; otherwise, continue!
      DECLARE @IsComplete BIT

      SELECT TOP 1
          @EnvelopeId = [_outbox].[EnvelopeId],
          @{{pascal Name}}MinLsn = [_outbox].[{{pascal Name}}MaxLsn],
{{#each Joins}}
          @{{pascal Name}}MinLsn = [_outbox].[{{pascal Name}}MaxLsn],
{{/each}}
          @IsComplete = [_outbox].IsComplete
        FROM [{{CdcSchema}}].[{{EnvelopeTableName}}] AS [_outbox]
        ORDER BY [_outbox].[IsComplete] ASC, [_outbox].[EnvelopeId] DESC

      IF (@IsComplete = 0)
      BEGIN
        SELECT [_outbox].[EnvelopeId], [_outbox].[CreatedDate], [_outbox].[IsComplete], [_outbox].[CompletedDate]
          FROM [{{CdcSchema}}].[{{EnvelopeTableName}}] AS [_outbox]
          WHERE [_outbox].EnvelopeId = @EnvelopeId 

        COMMIT TRANSACTION
        RETURN -1;
      END

      IF (@IsComplete IS NULL)
      BEGIN
        SET @{{pascal Name}}MinLsn = sys.fn_cdc_get_min_lsn('{{Schema}}_{{Name}}');
{{#each Joins}}
        SET @{{pascal Name}}MinLsn = sys.fn_cdc_get_min_lsn('{{Schema}}_{{TableName}}');
{{/each}}
      END

      SET @{{pascal Name}}MaxLsn = sys.fn_cdc_get_max_lsn();
{{#each Joins}}
      SET @{{pascal Name}}MaxLsn = @{{pascal Parent.Name}}MaxLsn
{{/each}}

      IF (@MaxBatchSize IS NULL OR @MaxBatchSize < 1 OR @MaxBatchSize > 10000)
      BEGIN
        SET @MaxBatchSize = 100  -- Reset where the value appears invalid. 
      END
    END

    -- Find changes on the root table: {{Schema}}.{{Name}} - this determines overall operation type: 'create', 'update' or 'delete'.
    SELECT TOP (@MaxBatchSize)
        [_cdc].[__$start_lsn] AS [_Lsn],
        [_cdc].[__$operation] AS [_Op],
{{#each PrimaryKeyColumns}}
        [_cdc].[{{Name}}] AS [{{NameAlias}}]{{#unless @last}},{{/unless}}
{{/each}}
      INTO #_changes
      FROM cdc.fn_cdc_get_net_changes_{{Schema}}_{{Name}}(@{{pascal Name}}MinLsn, @{{pascal Name}}MaxLsn, 'all') AS [_cdc]

    IF (@@ROWCOUNT <> 0)
    BEGIN
      SELECT @{{pascal Name}}MinLsn = MIN([_Lsn]), @{{pascal Name}}MaxLsn = MAX([_Lsn]) FROM #_changes
    END

{{#each Joins}}
    -- Find changes on related table: {{Name}} ({{Schema}}.{{TableName}}) - assume all are 'update' operation (i.e. it doesn't matter).
    SELECT TOP (@MaxBatchSize)
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

    IF (@@ROWCOUNT <> 0)
    BEGIN
      SELECT @{{pascal Name}}MinLsn = MIN([_Lsn]), @{{pascal Name}}MaxLsn = MAX([_Lsn]) FROM #{{Alias}}
    END

    INSERT INTO #_changes
      SELECT * 
        FROM #{{Alias}} AS [_{{Alias}}]{{setkv1 Alias}}
        WHERE NOT EXISTS (SELECT * FROM #_changes AS [_chg] WHERE {{#each Parent.PrimaryKeyColumns}}{{#unless @first}} AND {{/unless}}[_chg].[{{Name}}] = [_{{Root.KV1}}].[{{Name}}]{{/each}})

{{/each}}
    -- Create a new envelope where not processing an existing.
    IF (@EnvelopeId IS NULL)
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
    SELECT [_outbox].[EnvelopeId], [_outbox].[CreatedDate], [_outbox].[IsComplete], [_outbox].[CompletedDate]
      FROM [{{CdcSchema}}].[{{EnvelopeTableName}}] AS [_outbox]
      WHERE [_outbox].EnvelopeId = @EnvelopeId 

    -- Root table: {{Schema}}.{{Name}} - uses LEFT OUTER JOIN so we get the deleted records too.
    SELECT
        [_chg].[_Op] AS [_OperationType],
{{#each PrimaryKeyColumns}}
        [_chg].[{{Name}}] AS [{{NameAlias}}]{{#ifne Parent.SelectedColumnsExcludingPrimaryKey.Count 0}},{{/ifne}}
{{/each}}
{{#each SelectedColumnsExcludingPrimaryKey}}
        [{{Parent.Alias}}].[{{Name}}] AS [{{NameAlias}}]{{#unless @last}},{{/unless}}
{{/each}}
      FROM #_changes AS [_chg]
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