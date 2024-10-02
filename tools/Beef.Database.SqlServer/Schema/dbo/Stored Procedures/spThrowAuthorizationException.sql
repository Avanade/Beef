-- Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

CREATE OR ALTER PROCEDURE [dbo].[spThrowAuthorizationException]
  @Message NVARCHAR(2048) = NULL
AS
BEGIN
  SET NOCOUNT ON;
  THROW 56003, @Message, 1
END