# 'OrderBy' object (database-driven) - YAML/JSON

The `OrderBy` object defines the query order. Only valid for `StoredProcedure.Type` of `GetAll`.

<br/>

## Example

A YAML example is as follows:
``` yaml
tables:
- { name: Table, schema: Test, create: true, update: true, upsert: true, delete: true, merge: true, udt: true, getAll: true, getAllOrderBy: [ Name Des ], excludeColumns: [ Other ], permission: TestSec,
    storedProcedures: [
      { name: GetByArgs, type: GetColl, excludeColumns: [ Count ],
        parameters: [
          { name: Name, nullable: true, operator: LIKE },
          { name: MinCount, operator: GE, column: Count },
          { name: MaxCount, operator: LE, column: Count, nullable: true }
        ]
      },
      { name: Get, type: Get, withHints: NOLOCK,
        execute: [
          { statement: EXEC Demo.Before, location: Before },
          { statement: EXEC Demo.After }
        ]
      },
      { name: Update, type: Update }
    ]
  }
```

<br/>

## Properties
The `OrderBy` object supports a number of properties that control the generated code output. The properties with a bold name are those that are more typically used (considered more important).

Property | Description
-|-
**`name`** | The name of the `Column` to order by.
**`order`** | The corresponding sort order. Valid options are: `Ascending`, `Descending`. Defaults to `Ascending`.

<br/>

<sub><sup>Note: This markdown file is generated; any changes will be lost.</sup></sub>
