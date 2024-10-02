CREATE OR ALTER PROCEDURE [Test].[spTestQueryGetByArgs]
  @Name AS NVARCHAR(50) NULL = NULL
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */

  SET NOCOUNT ON;

  -- Select the requested data.
  SELECT
      [vtq].[TableId],
      [vtq].[Name],
      [vtq].[Count],
      [vtq].[Amount],
      [vtq].[Other],
      [vtq].[GenderCode],
      [vtq].[OrgUnitId],
      [vtq].[RowVersion],
      [vtq].[CreatedDate],
      [vtq].[UpdatedDate],
      [vtq].[PersonId],
      [vtq].[FirstName],
      [vtq].[LastName],
      [vtq].[Birthday],
      [vtq].[GenderId],
      [vtq].[Street],
      [vtq].[City],
      [vtq].[RowVersionP],
      [vtq].[CreatedBy],
      [vtq].[UpdatedBy],
      [vtq].[UniqueCode],
      [vtq].[EyeColorCode],
      [vtq].[MetadataJson]
    FROM [Test].[vwTestQuery] AS [vtq]
    WHERE (@Name IS NULL OR [vtq].[Name] LIKE @Name)
END