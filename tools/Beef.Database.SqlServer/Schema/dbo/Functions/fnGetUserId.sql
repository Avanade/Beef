-- Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

CREATE OR ALTER FUNCTION [dbo].[fnGetUserId]
(
  @Override as NVARCHAR(1024) = null
)
RETURNS NVARCHAR(1024)
AS
BEGIN
  DECLARE @UserId NVARCHAR(1024)
  IF @Override IS NULL
  BEGIN
    SET @UserId = CONVERT(NVARCHAR(1024), SESSION_CONTEXT(N'UserId'));
  END
  ELSE
  BEGIN
    SET @UserId = @Override
  END
  
  RETURN @UserId
END