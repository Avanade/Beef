//#if (implement_sqlserver || implement_database)
CREATE TABLE [AppName].[Person](
   [PersonId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
   [FirstName] NVARCHAR(100) NULL,
   [LastName] NVARCHAR(100) NULL,
   [GenderCode] NVARCHAR(50) NULL,
   [Birthday] DATE NULL,
   [RowVersion] TIMESTAMP NOT NULL,
   [CreatedBy] NVARCHAR(250) NULL,
   [CreatedDate] DATETIME2 NULL,
   [UpdatedBy] NVARCHAR(250) NULL,
   [UpdatedDate] DATETIME2 NULL,
)
//#endif
//#if (implement_postgres)
CREATE TABLE "lowerapp"."person" (
  "person_id" SERIAL PRIMARY KEY,
  "first_name" VARCHAR(100) NULL,
  "last_name" VARCHAR(100) NULL,
  "gender_code" VARCHAR(50) NULL,
  "birthday" DATE NULL,
  "created_by" VARCHAR(250) NULL,
  "created_date" TIMESTAMPTZ NULL,
  "updated_by" VARCHAR(250) NULL,
  "updated_date" TIMESTAMPTZ NULL
);
//#endif