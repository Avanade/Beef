CREATE PROCEDURE [Ref].[spGenderGetAll]
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */

  SET NOCOUNT ON;

  -- Select the requested data.
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
    ORDER BY [g].[SortOrder] ASC, [g].[Code] ASC
END