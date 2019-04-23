CREATE PROCEDURE [Demo].[spPersonGetAll]
   @PagingSkip AS INT = 0
  ,@PagingTake AS INT = 250
  ,@PagingCount AS BIT = NULL
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */
 
  SET NOCOUNT ON;

  -- Select the requested data.
  SELECT
        [p].[PersonId]
       ,[p].[FirstName]
       ,[p].[LastName]
       ,[p].[Birthday]
       ,[p].[GenderId]
       ,[p].[Street]
       ,[p].[City]
       ,[p].[RowVersion]
       ,[p].[CreatedBy]
       ,[p].[CreatedDate]
       ,[p].[UpdatedBy]
       ,[p].[UpdatedDate]
       ,[p].[UniqueCode]
    FROM [Demo].[Person] AS [p]
    ORDER BY [p].[LastName] ASC, [p].[FirstName] ASC
      OFFSET @PagingSkip ROWS FETCH NEXT @PagingTake ROWS ONLY

  -- Return the full (all pages) row count.
  IF (@PagingCount IS NOT NULL AND @PagingCount = 1)
  BEGIN
    RETURN (SELECT COUNT(*)
      FROM [Demo].[Person] AS [p])
  END
END