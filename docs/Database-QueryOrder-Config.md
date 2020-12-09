# 'QueryOrder' object (database-driven) - YAML/JSON

The `QueryOrder` object that defines the query order.

<br/>

## Properties
The `QueryOrder` object supports a number of properties that control the generated code output. The properties with a bold name are those that are more typically used (considered more important).

Property | Description
-|-
**`name`** | The name of the `Column` to order by. See also `Schema` and `Table` as these all relate.
`schema` | The name of order by table schema. See also `Name` and `Column` as these all relate. Defaults to `Query.Schema`.
`table` | The name of the order by table. Defaults to `Table.Name`; i.e. primary table. See also `Schema` and `Column` as these all relate.
**`order`** | The corresponding sort order. Valid options are: `Ascending`, `Descending`. Defaults to `Ascending`.

<br/>

<sub><sup>Note: This markdown file is generated; any changes will be lost.</sup></sub>
