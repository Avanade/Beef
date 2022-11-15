CREATE PROCEDURE [Test].[spTable3Delete]
  @PartA AS NVARCHAR(10),
  @PartB AS NVARCHAR(10)
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
    DELETE [t] FROM [Test].[Table3] AS [t]
      WHERE [t].[PartA] = @PartA
        AND [t].[PartB] = @PartB

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