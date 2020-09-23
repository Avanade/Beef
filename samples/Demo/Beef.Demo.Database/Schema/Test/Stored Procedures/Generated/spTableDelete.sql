CREATE PROCEDURE [Test].[spTableDelete]
   @TableId AS UNIQUEIDENTIFIER
  ,@UpdatedBy AS NVARCHAR(250) NULL
  ,@UpdatedDate AS DATETIME2 NULL
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */
 
  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

    -- Set the tenant identifier.
    DECLARE @TenantId uniqueidentifier
    SET @TenantId = dbo.fnGetTenantId(NULL)

    -- Check user has permission.
    EXEC [Sec].[spCheckUserHasPermission] @TenantId, NULL, 'TESTSEC.DELETE'
    
    -- Check user has permission to org unit.
  DECLARE @CurrOrgUnitId UNIQUEIDENTIFIER = NULL
  SET @CurrOrgUnitId = (SELECT TOP 1 [x].[OrgUnitId] FROM [Test].[Table] AS x 
    WHERE x.[TableId] = @TableId AND ISNULL(x.[IsDeleted], 0) = 0 AND x.[TenantId] = @TenantId)

  IF (@CurrOrgUnitId IS NOT NULL AND (SELECT COUNT(*) FROM [Sec].[fnGetUserOrgUnits]() AS orgunits WHERE orgunits.OrgUnitId = @CurrOrgUnitId) = 0)
  BEGIN
    EXEC [dbo].[spThrowAuthorizationException]
  END

    -- Set audit details.
    EXEC @UpdatedDate = fnGetTimestamp @UpdatedDate
    EXEC @UpdatedBy = fnGetUsername @UpdatedBy
  
    -- Update the IsDeleted bit (logically delete).
    UPDATE [Test].[Table] SET
        [IsDeleted] = 1
       ,[UpdatedDate] = @UpdatedDate
       ,[UpdatedBy] = @UpdatedBy
      WHERE ISNULL([IsDeleted], 0) = 0
        AND [TableId] = @TableId
        AND [TenantId] = dbo.fnGetTenantId(NULL)

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