CREATE PROCEDURE [Hr].[spEventOutboxEnqueue]
  @EventList AS [Hr].[udtEventOutboxList] READONLY
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
            @enqueuedDate DATETIME

    SET @enqueuedDate = SYSUTCDATETIME()

    -- Enqueued outbox resultant identifier.
    DECLARE @enqueuedIdentity TABLE([EventOutboxId] BIGINT)

    -- Cursor output variables.
    DECLARE @eventId UNIQUEIDENTIFIER,
            @subject NVARCHAR(1024),
            @action NVARCHAR(128),
            @correlationId NVARCHAR(64),
            @TenantId UNIQUEIDENTIFIER,
            @partitionKey NVARCHAR(128),
            @ValueType NVARCHAR(1024),
            @EventData VARBINARY(MAX)

    -- Declare, open, and fetch first event from cursor.
    DECLARE c CURSOR FORWARD_ONLY 
      FOR SELECT [EventId], [Subject], [Action], [CorrelationId], [TenantId], [PartitionKey], [ValueType], [EventData] FROM @EventList

    OPEN c
    FETCH NEXT FROM c INTO @eventId, @subject, @action, @correlationId, @tenantId, @partitionKey, @valueType, @eventdata

    -- Iterate the event(s).
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Enqueue event into outbox 
        INSERT INTO [Hr].[EventOutbox] ([EnqueuedDate])
          OUTPUT inserted.EventOutboxId INTO @enqueuedIdentity 
          VALUES (@enqueuedDate)

        SELECT @eventOutboxId = [EventOutboxId] FROM @enqueuedIdentity

        -- Insert corresponding event data.
        INSERT INTO [Hr].[EventOutboxData] (
          [EventOutboxId],
          [EventId],
          [Subject], 
          [Action], 
          [CorrelationId], 
          [TenantId],
          [PartitionKey], 
          [ValueType], 
          [EventData]
        ) 
        VALUES (
          @eventOutboxId,
          @eventId,
          @subject, 
          @action,
          @correlationId, 
          @tenantId, 
          @partitionKey,
          @valueType, 
          @eventdata
        )

        -- Fetch the next event from the cursor.
        FETCH NEXT FROM c INTO @eventId, @subject, @action, @correlationId, @tenantId, @partitionKey, @valueType, @eventdata
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