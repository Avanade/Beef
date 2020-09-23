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

    -- Set audit details, etc.
    DECLARE @AuditDate DATETIME
    DECLARE @AuditBy NVARCHAR(100)
    EXEC @AuditDate = fnGetTimestamp @AuditDate
    EXEC @AuditBy = fnGetUsername @AuditBy

    -- Set the tenant identifier.
    DECLARE @TenantId uniqueidentifier
    SET @TenantId = dbo.fnGetTenantId(NULL)

    -- Check valid for merge.
    DECLARE @ListCount INT
    SET @ListCount = (SELECT COUNT(*) FROM @List WHERE [TableId] IS NOT NULL AND [TableId] <> CONVERT(UNIQUEIDENTIFIER, '00000000-0000-0000-0000-000000000000'))

    DECLARE @RecordCount INT
    SET @RecordCount = (SELECT COUNT(*) FROM @List as [list]
      INNER JOIN [Test].[Table] as [t]
        ON [t].[TableId] = [list].[TableId]
        AND [t].[TenantId] = @TenantId
        AND [t].[RowVersion] = [list].[RowVersion]
        AND ISNULL([t].[IsDeleted], 0) = 0)
      
    IF @ListCount <> @RecordCount
    BEGIN
      EXEC spThrowConcurrencyException
    END

    -- Merge the records.
    MERGE INTO [Test].[Table] WITH (HOLDLOCK) AS [t]
      USING @List as [s]
        ON ([t].[TableId] = [s].[TableId]
        AND [t].[TenantId] = @TenantId)
      WHEN MATCHED AND EXISTS
          (SELECT [s].[Name], [s].[Count], [s].[Amount], [s].[GenderCode], [s].[OrgUnitId]
           EXCEPT
           SELECT [t].[Name], [t].[Count], [t].[Amount], [t].[GenderCode], [t].[OrgUnitId])
        THEN UPDATE SET
           [t].[Name] = [s].[Name]
          ,[t].[Count] = [s].[Count]
          ,[t].[Amount] = [s].[Amount]
          ,[t].[GenderCode] = [s].[GenderCode]
          ,[t].[OrgUnitId] = [s].[OrgUnitId]
          ,[t].[UpdatedBy] = @AuditBy
          ,[t].[UpdatedDate] = @AuditDate
      WHEN NOT MATCHED BY TARGET
        THEN INSERT (
           [Name]
          ,[Count]
          ,[Amount]
          ,[GenderCode]
          ,[OrgUnitId]
          ,[CreatedBy]
          ,[CreatedDate]
          ,[TenantId]
        )
        VALUES (
          [s].[Name]
         ,[s].[Count]
         ,[s].[Amount]
         ,[s].[GenderCode]
         ,[s].[OrgUnitId]
         ,@AuditBy
         ,@AuditDate
         ,@TenantId
        )
      WHEN NOT MATCHED BY SOURCE
        AND [t].[TenantId] = @TenantId
        THEN UPDATE SET
          [t].[IsDeleted] = 1;

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