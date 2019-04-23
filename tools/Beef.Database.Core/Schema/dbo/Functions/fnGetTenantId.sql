CREATE FUNCTION [dbo].[fnGetTenantId]
(
	@Override as uniqueidentifier = null
)
RETURNS uniqueidentifier
AS
BEGIN
	DECLARE @TenantId uniqueidentifier
    IF @Override IS NULL
	BEGIN
		SET @TenantId = CONVERT(uniqueidentifier, SESSION_CONTEXT(N'TenantId'));
	END
	ELSE
	BEGIN
		SET @TenantId = @Override
	END

	RETURN @TenantId
END

