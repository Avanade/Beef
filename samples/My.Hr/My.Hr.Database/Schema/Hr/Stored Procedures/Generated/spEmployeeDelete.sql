CREATE OR ALTER PROCEDURE [Hr].[spEmployeeDelete]
  @EmployeeId AS UNIQUEIDENTIFIER
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */

  SET NOCOUNT ON;

  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

    -- Delete the record.
    DELETE [e] FROM [Hr].[Employee] AS [e]
      WHERE [e].[EmployeeId] = @EmployeeId

    -- Select the row count.
    SELECT @@ROWCOUNT

    -- Execute additional statements.
    DELETE FROM [Hr].[EmergencyContact] WHERE [EmployeeId] = @EmployeeId
    DELETE FROM [Hr].[PerformanceReview] WHERE [EmployeeId] = @EmployeeId

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