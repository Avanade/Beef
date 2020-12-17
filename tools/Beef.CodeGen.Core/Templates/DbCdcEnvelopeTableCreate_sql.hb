{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
CREATE TABLE [{{CdcSchema}}].[{{CdcEnvelopeTableName}}] (
  /*
   * This is automatically generated; any changes will be lost.
   */

  [OutBoxEnvelopeId] INT IDENTITY (1, 1) NOT NULL PRIMARY KEY CLUSTERED ([OutBoxEnvelopeId] ASC),
  [CreatedDate] DATETIME NOT NULL,
  [FirstProcessedLSN] BINARY(10) NOT NULL,
  [LastProcessedLSN] BINARY(10) NOT NULL,
  [HasBeenCompleted] BIT NOT NULL,
  [ProcessedDate] DATETIME NULL
);