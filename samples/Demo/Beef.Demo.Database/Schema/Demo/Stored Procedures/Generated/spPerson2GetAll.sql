CREATE OR ALTER PROCEDURE [Demo].[spPerson2GetAll]
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */

  SET NOCOUNT ON;

  -- Select the requested data.
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
    WHERE ([p].[IsDeleted] IS NULL OR [p].[IsDeleted] = 0)
END