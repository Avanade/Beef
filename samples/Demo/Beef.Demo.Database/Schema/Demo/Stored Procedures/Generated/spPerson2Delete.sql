CREATE PROCEDURE [Demo].[spPerson2Delete]
  @PersonId AS UNIQUEIDENTIFIER,
  @UpdatedBy AS NVARCHAR(250) NULL = NULL,
  @UpdatedDate AS DATETIME2 NULL = NULL
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */

  SET NOCOUNT ON;

  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

    -- Set audit details.
    EXEC @UpdatedDate = fnGetTimestamp @UpdatedDate
    EXEC @UpdatedBy = fnGetUsername @UpdatedBy

    -- Update the IsDeleted bit (logically delete).
    UPDATE [p] SET
        [p].[IsDeleted] = 1,
        [p].[UpdatedBy] = @UpdatedBy,
        [p].[UpdatedDate] = @UpdatedDate
      FROM [Demo].[Person2] AS [p]
      WHERE [p].[PersonId] = @PersonId
        AND ([p].[IsDeleted] IS NULL OR [p].[IsDeleted] = 0)

    -- Select the row count.
    SELECT @@ROWCOUNT

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