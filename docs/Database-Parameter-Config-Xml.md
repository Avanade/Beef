# 'Parameter' object (database-driven)

The `Parameter` is used to define a stored procedure parameter and its charateristics. These are in addition to those that are automatically inferred (added) by the selected `StoredProcedure.Type`.

<br/>

## Properties
The `Parameter` object supports a number of properties that control the generated code output. The following properties with a bold name are those that are more typically used (considered more important).

Property | Description
-|-
**`Name`** | The parameter name (without the `@` prefix). [Mandatory]
`Column` | The corresponding column name; used to infer characteristics.<br/>&dagger; Defaults to `Name`.
`SqlType` | The SQL type definition (overrides inherited Column definition) including length/precision/scale.
`IsNullable` | Indicates whether the parameter is nullable.<br/>&dagger; Note that when the parameter value is `NULL` it will not be included in the query.
`TreatColumnNullAs` | Indicates whether the column value where NULL should be treated as the specified value; results in: `ISNULL([x].[col], value)`.
`IsCollection` | Indicates whether the parameter is a collection (one or more values to be included `IN` the query).
**`Operator`** | The where clause equality operator Valid options are: `EQ`, `NE`, `LT`, `LE`, `GT`, `GE`, `LIKE`.<br/>&dagger; Defaults to `EQ`.

