CREATE PROCEDURE [Demo].[spPersonGetDetail]
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
    [p].[UpdatedDate],
    [p].[UniqueCode],
    [p].[EyeColorCode]
    FROM [Demo].[Person] as p