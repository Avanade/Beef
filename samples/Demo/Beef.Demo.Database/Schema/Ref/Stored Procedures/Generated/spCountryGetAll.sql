CREATE PROCEDURE [Ref].[spCountryGetAll]
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */

  SET NOCOUNT ON;

  -- Select the requested data.
  SELECT
      [c].[CountryId],
      [c].[Code],
      [c].[Text],
      [c].[IsActive],
      [c].[SortOrder],
      [c].[RowVersion],
      [c].[CreatedBy],
      [c].[CreatedDate],
      [c].[UpdatedBy],
      [c].[UpdatedDate]
    FROM [Ref].[Country] AS [c]
    ORDER BY [c].[SortOrder] ASC, [c].[Code] ASC
END