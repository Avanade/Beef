-- Migration Script - creating with all special columns to validate correct code-gen.

BEGIN TRANSACTION

CREATE TABLE [Test].[Table3] (
  [PartA] NVARCHAR(10) NOT NULL,
  [PartB] NVARCHAR(10) NOT NULL,
  [Name] NVARCHAR(50) NULL,
  [Count] INT NULL
  CONSTRAINT [PK_Table3_PartAPartB] PRIMARY KEY CLUSTERED
  (
    [PartA] ASC,
    [PartB] ASC
  ) ON [PRIMARY]
);
	
COMMIT TRANSACTION