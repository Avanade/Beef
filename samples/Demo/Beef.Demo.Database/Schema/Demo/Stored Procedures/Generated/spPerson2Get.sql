CREATE PROCEDURE [Demo].[spPerson2Get]
  @PersonId AS UNIQUEIDENTIFIER
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */

  SET NOCOUNT ON;

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
    FROM [Demo].[Person2] as p