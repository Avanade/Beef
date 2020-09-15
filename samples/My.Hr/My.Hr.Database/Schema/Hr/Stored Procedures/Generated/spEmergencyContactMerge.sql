CREATE PROCEDURE [Hr].[spEmergencyContactMerge]
   @EmployeeId AS UNIQUEIDENTIFIER
  ,@List AS [Hr].[udtEmergencyContactList] READONLY
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
    SET @RecordCount = (SELECT COUNT(*) FROM @List as [list]
      INNER JOIN [Hr].[EmergencyContact] as [ec]
        ON [ec].[EmergencyContactId] = [list].[EmergencyContactId]
        AND [ec].[EmployeeId] = @EmployeeId)
      
    IF @ListCount <> @RecordCount
    BEGIN
      EXEC spThrowConcurrencyException
    END

    -- Merge the records.
    MERGE INTO [Hr].[EmergencyContact] WITH (HOLDLOCK) AS [t]
      USING @List as [s]
        ON ([t].[EmergencyContactId] = [s].[EmergencyContactId]
        AND [t].[EmployeeId] = @EmployeeId)
      WHEN MATCHED AND EXISTS
          (SELECT [s].[FirstName], [s].[LastName], [s].[PhoneNo], [s].[RelationshipTypeCode]
           EXCEPT
           SELECT [t].[FirstName], [t].[LastName], [t].[PhoneNo], [t].[RelationshipTypeCode])
        THEN UPDATE SET
           [t].[FirstName] = [s].[FirstName]
          ,[t].[LastName] = [s].[LastName]
          ,[t].[PhoneNo] = [s].[PhoneNo]
          ,[t].[RelationshipTypeCode] = [s].[RelationshipTypeCode]
      WHEN NOT MATCHED BY TARGET
        THEN INSERT (
           [EmployeeId]
          ,[FirstName]
          ,[LastName]
          ,[PhoneNo]
          ,[RelationshipTypeCode]
        )
        VALUES (
          @EmployeeId
         ,[s].[FirstName]
         ,[s].[LastName]
         ,[s].[PhoneNo]
         ,[s].[RelationshipTypeCode]
        )
      WHEN NOT MATCHED BY SOURCE
        AND [t].[EmployeeId] = @EmployeeId
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