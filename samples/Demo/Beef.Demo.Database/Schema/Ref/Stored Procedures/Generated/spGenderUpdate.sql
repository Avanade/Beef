CREATE PROCEDURE [Ref].[spGenderUpdate]
  @GenderId AS UNIQUEIDENTIFIER,
  @Code AS NVARCHAR(50),
  @Text AS NVARCHAR(250) NULL = NULL,
  @IsActive AS BIT NULL = NULL,
  @SortOrder AS INT NULL = NULL,
  @RowVersion AS TIMESTAMP,
  @UpdatedBy AS NVARCHAR(250) NULL = NULL,
  @UpdatedDate AS DATETIME2 NULL = NULL,
  @AlternateName AS NVARCHAR(50) NULL = NULL,
  @TripCode AS NVARCHAR(50) NULL = NULL,
  @CountryId AS UNIQUEIDENTIFIER NULL = NULL,
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
    SET @PrevRowVersion = (SELECT TOP 1 [g].[RowVersion] FROM [Ref].[Gender] AS [g] WHERE [g].[GenderId] = @GenderId)
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
    UPDATE [g] SET
        [g].[Code] = @Code,
        [g].[Text] = @Text,
        [g].[IsActive] = @IsActive,
        [g].[SortOrder] = @SortOrder,
        [g].[UpdatedBy] = @UpdatedBy,
        [g].[UpdatedDate] = @UpdatedDate,
        [g].[AlternateName] = @AlternateName,
        [g].[TripCode] = @TripCode,
        [g].[CountryId] = @CountryId
      FROM [Ref].[Gender] AS [g]
      WHERE [g].[GenderId] = @GenderId

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
    EXEC [Ref].[spGenderGet] @GenderId
  END
END