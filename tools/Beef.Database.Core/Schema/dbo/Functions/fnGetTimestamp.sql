CREATE FUNCTION [dbo].[fnGetTimestamp]
(
	@Override as datetime = null	
)
RETURNS datetime
AS
BEGIN
	DECLARE @Timestamp datetime
	IF @Override IS NULL
	BEGIN
		SET @Timestamp = CONVERT(datetime, SESSION_CONTEXT(N'Timestamp'));
		IF @Timestamp IS NULL
		BEGIN
			SET @Timestamp = CURRENT_TIMESTAMP
		END
	END
	ELSE
	BEGIN
		SET @Timestamp = @Override
	END

	RETURN @Timestamp
END

