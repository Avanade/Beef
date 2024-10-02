CREATE OR ALTER PROCEDURE [Demo].[spWorkHistoryMerge]
  @PersonId AS UNIQUEIDENTIFIER,
  @List AS [Demo].[udtWorkHistoryList] READONLY
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
    MERGE INTO [Demo].[WorkHistory] WITH (HOLDLOCK) AS [wh]
      USING @List AS [list]
        ON ([wh].[Name] = [List].[Name]
        AND [wh].[PersonId] = @PersonId)
      WHEN MATCHED AND EXISTS
         (SELECT [list].[Name], [list].[StartDate], [list].[EndDate]
          EXCEPT
          SELECT [wh].[Name], [wh].[StartDate], [wh].[EndDate])
        THEN UPDATE SET
          [wh].[PersonId] = @PersonId,
          [wh].[Name] = [list].[Name],
          [wh].[StartDate] = [list].[StartDate],
          [wh].[EndDate] = [list].[EndDate]
      WHEN NOT MATCHED BY TARGET
        THEN INSERT (
          [PersonId],
          [Name],
          [StartDate],
          [EndDate]
        ) VALUES (
          @PersonId,
          [list].[Name],
          [list].[StartDate],
          [list].[EndDate]
        )
      WHEN NOT MATCHED BY SOURCE
        AND [wh].[PersonId] = @PersonId
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