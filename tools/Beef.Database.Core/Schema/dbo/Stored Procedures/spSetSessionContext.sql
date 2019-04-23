

CREATE PROCEDURE [dbo].[spSetSessionContext]
	@Timestamp as datetime = null,
	@Username as nvarchar(1024) = null,
	@TenantId as uniqueidentifier = null
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
END
