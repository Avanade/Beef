CREATE PROCEDURE [Demo].[spEventOutboxDequeue]
  @MaxDequeueCount INT = 10,  -- Maximum number of events to dequeue.
  @PartitionKey NVARCHAR(128) NULL = NULL  -- Partition key; null indicates all.
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
    WITH cte([EventOutboxId], [PartitionKey], [DequeuedDate]) AS 
    (
       SELECT TOP(@MaxDequeueCount) [EventOutboxId], [PartitionKey], [DequeuedDate]
         FROM [Demo].[EventOutbox] WITH (ROWLOCK, UPDLOCK)
         WHERE [DequeuedDate] IS NULL
           AND (@PartitionKey IS NULL OR [PartitionKey] = @PartitionKey)
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
        [PartitionKey],
        [ValueType], 
        [EventData]
      FROM [Demo].[EventOutboxData]
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