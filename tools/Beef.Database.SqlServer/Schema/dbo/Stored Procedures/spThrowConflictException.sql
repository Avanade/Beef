-- Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

CREATE PROCEDURE [dbo].[spThrowConflictException]
	@Message NVARCHAR(2048) = NULL
AS
BEGIN
	SET NOCOUNT ON;
    THROW 56006, @Message, 1
END