{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
CREATE PROCEDURE [{{CdcSchema}}].[{{CdcIdentityMappingStoredProcedureName}}]
  @IdentityList AS [{{CdcSchema}}].[udt{{CdcIdentityMappingTableName}}List] READONLY
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost.
   */
 
  SET NOCOUNT ON;

  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

    -- Insert the identity only where a value does not already currently exist.
    INSERT INTO [{{CdcSchema}}].[{{CdcIdentityMappingTableName}}]
        ([Schema], [Table], [Key], [Identifier])
      SELECT [n].[Schema], [n].[Table], [n].[Key], [n].[Identifier]
        FROM [{{CdcSchema}}].[{{CdcIdentityMappingTableName}}] AS [n]
        WHERE NOT EXISTS (SELECT 0 FROM [{{CdcSchema}}].[{{CdcIdentityMappingTableName}}] AS [o]
                            WHERE [n].[Schema] = [o].[Schema] AND [n].[Table] = [o].[Table] AND [n].[Key] = [o].[Key])

    -- Get the latest (current) values as some may already have had an identifier created (i.e. not inserted above).
    SELECT [n].[Schema], [n].[Table], [n].[Key], [n].[Identifier]
      FROM [{{CdcSchema}}].[{{CdcIdentityMappingTableName}}] AS [n]
      INNER JOIN @IdentityList AS [o] ON [n].[Schema] = [o].[Schema] AND [n].[Table] = [o].[Table] AND [n].[Key] = [o].[Key]

    -- Commit the transaction.
    COMMIT TRANSACTION
    RETURN 0
  END TRY
  BEGIN CATCH
    -- Rollback transaction and rethrow error.
    IF @@TRANCOUNT > 0
      ROLLBACK TRANSACTION;

    THROW;
  END CATCH
END