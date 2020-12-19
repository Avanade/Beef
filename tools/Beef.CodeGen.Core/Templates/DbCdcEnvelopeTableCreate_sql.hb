{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
CREATE TABLE [{{CdcSchema}}].[{{EnvelopeTableName}}] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [EnvelopeId] INT IDENTITY (1, 1) NOT NULL PRIMARY KEY CLUSTERED ([EnvelopeId] ASC),
  [CreatedDate] DATETIME NOT NULL,
  [{{pascal Alias}}MinLsn] BINARY(10) NOT NULL,  -- Primary table: {{Schema}}.{{Name}}
  [{{pascal Alias}}MaxLsn] BINARY(10) NOT NULL,
{{#each Joins}}
  [{{pascal Alias}}MinLsn] BINARY(10) NOT NULL,  -- Related table: {{Schema}}.{{Name}}
  [{{pascal Alias}}MaxLsn] BINARY(10) NOT NULL,
{{/each}}
  [IsComplete] BIT NOT NULL,
  [CompletedDate] DATETIME NULL
);