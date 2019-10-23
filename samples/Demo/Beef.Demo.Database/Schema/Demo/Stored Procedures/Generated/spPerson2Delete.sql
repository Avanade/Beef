CREATE PROCEDURE [Demo].[spPerson2Delete]
   @PersonId AS UNIQUEIDENTIFIER
  ,@UpdatedBy AS NVARCHAR(250) NULL
  ,@UpdatedDate AS DATETIME2 NULL
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */
 
  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

    -- Set audit details.
    EXEC @UpdatedDate = fnGetTimestamp @UpdatedDate
    EXEC @UpdatedBy = fnGetUsername @UpdatedBy
  
    -- Update the IsDeleted bit (logically delete).
    UPDATE [Demo].[Person2] SET
        [IsDeleted] = 1
       ,[UpdatedDate] = @UpdatedDate
       ,[UpdatedBy] = @UpdatedBy
      WHERE ISNULL([IsDeleted], 0) = 0
        AND [PersonId] = @PersonId

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