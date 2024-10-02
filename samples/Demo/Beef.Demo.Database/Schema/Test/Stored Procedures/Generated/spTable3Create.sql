CREATE OR ALTER PROCEDURE [Test].[spTable3Create]
  @PartA AS NVARCHAR(10),
  @PartB AS NVARCHAR(10),
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
    INSERT INTO [Test].[Table3] (
      [PartA],
      [PartB],
      [Name],
      [Count]
    )
    VALUES (
      @PartA,
      @PartB,
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
    EXEC [Test].[spTable3Get] @PartA, @PartB
  END
END