CREATE PROCEDURE [Test].[spTableDelete]
  @TableId AS UNIQUEIDENTIFIER,
  @UpdatedBy AS NVARCHAR(250) NULL = NULL,
  @UpdatedDate AS DATETIME2 NULL = NULL
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */

  SET NOCOUNT ON;

  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

    -- Set the tenant identifier.
    DECLARE @TenantId UNIQUEIDENTIFIER
    SET @TenantId = dbo.fnGetTenantId(NULL)

    -- Check user has permission.
    EXEC [Sec].[spCheckUserHasPermission] @TenantId, NULL, 'TESTSEC.DELETE'

    -- Check user has permission to org unit.
    DECLARE @CurrOrgUnitId UNIQUEIDENTIFIER = NULL
    SET @CurrOrgUnitId = (SELECT TOP 1 [t].[OrgUnitId] FROM [Test].[Table] AS [t]
      WHERE [t].[TableId] = @TableId AND [t].[TenantId] = @TenantId AND ([t].[IsDeleted] IS NULL OR [t].[IsDeleted] = 0))

    IF (@CurrOrgUnitId IS NOT NULL AND (SELECT COUNT(*) FROM [Sec].[fnGetUserOrgUnits]() AS orgunits WHERE orgunits.OrgUnitId = @CurrOrgUnitId) = 0)
    BEGIN
      EXEC [dbo].[spThrowAuthorizationException]
    END

    -- Set audit details.
    EXEC @UpdatedDate = fnGetTimestamp @UpdatedDate
    EXEC @UpdatedBy = fnGetUsername @UpdatedBy

    -- Update the IsDeleted bit (logically delete).
    UPDATE [t] SET
        [t].[IsDeleted] = 1,
        [t].[UpdatedBy] = @UpdatedBy,
        [t].[UpdatedDate] = @UpdatedDate
      FROM [Test].[Table] AS [t]
      WHERE [t].[TableId] = @TableId
        AND [t].[TenantId] = @TenantId
        AND ([t].[IsDeleted] IS NULL OR [t].[IsDeleted] = 0)

    -- Select the row count.
    SELECT @@ROWCOUNT

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