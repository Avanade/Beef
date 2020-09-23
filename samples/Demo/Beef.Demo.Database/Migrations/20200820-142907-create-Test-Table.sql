-- Migration Script - creating with all special columns to validate correct code-gen.

BEGIN TRANSACTION

CREATE TABLE [Test].[Table] (
  [TableId] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY,
  [Name] NVARCHAR(50) NULL,
  [Count] INT NULL,
  [Amount] DECIMAL(16, 9) NULL,
  [Other] NVARCHAR(50) NULL,
  [GenderCode] NVARCHAR(50) NULL,
  [IsDeleted] BIT NULL,
  [TenantId] UNIQUEIDENTIFIER NULL,
  [OrgUnitId] UNIQUEIDENTIFIER NULL,
  [RowVersion] TIMESTAMP NOT NULL,
  [CreatedBy] NVARCHAR(250) NULL,
  [CreatedDate] DATETIME2 NULL,
  [UpdatedBy] NVARCHAR(250) NULL,
  [UpdatedDate] DATETIME2 NULL,
);
	
COMMIT TRANSACTION