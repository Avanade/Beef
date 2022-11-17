-- Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

CREATE PROCEDURE [dbo].[spSetSessionContext]
	@Timestamp AS DATETIME = null,
	@Username AS NVARCHAR(1024) = null,
	@TenantId AS UNIQUEIDENTIFIER = null,
	@UserId AS NVARCHAR(1024) = null
AS
BEGIN
	IF @Timestamp IS NOT NULL
	BEGIN
		EXEC sp_set_session_context 'Timestamp', @Timestamp, @read_only = 1;
	END

	IF @Username IS NOT NULL
	BEGIN
		EXEC sp_set_session_context 'Username', @Username, @read_only = 1;
	END

	IF @TenantId IS NOT NULL
	BEGIN
		EXEC sp_set_session_context 'TenantId', @TenantId, @read_only = 1;
	END
	
	IF @UserId IS NOT NULL
	BEGIN
		EXEC sp_set_session_context 'UserId', @UserId, @read_only = 1;
	END
END