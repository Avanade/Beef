CREATE PROCEDURE [Demo].[spPerson2Get]
  @PersonId AS UNIQUEIDENTIFIER
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */

  SET NOCOUNT ON;

  -- Execute the primary select query.
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
      WHERE [p].[PersonId] = @PersonId
        AND ISNULL([p].[IsDeleted], 0) = 0
END