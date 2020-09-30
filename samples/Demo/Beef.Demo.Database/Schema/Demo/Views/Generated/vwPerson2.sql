CREATE VIEW [Demo].[vwPerson2]
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
    [p].[UpdatedDate]
    FROM [Demo].[Person2] AS [p]
