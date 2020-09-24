{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
CREATE PROCEDURE [{{Parent.Schema}}].[sp{{Parent.Name}}{{Name}}]
{{#each Parent.UniqueKeyColumns}}
  {{ParameterSql}}{{#unless @last}},{{/unless}}
{{/each}}
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */

  SET NOCOUNT ON;

{{#ifval Parent.ColumnTenantId}}
  -- Set the tenant identifier.
  DECLARE @{{Parent.ColumnTenantId.Name}} UNIQUEIDENTIFIER
  SET @{{Parent.ColumnTenantId.Name}} = dbo.fnGetTenantId(NULL)

{{/ifval}}
{{#ifval Permission}}
  -- Check user has permission.
  EXEC {{Root.UserPermissionObject}} {{#ifval Parent.ColumnTenantId}}@{{Parent.ColumnTenantId.Name}}{{/ifval}}, NULL, '{{Permission}}'

{{/ifval}}
{{#ifval Parent.ColumnOrgUnitId}}
  -- Check user has permission to org unit.
  DECLARE @CurrOrgUnitId UNIQUEIDENTIFIER = NULL
  SET @CurrOrgUnitId = (SELECT TOP 1 [x].[{{Parent.ColumnOrgUnitId.Name}}] FROM [{{Parent.Schema}}].[{{Parent.Name}}] AS x 
    WHERE {{#each Parent.UniqueKeyColumns}}x.[{{Name}}] = @{{Name}}{{/each}}{{#ifval Parent.ColumnIsDeleted}} AND ISNULL(x.[{{Parent.ColumnIsDeleted.Name}}], 0) = 0{{/ifval}}{{#ifval Parent.ColumnTenantId}} AND x.[{{Parent.ColumnTenantId.Name}}] = @{{Parent.ColumnTenantId.Name}}{{/ifval}})

  IF (@CurrOrgUnitId IS NOT NULL AND (SELECT COUNT(*) FROM {{Root.OrgUnitJoinObject}} AS orgunits WHERE orgunits.{{Parent.ColumnOrgUnitId.Name}} = @CurrOrgUnitId) = 0)
  BEGIN
    EXEC [dbo].[spThrowAuthorizationException]
  END

{{/ifval}}
{{#each ExecuteBefore}}
  {{#if @first}}
  -- Execute additional (pre) statements.
  {{/if}}
  {{Statement}}
  {{#if @last}}

  {{/if}}
{{/each}}
  SELECT
{{#each Parent.CoreColumns}}
    {{QualifiedName}}{{#unless @last}},{{/unless}}
{{/each}}
    FROM {{Parent.QualifiedName}} as {{Parent.Alias}}