CREATE PROCEDURE [Demo].[spPersonGetDetail]
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
      [p].[UpdatedDate],
      [p].[UniqueCode],
      [p].[EyeColorCode],
      [p].[MetadataJson]
    FROM [Demo].[Person] AS [p]
      WHERE [p].[PersonId] = @PersonId

  -- Execute additional statements.
  EXEC [Demo].[spWorkHistoryGetByPersonId] @PersonId
END