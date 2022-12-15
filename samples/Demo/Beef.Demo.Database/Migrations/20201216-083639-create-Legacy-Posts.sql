-- Migration Script

BEGIN TRANSACTION

CREATE TABLE [Legacy].[Posts] (
  [PostsId] INT NOT NULL PRIMARY KEY,
  [Text] NVARCHAR(256) NULL UNIQUE,
  [Date] DATE NULL
);

COMMIT TRANSACTION