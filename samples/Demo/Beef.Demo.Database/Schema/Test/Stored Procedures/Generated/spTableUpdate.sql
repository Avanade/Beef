CREATE OR ALTER PROCEDURE [Test].[spTableUpdate]
  @TableId AS UNIQUEIDENTIFIER,
  @Name AS NVARCHAR(50) NULL = NULL,
  @Count AS INT NULL = NULL,
  @Amount AS DECIMAL(16, 9) NULL = NULL,
  @GenderCode AS NVARCHAR(50) NULL = NULL,
  @OrgUnitId AS UNIQUEIDENTIFIER NULL = NULL,
  @RowVersion AS TIMESTAMP,
  @UpdatedBy AS NVARCHAR(250) NULL = NULL,
  @UpdatedDate AS DATETIME2 NULL = NULL,
  @ReselectRecord AS BIT = 0
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
    EXEC [Sec].[spCheckUserHasPermission] @TenantId, NULL, 'TESTSEC.WRITE', @OrgUnitId

    -- Set audit details.
    EXEC @UpdatedDate = fnGetTimestamp @UpdatedDate
    EXEC @UpdatedBy = fnGetUsername @UpdatedBy

    -- Check exists.
    DECLARE @PrevRowVersion BINARY(8)
    SET @PrevRowVersion = (SELECT TOP 1 [t].[RowVersion] FROM [Test].[Table] AS [t] WHERE [t].[TableId] = @TableId AND [t].[TenantId] = @TenantId AND ([t].[IsDeleted] IS NULL OR [t].[IsDeleted] = 0))
    IF @PrevRowVersion IS NULL
    BEGIN
      EXEC spThrowNotFoundException
    END

    -- Check user has permission to org unit.
    DECLARE @CurrOrgUnitId UNIQUEIDENTIFIER = NULL
    SET @CurrOrgUnitId = (SELECT TOP 1 [t].[OrgUnitId] FROM [Test].[Table] AS [t] WHERE [t].[TableId] = @TableId AND [t].[TenantId] = @TenantId AND ([t].[IsDeleted] IS NULL OR [t].[IsDeleted] = 0))
    IF (@CurrOrgUnitId IS NOT NULL AND (@OrgUnitId <> @CurrOrgUnitId OR (SELECT COUNT(*) FROM [Sec].[fnGetUserOrgUnits]() AS orgunits WHERE orgunits.OrgUnitId = @CurrOrgUnitId) = 0))
    BEGIN
      EXEC [dbo].[spThrowAuthorizationException]
    END

    -- Check concurrency (where provided).
    IF @RowVersion IS NULL OR @PrevRowVersion <> @RowVersion
    BEGIN
      EXEC spThrowConcurrencyException
    END

    -- Update the record.
    UPDATE [t] SET
        [t].[Name] = @Name,
        [t].[Count] = @Count,
        [t].[Amount] = @Amount,
        [t].[GenderCode] = @GenderCode,
        [t].[OrgUnitId] = @OrgUnitId,
        [t].[UpdatedBy] = @UpdatedBy,
        [t].[UpdatedDate] = @UpdatedDate
      FROM [Test].[Table] AS [t]
      WHERE [t].[TableId] = @TableId
        AND [t].[TenantId] = @TenantId
        AND ([t].[IsDeleted] IS NULL OR [t].[IsDeleted] = 0)

    -- Commit the transaction.
    COMMIT TRANSACTION
  END TRY
  BEGIN CATCH
    -- Rollback transaction and rethrow error.
    IF @@TRANCOUNT > 0
      ROLLBACK TRANSACTION;

    THROW;
  END CATCH

  -- Reselect record.
  IF @ReselectRecord = 1
  BEGIN
    EXEC [Test].[spTableGet] @TableId
  END
END