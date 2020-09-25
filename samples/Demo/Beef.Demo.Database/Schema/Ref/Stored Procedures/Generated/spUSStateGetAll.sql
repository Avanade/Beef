CREATE PROCEDURE [Ref].[spUSStateGetAll]
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */

  SET NOCOUNT ON;

  -- Select the requested data.
  SELECT
      [us].[USStateId],
      [us].[Code],
      [us].[Text],
      [us].[IsActive],
      [us].[SortOrder],
      [us].[RowVersion],
      [us].[CreatedBy],
      [us].[CreatedDate],
      [us].[UpdatedBy],
      [us].[UpdatedDate]
    FROM [Ref].[USState] AS [us]
    ORDER BY [us].[SortOrder] ASC, [us].[Code] ASC
END