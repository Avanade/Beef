CREATE PROCEDURE [Demo].[spWorkHistoryMerge]
   @PersonId AS UNIQUEIDENTIFIER
  ,@List AS [Demo].[udtWorkHistoryList] READONLY
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */
 
  SET NOCOUNT ON;
  
  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

    -- Merge the records.
    MERGE INTO [Demo].[WorkHistory] WITH (HOLDLOCK) AS [t]
      USING @List as [s]
        ON ([t].[Name] = [s].[Name]
        AND [t].[PersonId] = @PersonId)
      WHEN MATCHED
        THEN UPDATE SET
           [t].[Name] = [s].[Name]
          ,[t].[StartDate] = [s].[StartDate]
          ,[t].[EndDate] = [s].[EndDate]
      WHEN NOT MATCHED BY TARGET
        THEN INSERT (
           [PersonId]
          ,[Name]
          ,[StartDate]
          ,[EndDate]
        )
        VALUES (
          @PersonId
         ,[s].[Name]
         ,[s].[StartDate]
         ,[s].[EndDate]
        )
      WHEN NOT MATCHED BY SOURCE
        AND [t].[PersonId] = @PersonId
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