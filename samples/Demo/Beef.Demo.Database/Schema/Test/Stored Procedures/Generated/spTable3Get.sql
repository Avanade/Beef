CREATE PROCEDURE [Test].[spTable3Get]
  @PartA AS NVARCHAR(10),
  @PartB AS NVARCHAR(10)
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */

  SET NOCOUNT ON;

  -- Execute the primary select query.
  SELECT
      [t].[PartA],
      [t].[PartB],
      [t].[Name],
      [t].[Count]
    FROM [Test].[Table3] AS [t]
      WHERE [t].[PartA] = @PartA
        AND [t].[PartB] = @PartB
END