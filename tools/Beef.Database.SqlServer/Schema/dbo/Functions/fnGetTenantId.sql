-- Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

CREATE OR ALTER FUNCTION [dbo].[fnGetTenantId]
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