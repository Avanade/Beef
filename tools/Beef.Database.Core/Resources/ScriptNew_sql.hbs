{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
-- Migration Script

BEGIN TRANSACTION

{{#ifeq Action 'CreateRef'}}
CREATE TABLE [{{Schema}}].[{{Table}}] (
  [{{Config.Table}}Id] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY,
  [Code] NVARCHAR(50) NOT NULL UNIQUE,
  [Text] NVARCHAR(250) NULL,
  [IsActive] BIT NULL,
  [SortOrder] INT NULL,
  [RowVersion] TIMESTAMP NOT NULL,
  [CreatedBy] NVARCHAR(250) NULL,
  [CreatedDate] DATETIME2 NULL,
  [UpdatedBy] NVARCHAR(250) NULL,
  [UpdatedDate] DATETIME2 NULL
);
{{else}}
  {{#ifeq Action 'Create'}}
CREATE TABLE [{{Schema}}].[{{Table}}] (
  [{{Config.Table}}Id] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY,
  -- [Code] NVARCHAR(50) NULL UNIQUE,
  -- [Text] NVARCHAR(250) NULL,
  -- [Bool] BIT NULL,
  -- [Date] DATE NULL,
  [RowVersion] TIMESTAMP NOT NULL,
  [CreatedBy] NVARCHAR(250) NULL,
  [CreatedDate] DATETIME2 NULL,
  [UpdatedBy] NVARCHAR(250) NULL,
  [UpdatedDate] DATETIME2 NULL
);
  {{else}}
    {{#ifeq Action 'Alter'}}
ALTER TABLE [{{Schema}}].[{{Table}}]
  -- ADD [Column] NVARCHAR(50) NULL
    {{else}}
-- SQL STATEMENT(s)
    {{/ifeq}}
  {{/ifeq}}
{{/ifeq}}

COMMIT TRANSACTION