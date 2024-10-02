CREATE OR ALTER PROCEDURE [Test].[spTableGet]
  @TableId AS UNIQUEIDENTIFIER
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

  -- Check user has permission to org unit.
  DECLARE @CurrOrgUnitId UNIQUEIDENTIFIER = NULL
  SET @CurrOrgUnitId = (SELECT TOP 1 [t].[OrgUnitId] FROM [Test].[Table] AS [t]
    WHERE [t].[TableId] = @TableId AND [t].[TenantId] = @TenantId AND ([t].[IsDeleted] IS NULL OR [t].[IsDeleted] = 0))

  IF (@CurrOrgUnitId IS NOT NULL AND (SELECT COUNT(*) FROM [Sec].[fnGetUserOrgUnits]() AS orgunits WHERE orgunits.OrgUnitId = @CurrOrgUnitId) = 0)
  BEGIN
    EXEC [dbo].[spThrowAuthorizationException]
  END

  -- Execute additional (pre) statements.
  EXEC Demo.Before

  -- Execute the primary select query.
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
    FROM [Test].[Table] AS [t] WITH (NOLOCK)
      WHERE [t].[TableId] = @TableId
        AND [t].[TenantId] = @TenantId
        AND ([t].[IsDeleted] IS NULL OR [t].[IsDeleted] = 0)

  -- Execute additional statements.
  EXEC Demo.After
END