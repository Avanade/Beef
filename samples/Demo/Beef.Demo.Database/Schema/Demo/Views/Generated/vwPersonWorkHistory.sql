CREATE VIEW [Demo].[vwPersonWorkHistory]
AS
  /*
   * This is automatically generated; any changes will be lost.
   */

  SELECT
      [p].[PersonId],
      [p].[FirstName],
      [p].[LastName],
      [p].[Birthday],
      [p].[GenderId],
      [p].[Street],
      [p].[City],
      [p].[RowVersion],
      [p].[CreatedBy],
      [p].[CreatedDate],
      [p].[UpdatedBy],
      [p].[UpdatedDate],
      [p].[UniqueCode],
      [p].[EyeColorCode],
      [wh].[WorkHistoryId],
      [wh].[Name],
      [wh].[StartDate],
      [wh].[EndDate]
    FROM [Demo].[Person] AS [p]
      INNER JOIN [Demo].[WorkHistory] AS [wh] ON ([wh].[PersonId] = [p].[PersonId])
