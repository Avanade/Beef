{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
CREATE PROCEDURE [{{Parent.Schema}}].[sp{{Parent.Name}}{{Name}}]
{{#each ArgumentParameters}}
  {{ParameterSql}}{{#if Parent.Paging}},{{else}}{{#unless @last}},{{/unless}}{{/if}}
{{/each}}
{{#if Paging}}
  @PagingSkip AS INT = 0,
  @PagingTake AS INT = 250,
  @PagingCount AS BIT = NULL
{{/if}}
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost. 
   */

  SET NOCOUNT ON;

{{#ifval Parent.ColumnTenantId}}
  -- Set the tenant identifier.
  DECLARE {{Parent.ColumnTenantId.ParameterName}} UNIQUEIDENTIFIER
  SET {{Parent.ColumnTenantId.ParameterName}} = dbo.fnGetTenantId(NULL)

{{/ifval}}
{{#ifval Permission}}
  -- Check user has permission.
  EXEC {{Root.CheckUserPermissionSql}} {{#ifval Parent.ColumnTenantId}}{{Parent.ColumnTenantId.ParameterName}}{{else}}NULL{{/ifval}}, NULL, '{{Permission}}'

{{/ifval}}
{{#each CollectionParameters}}
  {{#if @first}}
  -- Check list counts.
  {{/if}}
  DECLARE {{ParameterName}}Count AS INT
  SET {{ParameterName}}Count = (SELECT COUNT(*) FROM {{ParameterName}})

{{/each}}
{{#each ExecuteBefore}}
  {{#if @first}}
  -- Execute additional (pre) statements.
  {{/if}}
  {{Statement}}
  {{#if @last}}

  {{/if}}
{{/each}}
  -- Select the requested data.
  SELECT
{{#each Parent.CoreColumns}}
      {{QualifiedName}}{{#unless @last}},{{/unless}}
{{/each}}
{{#if IntoTempTable}}
    INTO [#{{Parent.Alias}}]
{{/if}}
    FROM {{Parent.QualifiedName}} AS [{{Parent.Alias}}]{{#ifval WithHints}} WITH ({{WithHints}}){{/ifval}}
{{#ifval Parent.ColumnOrgUnitId}}
    INNER JOIN {{Root.OrgUnitJoinSql}} AS orgunits ON [{{Parent.Alias}}].[{{Parent.ColumnOrgUnitId.Name}}] = [orgunits].[{{Parent.ColumnOrgUnitId.Name}}]
{{/ifval}}
{{#each Where}}
    {{#if @first}}WHERE{{else}}  AND{{/if}} {{{Statement}}}
{{/each}}
{{#ifne OrderBy.Count 0}}
    ORDER BY {{#each OrderBy}}{{OrderBySql}}{{#unless @last}}, {{/unless}}{{/each}}
{{/ifne}}
{{#if Paging}}
    OFFSET @PagingSkip ROWS FETCH NEXT @PagingTake ROWS ONLY
{{/if}}
{{#if IntoTempTable}}

  -- Select from the temp table.
  SELECT * FROM [#{{Parent.Alias}}]
{{/if}}
{{#each ExecuteAfter}}
  {{#if @first}}

  -- Execute additional statements.
  {{/if}}
  {{Statement}}
{{/each}}
{{#if Paging}}

  -- Return the full (all pages) row count.
  IF (@PagingCount IS NOT NULL AND @PagingCount = 1)
  BEGIN
    RETURN (SELECT COUNT(*)
      FROM {{Parent.QualifiedName}} AS [{{Parent.Alias}}]{{#ifval WithHints}} WITH ({{WithHints}}){{/ifval}}{{#ifeq Parameters.Count 0}}){{/ifeq}}
{{#each Where}}
      {{#if @first}}WHERE{{else}}  AND{{/if}} {{{Statement}}}{{#if @last}}){{/if}}
{{/each}}  
  END
{{/if}}
END