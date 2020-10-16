{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
CREATE PROCEDURE [{{CdcSchema}}].[{{CdcName}}]
  @BatchIdToMarkComplete INT,
  @ReturnIncompleteBatches BIT,
  @MaxBatchSize int = 100
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost.
   */

  SET NOCOUNT ON;

  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

    -- Mark the batch as complete first.
    IF (@BatchIdToMarkComplete IS NOT NULL)
    BEGIN
	  UPDATE [{{CdcSchema}}].[{{CdcEnvelope}}] SET
        HasbeenCompleted = 1,
        ProcessedDate = GETUTCDATE()
	    WHERE OutboxEnvelopeId = @BatchIdToMarkComplete 
    END

    -- Where the batch size is 0 then no-op; this is used when closing down the service - invoked to mark the batch complete but _not_ get a new one.
    IF (@MaxBatchSize = 0) 
    BEGIN
	  COMMIT TRANSACTION
	  RETURN 0;
    END

    -- Declare required variables.
    DECLARE @existingBatchId INT 
    DECLARE @min{{Name}}Lsn BINARY(10)
{{#each CdcJoins}}
    DECLARE @min{{Name}}Lsn BINARY(10)
{{/each}}
    DECLARE @maxlsn BINARY(10)

    -- This is a flag to specify if we need the min change. If this is a new batch, we want the change AFTER this change as this is set to the highest in the batch before.
    -- If this is a re-get of an existing batch, then we need to return it as its the first item in the batch
    DECLARE @includeMin BIT
    SET @includeMin = 0

    -- Where asking for any inprogress batches, we just need to find those and return them. 
    IF (@ReturnIncompleteBatches = 1)
    BEGIN
	  -- Need to find the existing batch to return 
	  SELECT TOP 1 
          @existingBatchId = [o].[OutboxEnvelopeId],
          @min{{Name}}Lsn = [o].[FirstProcessedLSN],
{{#each CdcJoins}}
          @min{{Name}}Lsn = [o].[FirstProcessedLSN],
{{/each}}
		  @maxlsn = [o].[LastProcessedLSN]
        FROM [{{CdcSchema}}].[{{CdcEnvelope}}] AS [o] 
        WHERE [o].[HasBeenCompleted] = 0 
        ORDER BY [o].[OutboxEnvelopeId]

      -- Where a batch is found then return
	  IF @existingBatchId IS NOT NULL
	  BEGIN
		-- Get the batch information from the table as this forms our first result set.
		SELECT * FROM [{{CdcSchema}}].[{{CdcEnvelope}}] AS [o] WHERE [o].[OutboxEnvelopeId] = @ExistingBatchId

		-- Mark the flag to include the first item in the batch too.
		SET @includeMin = 1
	  END
    END

    -- Where null then there isnt an existing batch, so we need to get these numbers.
    IF @min{{Name}}Lsn IS NULL 
    BEGIN
	  -- Get them from the last batch generated.
	  SELECT TOP 1
          @min{{Name}}Lsn = [o].[LastProcessedLSN]{{#ifne Joins.Count 0}},{{/ifne}}
{{#each CdcJoins}}
          @min{{Name}}Lsn = [o].[LastProcessedLSN]{{#unless @last}},{{/unless}}
{{/each}}
	    FROM [{{CdcSchema}}].[{{CdcEnvelope}}] AS [o] 
		ORDER BY [o].[OutBoxEnvelopeId] DESC

	  -- Where they are still NULL then this is the first batch ever, get them from the transaciton log.
	  IF @minAddressLsn IS NULL 
	  BEGIN
	    SET @min{{Name}}Lsn = sys.fn_cdc_get_min_lsn('{{Schema}}_{{Name}}') 
{{#each CdcJoins}}
	    SET @min{{Name}}Lsn = sys.fn_cdc_get_min_lsn('{{Schema}}_{{Name}}') 
{{/each}}

	    -- Also include the first item here, because it should be in this batch.
	    SET @includeMin = 1
	  END

	  -- Get the current max LSN from the transaction log
	  SET @maxlsn = sys.fn_cdc_get_max_lsn()  
    END

    -- Do not go larger than the max batch size; put into a temp table so we can aggregate it later.
    SELECT TOP (@MaxBatchSize) * 
	INTO #changes
    FROM 
	(
        -- Get the {{QualifiedName}} changes.
        SELECT TOP (@MaxBatchSize)
            '{{Name}}' AS [ChangeTable],
            [_chg].[__$start_lsn] AS [start_lsn],
            [_chg].[__$operation],
            [_chg].[__$update_mask],
{{#each SelectedColumns}}
            {{QualifiedNameWithAlias}}{{#unless @last}},{{/unless}}
{{/each}}
          FROM cdc.fn_cdc_get_net_changes_{{lower Schema}}_{{lower Name}}(@min{{Name}}Lsn, @maxlsn, 'all') AS [_chg]
          INNER JOIN {{QualifiedName}} AS [{{Alias}}] ON ({{#each PrimaryKeyColumns}}{{#unless @first}} AND {{/unless}}[{{Parent.Alias}}].[{{Name}}] = [_chg].[{{Name}}]{{/each}})
{{#each Joins}}
          {{JoinTypeSql}} {{QualifiedName}} AS [{{Alias}}] ON ({{#each On}}{{#unless @first}} AND {{/unless}}{{JoinOnSql}}{{/each}})
{{/each}}
          WHERE (@includeMin = 1 OR [_chg].[__$start_lsn] > @min{{Name}}Lsn)
          ORDER BY [_chg].[__$start_lsn] ASC
{{#each CdcJoins}}
      UNION
        -- Get the {{QualifiedName}} changes.
        SELECT TOP (@MaxBatchSize)
            '{{Name}}' AS [ChangeTable],
            [_chg].[__$start_lsn] AS [start_lsn],
            [_chg].[__$operation],
            [_chg].[__$update_mask],
  {{#each Parent.SelectedColumns}}
            {{QualifiedNameWithAlias}}{{#unless @last}},{{/unless}}
  {{/each}}
          FROM cdc.fn_cdc_get_net_changes_{{lower Schema}}_{{lower Name}}(@min{{Name}}Lsn, @maxlsn, 'all') AS [_chg]
          INNER JOIN {{QualifiedName}} AS [{{Alias}}] ON ({{#each PrimaryKeyColumns}}{{#unless @first}} AND {{/unless}}[{{Parent.Alias}}].[{{Name}}] = [_chg].[{{Name}}]{{/each}})
          INNER JOIN {{Parent.QualifiedName}} AS [{{Parent.Alias}}] ON ({{#each On}}{{#unless @first}} AND {{/unless}}{{JoinOnSql}}{{/each}})
  {{#each OtherJoins}}
          {{JoinTypeSql}} {{QualifiedName}} AS [{{Alias}}] ON ({{#each On}}{{#unless @first}} AND {{/unless}}{{JoinOnSql}}{{/each}})
  {{/each}}
          WHERE (@includeMin = 1 OR [_chg].[__$start_lsn] > @min{{Name}}Lsn)
          ORDER BY [_chg].[__$start_lsn] ASC
{{/each}}
    ) AS [_changes]
    ORDER BY start_lsn ASC -- We order by as we're taking TOP. We dont want to skip changes so we need them in LSN order

    -- Where there are changes we can make a batch, otherwise we just need to return the empty results.
    IF EXISTS (SELECT TOP 1 * from #changes)
    BEGIN
	  -- If we need to create a new batch coz we arent returning an old one.
	  IF ( @ExistingBatchId   IS NULL)
	  BEGIN
	    DECLARE @MinBatchLSN BINARY(10)
	    DECLARE @MaxBatchLSN BINARY(10)

	    -- Figure out the min and MAX LSN in this batch.
	    SELECT @MinBatchLSN = MIN( start_lsn), @MaxBatchLSN = MAX( start_lsn)
	      FROM #changes

	    -- Create the batch record in the table.
	    INSERT INTO [{{CdcSchema}}].[{{CdcEnvelope}}] (
		  [CreatedDate], 
          [FirstProcessedLSN],
          [LastProcessedLSN],
          [HasBeenCompleted]
        ) 
		VALUES (
		  GETUTCDATE(),
          @MinBatchLSN,
          @MaxBatchLSN,
          0
        )

	    -- Return as the first result set. If we arent making a new batch then we already returned it.
	    SELECT * FROM [{{CdcSchema}}].[{{CdcEnvelope}}] WHERE [OutboxEnvelopeId] = @@IDENTITY
	  END
    END
    ELSE
    BEGIN
	  -- There are no changes so just return an empty result set.
	  SELECT TOP 0 * FROM [{{CdcSchema}}].[{{CdcEnvelope}}]
    END

    -- Return the changes that we have.
    SELECT DISTINCT 
{{#each SelectedColumns}}
      [{{NameAlias}}]{{#unless @last}},{{/unless}}
{{/each}}
    FROM #changes

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