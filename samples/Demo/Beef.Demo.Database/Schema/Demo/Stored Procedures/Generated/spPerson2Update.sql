CREATE OR ALTER PROCEDURE [Demo].[spPerson2Update]
  @PersonId AS UNIQUEIDENTIFIER,
  @FirstName AS NVARCHAR(50) NULL = NULL,
  @LastName AS NVARCHAR(50) NULL = NULL,
  @Birthday AS DATE NULL = NULL,
  @GenderId AS UNIQUEIDENTIFIER NULL = NULL,
  @Street AS NVARCHAR(100) NULL = NULL,
  @City AS NVARCHAR(100) NULL = NULL,
  @RowVersion AS TIMESTAMP,
  @UpdatedBy AS NVARCHAR(250) NULL = NULL,
  @UpdatedDate AS DATETIME2 NULL = NULL,
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

    -- Set audit details.
    EXEC @UpdatedDate = fnGetTimestamp @UpdatedDate
    EXEC @UpdatedBy = fnGetUsername @UpdatedBy

    -- Check exists.
    DECLARE @PrevRowVersion BINARY(8)
    SET @PrevRowVersion = (SELECT TOP 1 [p].[RowVersion] FROM [Demo].[Person2] AS [p] WHERE [p].[PersonId] = @PersonId AND ([p].[IsDeleted] IS NULL OR [p].[IsDeleted] = 0))
    IF @PrevRowVersion IS NULL
    BEGIN
      EXEC spThrowNotFoundException
    END

    -- Check concurrency (where provided).
    IF @RowVersion IS NULL OR @PrevRowVersion <> @RowVersion
    BEGIN
      EXEC spThrowConcurrencyException
    END

    -- Update the record.
    UPDATE [p] SET
        [p].[FirstName] = @FirstName,
        [p].[LastName] = @LastName,
        [p].[Birthday] = @Birthday,
        [p].[GenderId] = @GenderId,
        [p].[Street] = @Street,
        [p].[City] = @City,
        [p].[UpdatedBy] = @UpdatedBy,
        [p].[UpdatedDate] = @UpdatedDate
      FROM [Demo].[Person2] AS [p]
      WHERE [p].[PersonId] = @PersonId
        AND ([p].[IsDeleted] IS NULL OR [p].[IsDeleted] = 0)

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
    EXEC [Demo].[spPerson2Get] @PersonId
  END
END