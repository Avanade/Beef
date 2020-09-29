CREATE TYPE [Demo].[udtWorkHistoryList] AS TABLE (
  /*
   * This is automatically generated; any changes will be lost. 
   */

  [WorkHistoryId] UNIQUEIDENTIFIER,
  [Name] NVARCHAR(100),
  [StartDate] DATE,
  [EndDate] DATE NULL
)