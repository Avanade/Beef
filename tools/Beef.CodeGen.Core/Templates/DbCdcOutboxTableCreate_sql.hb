{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
CREATE TABLE [{{CdcSchema}}].[{{OutboxTableName}}] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [OutboxId] INT IDENTITY (1, 1) NOT NULL PRIMARY KEY CLUSTERED ([OutboxId] ASC),
  [CreatedDate] DATETIME NOT NULL,
  [{{pascal Name}}MinLsn] BINARY(10) NOT NULL,  -- Primary table: {{Schema}}.{{Name}}
  [{{pascal Name}}MaxLsn] BINARY(10) NOT NULL,
{{#each CdcJoins}}
  [{{pascal Name}}MinLsn] BINARY(10) NOT NULL,  -- Related table: {{Schema}}.{{TableName}}
  [{{pascal Name}}MaxLsn] BINARY(10) NOT NULL,
{{/each}}
  [IsComplete] BIT NOT NULL,
  [CompletedDate] DATETIME NULL,
  [HasDataLoss] BIT NOT NULL
);