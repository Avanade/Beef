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
    EXEC {{Root.UserPermissionObject}} {{#ifval Parent.ColumnTenantId}}{{Parent.ColumnTenantId.ParameterName}}{{/ifval}}, NULL, '{{Permission}}'{{ifval Parent.ColumnOrgUnitId}}, @{{Parent.ColumnOrgUnitId.Name}}{{/ifval}}

{{/ifval}}
{{#ifval Parent.ColumnCreatedDate Parent.ColumnCreatedBy}}
    -- Set audit details.
  {{#ifval Parent.ColumnCreatedDate}}
    EXEC @{{Parent.ColumnCreatedDate.Name}} = fnGetTimestamp @{{Parent.ColumnCreatedDate.Name}}
  {{/ifval}}
  {{#ifval Parent.ColumnCreatedBy}}
    EXEC @{{Parent.ColumnCreatedBy.Name}} = fnGetUsername @{{Parent.ColumnCreatedBy.Name}}
  {{/ifval}}
{{/ifval}}

{{#each ExecuteBefore}}
  {{#if @first}}
    -- Execute additional (pre) statements.
  {{/if}}
    {{{Statement}}}
  {{#if @last}}

  {{/if}}
{{/each}}
    -- Create the record.
    DECLARE @InsertedIdentity TABLE({{#each Parent.PrimaryKeyIdentityColumns}}[{{Name}}] {{SqlType}}{{#unless @last}}, {{/unless}}{{/each}})

    INSERT INTO {{Parent.QualifiedName}} (
{{#each SettableColumnsInsert}}
      [{{Name}}]{{#unless @last}},{{/unless}}
{{/each}}
    )
    OUTPUT {{#each Parent.PrimaryKeyIdentityColumns}}inserted.{{Name}}{{#unless @last}}, {{/unless}}{{/each}} INTO @InsertedIdentity
    VALUES (
{{#each SettableColumnsInsert}}
      {{ParameterName}}{{#unless @last}},{{/unless}}
{{/each}}    )

{{#ifne Parent.PrimaryKeyIdentityColumns.Count 0}}
    -- Get the inserted identity.
  {{#each Parent.PrimaryKeyIdentityColumns}}
    SELECT @{{Name}} = [{{Name}}] FROM @InsertedIdentity
  {{/each}}
{{/ifne}}
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