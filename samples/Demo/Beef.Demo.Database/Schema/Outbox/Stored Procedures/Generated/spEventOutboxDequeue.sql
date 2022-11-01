CREATE PROCEDURE [Outbox].[spEventOutboxDequeue]
  @MaxDequeueSize INT = 10,  -- Maximum number of events to dequeue.
  @PartitionKey NVARCHAR(127) NULL = NULL,  -- Partition key; null indicates all.
  @Destination NVARCHAR(127) NULL = NULL  -- Destination (queue or topic); null indicates all.
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
    DECLARE @dequeuedId TABLE([EventOutboxId] BIGINT);

    -- Dequeue event -> ROWLOCK+UPDLOCK maintain singular access for ordering and concurrency
    WITH cte([EventOutboxId], [PartitionKey], [Destination], [DequeuedDate]) AS
    (
       SELECT TOP(@MaxDequeueSize) [EventOutboxId], [PartitionKey], [Destination], [DequeuedDate]
         FROM [Outbox].[EventOutbox] WITH (ROWLOCK, UPDLOCK)
         WHERE [DequeuedDate] IS NULL
           AND (@PartitionKey IS NULL OR [PartitionKey] = @PartitionKey)
           AND (@Destination IS NULL OR [Destination] = @Destination)
         ORDER BY [EventOutboxId]
    )
    UPDATE Cte
      SET [DequeuedDate] = SYSUTCDATETIME()
      OUTPUT deleted.EventOutboxId INTO @dequeuedId;

    -- Get the dequeued event outbox data.
    SELECT
        [EventOutboxDataId] as [EventOutboxId],
        [EventId],
        [Destination],
        [Subject],
        [Action],
        [Type],
        [Source],
        [Timestamp],
        [CorrelationId],
        [TenantId],
        [PartitionKey],
        [ETag],
        [Attributes],
        [Data]
      FROM [Outbox].[EventOutboxData]
      WHERE [EventOutboxDataId] IN (SELECT [EventOutboxId] FROM @dequeuedId)

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