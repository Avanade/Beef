-- Create Reference Data table: Ref.Status

BEGIN TRANSACTION

CREATE TABLE [Ref].[Status] (
  [StatusId] NVARCHAR(50) NOT NULL PRIMARY KEY,
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

COMMIT TRANSACTION