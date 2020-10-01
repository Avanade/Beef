{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
CREATE PROCEDURE [{{Parent.Schema}}].[sp{{Parent.Name}}{{Name}}]
{{#each ArgumentParameters}}
  {{ParameterSql}}{{#unless @last}},{{/unless}}
{{/each}}
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */

  SET NOCOUNT ON;

  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

{{#ifval Parent.ColumnTenantId}}
    -- Set the tenant identifier.
    DECLARE {{Parent.ColumnTenantId.ParameterName}} UNIQUEIDENTIFIER
    SET {{Parent.ColumnTenantId.ParameterName}} = dbo.fnGetTenantId(NULL)

{{/ifval}}
{{#ifval Permission}}
    -- Check user has permission.
    EXEC {{Root.CheckUserPermissionSql}} {{#ifval Parent.ColumnTenantId}}{{Parent.ColumnTenantId.ParameterName}}{{else}}NULL{{/ifval}}, NULL, '{{Permission}}'

{{/ifval}}
{{#ifval Parent.ColumnOrgUnitId}}
    -- Check user has permission to org unit.
    DECLARE @CurrOrgUnitId UNIQUEIDENTIFIER = NULL
    SET @CurrOrgUnitId = (SELECT TOP 1 [{{Parent.Alias}}].[{{Parent.ColumnOrgUnitId.Name}}] FROM {{Parent.QualifiedName}} AS [{{Parent.Alias}}]
      {{#each Where}}{{#if @first}}WHERE {{else}} AND {{/if}}{{{Statement}}}{{/each}})

    IF (@CurrOrgUnitId IS NOT NULL AND (SELECT COUNT(*) FROM {{Root.OrgUnitJoinSql}} AS orgunits WHERE orgunits.{{Parent.ColumnOrgUnitId.Name}} = @CurrOrgUnitId) = 0)
    BEGIN
      EXEC [dbo].[spThrowAuthorizationException]
    END

{{/ifval}}
{{#each ExecuteBefore}}
  {{#if @first}}
    -- Execute additional (pre) statements.
  {{/if}}
    {{{Statement}}}

{{/each}}
{{#ifval Parent.ColumnIsDeleted}}
  {{#if Parent.HasAuditDeleted}}
    -- Set audit details.
    {{#ifval Parent.ColumnDeletedDate}}
    EXEC @{{Parent.ColumnDeletedDate.Name}} = fnGetTimestamp @{{Parent.ColumnDeletedDate.Name}}
    {{/ifval}}
    {{#ifval Parent.ColumnDeletedBy}}
    EXEC @{{Parent.ColumnDeletedBy.Name}} = fnGetUsername @{{Parent.ColumnDeletedBy.Name}}
    {{/ifval}}

  {{/if}}
    -- Update the IsDeleted bit (logically delete).
    UPDATE [{{Parent.Alias}}] SET
      {{Parent.ColumnIsDeleted.QualifiedName}} = 1{{#ifne SettableColumnsUpdate.Count 0}},{{/ifne}}
  {{#each SettableColumnsUpdate}}
      {{QualifiedName}} = {{ParameterName}}{{#unless @last}},{{/unless}}
  {{/each}}
      FROM {{Parent.QualifiedName}} AS [{{Parent.Alias}}]
  {{#each Where}}
      {{#if @first}}WHERE{{else}}  AND{{/if}} {{{Statement}}}
  {{/each}}
{{else}}
    -- Delete the record.
    DELETE [{{Parent.Alias}}] FROM {{Parent.QualifiedName}} AS [{{Parent.Alias}}]
  {{#each Where}}
      {{#if @first}}WHERE{{else}}  AND{{/if}} {{{Statement}}}
  {{/each}}
{{/ifval}}
{{#each ExecuteAfter}}
  {{#if @first}}

    -- Execute additional statements.
  {{/if}}
    {{{Statement}}}
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