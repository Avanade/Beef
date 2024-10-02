-- Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

CREATE OR ALTER PROCEDURE [dbo].[spThrowValidationException]
  @Message NVARCHAR(2048) = NULL
AS
BEGIN
  SET NOCOUNT ON;
  THROW 56001, @Message, 1
END