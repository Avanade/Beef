{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
{{#if IsMerge}}
CREATE TABLE #temp (
  {{#each Columns}}
    [{{Name}}] {{SqlType}}{{#unless @last}}, {{/unless}}
  {{/each}}
)

  {{#each Rows}}
INSERT INTO #temp ({{#each Columns}}[{{Name}}]{{#unless @last}}, {{/unless}}{{/each}}) VALUES ({{#each Columns}}{{#if UseForeignKeyQueryForId}}(SELECT [{{DbColumn.ForeignColumn}}] FROM [{{DbColumn.ForeignSchema}}].[{{DbColumn.ForeignTable}}] WHERE [Code] = {{SqlValue}}){{else}}{{SqlValue}}{{/if}}{{#unless @last}}, {{/unless}}{{/each}})
  {{/each}}

MERGE INTO [{{Schema}}].[{{Name}}] WITH (HOLDLOCK) as [t]
  USING (SELECT {{#each Columns}}[{{Name}}]{{#unless @last}}, {{/unless}}{{/each}} FROM #temp) AS [s] 
    ON ({{#if IsRefData}}[s].[Code] = [t].[Code]{{else}}{{#each PrimaryKeyColumns}}{{#unless @first}} ON {{/unless}}[s].[{{Name}}] = [t].[{{Name}}]{{/each}}{{/if}})
  WHEN MATCHED AND EXISTS (
      SELECT {{#each MergeMatchColumns}}[s].[{{Name}}]{{#unless @last}}, {{/unless}}{{/each}}
      EXCEPT
      SELECT {{#each MergeMatchColumns}}[t].[{{Name}}]{{#unless @last}}, {{/unless}}{{/each}})
    THEN UPDATE SET {{#each Columns}}[t].[{{Name}}] = [s].[{{Name}}]{{#unless @last}}, {{/unless}}{{/each}}
  WHEN NOT MATCHED BY TARGET
    THEN INSERT ({{#each Columns}}[{{Name}}]{{#unless @last}}, {{/unless}}{{/each}})
      VALUES ({{#each Columns}}[s].[{{Name}}]{{#unless @last}}, {{/unless}}{{/each}});

SELECT @@ROWCOUNT
DROP TABLE #temp
{{else}}
  {{#each Rows}}
INSERT INTO [{{Table.Schema}}].[{{Table.Name}}] ({{#each Columns}}[{{Name}}]{{#unless @last}}, {{/unless}}{{/each}}) VALUES ({{#each Columns}}{{#if UseForeignKeyQueryForId}}(SELECT [{{DbColumn.ForeignColumn}}] FROM [{{DbColumn.ForeignSchema}}].[{{DbColumn.ForeignTable}}] WHERE [Code] = {{SqlValue}}){{else}}{{SqlValue}}{{/if}}{{#unless @last}}, {{/unless}}{{/each}})
  {{/each}}
SELECT {{Rows.Count}} -- Total rows inserted
{{/if}}