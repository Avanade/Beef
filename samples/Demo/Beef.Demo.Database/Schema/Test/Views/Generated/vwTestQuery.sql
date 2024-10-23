CREATE VIEW [Test].[vwTestQuery]
AS
  /*
   * This is automatically generated; any changes will be lost.
   */

  SELECT
      [t].[TableId],
      [t].[Name],
      [t].[Count],
      [t].[Amount],
      [t].[Other],
      [t].[GenderCode],
      [t].[OrgUnitId],
      [t].[RowVersion],
      [t].[CreatedDate],
      [t].[UpdatedDate],
      [p].[PersonId],
      [p].[FirstName],
      [p].[LastName],
      [p].[Birthday],
      [p].[GenderId],
      [p].[Street],
      [p].[City],
      [p].[RowVersion] AS [RowVersionP],
      [p].[CreatedBy],
      [p].[UpdatedBy],
      [p].[UniqueCode],
      [p].[EyeColorCode],
      [p].[MetadataJson]
    FROM [Test].[Table] AS [t]
      INNER JOIN [Sec].[fnGetUserOrgUnits]() AS [orgunits] ON ([t].[OrgUnitId] = [orgunits].[OrgUnitId])
      INNER JOIN [Demo].[Person] AS [p] ON ([p].[PersonId] = [t].[TableId])
    WHERE [t].[TenantId] = dbo.fnGetTenantId(NULL)
      AND ([t].[IsDeleted] IS NULL OR [t].[IsDeleted] = 0)
      AND [Sec].[fnGetUserHasPermission](NULL, NULL, 'TESTSEC.READ', NULL) = 1
