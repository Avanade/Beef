


CREATE PROCEDURE [dbo].[spThrowAuthorizationException]
	@Message NVARCHAR(2048) = NULL
AS
BEGIN
	SET NOCOUNT ON;
    THROW 56003, @Message, 1
END


