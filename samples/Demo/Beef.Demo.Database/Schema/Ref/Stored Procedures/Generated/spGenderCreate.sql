CREATE PROCEDURE [Ref].[spGenderCreate]
  @GenderId AS UNIQUEIDENTIFIER = NULL OUTPUT,
  @Code AS NVARCHAR(50),
  @Text AS NVARCHAR(250) NULL = NULL,
  @IsActive AS BIT NULL = NULL,
  @SortOrder AS INT NULL = NULL,
  @CreatedBy AS NVARCHAR(250) NULL = NULL,
  @CreatedDate AS DATETIME2 NULL = NULL,
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
    EXEC @CreatedDate = fnGetTimestamp @CreatedDate
    EXEC @CreatedBy = fnGetUsername @CreatedBy

    -- Create the record.
    DECLARE @InsertedIdentity TABLE([GenderId] UNIQUEIDENTIFIER)

    INSERT INTO [Ref].[Gender] (
      [Code],
      [Text],
      [IsActive],
      [SortOrder],
      [CreatedBy],
      [CreatedDate],
      [AlternateName],
      [TripCode],
      [CountryId]
    )
    OUTPUT inserted.GenderId INTO @InsertedIdentity
    VALUES (
      @Code,
      @Text,
      @IsActive,
      @SortOrder,
      @CreatedBy,
      @CreatedDate,
      @AlternateName,
      @TripCode,
      @CountryId
    )

    -- Get the inserted identity.
    SELECT @GenderId = [GenderId] FROM @InsertedIdentity

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