CREATE OR ALTER PROCEDURE [Test].[spTable2Get]
  @Table2Id AS UNIQUEIDENTIFIER
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */

  SET NOCOUNT ON;

  -- Execute the primary select query.
  SELECT
      [t].[Table2Id],
      [t].[Name],
      [t].[Count]
    FROM [Test].[Table2] AS [t]
      WHERE [t].[Table2Id] = @Table2Id
END