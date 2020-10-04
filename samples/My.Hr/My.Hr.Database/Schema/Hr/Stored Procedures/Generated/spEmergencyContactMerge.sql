CREATE PROCEDURE [Hr].[spEmergencyContactMerge]
  @EmployeeId AS UNIQUEIDENTIFIER,
  @List AS [Hr].[udtEmergencyContactList] READONLY
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */

  SET NOCOUNT ON;

  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

    -- Check valid for merge.
    DECLARE @ListCount INT
    SET @ListCount = (SELECT COUNT(*) FROM @List WHERE [EmergencyContactId] IS NOT NULL AND [EmergencyContactId] <> CONVERT(UNIQUEIDENTIFIER, '00000000-0000-0000-0000-000000000000'))

    DECLARE @RecordCount INT
    SET @RecordCount = (SELECT COUNT(*) FROM @List AS [list]
      INNER JOIN [Hr].[EmergencyContact] AS [ec]
        ON [ec].[EmergencyContactId] = [List].[EmergencyContactId])

    IF @ListCount <> @RecordCount
    BEGIN
      EXEC spThrowConcurrencyException
    END

    -- Merge the records.
    MERGE INTO [Hr].[EmergencyContact] WITH (HOLDLOCK) AS [ec]
      USING @List AS [list]
        ON ([ec].[EmergencyContactId] = [List].[EmergencyContactId])
      WHEN MATCHED AND EXISTS
         (SELECT [list].[FirstName], [list].[LastName], [list].[PhoneNo], [list].[RelationshipTypeCode]
          EXCEPT
          SELECT [ec].[FirstName], [ec].[LastName], [ec].[PhoneNo], [ec].[RelationshipTypeCode])
        THEN UPDATE SET
          [ec].[EmployeeId] = @EmployeeId,
          [ec].[FirstName] = [list].[FirstName],
          [ec].[LastName] = [list].[LastName],
          [ec].[PhoneNo] = [list].[PhoneNo],
          [ec].[RelationshipTypeCode] = [list].[RelationshipTypeCode]
      WHEN NOT MATCHED BY TARGET
        THEN INSERT (
          [EmployeeId],
          [FirstName],
          [LastName],
          [PhoneNo],
          [RelationshipTypeCode]
        ) VALUES (
          @EmployeeId,
          [list].[FirstName],
          [list].[LastName],
          [list].[PhoneNo],
          [list].[RelationshipTypeCode]
        )
      WHEN NOT MATCHED BY SOURCE
        AND [ec].[EmployeeId] = @EmployeeId
        THEN DELETE;
  
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