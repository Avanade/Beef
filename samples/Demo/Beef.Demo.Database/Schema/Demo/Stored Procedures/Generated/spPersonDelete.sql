CREATE PROCEDURE [Demo].[spPersonDelete]
   @PersonId AS UNIQUEIDENTIFIER
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */
 
  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

    -- Delete the record.
    DELETE FROM [Demo].[Person]
      WHERE [PersonId] = @PersonId

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