{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
CREATE TYPE [{{Schema}}].[udt{{Name}}List] AS TABLE (
  /*
   * This is automatically generated; any changes will be lost. 
   */

{{#each UdtColumns}}
  {{UdtSql}}{{#unless @last}},{{/unless}}
{{/each}}
)