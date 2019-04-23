CREATE PROCEDURE [Demo].[spWorkHistoryGetByPersonId]
   @PersonId AS UNIQUEIDENTIFIER
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */
 
  SET NOCOUNT ON;

  -- Select the requested data.
  SELECT
        [wh].[WorkHistoryId]
       ,[wh].[PersonId]
       ,[wh].[Name]
       ,[wh].[StartDate]
       ,[wh].[EndDate]
    FROM [Demo].[WorkHistory] AS [wh]
    WHERE ([wh].[PersonId] = @PersonId)
    ORDER BY [wh].[StartDate] DESC
END