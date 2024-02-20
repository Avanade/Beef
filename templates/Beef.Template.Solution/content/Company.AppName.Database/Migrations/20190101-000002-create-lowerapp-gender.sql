//#if (implement_sqlserver || implement_database)
CREATE TABLE [AppName].[Gender] (
	[GenderId] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY,
	[Code] NVARCHAR (50) NOT NULL UNIQUE,
	[Text] NVARCHAR (250) NULL,
	[IsActive] BIT NULL,
	[SortOrder] INT NULL,
	[RowVersion] TIMESTAMP NOT NULL,
	[CreatedBy] NVARCHAR(250) NULL,
	[CreatedDate] DATETIME2 NULL,
	[UpdatedBy] NVARCHAR(250) NULL,
	[UpdatedDate] DATETIME2 NULL
);
//#endif
//#if (implement_postgres)
CREATE TABLE "lowerapp"."gender" (
  "gender_id" SERIAL PRIMARY KEY,
  "code" VARCHAR(50) NOT NULL UNIQUE,
  "text" VARCHAR(250) NULL,
  "is_active" BOOLEAN NULL,
  "sort_order" INT NULL,
  "created_by" VARCHAR(250) NULL,
  "created_date" TIMESTAMPTZ NULL,
  "updated_by" VARCHAR(250) NULL,
  "updated_date" TIMESTAMPTZ NULL
);
//#endif