CREATE VIEW [Test].[vwTable]
AS
  /*
   * This is automatically generated; any changes will be lost. 
   */

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
    WHERE [t].[TenantId] = dbo.fnGetTenantId(NULL)
      AND [Sec].[fnGetUserHasPermission](NULL, NULL, 'TESTSEC.READ', NULL) = 1
