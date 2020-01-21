CREATE FUNCTION [dbo].[fnGetUsername]
(
	@Override as nvarchar(50) = null
)
RETURNS nvarchar(50)
AS
BEGIN
	DECLARE @Username nvarchar(50)
    IF @Override IS NULL
	BEGIN
		SET @Username = CONVERT(nvarchar(50), SESSION_CONTEXT(N'Username'));
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

