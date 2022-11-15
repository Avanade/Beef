CREATE PROCEDURE [Test].[spTable2Delete]
  @Table2Id AS UNIQUEIDENTIFIER
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
    DELETE [t] FROM [Test].[Table2] AS [t]
      WHERE [t].[Table2Id] = @Table2Id

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