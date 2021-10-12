# 'Execute' object (database-driven)

The _Execute_ object enables additional TSQL statements to be embedded within the stored procedure.

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
The `Execute` object supports a number of properties that control the generated code output. The following properties with a bold name are those that are more typically used (considered more important).

Property | Description
-|-
**`statement`** | The additional TSQL statement. [Mandatory]
**`location`** | The location of the statement in relation to the underlying primary stored procedure statement. Valid options are: `Before`, `After`.<br/><br/>Defaults to `After`.

