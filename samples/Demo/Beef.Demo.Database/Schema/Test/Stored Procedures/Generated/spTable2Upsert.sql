CREATE OR ALTER PROCEDURE [Test].[spTable2Upsert]
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

    -- Check exists.
    DECLARE @PrevRowCount INT
    SET @PrevRowCount = (SELECT COUNT(*) FROM [Test].[Table2] AS [t] WHERE [t].[Table2Id] = @Table2Id)
    IF @PrevRowCount <> 1
    BEGIN
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
    END
    ELSE
    BEGIN
      -- Update the record.
      UPDATE [t] SET
        [t].[Name] = @Name,
        [t].[Count] = @Count
        FROM [Test].[Table2] AS [t]
        WHERE [t].[Table2Id] = @Table2Id
    END

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