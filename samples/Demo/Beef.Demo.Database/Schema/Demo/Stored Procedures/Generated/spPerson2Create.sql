CREATE OR ALTER PROCEDURE [Demo].[spPerson2Create]
  @PersonId AS UNIQUEIDENTIFIER = NULL OUTPUT,
  @FirstName AS NVARCHAR(50) NULL = NULL,
  @LastName AS NVARCHAR(50) NULL = NULL,
  @Birthday AS DATE NULL = NULL,
  @GenderId AS UNIQUEIDENTIFIER NULL = NULL,
  @Street AS NVARCHAR(100) NULL = NULL,
  @City AS NVARCHAR(100) NULL = NULL,
  @CreatedBy AS NVARCHAR(250) NULL = NULL,
  @CreatedDate AS DATETIME2 NULL = NULL,
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
    DECLARE @InsertedIdentity TABLE([PersonId] UNIQUEIDENTIFIER)

    INSERT INTO [Demo].[Person2] (
      [FirstName],
      [LastName],
      [Birthday],
      [GenderId],
      [Street],
      [City],
      [CreatedBy],
      [CreatedDate]
    )
    OUTPUT inserted.PersonId INTO @InsertedIdentity
    VALUES (
      @FirstName,
      @LastName,
      @Birthday,
      @GenderId,
      @Street,
      @City,
      @CreatedBy,
      @CreatedDate
    )

    -- Get the inserted identity.
    SELECT @PersonId = [PersonId] FROM @InsertedIdentity

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