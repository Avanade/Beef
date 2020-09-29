CREATE PROCEDURE [Test].[spTableMerge]
  @List AS [Test].[udtTableList] READONLY
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */

  SET NOCOUNT ON;

  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

    -- Set audit details.
    DECLARE @AuditDate DATETIME
    DECLARE @AuditBy NVARCHAR(100)
    EXEC @AuditDate = fnGetTimestamp @AuditDate
    EXEC @AuditBy = fnGetUsername @AuditBy

    -- Set the tenant identifier.
    DECLARE @TenantId UNIQUEIDENTIFIER
    SET @TenantId = dbo.fnGetTenantId(NULL)

    -- Check user has permission.
    EXEC [Sec].[spCheckUserHasPermission] @TenantId, NULL, 'TESTSEC.WRITE'

    -- Check valid for merge.
    DECLARE @ListCount INT
    SET @ListCount = (SELECT COUNT(*) FROM @List WHERE [TableId] IS NOT NULL AND [TableId] <> CONVERT(UNIQUEIDENTIFIER, '00000000-0000-0000-0000-000000000000'))

    DECLARE @RecordCount INT
    SET @RecordCount = (SELECT COUNT(*) FROM @List AS [list]
      INNER JOIN [Test].[Table] AS [t]
        ON [t].[TableId] = [List].[TableId]
        AND ISNULL([t].[IsDeleted], 0) = 0
        AND [t].[TenantId] = @TenantId
        AND [t].[RowVersion] = [List].[RowVersion])

    IF @ListCount <> @RecordCount
    BEGIN
      EXEC spThrowConcurrencyException
    END

    -- Merge the records.
    MERGE INTO [Test].[Table] WITH (HOLDLOCK) AS [t]
      USING @List AS [list]
        ON ([t].[TableId] = [List].[TableId]
        AND [t].[TenantId] = @TenantId)
      WHEN MATCHED AND EXISTS
         (SELECT [list].[Name], [list].[Count], [list].[Amount], [list].[GenderCode], [list].[OrgUnitId]
          EXCEPT
          SELECT [t].[Name], [t].[Count], [t].[Amount], [t].[GenderCode], [t].[OrgUnitId])
        THEN UPDATE SET
          [t].[Name] = [list].[Name],
          [t].[Count] = [list].[Count],
          [t].[Amount] = [list].[Amount],
          [t].[GenderCode] = [list].[GenderCode],
          [t].[OrgUnitId] = [list].[OrgUnitId],
          [t].[UpdatedBy] = @AuditBy,
          [t].[UpdatedDate] = @AuditDate
      WHEN NOT MATCHED BY TARGET
        THEN INSERT (
          [Name],
          [Count],
          [Amount],
          [GenderCode],
          [TenantId],
          [OrgUnitId],
          [CreatedBy],
          [CreatedDate]
        ) VALUES (
          [list].[Name],
          [list].[Count],
          [list].[Amount],
          [list].[GenderCode],
          @TenantId,
          [list].[OrgUnitId],
          @AuditBy,
          @AuditDate
        )
      WHEN NOT MATCHED BY SOURCE
        AND [t].[TenantId] = @TenantId
        AND ISNULL([t].[IsDeleted], 0) = 0
        THEN UPDATE SET
          [t].[IsDeleted] = 1,
          [t].[UpdatedBy] = @AuditBy,
          [t].[UpdatedDate] = @AuditDate;
  
  -- Commit the transaction.
    COMMIT TRANSACTION
  END TRY
  BEGIN CATCH
    -- Rollback transaction and rethrow error.
    IF @@TRANCOUNT > 0
      ROLLBACK TRANSACTION;

    THROW;
  END CATCH
END