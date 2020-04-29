CREATE PROCEDURE [Ref].[spGenderGet]
   @GenderId AS UNIQUEIDENTIFIER
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */
 
  SET NOCOUNT ON;

  SELECT
        [g].[GenderId]
       ,[g].[Code]
       ,[g].[Text]
       ,[g].[IsActive]
       ,[g].[SortOrder]
       ,[g].[RowVersion]
       ,[g].[CreatedBy]
       ,[g].[CreatedDate]
       ,[g].[UpdatedBy]
       ,[g].[UpdatedDate]
       ,[g].[AlternateName]
       ,[g].[TripCode]
    FROM [Ref].[Gender] AS [g]
    WHERE [g].[GenderId] = @GenderId
END