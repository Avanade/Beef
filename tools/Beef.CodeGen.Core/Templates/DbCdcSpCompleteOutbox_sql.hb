{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
CREATE PROCEDURE [{{CdcSchema}}].[{{CompleteStoredProcedureName}}]
  @OutboxId INT NULL = NULL,
  @TrackingList AS [{{Root.CdcSchema}}].[udt{{Root.CdcTrackingTableName}}List] READONLY
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
    DECLARE @IsCompleteAlready BIT
    SELECT @IsCompleteAlready = [_outbox].[IsComplete]
      FROM [{{CdcSchema}}].[{{OutboxTableName}}] AS [_outbox]
      WHERE OutboxId = @OutboxId 

    DECLARE @Msg NVARCHAR(256)
    IF @@ROWCOUNT <> 1
    BEGIN
      SET @Msg = CONCAT('Outbox ''', @OutboxId, ''' cannot be completed as it does not exist.');
      {{#if Root.HasBeefDbo}}EXEC spThrowNotFoundException {{else}}THROW 56005, {{/if}}@Msg{{#unless Root.HasBeefDbo}}, 1;{{/unless}}
    END

    IF @IsCompleteAlready = 1
    BEGIN
      SET @Msg = CONCAT('Outbox ''', @OutboxId, ''' is already complete; cannot be completed more than once.');
      {{#if Root.HasBeefDbo}}EXEC spThrowBusinessException {{else}}THROW 56002, {{/if}}@Msg{{#unless Root.HasBeefDbo}}, 1;{{/unless}}
    END

    UPDATE [_outbox] SET
        [_outbox].[IsComplete] = 1,
        [_outbox].[CompletedDate] = GETUTCDATE()
      FROM [{{CdcSchema}}].[{{OutboxTableName}}] AS [_outbox]
      WHERE OutboxId = @OutboxId 

    MERGE INTO [{{Root.CdcSchema}}].[{{Root.CdcTrackingTableName}}] WITH (HOLDLOCK) AS [_ct]
      USING @TrackingList AS [_list] ON ([_ct].[Schema] = '{{Schema}}' AND [_ct].[Table] = '{{Table}}' AND [_ct].[Key] = [_list].[Key])
      WHEN MATCHED AND EXISTS (
          SELECT [_list].[Key], [_list].[Hash]
          EXCEPT
          SELECT [_ct].[Key], [_ct].[Hash])
        THEN UPDATE SET [_ct].[Hash] = [_list].[Hash], [_ct].[OutboxId] = @OutboxId
      WHEN NOT MATCHED BY TARGET
        THEN INSERT ([Schema], [Table], [Key], [Hash], [OutboxId])
          VALUES ('{{Schema}}', '{{Table}}', [_list].[Key], [_list].[Hash], @OutboxId);

    SELECT [_outbox].[OutboxId], [_outbox].[CreatedDate], [_outbox].[IsComplete], [_outbox].[CompletedDate], [_outbox].[HasDataLoss]
      FROM [{{CdcSchema}}].[{{OutboxTableName}}] AS [_outbox]
      WHERE [_outbox].OutboxId = @OutboxId

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