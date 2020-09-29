﻿{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
CREATE PROCEDURE [{{Parent.Schema}}].[sp{{Parent.Name}}{{Name}}]
{{#each ArgumentParameters}}
  {{ParameterSql}},
{{/each}}
  @List AS [{{Parent.Schema}}].[udt{{Parent.Name}}List] READONLY
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */

  SET NOCOUNT ON;

  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

{{#if Parent.HasAuditColumns}}
    -- Set audit details.
  {{#if Parent.HasAuditDateColumns}}
    DECLARE @AuditDate DATETIME
  {{/if}}
  {{#if Parent.HasAuditByColumns}}
    DECLARE @AuditBy NVARCHAR(100)
  {{/if}}
  {{#if Parent.HasAuditDateColumns}}
    EXEC @AuditDate = fnGetTimestamp @AuditDate
  {{/if}}
  {{#if Parent.HasAuditByColumns}}
    EXEC @AuditBy = fnGetUsername @AuditBy
  {{/if}}

{{/if}}
{{#ifval Parent.ColumnTenantId}}
    -- Set the tenant identifier.
    DECLARE {{Parent.ColumnTenantId.ParameterName}} UNIQUEIDENTIFIER
    SET {{Parent.ColumnTenantId.ParameterName}} = dbo.fnGetTenantId(NULL)

{{/ifval}}
{{#ifval Permission}}
    -- Check user has permission.
    EXEC {{Root.UserPermissionObject}} {{#ifval Parent.ColumnTenantId}}{{Parent.ColumnTenantId.ParameterName}}{{else}}NULL{{/ifval}}, NULL, '{{Permission}}'

{{/ifval}}
{{#ifeq MergeOverrideIdentityColumns.Count 0}}
    -- Check valid for merge.
    DECLARE @ListCount INT
    SET @ListCount = (SELECT COUNT(*) FROM @List WHERE {{#each Parent.PrimaryKeyColumns}}{{#unless @first}} AND {{/unless}}[{{Name}}] IS NOT NULL AND [{{Name}}] <> {{SqlInitialValue}}{{/each}})

    DECLARE @RecordCount INT
    SET @RecordCount = (SELECT COUNT(*) FROM @List AS [list]
      INNER JOIN {{Parent.QualifiedName}} AS [{{Parent.Alias}}]
{{#each MergeListJoinOn}}
        {{#if @first}}ON{{else}}AND{{/if}} {{{.}}}{{#if @last}}){{/if}}
{{/each}}

    IF @ListCount <> @RecordCount
    BEGIN
      EXEC spThrowConcurrencyException
    END

{{/ifeq}}
{{#each ExecuteBefore}}
  {{#if @first}}
    -- Execute additional (pre) statements.
  {{/if}}
    {{{Statement}}}
  {{#if @last}}

  {{/if}}
{{/each}}
    -- Merge the records.
    MERGE INTO {{Parent.QualifiedName}}{{#ifval WithHints}} WITH ({{WithHints}}){{else}} WITH (HOLDLOCK){{/ifval}} AS [{{Parent.Alias}}]
      USING @List AS [list]
{{#each MergeOn}}
        {{#if @first}}ON ({{else}}AND {{/if}}{{{.}}}{{#if @last}}){{/if}}
{{/each}}
      WHEN MATCHED AND EXISTS
         (SELECT {{#each MergeMatchSourceColumns}}{{.}}{{#unless @last}}, {{/unless}}{{/each}}
          EXCEPT
          SELECT {{#each MergeMatchTargetColumns}}{{.}}{{#unless @last}}, {{/unless}}{{/each}})
        THEN UPDATE SET
{{#each SettableColumnsUpdate}}
          {{QualifiedName}} = {{#ifval MergeValueSql}}{{MergeValueSql}}{{else}}[list].[{{Name}}]{{/ifval}}{{#unless @last}},{{/unless}}
{{/each}}
      WHEN NOT MATCHED BY TARGET
        THEN INSERT (
{{#each SettableColumnsInsert}}
          [{{Name}}]{{#unless @last}},{{/unless}}
{{/each}}
        ) VALUES (
{{#each SettableColumnsInsert}}
          {{#ifval MergeValueSql}}{{MergeValueSql}}{{else}}{{#if IsTenantIdColumn}}@TenantId{{else}}[list].[{{Name}}]{{/if}}{{/ifval}}{{#unless @last}},{{/unless}}
{{/each}}
        )
      WHEN NOT MATCHED BY SOURCE
{{#each Where}}
        AND {{{Statement}}}
{{/each}}
{{ifval Parent.ColumnIsDeleted}}        THEN UPDATE SET
          {{Parent.ColumnIsDeleted.QualifiedName}} = 1{{#ifne SettableColumnsDelete.Count 0}},{{/ifne}}
  {{#each SettableColumnsDelete}}
          {{QualifiedName}} = {{#if IsAudit}}{{AuditParameterName}}{{else}}{{ParameterName}}{{/if}}{{#if @last}};{{else}},{{/if}}
  {{/each}}
{{else}}
        THEN DELETE;
{{/ifval}}
{{#each ExecuteAfter}}
  {{#if @first}}

  -- Execute additional statements.
  {{/if}}
  {{Statement}}
{{/each}}
  
  -- Commit the transaction.
    COMMIT TRANSACTION
  END TRY
  BEGIN CATCH
    -- Rollback transaction and rethrow error.
    IF @@TRANCOUNT > 0
      ROLLBACK TRANSACTION;

    THROW;
  END CATCH
END