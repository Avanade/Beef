CREATE PROCEDURE [DemoCdc].[spCompletePerson2CdcOutbox]
  @OutboxId INT NULL = NULL,
  @TrackingList AS [DemoCdc].[udtCdcTrackingList] READONLY
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
      FROM [DemoCdc].[Person2Outbox] AS [_outbox]
      WHERE OutboxId = @OutboxId 

    DECLARE @Msg NVARCHAR(256)
    IF @@ROWCOUNT <> 1
    BEGIN
      SET @Msg = CONCAT('Outbox ''', @OutboxId, ''' cannot be completed as it does not exist.');
      THROW 56005, @Msg, 1;
    END

    IF @IsCompleteAlready = 1
    BEGIN
      SET @Msg = CONCAT('Outbox ''', @OutboxId, ''' is already complete; cannot be completed more than once.');
      THROW 56002, @Msg, 1;
    END

    UPDATE [_outbox] SET
        [_outbox].[IsComplete] = 1,
        [_outbox].[CompletedDate] = GETUTCDATE()
      FROM [DemoCdc].[Person2Outbox] AS [_outbox]
      WHERE OutboxId = @OutboxId 

    MERGE INTO [DemoCdc].[CdcTracking] WITH (HOLDLOCK) AS [_ct]
      USING @TrackingList AS [_list] ON ([_ct].[Schema] = 'Demo' AND [_ct].[Table] = 'Person2' AND [_ct].[Key] = [_list].[Key])
      WHEN MATCHED AND EXISTS (
          SELECT [_list].[Key], [_list].[Hash]
          EXCEPT
          SELECT [_ct].[Key], [_ct].[Hash])
        THEN UPDATE SET [_ct].[Hash] = [_list].[Hash], [_ct].[OutboxId] = @OutboxId
      WHEN NOT MATCHED BY TARGET
        THEN INSERT ([Schema], [Table], [Key], [Hash], [OutboxId])
          VALUES ('Demo', 'Person2', [_list].[Key], [_list].[Hash], @OutboxId);

    SELECT [_outbox].[OutboxId], [_outbox].[CreatedDate], [_outbox].[IsComplete], [_outbox].[CompletedDate], [_outbox].[HasDataLoss]
      FROM [DemoCdc].[Person2Outbox] AS [_outbox]
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