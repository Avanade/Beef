CREATE PROCEDURE [Test].[spTable2Create]
  @Table2Id AS UNIQUEIDENTIFIER,
  @Name AS NVARCHAR(50) NULL = NULL,
  @Count AS INT NULL = NULL,
  @ReselectRecord AS BIT = 0
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */

  SET NOCOUNT ON;

  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

    -- Create the record.
    INSERT INTO [Test].[Table2] (
      [Table2Id],
      [Name],
      [Count]
    )
    VALUES (
      @Table2Id,
      @Name,
      @Count
    )

    -- Commit the transaction.
    COMMIT TRANSACTION
  END TRY
  BEGIN CATCH
    -- Rollback transaction and rethrow error.
    IF @@TRANCOUNT > 0
      ROLLBACK TRANSACTION;

    THROW;
  END CATCH
  
  -- Reselect record.
  IF @ReselectRecord = 1
  BEGIN
    EXEC [Test].[spTable2Get] @Table2Id
  END
END