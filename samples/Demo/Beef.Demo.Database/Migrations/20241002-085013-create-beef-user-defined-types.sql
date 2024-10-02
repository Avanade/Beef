-- Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

IF NOT EXISTS (SELECT * FROM sys.types WHERE name = 'udtBigIntList' AND is_user_defined = 1)
BEGIN
  CREATE TYPE [dbo].[udtBigIntList] AS TABLE
  (
     [Value] BIGINT
  )
END

IF NOT EXISTS (SELECT * FROM sys.types WHERE name = 'udtDateTime2List' AND is_user_defined = 1)
BEGIN
  CREATE TYPE [dbo].[udtDateTime2List] AS TABLE
  (
    [Value] DATETIME2
  )
END

IF NOT EXISTS (SELECT * FROM sys.types WHERE name = 'udtIntList' AND is_user_defined = 1)
BEGIN
  CREATE TYPE [dbo].[udtIntList] AS TABLE
  (
     [Value] INT
  )
END

IF NOT EXISTS (SELECT * FROM sys.types WHERE name = 'udtNVarCharList' AND is_user_defined = 1)
BEGIN
  CREATE TYPE [dbo].[udtNVarCharList] AS TABLE
  (
    [Value] NVARCHAR(MAX)
  )
END

IF NOT EXISTS (SELECT * FROM sys.types WHERE name = 'udtUniqueIdentifierList' AND is_user_defined = 1)
BEGIN
  CREATE TYPE [dbo].[udtUniqueIdentifierList] AS TABLE
  (
    [Value] UNIQUEIDENTIFIER
  )
END