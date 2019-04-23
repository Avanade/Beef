


CREATE PROCEDURE [dbo].[spThrowValidationException]
	@Message NVARCHAR(2048) = NULL
AS
BEGIN
	SET NOCOUNT ON;
	THROW 56001, @Message, 1
END


