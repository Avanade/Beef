CREATE PROCEDURE [Hr].[spEventOutboxDequeue]
  @MaxDequeueCount INT = 10  -- Maximum number of events to dequeue
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost.
   */
 
  SET NOCOUNT ON;

  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

    -- Dequeued outbox resultant identifier.
    DECLARE @dequeuedIdentity TABLE([EventOutboxId] BIGINT);

    -- Dequeue event -> ROWLOCK+UPDLOCK maintain singular access for ordering and concurrency
    WITH cte([EventOutboxId], [DequeuedDate]) AS 
    (
       SELECT TOP(@MaxDequeueCount) [EventOutboxId], [DequeuedDate]
         FROM [Hr].[EventOutbox] WITH (ROWLOCK, UPDLOCK)
         WHERE [DequeuedDate] IS NULL
         ORDER BY [EventOutboxId]
    ) 
    UPDATE Cte
      SET [DequeuedDate] = SYSUTCDATETIME()
      OUTPUT deleted.EventOutboxId INTO @dequeuedIdentity;

    -- Get the dequeued event outbox data.
    SELECT
        [EventOutboxId],
        [EventId],
        [Subject], 
        [Action], 
        [CorrelationId], 
        [TenantId], 
        [ValueType], 
        [EventData]
      FROM [Hr].[EventOutboxData]
      WHERE [EventOutboxId] IN (SELECT [EventOutboxId] FROM @dequeuedIdentity)

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