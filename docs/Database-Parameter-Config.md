# 'Parameter' object (database-driven) - YAML/JSON

The `Parameter` is used to define a stored procedure parameter and its charateristics. These are in addition to those that are automatically inferred (added) by the selected `StoredProcedure.Type`.

<br/>

## Example

Under construction.

<br/>

## Properties
The `Parameter` object supports a number of properties that control the generated code output. The properties with a bold name are those that are more typically used (considered more important).

Property | Description
-|-
**`name`** | The parameter name (without the `@` prefix).
`column` | The corresponding column name; used to infer characteristics. Defaults to `Name`.
`sqlType` | The SQL type definition (overrides inherited Column definition) including length/precision/scale.
`nullable` | Indicates whether the parameter is nullable. Note that when the parameter value is `NULL` it will not be included in the query.
`treatColumnNullAs` | Indicates whether the column value where NULL should be treated as the specified value; results in: `ISNULL([x].[col], value)`.
`collection` | Indicates whether the parameter is a collection (one or more values to be included `IN` the query).
**`operator`** | The where clause equality operator Valid options are: `EQ`, `NE`, `LT`, `LE`, `GT`, `GE`, `LIKE`. Defaults to `EQ`.

<br/>

<sub><sup>Note: This markdown file is generated; any changes will be lost.</sup></sub>
