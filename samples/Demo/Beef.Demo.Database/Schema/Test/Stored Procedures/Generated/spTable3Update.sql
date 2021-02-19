CREATE PROCEDURE [Test].[spTable3Update]
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

    -- Check exists.
    DECLARE @PrevRowCount INT
    SET @PrevRowCount = (SELECT COUNT(*) FROM [Test].[Table3] AS [t] WHERE [t].[PartA] = @PartA AND [t].[PartB] = @PartB)
    IF @PrevRowCount <> 1
    BEGIN
      EXEC spThrowNotFoundException
    END

    -- Update the record.
    UPDATE [t] SET
        [t].[Name] = @Name,
        [t].[Count] = @Count
      FROM [Test].[Table3] AS [t]
      WHERE [t].[PartA] = @PartA
        AND [t].[PartB] = @PartB

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