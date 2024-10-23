CREATE PROCEDURE [Test].[spTableCreate]
  @TableId AS UNIQUEIDENTIFIER = NULL OUTPUT,
  @Name AS NVARCHAR(50) NULL = NULL,
  @Count AS INT NULL = NULL,
  @Amount AS DECIMAL(16, 9) NULL = NULL,
  @GenderCode AS NVARCHAR(50) NULL = NULL,
  @OrgUnitId AS UNIQUEIDENTIFIER NULL = NULL,
  @CreatedBy AS NVARCHAR(250) NULL = NULL,
  @CreatedDate AS DATETIME2 NULL = NULL,
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
    EXEC @CreatedDate = fnGetTimestamp @CreatedDate
    EXEC @CreatedBy = fnGetUsername @CreatedBy

    -- Create the record.
    DECLARE @InsertedIdentity TABLE([TableId] UNIQUEIDENTIFIER)

    INSERT INTO [Test].[Table] (
      [Name],
      [Count],
      [Amount],
      [GenderCode],
      [TenantId],
      [OrgUnitId],
      [CreatedBy],
      [CreatedDate]
    )
    OUTPUT inserted.TableId INTO @InsertedIdentity
    VALUES (
      @Name,
      @Count,
      @Amount,
      @GenderCode,
      @TenantId,
      @OrgUnitId,
      @CreatedBy,
      @CreatedDate
    )

    -- Get the inserted identity.
    SELECT @TableId = [TableId] FROM @InsertedIdentity

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