-- Migration Script - creating with all special columns to validate correct code-gen.

BEGIN TRANSACTION

CREATE TABLE [Test].[Table2] (
  [Table2Id] UNIQUEIDENTIFIER NOT NULL,
  [Name] NVARCHAR(50) NULL,
  [Count] INT NULL
  CONSTRAINT [PK_Table2_Table2Id] PRIMARY KEY CLUSTERED
  (
    [Table2Id] ASC
  ) ON [PRIMARY]
);
	
COMMIT TRANSACTION