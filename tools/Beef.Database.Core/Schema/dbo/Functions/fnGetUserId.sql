-- Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

CREATE FUNCTION [dbo].[fnGetUserId]
(
	@Override as uniqueidentifier = null
)
RETURNS uniqueidentifier
AS
BEGIN
	DECLARE @UserId uniqueidentifier
    IF @Override IS NULL
	BEGIN
		SET @UserId = CONVERT(uniqueidentifier, SESSION_CONTEXT(N'UserId'));
	END
	ELSE
	BEGIN
		SET @UserId = @Override
	END

	RETURN @UserId
END