CREATE PROCEDURE [Outbox].[spEventOutboxEnqueue]
  @SetEventsAsDequeued AS BIT = 0,
  @EventList AS [Outbox].[udtEventOutboxList] READONLY
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost.
   */

  SET NOCOUNT ON;

  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

    -- Working variables.
    DECLARE @eventOutboxId BIGINT,
            @enqueuedDate DATETIME,
            @dequeuedDate DATETIME

    SET @enqueuedDate = SYSUTCDATETIME()
    SET @dequeuedDate = @enqueuedDate

    -- Enqueued outbox resultant identifier.
    DECLARE @enqueuedId TABLE([EventOutboxId] BIGINT)

    -- Cursor output variables.
    DECLARE @eventId NVARCHAR(127),
            @eventDequeued BIT,
            @destination NVARCHAR(127),
            @subject NVARCHAR(511),
            @action NVARCHAR(255),
            @type NVARCHAR(1023),
            @source NVARCHAR(1023),
            @timestamp DATETIMEOFFSET,
            @correlationId NVARCHAR(127),
            @tenantId NVARCHAR(127),
            @partitionKey NVARCHAR(127),
            @etag NVARCHAR(127),
            @attributes VARBINARY(MAX),
            @data VARBINARY(MAX)

    -- Declare, open, and fetch first event from cursor.
    DECLARE c CURSOR FORWARD_ONLY
      FOR SELECT [EventId], [EventDequeued], [Destination], [Subject], [Action], [Type], [Source], [Timestamp], [CorrelationId], [TenantId], [PartitionKey], [ETag], [Attributes], [Data] FROM @EventList

    OPEN c
    FETCH NEXT FROM c INTO @eventId, @eventDequeued, @destination, @subject, @action, @type, @source, @timestamp, @correlationId, @tenantId, @partitionKey, @etag, @attributes, @data

    -- Iterate the event(s).
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Enqueue event into outbox
        INSERT INTO [Outbox].[EventOutbox] ([EnqueuedDate], [PartitionKey], [Destination], [DequeuedDate])
          OUTPUT inserted.EventOutboxId INTO @enqueuedId
          VALUES (@enqueuedDate, @partitionKey, @destination, CASE WHEN @eventDequeued IS NULL OR @eventDequeued = 0 THEN NULL ELSE @dequeuedDate END)

        SELECT @eventOutboxId = [EventOutboxId] FROM @enqueuedId

        -- Insert corresponding event data.
        INSERT INTO [Outbox].[EventOutboxData] (
          [EventOutboxDataId],
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
        )
        VALUES (
          @eventOutboxId,
          @eventId,
          @destination,
          @subject,
          @action,
          @type,
          @source,
          @timestamp,
          @correlationId,
          @tenantId,
          @partitionKey,
          @etag,
          @attributes,
          @data
        )

        -- Fetch the next event from the cursor.
        FETCH NEXT FROM c INTO @eventId, @eventDequeued, @destination, @subject, @action, @type, @source, @timestamp, @correlationId, @tenantId, @partitionKey, @etag, @attributes, @data
    END

    -- Close the cursor.
    CLOSE c
    DEALLOCATE c

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