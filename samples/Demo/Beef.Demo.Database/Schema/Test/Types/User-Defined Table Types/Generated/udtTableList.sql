CREATE TYPE [Test].[udtTableList] AS TABLE (
   /*
   * This is automatically generated; any changes will be lost. 
   */
   
   [TableId] UNIQUEIDENTIFIER NULL
  ,[Name] NVARCHAR(50) NULL
  ,[Count] INT NULL
  ,[Amount] DECIMAL(16, 9) NULL
  ,[GenderCode] NVARCHAR(50) NULL
  ,[OrgUnitId] UNIQUEIDENTIFIER NULL
  ,[RowVersion] TIMESTAMP NULL
)