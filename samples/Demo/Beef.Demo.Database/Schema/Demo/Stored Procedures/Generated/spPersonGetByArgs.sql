CREATE OR ALTER PROCEDURE [Demo].[spPersonGetByArgs]
  @FirstName AS NVARCHAR(50) NULL = NULL,
  @LastName AS NVARCHAR(50) NULL = NULL,
  @GenderIds AS NVARCHAR(MAX) NULL = NULL, -- JSON Array
  @PagingSkip AS INT = 0,
  @PagingTake AS INT = 250,
  @PagingCount AS BIT = NULL
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */

  SET NOCOUNT ON;

  -- Check list counts.
  DECLARE @GenderIdsCount AS INT
  SET @GenderIdsCount = (SELECT COUNT(*) FROM OPENJSON(@GenderIds))

  -- Select the requested data.
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
    WHERE (@FirstName IS NULL OR [p].[FirstName] LIKE @FirstName)
      AND (@LastName IS NULL OR [p].[LastName] LIKE @LastName)
      AND (@GenderIdsCount = 0 OR [p].[GenderId] IN (SELECT VALUE FROM OPENJSON(@GenderIds)))
    ORDER BY [p].[LastName] ASC, [p].[FirstName] ASC
    OFFSET @PagingSkip ROWS FETCH NEXT @PagingTake ROWS ONLY

  -- Return the full (all pages) row count.
  IF (@PagingCount IS NOT NULL AND @PagingCount = 1)
  BEGIN
    RETURN (SELECT COUNT(*)
      FROM [Demo].[Person] AS [p]
      WHERE (@FirstName IS NULL OR [p].[FirstName] LIKE @FirstName)
        AND (@LastName IS NULL OR [p].[LastName] LIKE @LastName)
        AND (@GenderIdsCount = 0 OR [p].[GenderId] IN (SELECT VALUE FROM OPENJSON(@GenderIds))))
  END
END