# 'OrderBy' object (database-driven)

The `OrderBy` object defines the query order. Only valid for `StoredProcedure.Type` of `GetAll`.

<br/>

## Properties
The `OrderBy` object supports a number of properties that control the generated code output. The following properties with a bold name are those that are more typically used (considered more important).

Property | Description
-|-
**`Name`** | The name of the `Column` to order by. [Mandatory]
`Order` | The corresponding sort order. Valid options are: `Asc`, `Desc`.<br/><br/>Defaults to `Asc`.

