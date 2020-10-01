{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
CREATE PROCEDURE [{{Parent.Schema}}].[sp{{Parent.Name}}{{Name}}]
{{#each ArgumentParameters}}
  {{ParameterSql}},
{{/each}}
  @ReselectRecord AS BIT = 0
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
    EXEC {{Root.CheckUserPermissionSql}} {{#ifval Parent.ColumnTenantId}}{{Parent.ColumnTenantId.ParameterName}}{{else}}NULL{{/ifval}}, NULL, '{{Permission}}'{{ifval Parent.ColumnOrgUnitId}}, @{{Parent.ColumnOrgUnitId.Name}}{{/ifval}}

{{/ifval}}
{{#if Parent.HasAuditUpdated}}
    -- Set audit details.
  {{#ifval Parent.ColumnUpdatedDate}}
    EXEC @{{Parent.ColumnUpdatedDate.Name}} = fnGetTimestamp @{{Parent.ColumnUpdatedDate.Name}}
  {{/ifval}}
  {{#ifval Parent.ColumnUpdatedBy}}
    EXEC @{{Parent.ColumnUpdatedBy.Name}} = fnGetUsername @{{Parent.ColumnUpdatedBy.Name}}
  {{/ifval}}
{{/if}}

    -- Check exists.
    DECLARE @PrevRowVersion BINARY(8)
    SET @PrevRowVersion = (SELECT TOP 1 [{{Parent.Alias}}].[{{Parent.ColumnRowVersion.Name}}] FROM {{Parent.QualifiedName}} AS [{{Parent.Alias}}] {{#each Where}}{{#if @first}}WHERE {{else}} AND {{/if}}{{{Statement}}}{{/each}})
    IF @PrevRowVersion IS NULL
    BEGIN
      EXEC spThrowNotFoundException
    END

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
    -- Check concurrency (where provided).
    IF @RowVersion IS NULL OR @PrevRowVersion <> @RowVersion
    BEGIN
      EXEC spThrowConcurrencyException
    END
{{#each ExecuteBefore}}
  {{#if @first}}
    -- Execute additional (pre) statements.
  {{/if}}
    {{{Statement}}}
  {{#if @last}}

  {{/if}}
{{/each}}

    -- Update the record.
    UPDATE [{{Parent.Alias}}] SET
{{#each SettableColumnsUpdate}}
      {{QualifiedName}} = {{ParameterName}}{{#unless @last}},{{/unless}}
{{/each}}
      FROM {{Parent.QualifiedName}} AS [{{Parent.Alias}}]
{{#each Where}}
      {{#if @first}}WHERE{{else}}  AND{{/if}} {{{Statement}}}
{{/each}}
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
  
  -- Reselect record.
  IF @ReselectRecord = 1
  BEGIN
  {{#ifval ReselectStatement}}
    {{{ReselectStatement}}}
  {{else}}
    EXEC [{{Parent.Schema}}].[sp{{Parent.Name}}Get] {{#each Parent.PrimaryKeyColumns}}@{{Name}}{{#unless @last}}, {{/unless}}{{/each}}
  {{/ifval}}
  END
END