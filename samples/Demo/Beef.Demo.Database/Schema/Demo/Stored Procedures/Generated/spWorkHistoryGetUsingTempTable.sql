CREATE OR ALTER PROCEDURE [Demo].[spWorkHistoryGetUsingTempTable]
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */

  SET NOCOUNT ON;

  -- Select the requested data.
  SELECT
      [wh].[WorkHistoryId],
      [wh].[PersonId],
      [wh].[Name],
      [wh].[StartDate],
      [wh].[EndDate]
    FROM [Demo].[WorkHistory] AS [wh]
    WHERE [PersonId] IN (SELECT [PersonId] from #p)
    ORDER BY [wh].[PersonId] ASC, [wh].[StartDate] DESC
END