CREATE OR ALTER PROCEDURE [Hr].[spEmployeeCreate]
  @EmployeeId AS UNIQUEIDENTIFIER = NULL OUTPUT,
  @Email AS NVARCHAR(250) NULL = NULL,
  @FirstName AS NVARCHAR(100) NULL = NULL,
  @LastName AS NVARCHAR(100) NULL = NULL,
  @GenderCode AS NVARCHAR(50) NULL = NULL,
  @Birthday AS DATE NULL = NULL,
  @StartDate AS DATE NULL = NULL,
  @TerminationDate AS DATE NULL = NULL,
  @TerminationReasonCode AS NVARCHAR(50) NULL = NULL,
  @PhoneNo AS NVARCHAR(50) NULL = NULL,
  @AddressJson AS NVARCHAR(500) NULL = NULL,
  @CreatedBy AS NVARCHAR(250) NULL = NULL,
  @CreatedDate AS DATETIME2 NULL = NULL,
  @EmergencyContactList AS NVARCHAR(MAX),
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
    DECLARE @InsertedIdentity TABLE([EmployeeId] UNIQUEIDENTIFIER)

    INSERT INTO [Hr].[Employee] (
      [Email],
      [FirstName],
      [LastName],
      [GenderCode],
      [Birthday],
      [StartDate],
      [TerminationDate],
      [TerminationReasonCode],
      [PhoneNo],
      [AddressJson],
      [CreatedBy],
      [CreatedDate]
    )
    OUTPUT inserted.EmployeeId INTO @InsertedIdentity
    VALUES (
      @Email,
      @FirstName,
      @LastName,
      @GenderCode,
      @Birthday,
      @StartDate,
      @TerminationDate,
      @TerminationReasonCode,
      @PhoneNo,
      @AddressJson,
      @CreatedBy,
      @CreatedDate
    )

    -- Get the inserted identity.
    SELECT @EmployeeId = [EmployeeId] FROM @InsertedIdentity

    -- Execute additional statements.
    EXEC [Hr].[spEmergencyContactMerge] @EmployeeId, @EmergencyContactList

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
    EXEC [Hr].[spEmployeeGet] @EmployeeId
  END
END