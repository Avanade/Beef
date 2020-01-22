CREATE FUNCTION [dbo].[fnGetUsername]
(
	@Override AS NVARCHAR(1024) = null
)
RETURNS NVARCHAR(1024)
AS
BEGIN
	DECLARE @Username NVARCHAR(1024)
    IF @Override IS NULL
	BEGIN
		SET @Username = CONVERT(NVARCHAR(1024), SESSION_CONTEXT(N'Username'));
		IF @Username IS NULL
		BEGIN
			SET @Username = SYSTEM_USER
		END
	END
	ELSE
	BEGIN
		SET @Username = @Override
	END

	RETURN @Username
END