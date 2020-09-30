CREATE FUNCTION [Sec].[fnGetUserHasPermission](
  @TenantId UNIQUEIDENTIFIER,
  @UserId UNIQUEIDENTIFIER,
  @Permission NVARCHAR(50),
  @OrgUnitId UNIQUEIDENTIFIER NULL)
RETURNS BIT
AS
BEGIN
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
	  RETURN 0
	END
  END

  RETURN 1 -- 1 if allowed; zero if not allowed.
END