CREATE OR ALTER PROCEDURE [Sec].[spCheckUserHasPermission]
  @TenantId UNIQUEIDENTIFIER,
  @UserId UNIQUEIDENTIFIER,
  @Permission NVARCHAR(50),
  @OrgUnitId UNIQUEIDENTIFIER NULL
AS
BEGIN
  SET NOCOUNT ON

  IF @TenantId IS NULL
  BEGIN
	SET @TenantId = [dbo].[fnGetTenantId](@TenantId)
  END

  IF @UserId IS NULL
  BEGIN
	SET @UserId = [dbo].[fnGetUserId](@UserId)  
  END

  IF @OrgUnitId IS NOT NULL
  BEGIN
	IF (SELECT COUNT(*) FROM [Sec].[fnGetUserOrgUnits]() WHERE [OrgUnitId] = @OrgUnitId) = 0
	BEGIN
	  EXEC [dbo].[spThrowAuthorizationException]
	END
  END

  -- Should be logic here to do an actual permissions check; if not authorized then [dbo].[spThrowAuthorizationException]; otherwise, return.
END