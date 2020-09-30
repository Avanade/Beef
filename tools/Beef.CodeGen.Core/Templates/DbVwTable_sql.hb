{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
CREATE VIEW [{{Schema}}].[vw{{Name}}]
AS
  /*
   * This is automatically generated; any changes will be lost. 
   */

  SELECT 
{{#each CoreColumns}}
    {{QualifiedName}}{{#unless @last}},{{/unless}}
{{/each}}
    FROM {{QualifiedName}} AS [{{Alias}}]
{{#ifval ColumnOrgUnitId}}
      INNER JOIN {{Root.OrgUnitJoinSql}} AS orgunits ON [{{Alias}}].[{{ColumnOrgUnitId.Name}}] = [orgunits].[{{ColumnOrgUnitId.Name}}]
{{/ifval}}
{{#each ViewWhere}}
    {{#if @first}}WHERE{{else}}  AND{{/if}} {{{.}}}
{{/each}}