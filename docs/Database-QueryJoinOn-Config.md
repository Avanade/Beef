# 'QueryJoinOn' object (database-driven)

The `QueryJoinOn` object defines the join on characteristics for a join within a query.

<br/>

## Example

A YAML configuration example is as follows:
``` yaml
queries:
- { name: Table, schema: Test, view: true, viewName: vwTestQuery, excludeColumns: [CreatedBy, UpdatedBy], permission: TestSec,
    joins: [
      { name: Person, schema: Demo, excludeColumns: [CreatedDate, UpdatedDate], aliasColumns: [RowVersion ^ RowVersionP],
        on: [
          { name: PersonId, toColumn: TableId }
        ]
      }
    ]
  }
```

<br/>

## Properties
The `QueryJoinOn` object supports a number of properties that control the generated code output. The following properties with a bold name are those that are more typically used (considered more important).

Property | Description
-|-
**`name`** | The name of the join column (from the `Join` table). [Mandatory]
`toSchema` | The name of the other join to table schema.<br/><br/>Defaults to `Table.Schema`; i.e. same schema. See also `ToTable` and `ToColumn` as these all relate.
`toTable` | The name of the other join to table.<br/><br/>Defaults to `Table.Name`; i.e. primary table. See also `ToSchema` and `ToColumn` as these all relate.
**`toColumn`** | The name of the other join to column.<br/><br/>Defaults to `Name`; i.e. assumes same name. See also `ToSchema` and `ToTable` as these all relate.
`toStatement` | The fully qualified name (`Alias.Name`) of the other column being joined to or other valid SQL (e.g. function) bypassing the corresponding `Schema`, `Table` and `Column` logic.

