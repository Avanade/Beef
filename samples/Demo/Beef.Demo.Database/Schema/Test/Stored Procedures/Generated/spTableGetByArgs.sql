CREATE PROCEDURE [Test].[spTableGetByArgs]
  @Name AS NVARCHAR(50) NULL = NULL,
  @MinCount AS INT,
  @MaxCount AS INT NULL = NULL
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */

  SET NOCOUNT ON;

  -- Set the tenant identifier.
  DECLARE @TenantId UNIQUEIDENTIFIER
  SET @TenantId = dbo.fnGetTenantId(NULL)

  -- Check user has permission.
  EXEC [Sec].[spCheckUserHasPermission] @TenantId, NULL, 'TESTSEC.READ'

  -- Select the requested data.
  SELECT
      [t].[TableId],
      [t].[Name],
      [t].[Count],
      [t].[Amount],
      [t].[GenderCode],
      [t].[OrgUnitId],
      [t].[RowVersion],
      [t].[CreatedBy],
      [t].[CreatedDate],
      [t].[UpdatedBy],
      [t].[UpdatedDate]
    FROM [Test].[Table] AS [t]
    INNER JOIN [Sec].[fnGetUserOrgUnits]() AS orgunits ON [t].[OrgUnitId] = [orgunits].[OrgUnitId]
    WHERE [t].[TenantId] = @TenantId
      AND (@Name IS NULL OR [t].[Name] LIKE @Name)
      AND [t].[Count] >= @MinCount
      AND (@MaxCount IS NULL OR [t].[Count] <= @MaxCount)
      AND ISNULL([t].[IsDeleted], 0) = 0
END