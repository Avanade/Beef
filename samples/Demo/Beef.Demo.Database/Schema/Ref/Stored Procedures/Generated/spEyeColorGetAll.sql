CREATE PROCEDURE [Ref].[spEyeColorGetAll]
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */
 
  SET NOCOUNT ON;

  -- Select the requested data.
  SELECT
        [ec].[EyeColorId]
       ,[ec].[Code]
       ,[ec].[Text]
       ,[ec].[IsActive]
       ,[ec].[SortOrder]
       ,[ec].[RowVersion]
       ,[ec].[CreatedBy]
       ,[ec].[CreatedDate]
       ,[ec].[UpdatedBy]
       ,[ec].[UpdatedDate]
    FROM [Ref].[EyeColor] AS [ec]
    ORDER BY [ec].[SortOrder] ASC, [ec].[Code] ASC
END