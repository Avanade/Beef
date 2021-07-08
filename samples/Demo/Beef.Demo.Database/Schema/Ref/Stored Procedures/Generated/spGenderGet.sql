CREATE PROCEDURE [Ref].[spGenderGet]
  @GenderId AS UNIQUEIDENTIFIER
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */

  SET NOCOUNT ON;

  -- Execute the primary select query.
  SELECT
      [g].[GenderId],
      [g].[Code],
      [g].[Text],
      [g].[IsActive],
      [g].[SortOrder],
      [g].[RowVersion],
      [g].[CreatedBy],
      [g].[CreatedDate],
      [g].[UpdatedBy],
      [g].[UpdatedDate],
      [g].[AlternateName],
      [g].[TripCode],
      [g].[CountryId]
    FROM [Ref].[Gender] AS [g]
      WHERE [g].[GenderId] = @GenderId
END